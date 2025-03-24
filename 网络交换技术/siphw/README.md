# SIP即时聊天程序

这是一个基于SIP协议的即时聊天程序，实现了VoIP呼叫接续、认证注册等功能。

## 项目结构

- `SIPClient/` - Windows桌面客户端（C#/WPF）
- `SIPServer/` - SIP服务器配置和脚本

## 功能特性

- 用户注册与认证
- 联系人管理
- 即时消息收发
- 语音通话
- 呼叫接续

## 技术栈

- 客户端：C#/WPF, PJSIP库
- 服务器：Kamailio SIP服务器

## 安装与使用

### 客户端

1. 打开`SIPClient/SIPClient.sln`解决方案文件
2. 使用Visual Studio编译项目
3. 运行生成的可执行文件

### 服务器

请参考`SIPServer/README.md`中的服务器配置说明。