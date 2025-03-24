# Kamailio数据库配置指南

本指南将帮助您配置Kamailio服务器的MySQL数据库。

## 1. 安装MySQL服务器

```bash
# 安装MySQL服务器
sudo apt-get install mysql-server
# 或者在CentOS上
sudo yum install mysql-server
```

## 2. 创建数据库和用户

登录到MySQL：

```bash
mysql -u root -p
```

执行以下SQL命令：

```sql
# 创建数据库
CREATE DATABASE kamailio;

# 创建用户并授权
CREATE USER 'kamailio'@'localhost' IDENTIFIED BY 'kamailiorw';
GRANT ALL PRIVILEGES ON kamailio.* TO 'kamailio'@'localhost';
FLUSH PRIVILEGES;
```

## 3. 导入Kamailio数据库架构

```bash
# 进入Kamailio的MySQL架构目录
cd /usr/share/kamailio/mysql

# 导入数据库架构
mysql -u kamailio -p kamailio < standard-create.sql
```

## 4. 验证数据库配置

登录MySQL检查数据库：

```bash
mysql -u kamailio -p kamailio

# 在MySQL提示符下执行
SHOW TABLES;
```

## 5. 配置Kamailio数据库连接

编辑Kamailio配置文件中的数据库URL：

```
# 数据库URL
modparam("db_mysql", "url", "mysql://kamailio:kamailiorw@localhost/kamailio")

# 用户位置模块参数
modparam("usrloc", "db_url", "mysql://kamailio:kamailiorw@localhost/kamailio")

# 认证参数
modparam("auth_db", "db_url", "mysql://kamailio:kamailiorw@localhost/kamailio")
```

## 6. 重启Kamailio服务

```bash
sudo systemctl restart kamailio
```

## 7. 验证配置

检查Kamailio日志文件是否有数据库连接错误：

```bash
tail -f /var/log/kamailio.log
```

## 注意事项

1. 请确保使用强密码替换示例中的'kamailiorw'
2. 根据您的安全需求，可以限制数据库用户的访问权限
3. 定期备份数据库
4. 确保MySQL服务器已启动并运行