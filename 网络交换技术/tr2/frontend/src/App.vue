<template>
  <div class="chat-container">
    <div class="chat-header">
      <h2>HEYUQI CHAT</h2>
      <el-select v-model="selectedModel" placeholder="选择模型">
        <el-option label="DeepSeek 7B" value="deepseek-r1:7b" />
        <el-option label="DeepSeek 1.5B" value="deepseek-r1:1.5b" />
      </el-select>
    </div>
    
    <div class="chat-messages" ref="messagesContainer">
      <div v-for="(message, index) in messages" :key="index" class="message" :class="{ 'user-message': message.startsWith('好运气:'), 'ai-message': message.startsWith('YXQ:') }">
        <div class="message-content">{{ message }}</div>
      </div>
    </div>
    
    <div class="chat-input">
      <el-input
        v-model="inputMessage"
        placeholder="输入消息..."
        :disabled="isLoading"
        @keyup.enter="sendMessage"
      >
        <template #append>
          <el-button @click="sendMessage" :loading="isLoading" type="primary">
            发送
          </el-button>
        </template>
      </el-input>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'

const messages = ref([])
const inputMessage = ref('')
const selectedModel = ref('deepseek-r1:7b')
const isLoading = ref(false)
const messagesContainer = ref(null)
let ws = null

const scrollToBottom = async () => {
  await nextTick()
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

const connectWebSocket = () => {
  ws = new WebSocket('ws://localhost:8000/ws/chat')
  
  ws.onmessage = (event) => {
    try {
      // 尝试解析JSON数据
      const data = JSON.parse(event.data)
      
      // 处理完成标记
      if (data.status === 'done') {
        isLoading.value = false
        return
      }
      
      // 处理错误
      if (data.error) {
        ElMessage.error(data.error)
        isLoading.value = false
        return
      }
      
      // 处理正常消息
      if (messages.value.length === 0 || !messages.value[messages.value.length - 1].startsWith('YXQ: ')) {
        messages.value.push('YXQ: ')
      }
      
      const content = typeof event.data === 'string' ? event.data : (data.message?.content || '');
      if (content) {
        messages.value[messages.value.length - 1] += content
      }
      scrollToBottom()
    } catch (error) {
      // 如果不是JSON格式，则为普通文本消息
      if (messages.value.length === 0 || !messages.value[messages.value.length - 1].startsWith('YXQ: ')) {
        messages.value.push('YXQ: ')
      }
      if (event.data) {
        messages.value[messages.value.length - 1] += event.data
      }
      scrollToBottom()
    }
  }
  
  ws.onerror = (error) => {
    ElMessage.error('WebSocket连接错误')
    isLoading.value = false
  }
  
  ws.onclose = () => {
    setTimeout(connectWebSocket, 1000)
  }
}

const sendMessage = () => {
  if (!inputMessage.value.trim() || isLoading.value) return
  
  if (ws?.readyState !== WebSocket.OPEN) {
    ElMessage.error('正在重新连接服务器...')
    return
  }
  
  isLoading.value = true
  messages.value.push(`好运气: ${inputMessage.value}`)
  messages.value.push('') // 为YXQ响应预留一个空字符串
  
  const messageData = {
    message: inputMessage.value,
    model: selectedModel.value
  }
  ws.send(JSON.stringify(messageData))
  
  inputMessage.value = ''
  scrollToBottom()
}

onMounted(() => {
  connectWebSocket()
})
</script>

<style scoped>
.chat-container {
  max-width: min(1200px, 90%);
  margin: 0 auto;
  height: 100vh;
  display: flex;
  flex-direction: column;
  padding: 20px;
  position: relative;
}

.chat-container::before {
  content: '';
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #1a237e 0%, #0277bd 50%, #00bcd4 100%);
  z-index: -1;
  opacity: 0.8;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  background: rgba(255, 255, 255, 0.9);
  padding: 15px;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.chat-header h2 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.8em;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: rgba(255, 255, 255, 0.85);
  border-radius: 12px;
  margin-bottom: 20px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.message {
  margin-bottom: 16px;
  max-width: 85%;
  clear: both;
}

.user-message {
  float: right;
}

.ai-message {
  float: left;
}

.message-content {
  padding: 12px 16px;
  border-radius: 12px;
  white-space: pre-wrap;
  word-break: break-word;
  line-height: 1.5;
  font-size: 1.1em;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.user-message .message-content {
  background: #1989fa;
  color: white;
  border-bottom-right-radius: 4px;
}

.ai-message .message-content {
  background: white;
  color: #2c3e50;
  border-bottom-left-radius: 4px;
}

.chat-input {
  margin-bottom: 20px;
  background: rgba(255, 255, 255, 0.9);
  padding: 15px;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

:deep(.el-input__wrapper) {
  box-shadow: none !important;
  border: 1px solid #dcdfe6;
  border-radius: 8px;
}

:deep(.el-input__inner) {
  height: 44px;
  font-size: 1.1em;
}

:deep(.el-button) {
  height: 44px;
  font-size: 1.1em;
  padding: 0 20px;
}
</style>