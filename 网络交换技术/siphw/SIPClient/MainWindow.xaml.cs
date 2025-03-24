using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SIPClient
{
    public partial class MainWindow : Window
    {
        private SIPManager sipManager;
        private ObservableCollection<Contact> contacts;
        private ObservableCollection<Message> messages;
        private Contact currentContact;

        public MainWindow()
        {
            InitializeComponent();
            
            // 初始化数据
            contacts = new ObservableCollection<Contact>();
            messages = new ObservableCollection<Message>();
            
            // 绑定数据源
            lstContacts.ItemsSource = contacts;
            lstMessages.ItemsSource = messages;
            
            // 注册值转换器
            Resources.Add("BoolToColorConverter", new BoolToColorConverter());
            Resources.Add("BoolToAlignmentConverter", new BoolToAlignmentConverter());
        }

        #region 事件处理

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            string server = txtServer.Text.Trim();
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(server))
            {
                lblStatus.Text = "请填写所有必填字段";
                return;
            }

            try
            {
                int port = int.Parse(txtPort.Text);
                
                // 创建SIP管理器
                sipManager = new SIPManager(username, password, server, port);
                
                // 注册事件
                sipManager.RegistrationStateChanged += SipManager_RegistrationStateChanged;
                sipManager.MessageReceived += SipManager_MessageReceived;
                sipManager.CallStateChanged += SipManager_CallStateChanged;
                
                // 开始注册
                lblStatus.Text = "正在连接到SIP服务器...";
                sipManager.Register();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"登录失败: {ex.Message}";
            }
        }

        private void btnAddContact_Click(object sender, RoutedEventArgs e)
        {
            // 显示添加联系人对话框
            var dialog = new AddContactDialog();
            if (dialog.ShowDialog() == true)
            {
                contacts.Add(dialog.Contact);
            }
        }

        private void lstContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentContact = lstContacts.SelectedItem as Contact;
            if (currentContact != null)
            {
                // 显示聊天面板
                ChatPanel.Visibility = Visibility.Visible;
                WelcomePanel.Visibility = Visibility.Collapsed;
                
                // 更新联系人信息
                lblCurrentContact.Text = currentContact.DisplayName;
                
                // 加载聊天记录
                LoadMessages(currentContact);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            if (currentContact != null && sipManager != null)
            {
                try
                {
                    sipManager.MakeCall(currentContact.SipUri);
                    btnCall.Visibility = Visibility.Collapsed;
                    btnEndCall.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"呼叫失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEndCall_Click(object sender, RoutedEventArgs e)
        {
            if (sipManager != null)
            {
                sipManager.EndCall();
                btnCall.Visibility = Visibility.Visible;
                btnEndCall.Visibility = Visibility.Collapsed;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RegisterDialog();
            if (dialog.ShowDialog() == true)
            {
                // 如果注册成功，自动填充登录信息
                txtUsername.Text = dialog.txtUsername.Text;
                txtPassword.Password = dialog.txtPassword.Password;
                txtServer.Text = dialog.txtServer.Text;
                txtPort.Text = dialog.txtPort.Text;
                lblStatus.Text = "注册成功，请登录";
            }
        }

        #endregion

        #region 辅助方法

        private void SendMessage()
        {
            string content = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(content) && currentContact != null && sipManager != null)
            {
                try
                {
                    // 发送消息
                    sipManager.SendMessage(currentContact.SipUri, content);
                    
                    // 添加到消息列表
                    var message = new Message
                    {
                        Content = content,
                        IsFromMe = true,
                        Timestamp = DateTime.Now
                    };
                    messages.Add(message);
                    
                    // 清空输入框
                    txtMessage.Text = string.Empty;
                    
                    // 滚动到底部
                    scrollMessages.ScrollToEnd();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"发送失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadMessages(Contact contact)
        {
            // 清空消息列表
            messages.Clear();
            
            // TODO: 从数据库加载聊天记录
            // 这里仅作示例
            messages.Add(new Message { Content = "你好！", IsFromMe = false, Timestamp = DateTime.Now.AddMinutes(-5) });
            messages.Add(new Message { Content = "你好，最近怎么样？", IsFromMe = true, Timestamp = DateTime.Now.AddMinutes(-4) });
            
            // 滚动到底部
            scrollMessages.ScrollToEnd();
        }

        #endregion

        #region SIP事件处理

        private void SipManager_RegistrationStateChanged(object sender, RegistrationStateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.IsRegistered)
                {
                    lblStatus.Text = "已成功登录";
                    
                    // 显示联系人面板
                    LoginPanel.Visibility = Visibility.Collapsed;
                    ContactsPanel.Visibility = Visibility.Visible;
                    
                    // 加载联系人列表
                    LoadContacts();
                }
                else
                {
                    lblStatus.Text = $"注册失败: {e.ErrorMessage}";
                }
            });
        }

        private void SipManager_MessageReceived(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // 查找联系人
                Contact contact = FindContactBySipUri(e.FromUri);
                if (contact == null)
                {
                    // 创建新联系人
                    contact = new Contact
                    {
                        DisplayName = e.FromName,
                        SipUri = e.FromUri
                    };
                    contacts.Add(contact);
                }
                
                // 如果是当前联系人，添加到消息列表
                if (currentContact != null && currentContact.SipUri == e.FromUri)
                {
                    var message = new Message
                    {
                        Content = e.Content,
                        IsFromMe = false,
                        Timestamp = DateTime.Now
                    };
                    messages.Add(message);
                    
                    // 滚动到底部
                    scrollMessages.ScrollToEnd();
                }
                else
                {
                    // TODO: 显示通知
                }
            });
        }

        private void SipManager_CallStateChanged(object sender, CallStateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.State)
                {
                    case CallState.Incoming:
                        // 显示来电对话框
                        var result = MessageBox.Show($"来自 {e.RemoteName} 的呼叫，是否接听？", "来电", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            sipManager.AnswerCall();
                            btnCall.Visibility = Visibility.Collapsed;
                            btnEndCall.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            sipManager.RejectCall();
                        }
                        break;
                        
                    case CallState.Connected:
                        btnCall.Visibility = Visibility.Collapsed;
                        btnEndCall.Visibility = Visibility.Visible;
                        break;
                        
                    case CallState.Disconnected:
                        btnCall.Visibility = Visibility.Visible;
                        btnEndCall.Visibility = Visibility.Collapsed;
                        break;
                }
            });
        }

        private void LoadContacts()
        {
            // 清空联系人列表
            contacts.Clear();
            
            // TODO: 从数据库加载联系人
            // 这里仅作示例
            contacts.Add(new Contact { DisplayName = "张三", SipUri = "sip:zhangsan@example.com" });
            contacts.Add(new Contact { DisplayName = "李四", SipUri = "sip:lisi@example.com" });
        }

        private Contact FindContactBySipUri(string sipUri)
        {
            foreach (var contact in contacts)
            {
                if (contact.SipUri == sipUri)
                {
                    return contact;
                }
            }
            return null;
        }

        #endregion
    }

    #region 值转换器

    public class BoolToColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isFromMe = (bool)value;
            return isFromMe ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToAlignmentConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isFromMe = (bool)value;
            return isFromMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}