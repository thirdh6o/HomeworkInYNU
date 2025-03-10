from fastapi import FastAPI, WebSocket
from fastapi.middleware.cors import CORSMiddleware
import requests
import json
import asyncio

app = FastAPI()

# 配置CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.websocket("/ws/chat")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    
    try:
        while True:
            # 接收消息
            data = await websocket.receive_json()
            message = data.get("message", "")
            model = data.get("model", "deepseek-r1:7b")
            print(f"收到消息请求: 模型={model}, 消息={message}")
            
            # 调用Ollama API
            response = requests.post(
                "http://localhost:11434/api/chat",
                json={
                    "model": model,
                    "messages": [
                        {
                            "role": "user",
                            "content": message
                        }
                    ],
                    "stream": True
                },
                stream=True
            )
            
            try:
                # 处理流式响应
                for line in response.iter_lines():
                    if line:
                        try:
                            data = json.loads(line.decode('utf-8'))
                            if 'message' in data and 'content' in data['message']:
                                await websocket.send_text(data['message']['content'])
                        except Exception as e:
                            await websocket.send_json({"error": str(e)})
                            break
                
                # 发送完成标记
                await websocket.send_json({"status": "done"})
            except Exception as e:
                await websocket.send_json({"error": str(e)})
                        
    except Exception as e:
        print(f"WebSocket error: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)