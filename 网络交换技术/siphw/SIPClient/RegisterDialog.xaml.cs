using System;
using System.Windows;
using System.Text.RegularExpressions;

namespace SIPClient
{
    public partial class RegisterDialog : Window
    {
        private SIPManager sipManager;

        public RegisterDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            // 清除之前的错误信息
            lblStatus.Text = string.Empty;

            // 获取输入值
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();
            string confirmPassword = txtConfirmPassword.Password.Trim();
            string email = txtEmail.Text.Trim();
            string server = txtServer.Text.Trim();

            // 验证输入
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(server))
            {
                lblStatus.Text = "请填写所有必填字段";
                return;
            }

            // 验证密码匹配
            if (password != confirmPassword)
            {
                lblStatus.Text = "两次输入的密码不匹配";
                return;
            }

            // 验证邮箱格式
            if (!IsValidEmail(email))
            {
                lblStatus.Text = "请输入有效的邮箱地址";
                return;
            }

            try
            {
                // 验证端口号
                if (!int.TryParse(txtPort.Text, out int port))
                {
                    lblStatus.Text = "请输入有效的端口号";
                    return;
                }

                // 创建SIP管理器实例
                sipManager = new SIPManager(username, password, server, port);

                // 注册事件处理
                sipManager.RegistrationStateChanged += SipManager_RegistrationStateChanged;

                // 开始注册
                lblStatus.Text = "正在注册...";
                btnRegister.IsEnabled = false;
                sipManager.Register();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"注册失败: {ex.Message}";
                btnRegister.IsEnabled = true;
            }
        }

        private void SipManager_RegistrationStateChanged(object sender, RegistrationStateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.IsRegistered)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    lblStatus.Text = $"注册失败: {e.ErrorMessage}";
                    btnRegister.IsEnabled = true;
                }
            });
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }
}