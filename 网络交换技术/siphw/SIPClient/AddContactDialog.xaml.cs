using System.Windows;
using SIPClient.Models;

namespace SIPClient
{
    public partial class AddContactDialog : Window
    {
        public Contact Contact { get; private set; }

        public AddContactDialog()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string displayName = txtDisplayName.Text.Trim();
            string sipUri = txtSipUri.Text.Trim();

            if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(sipUri))
            {
                MessageBox.Show("请填写所有必填字段", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 验证SIP URI格式
            if (!sipUri.StartsWith("sip:") || !sipUri.Contains("@"))
            {
                MessageBox.Show("SIP URI格式不正确，应为：sip:username@domain.com", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 创建联系人对象
            Contact = new Contact
            {
                DisplayName = displayName,
                SipUri = sipUri,
                IsOnline = false,
                LastSeen = System.DateTime.Now
            };

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}