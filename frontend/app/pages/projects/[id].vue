<template>
  <div class="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8 text-white min-h-[85vh]">
    
    <!-- Loading -->
    <div v-if="loading" class="flex justify-center items-center h-64">
      <div class="w-12 h-12 border-4 border-indigo-500/30 border-t-indigo-500 rounded-full animate-spin"></div>
    </div>
    
    <!-- Load error -->
    <div v-else-if="errorMsg" class="text-center text-red-400 p-8 bg-red-500/10 border border-red-500/20 rounded-xl">
      <h3 class="text-xl font-bold mb-2">Error Loading Project</h3>
      <p>{{ errorMsg }}</p>
      <NuxtLink to="/dashboard" class="mt-4 inline-block px-4 py-2 bg-slate-800 rounded hover:bg-slate-700">Back to Dashboard</NuxtLink>
    </div>

    <div v-else class="space-y-8">

      <!-- Header -->
      <div class="border-b border-slate-800 pb-6 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <NuxtLink to="/dashboard" class="text-xs text-slate-500 hover:text-indigo-400 transition-colors mb-2 inline-flex items-center gap-1">
            <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
            Back to Dashboard
          </NuxtLink>
          <div class="flex items-center gap-3">
            <h2 class="text-3xl font-extrabold text-white mb-1">{{ project.title }}</h2>
            <button @click="showDeleteModal = true" class="p-1.5 rounded-lg text-slate-500 hover:text-red-400 hover:bg-red-500/10 transition-all" title="Delete Project">
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
            </button>
          </div>
          <p class="text-slate-400 text-sm">{{ project.description }}</p>
        </div>
        <!-- Step progress -->
        <div class="flex items-center gap-2 self-start md:self-auto">
          <div v-for="(step, i) in 3" :key="i" 
               class="flex items-center gap-1.5">
            <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold border transition-all"
                 :class="uiStep > i + 1 
                   ? 'bg-emerald-500 border-emerald-500 text-white' 
                   : uiStep === i + 1 
                     ? stepColors[i].active 
                     : 'bg-slate-800 border-slate-700 text-slate-500'">
              <span v-if="uiStep > i + 1">✓</span>
              <span v-else>{{ i + 1 }}</span>
            </div>
            <div v-if="i < 2" class="w-8 h-px" :class="uiStep > i + 1 ? 'bg-emerald-500' : 'bg-slate-700'"></div>
          </div>
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div v-if="showDeleteModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
        <div class="bg-slate-900 border border-white/10 rounded-2xl shadow-2xl p-8 max-w-sm w-full animate-fade-in-up">
          <h3 class="text-xl font-bold text-white mb-4 text-center">Delete Project?</h3>
          <p class="text-slate-400 text-sm mb-8 text-center">This will permanently delete the project and all generated AI assets. This action cannot be undone.</p>
          <div class="flex gap-4">
            <button @click="showDeleteModal = false" class="flex-1 px-4 py-3 rounded-xl bg-slate-800 text-white font-semibold hover:bg-slate-700 transition-colors">Cancel</button>
            <button @click="deleteProject" :disabled="deletingProject" class="flex-1 px-4 py-3 rounded-xl bg-red-600 text-white font-semibold hover:bg-red-500 transition-colors disabled:opacity-50">
              {{ deletingProject ? 'Deleting...' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Action error banner -->
      <div v-if="actionError" class="p-4 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 flex items-start gap-3">
        <svg class="w-5 h-5 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        <div class="flex-1">
          <p class="font-semibold text-sm">Error</p>
          <p class="text-xs mt-0.5 text-red-400/80">{{ actionError }}</p>
        </div>
        <button @click="actionError = ''" class="text-red-400/60 hover:text-red-300 text-lg leading-none">✕</button>
      </div>

      <!-- ========== STEP 1: Script ========== -->
      <div class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
           :class="uiStep === 1 ? 'border-indigo-500/40 shadow-lg shadow-indigo-500/5' : 'border-white/5 opacity-60'">
        
        <!-- Active indicator -->
        <div v-if="uiStep === 1" class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-indigo-500 rounded-br-lg"></div>
        
        <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
          <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border"
                :class="uiStep > 1 ? 'bg-emerald-500 border-emerald-400 text-white' : 'bg-indigo-500/20 text-indigo-400 border-indigo-500/30'">
            <span v-if="uiStep > 1">✓</span><span v-else>1</span>
          </span>
          AI Script Generation
          <span v-if="uiStep > 1" class="text-sm font-normal text-emerald-400">Script approved</span>
        </h3>

        <!-- No script yet -->
        <template v-if="!scriptText">
          <p class="text-slate-400 mb-6 max-w-2xl">We'll send your topic to the AI to generate a full visual and voiceover script.</p>
          
          <!-- AI Provider Selector -->
          <div class="flex items-center gap-3 mb-8 p-1.5 bg-slate-950/50 border border-white/5 rounded-xl w-fit">
            <button @click="selectedProvider = 'claude'" 
                    class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                    :class="selectedProvider === 'claude' ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' : 'text-slate-500 hover:text-slate-300'">
              <span class="w-2 h-2 rounded-full bg-orange-400" v-if="selectedProvider === 'claude'"></span>
              Claude 3.5
            </button>
            <button @click="selectedProvider = 'gemini'" 
                    class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                    :class="selectedProvider === 'gemini' ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' : 'text-slate-500 hover:text-slate-300'">
              <span class="w-2 h-2 rounded-full bg-blue-400" v-if="selectedProvider === 'gemini'"></span>
              Gemini 1.5
            </button>
          </div>

          <button @click="generateScript" :disabled="generating || uiStep !== 1"
                  class="px-7 py-3.5 bg-indigo-600 font-bold rounded-lg hover:bg-indigo-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
            <span v-if="generating" class="flex items-center gap-2">
              <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
              Generating with {{ selectedProvider === 'claude' ? 'Claude' : 'Gemini' }}...
            </span>
            <span v-else>✨ Generate Script with {{ selectedProvider === 'claude' ? 'Claude' : 'Gemini' }}</span>
          </button>
        </template>

        <!-- Script ready — review & approve -->
        <template v-else>
          <label class="block text-sm font-medium text-emerald-400 mb-2">
            {{ uiStep === 1 ? 'Review and edit your script before approving' : 'Approved script' }}
          </label>
          <textarea v-model="scriptText" rows="9" :readonly="uiStep > 1"
                    class="w-full px-4 py-3 bg-slate-950/80 border border-slate-700 rounded-lg text-slate-300 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/40 resize-y transition-all read-only:opacity-60 read-only:cursor-default"></textarea>
          
          <!-- Actions only when on step 1 -->
          <div v-if="uiStep === 1" class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 pt-4 mt-4 border-t border-slate-800">
            <button @click="generateScript" :disabled="generating" 
                    class="text-sm text-slate-400 hover:text-white py-2 px-4 border border-slate-700 rounded-lg hover:bg-slate-800 transition-colors disabled:opacity-50">
              ↺ Regenerate
            </button>
            <button @click="approveScript"
                    class="px-7 py-3 bg-emerald-600 rounded-lg hover:bg-emerald-500 font-bold text-sm transition-all hover:scale-105 shadow-lg shadow-emerald-600/20">
              Approve & Continue →
            </button>
          </div>
        </template>
      </div>

      <!-- ========== STEP 2: Audio ========== -->
      <transition name="slide-fade">
        <div v-if="uiStep >= 2" 
             class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
             :class="uiStep === 2 ? 'border-purple-500/40 shadow-lg shadow-purple-500/5' : 'border-white/5 opacity-60'">
          
          <div v-if="uiStep === 2" class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-purple-500 rounded-br-lg"></div>
          
          <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
            <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border"
                  :class="uiStep > 2 ? 'bg-emerald-500 border-emerald-400 text-white' : 'bg-purple-500/20 text-purple-400 border-purple-500/30'">
              <span v-if="uiStep > 2">✓</span><span v-else>2</span>
            </span>
            AI Voice Over
            <span v-if="uiStep > 2" class="text-sm font-normal text-emerald-400">Audio approved</span>
          </h3>

          <!-- No audio yet -->
          <template v-if="!audioUrl">
            <p class="text-slate-400 mb-6 max-w-2xl">Script approved. Now we'll synthesize a human-like voice from your script.</p>

            <!-- Audio Provider Selector -->
            <div class="mb-6">
              <label class="block text-xs font-semibold text-purple-400 uppercase tracking-wider mb-3">AI Audio Engine</label>
              <div class="flex items-center gap-3 p-1.5 bg-slate-950/50 border border-white/5 rounded-xl w-fit">
                <button @click="selectedAudioProvider = 'genaipro'" 
                        class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                        :class="selectedAudioProvider === 'genaipro' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-500 hover:text-slate-300'">
                  GenAiPro.vn (Best)
                </button>
                <button @click="selectedAudioProvider = 'gemini'" 
                        class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                        :class="selectedAudioProvider === 'gemini' ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-500 hover:text-slate-300'">
                  Gemini Flash
                </button>
              </div>
            </div>

            <!-- Voice Selector -->
            <div class="mb-8">
              <label class="block text-xs font-semibold text-purple-400 uppercase tracking-wider mb-3">Select AI Voice</label>
              <div class="flex flex-wrap gap-3">
                <button v-for="v in ['Kore', 'Aoide', 'Charon', 'Fenrir']" :key="v"
                        @click="selectedVoice = v" 
                        class="px-5 py-2.5 rounded-xl border transition-all flex items-center gap-2 text-sm font-medium"
                        :class="selectedVoice === v ? 'bg-purple-600 border-purple-500 text-white shadow-lg shadow-purple-600/20' : 'bg-slate-950/40 border-white/5 text-slate-400 hover:border-white/10 hover:text-slate-300'">
                  <div class="w-1.5 h-1.5 rounded-full" :class="selectedVoice === v ? 'bg-white' : 'bg-slate-600'"></div>
                  {{ v }}
                </button>
              </div>
            </div>

            <button @click="generateAudio" :disabled="generatingAudio || uiStep !== 2"
                    class="px-7 py-3.5 bg-purple-600 font-bold rounded-lg hover:bg-purple-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
              <span v-if="generatingAudio" class="flex items-center gap-2">
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
                Generating Voice ({{ selectedVoice }})...
              </span>
              <span v-else>🎙️ Generate Voice Over with {{ selectedVoice }}</span>
            </button>
          </template>

          <!-- Audio ready — review & proceed -->
          <template v-else>
            <p class="text-emerald-400 font-medium mb-4">Voiceover generated!</p>
            <audio controls class="w-full rounded-lg mb-6 bg-slate-800">
              <source :src="`http://localhost:5239${audioUrl}`" type="audio/mpeg">
            </audio>
            <div v-if="uiStep === 2" class="flex justify-end pt-4 border-t border-slate-800">
              <button @click="proceedToVideo"
                      class="px-7 py-3 bg-emerald-600 rounded-lg hover:bg-emerald-500 font-bold text-sm transition-all hover:scale-105 shadow-lg shadow-emerald-600/20">
                Proceed to Video →
              </button>
            </div>
          </template>
        </div>
      </transition>

      <!-- ========== STEP 3: Video ========== -->
      <transition name="slide-fade">
        <div v-if="uiStep >= 3"
             class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
             :class="uiStep === 3 ? 'border-pink-500/40 shadow-lg shadow-pink-500/5' : 'border-white/5 opacity-60'">
          
          <div class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-pink-500 rounded-br-lg"></div>
          
          <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
            <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border bg-pink-500/20 text-pink-400 border-pink-500/30">3</span>
            AI Video Rendering
          </h3>

          <!-- Waiting to queue -->
          <template v-if="!videoQueued && !videoUrl">
            <p class="text-slate-400 mb-6 max-w-2xl">Audio confirmed. The generation task will run in the background via RabbitMQ consumer.</p>
            <button @click="generateVideo" :disabled="generatingVideo"
                    class="px-7 py-3.5 bg-pink-600 font-bold rounded-lg hover:bg-pink-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
              <span v-if="generatingVideo" class="flex items-center gap-2">
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
                Sending to queue...
              </span>
              <span v-else>🎬 Start Background Render</span>
            </button>
          </template>

          <!-- Rendering in progress -->
          <div v-else-if="videoQueued && !videoUrl" class="text-center py-8">
            <div class="inline-block w-16 h-16 border-4 border-pink-500/30 border-t-pink-500 rounded-full animate-spin mb-4"></div>
            <p class="text-pink-400 font-medium animate-pulse mb-2">Rendering in background...</p>
            <p class="text-slate-500 text-sm mb-6">The background worker is processing your video. Auto-refreshing every 3s.</p>
            <button @click="fetchProject" class="px-4 py-2 border border-slate-700 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors text-sm">↻ Refresh now</button>
          </div>

          <!-- Done -->
          <template v-else-if="videoUrl">
            <p class="text-emerald-400 font-medium text-xl mb-4">🎉 Your video is ready!</p>
            <video controls class="w-full aspect-video rounded-xl shadow-2xl border border-white/10 mb-6 bg-black">
              <source :src="`http://localhost:5239${videoUrl}`" type="video/mp4">
            </video>
            <div class="flex justify-center">
              <a :href="`http://localhost:5239${videoUrl}`" download 
                 class="px-8 py-4 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full font-bold text-white shadow-lg shadow-indigo-500/25 hover:scale-105 transition-transform">⬇ Download Video</a>
            </div>
          </template>
        </div>
      </transition>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const projectId = route.params.id
const apiBase = 'http://localhost:5239'

// ── State ────────────────────────────────────────────────────────────────────
const project   = ref(null)
const loading   = ref(true)
const errorMsg  = ref('')
const actionError = ref('')
const showDeleteModal = ref(false)
const deletingProject = ref(false)

// Explicit UI step controls which sections are VISIBLE
// 1 = Script, 2 = Audio (after script approved), 3 = Video (after audio approved)
const uiStep = ref(1)

// Separate state for content — not derived from project.status directly
const scriptText    = ref('')
const audioUrl      = ref('')
const videoUrl      = ref('')
const videoQueued   = ref(false)

const generating      = ref(false)
const generatingAudio = ref(false)
const generatingVideo = ref(false)
const selectedProvider      = ref('claude')
const selectedAudioProvider = ref('genaipro')
const selectedVoice          = ref('Kore')

const stepColors = [
  { active: 'bg-indigo-500/20 text-indigo-400 border-indigo-500/30' },
  { active: 'bg-purple-500/20 text-purple-400 border-purple-500/30' },
  { active: 'bg-pink-500/20 text-pink-400 border-pink-500/30'   },
]

let pollingInterval = null

// ── Helpers ──────────────────────────────────────────────────────────────────
const getToken = () => {
  const t = localStorage.getItem('token')
  if (!t) router.push('/login')
  return t
}

// Keep dashboard cache in sync
const updateCache = (status) => {
  try {
    const cached = JSON.parse(localStorage.getItem('cached_projects') || '[]')
    const idx = cached.findIndex(p => p.id === projectId)
    if (idx !== -1) {
      cached[idx] = { ...cached[idx], status, _cached: false }
    } else if (project.value) {
      cached.unshift({ id: projectId, title: project.value.title, description: project.value.description, status, _cached: false })
    }
    localStorage.setItem('cached_projects', JSON.stringify(cached))
  } catch {}
}

// ── Fetch ─────────────────────────────────────────────────────────────────────
const fetchProject = async () => {
  const token = getToken()
  if (!token) return
  try {
    const res = await fetch(`${apiBase}/api/projects/${projectId}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    if (res.status === 401) { localStorage.removeItem('token'); router.push('/login'); return }
    if (!res.ok) throw new Error(`Server returned ${res.status}`)

    const data = await res.json()
    project.value = data

    // Sync content refs from server on initial load
    if (data.generatedScript) scriptText.value = data.generatedScript
    if (data.audioUrl)         audioUrl.value  = data.audioUrl
    if (data.videoUrl)         videoUrl.value  = data.videoUrl

    // Update dashboard cache with latest server status
    updateCache(data.status)

    // Restore uiStep from server status (so page refreshes don't go back to step 1)
    if (data.videoUrl || data.status === 5) {
      uiStep.value = 3; videoQueued.value = false
    } else if (data.status === 4) {
      uiStep.value = 3; videoQueued.value = true
    } else if (data.audioUrl || data.status >= 3) {
      uiStep.value = 3
    } else if (data.generatedScript && data.status >= 2) {
      uiStep.value = 2
    } else {
      uiStep.value = 1
    }

    // Polling while rendering
    if (data.status === 4 && !pollingInterval) {
      videoQueued.value = true
      pollingInterval = setInterval(fetchProject, 3000)
    } else if (data.status === 5 && pollingInterval) {
      clearInterval(pollingInterval); pollingInterval = null
      videoQueued.value = false
    }
  } catch (err) {
    errorMsg.value = err.message
  } finally {
    loading.value = false
  }
}

// ── Script ────────────────────────────────────────────────────────────────────
const generateScript = async () => {
  actionError.value = ''
  generating.value = true
  const token = getToken()
  if (!token) { generating.value = false; return }
  try {
    const res = await fetch(`${apiBase}/api/projects/${projectId}/script?provider=${selectedProvider.value}`, {
      method: 'POST', headers: { 'Authorization': `Bearer ${token}` }
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}: ${await res.text()}`)
    const data = await res.json()
    scriptText.value = data.script ?? ''
  } catch (err) {
    actionError.value = err.message
  } finally {
    generating.value = false
  }
}

const approveScript = () => {
  if (!scriptText.value.trim()) { actionError.value = 'Script is empty. Generate a script first.'; return }
  uiStep.value = 2
  updateCache(2)
}

// ── Audio ─────────────────────────────────────────────────────────────────────
const generateAudio = async () => {
  actionError.value = ''
  generatingAudio.value = true
  const token = getToken()
  if (!token) { generatingAudio.value = false; return }
  try {
    const res = await fetch(`${apiBase}/api/projects/${projectId}/audio?provider=${selectedAudioProvider.value}`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ 
        scriptText: scriptText.value,
        voiceModelId: selectedVoice.value
      })
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}: ${await res.text()}`)
    const data = await res.json()
    audioUrl.value = data.audioUrl ?? ''
    updateCache(3)
  } catch (err) {
    actionError.value = err.message
  } finally {
    generatingAudio.value = false
  }
}

const proceedToVideo = () => {
  if (!audioUrl.value) { actionError.value = 'Generate audio first.'; return }
  uiStep.value = 3
  updateCache(3)
}

// ── Video ─────────────────────────────────────────────────────────────────────
const generateVideo = async () => {
  actionError.value = ''
  generatingVideo.value = true
  const token = getToken()
  if (!token) { generatingVideo.value = false; return }
  try {
    const res = await fetch(`${apiBase}/api/projects/${projectId}/video`, {
      method: 'POST', headers: { 'Authorization': `Bearer ${token}` }
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}: ${await res.text()}`)
    videoQueued.value = true
    updateCache(4)
    pollingInterval = setInterval(fetchProject, 3000)
  } catch (err) {
    actionError.value = err.message
  } finally {
    generatingVideo.value = false
  }
}

const deleteProject = async () => {
  const token = getToken()
  if (!token) return
  deletingProject.value = true
  try {
    const res = await fetch(`${apiBase}/api/projects/${projectId}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    })
    if (res.ok) {
      // Remove from localStorage cache
      try {
        const cached = JSON.parse(localStorage.getItem('cached_projects') || '[]')
        const filtered = cached.filter(p => p.id === projectId)
        localStorage.setItem('cached_projects', JSON.stringify(filtered))
      } catch {}
      router.push('/dashboard')
    } else {
      throw new Error('Failed to delete project')
    }
  } catch (err) {
    actionError.value = err.message
    showDeleteModal.value = false
  } finally {
    deletingProject.value = false
  }
}

onMounted(() => fetchProject())
onUnmounted(() => { if (pollingInterval) clearInterval(pollingInterval) })
</script>

<style scoped>
@keyframes fadeInUp { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

.slide-fade-enter-active { transition: all 0.45s cubic-bezier(.4,0,.2,1); }
.slide-fade-enter-from   { opacity: 0; transform: translateY(16px); }
</style>
