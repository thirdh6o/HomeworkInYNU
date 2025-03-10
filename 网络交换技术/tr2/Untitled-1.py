import requests
import json
# 生成文本
response = requests.post(
    "http://localhost:11434/api/chat",
    json={
        "model": "deepseek-r1:7b",
        "messages": [
            {
                "role": "user",
                "content": "给我几个英语单词供我练习?"
            }
        ],
        "stream": True
    },
    stream=True  # 启用requests的流式传输
)

# 处理流式响应
for line in response.iter_lines():
    if line:
        # 解析每个数据块
        try:
            # 将字节数据解码为字符串并解析JSON
            data = json.loads(line.decode('utf-8'))
            # 只输出消息内容
            if 'message' in data and 'content' in data['message']:
                print(data['message']['content'], end='', flush=True)
        except Exception as e:
            print(f"解析错误: {e}")