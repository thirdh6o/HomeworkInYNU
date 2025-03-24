using System;
using System.Windows;

namespace SIPClient
{
    public partial class App : Application
    {
        public App()
        {
            // 注册全局异常处理
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"应用程序发生错误：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                MessageBox.Show($"应用程序发生严重错误：{exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}