# SIP服务器配置指南

本文档提供了使用Kamailio配置SIP服务器的步骤，用于支持SIP即时聊天程序的服务器端功能。

## 安装Kamailio

### Ubuntu/Debian系统

```bash
# 添加Kamailio仓库
sudo apt-get update
sudo apt-get install software-properties-common
sudo add-apt-repository ppa:kamailio/releases

# 安装Kamailio及其模块
sudo apt-get update
sudo apt-get install kamailio kamailio-mysql-modules kamailio-tls-modules kamailio-websocket-modules
```

### CentOS/RHEL系统

```bash
# 添加Kamailio仓库
sudo yum install -y epel-release
sudo rpm -Uvh https://rpm.kamailio.org/centos/kamailio-centos-$releasever.rpm

# 安装Kamailio及其模块
sudo yum install -y kamailio kamailio-mysql kamailio-tls kamailio-websocket
```

## 配置Kamailio

1. 编辑主配置文件：`/etc/kamailio/kamailio.cfg`
2. 配置MySQL数据库（用于存储用户账户）
3. 配置TLS（用于安全通信）

## 基本配置示例

以下是一个基本的Kamailio配置示例，支持用户注册、认证和消息路由：

```
#!KAMAILIO

#!define WITH_MYSQL
#!define WITH_AUTH
#!define WITH_USRLOCDB
#!define WITH_TLS

# 包含标准模块
#!include_file "modules.k"

# 全局参数
listener=udp:0.0.0.0:5060
listener=tcp:0.0.0.0:5060

#!ifdef WITH_TLS
listener=tls:0.0.0.0:5061
#!endif

# 模块加载
loadmodule "tm.so"
loadmodule "sl.so"
loadmodule "rr.so"
loadmodule "pv.so"
loadmodule "maxfwd.so"
loadmodule "textops.so"
loadmodule "siputils.so"
loadmodule "xlog.so"
loadmodule "sanity.so"
loadmodule "ctl.so"
loadmodule "cfg_rpc.so"
loadmodule "acc.so"
loadmodule "counters.so"

#!ifdef WITH_AUTH
loadmodule "auth.so"
loadmodule "auth_db.so"
#!endif

#!ifdef WITH_MYSQL
loadmodule "db_mysql.so"
#!endif

loadmodule "usrloc.so"
loadmodule "registrar.so"
loadmodule "rtpproxy.so"
loadmodule "nathelper.so"
loadmodule "path.so"

# 模块参数

#!ifdef WITH_MYSQL
# 数据库URL
modparam("db_mysql", "url", "mysql://root:b75dd9176641dc49@localhost/kamailio")
#!endif

# 用户位置模块参数
modparam("usrloc", "db_url", "mysql://root:b75dd9176641dc49@localhost/kamailio")
modparam("usrloc", "db_mode", 2)

# 认证参数
#!ifdef WITH_AUTH
modparam("auth_db", "db_url", "mysql://root:b75dd9176641dc49@localhost/kamailio")
modparam("auth_db", "calculate_ha1", 1)
modparam("auth_db", "password_column", "password")
modparam("auth_db", "load_credentials", "")
#!endif

# 路由逻辑
request_route {
    # 最大转发检查
    if (!mf_process_maxfwd_header("10")) {
        sl_send_reply("483", "Too Many Hops");
        exit;
    }

    # 请求完整性检查
    if (!sanity_check(1511, 7)) {
        xlog("L_WARN", "Malformed SIP message from $si:$sp\n");
        exit;
    }

    # 处理CANCEL请求
    if (is_method("CANCEL")) {
        if (t_check_trans()) {
            t_relay();
        }
        exit;
    }

    # 处理REGISTER请求
    if (is_method("REGISTER")) {
        # 认证请求
        #!ifdef WITH_AUTH
        if (!auth_check("$fd", "subscriber", "1")) {
            auth_challenge("$fd", "0");
            exit;
        }
        #!endif

        # 保存位置信息
        if (!save("location")) {
            sl_reply_error();
        }
        exit;
    }

    # 记录路由
    if (!is_method("REGISTER|MESSAGE")) {
        record_route();
    }

    # 处理初始请求
    if (is_method("INVITE|SUBSCRIBE|MESSAGE|PUBLISH|INFO")) {
        # 认证请求
        #!ifdef WITH_AUTH
        if (!auth_check("$fd", "subscriber", "1")) {
            auth_challenge("$fd", "0");
            exit;
        }
        #!endif

        # 查找用户位置
        if (!lookup("location")) {
            t_reply("404", "Not Found");
            exit;
        }
    }

    # 转发请求
    if (!t_relay()) {
        sl_reply_error();
    }
}
```

## 创建用户账户

使用以下命令创建SIP用户账户：

```bash
kamctl add username password email
```

例如：

```bash
kamctl add alice secret alice@example.com
kamctl add bob secret bob@example.com
```

## 启动和管理服务

```bash
# 启动Kamailio服务
sudo systemctl start kamailio

# 设置开机自启
sudo systemctl enable kamailio

# 检查服务状态
sudo systemctl status kamailio

# 重新加载配置
sudo kamctl reload
```

## 防火墙配置

确保以下端口在防火墙中开放：

- UDP/TCP 5060 (SIP)
- UDP/TCP 5061 (SIP-TLS)
- UDP 10000-20000 (RTP媒体流，如果使用RTPProxy)

## 故障排除

- 检查Kamailio日志：`/var/log/kamailio.log`
- 使用SIP调试工具如SIPp或Wireshark分析SIP流量
- 确保数据库连接正常

## 安全建议

- 使用TLS加密SIP信令
- 配置防火墙限制访问
- 定期更新Kamailio和系统
- 使用强密码
- 考虑实施SIP防火墙或SBC (Session Border Controller)