using System;
using System.Threading.Tasks;

namespace SIPClient
{
    public enum CallState
    {
        Idle,
        Outgoing,
        Incoming,
        Connected,
        Disconnected
    }

    public class RegistrationStateEventArgs : EventArgs
    {
        public bool IsRegistered { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class MessageEventArgs : EventArgs
    {
        public string FromUri { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class CallStateEventArgs : EventArgs
    {
        public CallState State { get; set; }
        public string RemoteUri { get; set; } = string.Empty;
        public string RemoteName { get; set; } = string.Empty;
    }

    public class SIPManager
    {
        private string username;
        private string password;
        private string server;
        private int port;
        private bool isRegistered;
        private CallState currentCallState;

        // 事件
        public event EventHandler<RegistrationStateEventArgs> RegistrationStateChanged;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<CallStateEventArgs> CallStateChanged;

        public SIPManager(string username, string password, string server, int port)
        {
            this.username = username;
            this.password = password;
            this.server = server;
            this.port = port;
            this.isRegistered = false;
            this.currentCallState = CallState.Idle;

            // TODO: 初始化PJSIP库
            // 这里应该初始化PJSIP库，但由于我们没有实际的PJSIP库，所以这里只是模拟
            Console.WriteLine("SIP Manager initialized");

            // 模拟接收消息
            Task.Run(async () =>
            {
                await Task.Delay(10000); // 10秒后模拟收到消息
                OnMessageReceived("sip:zhangsan@example.com", "张三", "你好，这是一条测试消息！");
            });
        }

        public void Register()
        {
            // TODO: 实现SIP注册
            // 这里应该调用PJSIP库进行SIP注册，但由于我们没有实际的PJSIP库，所以这里只是模拟
            Console.WriteLine($"Registering to {server}:{port} with username {username}");

            // 模拟注册成功
            Task.Run(async () =>
            {
                await Task.Delay(2000); // 模拟网络延迟
                isRegistered = true;
                OnRegistrationStateChanged(true, string.Empty);
            });
        }

        public void Unregister()
        {
            // TODO: 实现SIP注销
            Console.WriteLine("Unregistering from SIP server");

            // 模拟注销成功
            isRegistered = false;
            OnRegistrationStateChanged(false, string.Empty);
        }

        public void SendMessage(string toUri, string content)
        {
            // TODO: 实现发送SIP消息
            Console.WriteLine($"Sending message to {toUri}: {content}");

            // 这里应该调用PJSIP库发送SIP消息，但由于我们没有实际的PJSIP库，所以这里只是模拟
            // 模拟发送成功
        }

        public void MakeCall(string toUri)
        {
            // TODO: 实现拨打电话
            Console.WriteLine($"Making call to {toUri}");

            // 这里应该调用PJSIP库拨打电话，但由于我们没有实际的PJSIP库，所以这里只是模拟
            currentCallState = CallState.Outgoing;
            OnCallStateChanged(CallState.Outgoing, toUri, GetDisplayNameFromUri(toUri));

            // 模拟对方接听
            Task.Run(async () =>
            {
                await Task.Delay(3000); // 模拟等待3秒
                currentCallState = CallState.Connected;
                OnCallStateChanged(CallState.Connected, toUri, GetDisplayNameFromUri(toUri));
            });
        }

        public void AnswerCall()
        {
            // TODO: 实现接听电话
            Console.WriteLine("Answering incoming call");

            // 这里应该调用PJSIP库接听电话，但由于我们没有实际的PJSIP库，所以这里只是模拟
            currentCallState = CallState.Connected;
            OnCallStateChanged(CallState.Connected, "", "");
        }

        public void RejectCall()
        {
            // TODO: 实现拒绝电话
            Console.WriteLine("Rejecting incoming call");

            // 这里应该调用PJSIP库拒绝电话，但由于我们没有实际的PJSIP库，所以这里只是模拟
            currentCallState = CallState.Idle;
            OnCallStateChanged(CallState.Disconnected, "", "");
        }

        public void EndCall()
        {
            // TODO: 实现挂断电话
            Console.WriteLine("Ending call");

            // 这里应该调用PJSIP库挂断电话，但由于我们没有实际的PJSIP库，所以这里只是模拟
            currentCallState = CallState.Idle;
            OnCallStateChanged(CallState.Disconnected, "", "");
        }

        private void OnRegistrationStateChanged(bool isRegistered, string errorMessage)
        {
            RegistrationStateChanged?.Invoke(this, new RegistrationStateEventArgs
            {
                IsRegistered = isRegistered,
                ErrorMessage = errorMessage
            });
        }

        private void OnMessageReceived(string fromUri, string fromName, string content)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs
            {
                FromUri = fromUri,
                FromName = fromName,
                Content = content
            });
        }

        private void OnCallStateChanged(CallState state, string remoteUri, string remoteName)
        {
            CallStateChanged?.Invoke(this, new CallStateEventArgs
            {
                State = state,
                RemoteUri = remoteUri,
                RemoteName = remoteName
            });
        }

        private string GetDisplayNameFromUri(string uri)
        {
            // 简单实现从SIP URI中提取显示名称
            // 例如：从 "sip:zhangsan@example.com" 提取 "zhangsan"
            try
            {
                string username = uri.Split(':')[1].Split('@')[0];
                return username;
            }
            catch
            {
                return uri;
            }
        }
    }
}