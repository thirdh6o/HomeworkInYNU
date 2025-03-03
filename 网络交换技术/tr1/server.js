const express = require('express');
const bodyParser = require('body-parser');
const { exec } = require('child_process');

const app = express();
const PORT = 3000;

app.use(bodyParser.json());
app.use(express.static('public')); // 确保 public 文件夹被正确服务

app.post('/api/chat', (req, res) => {
    const input = req.body.messages[0].content; // 获取用户输入

    // 调用 DeepSeek 模型的命令行接口
    exec(`ollama run deepseek-r1:7b "${input}"`, (error, stdout, stderr) => {
        if (error) {
            console.error(`执行错误: ${error}`);
            return res.status(500).json({ message: '发生错误，请重试。' });
        }
        res.json({ message: { role: "assistant", content: stdout.trim() } }); // 返回格式化的输出
    });
});

app.listen(PORT, () => {
    console.log(`服务器正在运行，访问 http://localhost:${PORT}`);
}); 