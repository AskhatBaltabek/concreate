<template>
  <div class="max-w-3xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
    <div class="bg-slate-900/80 backdrop-blur-xl border border-white/10 rounded-2xl shadow-2xl p-8 sm:p-10 hover:shadow-indigo-500/10 transition-shadow duration-500">
      <h2 class="text-3xl font-bold text-white mb-2">Create New Project</h2>
      <p class="text-slate-400 mb-8">Setup the parameters for your AI Faceless Video</p>

      <form @submit.prevent="handleCreateProject" class="space-y-6">
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-slate-300 mb-1">Project Title</label>
            <input v-model="title" type="text" required placeholder="My Awesome Video"
                   class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
          </div>

          <div>
            <label class="block text-sm font-medium text-slate-300 mb-1">Description / Topic</label>
            <textarea v-model="description" required rows="3" placeholder="A short video about quantum physics..."
                      class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all"></textarea>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-slate-300 mb-1">Video Length</label>
              <select v-model="lengthSeconds" class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all">
                <option :value="30">30 Seconds (Short)</option>
                <option :value="60">60 Seconds (Standard)</option>
                <option :value="90">90 Seconds (Long)</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-slate-300 mb-1">Video Format</label>
              <select v-model="format" class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all">
                <option value="9:16">Vertical (9:16) - TikTok/Reels</option>
                <option value="16:9">Horizontal (16:9) - YouTube</option>
                <option value="1:1">Square (1:1) - Instagram</option>
              </select>
            </div>
          </div>
        </div>

        <div class="pt-6 flex justify-end space-x-4 border-t border-slate-800">
          <NuxtLink to="/dashboard" class="px-6 py-3 border border-slate-700 text-slate-300 font-medium rounded-lg hover:bg-slate-800 transition-colors">Cancel</NuxtLink>
          <button type="submit" :disabled="loading" 
                  class="px-6 py-3 bg-indigo-600 text-white font-bold rounded-lg hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-slate-900 focus:ring-indigo-500 transition-colors disabled:opacity-70 disabled:cursor-not-allowed">
            {{ loading ? 'Creating...' : 'Create & Generate Script' }}
          </button>
        </div>
        <p v-if="errorMsg" class="text-red-400 text-end text-sm font-medium mt-2">{{ errorMsg }}</p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const title = ref('')
const description = ref('')
const lengthSeconds = ref(30)
const format = ref('9:16')
const loading = ref(false)
const errorMsg = ref('')
const router = useRouter()
const token = ref(null)

onMounted(() => {
  token.value = localStorage.getItem('token')
  if (!token.value) {
    router.push('/login')
  }
})

const handleCreateProject = async () => {
  loading.value = true
  errorMsg.value = ''
  try {
    const response = await fetch('http://localhost:5239/api/projects', {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token.value}`
      },
      body: JSON.stringify({
        title: title.value,
        description: description.value,
        lengthSeconds: lengthSeconds.value,
        format: format.value
      })
    })
    
    if (response.status === 401) {
      localStorage.removeItem('token')
      router.push('/login')
      return
    }
    
    if (!response.ok) throw new Error('Failed to create project')
    
    const data = await response.json()
    
    // 💾 Save to local cache immediately so dashboard shows it right away
    const cached = JSON.parse(localStorage.getItem('cached_projects') || '[]')
    cached.unshift({
      id: data.projectId,
      title: title.value,
      description: description.value,
      status: 0,
      generatedScript: null,
      audioUrl: null,
      videoUrl: null,
      _cached: true
    })
    localStorage.setItem('cached_projects', JSON.stringify(cached))
    
    router.push(`/projects/${data.projectId}`)
  } catch(e) {
    errorMsg.value = e.message
  } finally {
    loading.value = false
  }
}
</script>
