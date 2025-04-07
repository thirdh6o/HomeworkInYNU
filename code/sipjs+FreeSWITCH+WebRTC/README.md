# SIP.js + Asterisk + WebRTC 配置指南

## 服务器配置步骤

### 1. 安装Asterisk

```bash
# 安装依赖
apt-get update
apt-get install -y build-essential wget libssl-dev libncurses5-dev libnewt-dev libxml2-dev linux-headers-$(uname -r) libsqlite3-dev uuid-dev

# 下载并安装Asterisk
cd /usr/src
wget http://downloads.asterisk.org/pub/telephony/asterisk/asterisk-18-current.tar.gz
tar xvf asterisk-18-current.tar.gz
cd asterisk-18*/

# 配置并编译Asterisk
./configure
make menuselect
make
make install
make samples

# 创建asterisk用户
adduser --system --group --home /var/lib/asterisk --no-create-home --gecos "Asterisk PBX" asterisk
chown -R asterisk:asterisk /var/{lib,log,run,spool}/asterisk /usr/lib/asterisk
chmod -R 750 /var/{lib,log,run,spool}/asterisk /usr/lib/asterisk
```

### 2. 配置Asterisk
#### 2.1 配置SIP和WebSocket支持

编辑 `/etc/asterisk/http.conf`，启用WebSocket支持：

```ini
[general]
enabled=yes
bindaddr=0.0.0.0
bindport=8088
tlsenable=yes
tlsbindaddr=0.0.0.0:8089
tlscertfile=/etc/asterisk/keys/asterisk.pem
tlsprivatekey=/etc/asterisk/keys/asterisk.key
```

编辑 `/etc/asterisk/pjsip.conf`，配置SIP终端：

```ini
[transport-ws]
type=transport
protocol=ws
bind=0.0.0.0:8088

[transport-wss]
type=transport
protocol=wss
bind=0.0.0.0:8089

[webrtc_client]
type=endpoint
transport=transport-ws
context=default
disallow=all
allow=opus,ulaw
dtls_auto_generate_cert=yes
webrtc=yes
```

### 3. 配置SSL证书

```bash
# 生成自签名证书
openssl req -x509 -newkey rsa:2048 -keyout /etc/freeswitch/tls/wss.pem -out /etc/freeswitch/tls/wss.pem -days 365 -nodes
```

### 4. 安装和配置Nginx

```bash
# 安装Nginx
apt-get install -y nginx

# 配置Nginx虚拟主机
cat > /etc/nginx/sites-available/sip-webrtc << EOF
server {
    listen 80;
    server_name your_domain.com;

    root /var/www/html/sip-webrtc;
    index index.html;

    location / {
        try_files \$uri \$uri/ =404;
    }

    location /ws {
        proxy_pass http://127.0.0.1:5066;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
EOF

# 启用站点配置
ln -s /etc/nginx/sites-available/sip-webrtc /etc/nginx/sites-enabled/
```

### 5. 部署网站文件

```bash
# 创建网站目录
mkdir -p /var/www/html/sip-webrtc

# 复制网站文件
cp -r * /var/www/html/sip-webrtc/
```

### 6. 配置防火墙

```bash
# 开放必要端口
ufw allow 80/tcp    # HTTP
ufw allow 443/tcp   # HTTPS
ufw allow 5060/tcp  # SIP
ufw allow 5060/udp  # SIP
ufw allow 5066/tcp  # WebSocket
ufw allow 7443/tcp  # WSS
ufw allow 16384:32768/udp  # RTP媒体流
```

### 7. 修改网站配置

在index.html中，修改WebSocket服务器地址为本地地址：

```javascript
var wsServer = "ws://your_domain.com/ws";
// 或者使用WSS
var wsServer = "wss://your_domain.com:7443";
```

### 8. 启动服务

```bash
# 启动FreeSWITCH
systemctl start freeswitch

# 启动Nginx
systemctl start nginx
```

## 测试配置

1. 访问 `http://your_domain.com` 确认网站可以正常访问
2. 使用SIP客户端（如index.html页面）尝试登录
3. 测试音频通话功能

## 故障排查

1. 检查FreeSWITCH日志：`tail -f /var/log/freeswitch/freeswitch.log`
2. 检查Nginx日志：`tail -f /var/log/nginx/error.log`
3. 确认防火墙端口已开放：`ufw status`
4. 检查WebSocket连接状态：使用浏览器开发者工具的Network标签

## 注意事项

1. 生产环境必须使用有效的SSL证书以支持WSS连接
2. 确保防火墙开放8088(WS)和8089(WSS)端口
3. 配置Asterisk的DTLS证书以支持WebRTC
4. 定期备份配置文件
5. 及时更新系统和组件的安全补丁