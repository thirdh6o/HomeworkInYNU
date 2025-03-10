<template>
  <div class="chat-container">
    <div class="chat-header">
      <h2>Ollama Chat</h2>
      <el-select v-model="selectedModel" placeholder="选择模型">
        <el-option label="DeepSeek 7B" value="deepseek-r1:7b" />
        <el-option label="DeepSeek 1.5B" value="deepseek-r1:1.5b" />
      </el-select>
    </div>
    
    <div class="chat-messages" ref="messagesContainer">
      <div v-for="(message, index) in messages" :key="index" class="message">
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
          <el-button @click="sendMessage" :loading="isLoading">
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
      if (data.message && data.message.content) {
        messages.value[messages.value.length - 1] += data.message.content
      } else {
        messages.value[messages.value.length - 1] += data
      }
      scrollToBottom()
    } catch {
      // 如果不是JSON格式，则为普通文本消息
      messages.value[messages.value.length - 1] += event.data
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
  messages.value.push(`用户 (使用模型: ${selectedModel.value}): ${inputMessage.value}`)
  messages.value.push('')
  
  const messageData = {
    message: inputMessage.value,
    model: selectedModel.value
  }
  console.log('发送消息数据:', messageData)
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
  max-width: 800px;
  margin: 0 auto;
  height: 100vh;
  display: flex;
  flex-direction: column;
  padding: 20px;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: #f5f7fa;
  border-radius: 8px;
  margin-bottom: 20px;
}

.message {
  margin-bottom: 12px;
  padding: 8px 12px;
  background: white;
  border-radius: 4px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.message-content {
  white-space: pre-wrap;
  word-break: break-word;
}

.chat-input {
  margin-bottom: 20px;
}
</style>