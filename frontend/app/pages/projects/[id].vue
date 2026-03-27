<template>
  <div class="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8 text-white min-h-[85vh]">
    
    <div v-if="loading" class="flex justify-center items-center h-64">
      <div class="w-12 h-12 border-4 border-indigo-500/30 border-t-indigo-500 rounded-full animate-spin"></div>
    </div>
    
    <div v-else-if="errorMsg" class="text-center text-red-400 p-8 bg-red-500/10 border border-red-500/20 rounded-xl">
      <h3 class="text-xl font-bold mb-2">Error Loading Project</h3>
      <p>{{ errorMsg }}</p>
      <NuxtLink to="/dashboard" class="mt-4 inline-block px-4 py-2 bg-slate-800 rounded hover:bg-slate-700">Back to Dashboard</NuxtLink>
    </div>

    <div v-else-if="project" class="space-y-8">
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
          <div v-for="(step, i) in 3" :key="i" class="flex items-center gap-1.5">
            <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold border transition-all"
                 :class="uiStep > i + 1 ? 'bg-emerald-500 border-emerald-500 text-white' : uiStep === i + 1 ? stepColors[i].active : 'bg-slate-800 border-slate-700 text-slate-500'">
              <span v-if="uiStep > i + 1">✓</span>
              <span v-else>{{ i + 1 }}</span>
            </div>
            <div v-if="i < 2" class="w-8 h-px" :class="uiStep > i + 1 ? 'bg-emerald-500' : 'bg-slate-700'"></div>
          </div>
        </div>
      </div>

      <AppConfirmModal
        v-if="showDeleteModal"
        title="Delete Project?"
        description="This will permanently delete the project and all generated AI assets. This action cannot be undone."
        confirm-text="Delete"
        :loading="deletingProject"
        @confirm="deleteProject"
        @cancel="showDeleteModal = false"
      />

      <!-- Action error banner -->
      <div v-if="actionError" class="p-4 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 flex items-start gap-3">
        <svg class="w-5 h-5 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        <div class="flex-1">
          <p class="font-semibold text-sm">Error</p>
          <p class="text-xs mt-0.5 text-red-400/80">{{ actionError }}</p>
        </div>
        <button @click="actionError = ''" class="text-red-400/60 hover:text-red-300 text-lg leading-none">✕</button>
      </div>

      <!-- Step 1: Script -->
      <ProjectScriptStep
        v-model="scriptText"
        :is-active="uiStep === 1"
        :is-completed="uiStep > 1"
        :loading="generatingScript"
        @generate="onGenerateScript"
        @approve="onApproveScript"
      />

      <!-- Step 2: Audio -->
      <transition name="slide-fade">
        <ProjectAudioStep
          v-if="uiStep >= 2"
          :audio-url="audioUrl"
          :is-queued="audioQueued"
          :is-active="uiStep === 2"
          :is-completed="uiStep > 2"
          :loading="generatingAudio"
          :api-base="apiBase"
          @generate="onGenerateAudio"
          @approve="onApproveAudio"
          @refresh="fetchProject"
        />
      </transition>

      <!-- Step 3: Video -->
      <transition name="slide-fade">
        <ProjectVideoStep
          v-if="uiStep >= 3"
          :video-url="videoUrl"
          :is-queued="videoQueued"
          :is-active="uiStep === 3"
          :loading="generatingVideo"
          :api-base="apiBase"
          @generate="onGenerateVideo"
          @refresh="fetchProject"
        />
      </transition>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { Project } from '~/types/project'
import { ProjectStatus } from '~/types/project'

const route = useRoute()
const router = useRouter()
const { apiBase } = useApi()
const { getById, deleteById, generateScript, generateAudio, generateVideo, updateInCache } = useProjects()

const projectId = route.params.id as string

// ── State ────────────────────────────────────────────────────────────────────
const project = ref<Project | null>(null)
const loading = ref(true)
const errorMsg = ref('')
const actionError = ref('')
const showDeleteModal = ref(false)
const deletingProject = ref(false)

const uiStep = ref(1)
const scriptText = ref('')
const audioUrl = ref('')
const videoUrl = ref('')
const audioQueued = ref(false)
const videoQueued = ref(false)

const generatingScript = ref(false)
const generatingAudio = ref(false)
const generatingVideo = ref(false)

const stepColors = [
  { active: 'bg-indigo-500/20 text-indigo-400 border-indigo-500/30' },
  { active: 'bg-purple-500/20 text-purple-400 border-purple-500/30' },
  { active: 'bg-pink-500/20 text-pink-400 border-pink-500/30' },
]

let pollingInterval: NodeJS.Timeout | null = null

// ── Logic ─────────────────────────────────────────────────────────────────────
const fetchProject = async () => {
  try {
    const data = await getById(projectId)
    if (!data) return
    
    project.value = data
    if (data.generatedScript) scriptText.value = data.generatedScript
    if (data.audioUrl) audioUrl.value = data.audioUrl
    if (data.videoUrl) videoUrl.value = data.videoUrl

    updateInCache(projectId, { status: data.status })

    // UI Step Logic
    if (data.videoUrl || data.status === ProjectStatus.Completed) {
      uiStep.value = 3; videoQueued.value = false; audioQueued.value = false
    } else if (data.status === ProjectStatus.GeneratingVideo) {
      uiStep.value = 3; videoQueued.value = true; audioQueued.value = false
    } else if (data.status === ProjectStatus.GeneratingAudio) {
      uiStep.value = 2; audioQueued.value = true; videoQueued.value = false
    } else if (data.audioUrl) {
      uiStep.value = 3; audioQueued.value = false
    } else if ((data.generatedScript || scriptText.value) && data.status >= ProjectStatus.UserReview) {
      uiStep.value = 2
    } else {
      uiStep.value = 1
    }

    // Polling setup
    const needsPolling = data.status === ProjectStatus.GeneratingAudio || data.status === ProjectStatus.GeneratingVideo
    if (needsPolling && !pollingInterval) {
      pollingInterval = setInterval(fetchProject, 3000)
    } else if (!needsPolling && pollingInterval) {
      clearInterval(pollingInterval)
      pollingInterval = null
    }

    audioQueued.value = data.status === ProjectStatus.GeneratingAudio
    videoQueued.value = data.status === ProjectStatus.GeneratingVideo

  } catch (err: any) {
    errorMsg.value = err.message
  } finally {
    loading.value = false
  }
}

const onGenerateScript = async (provider: string) => {
  actionError.value = ''
  generatingScript.value = true
  try {
    const data = await generateScript(projectId, provider)
    scriptText.value = data.script
  } catch (err: any) {
    actionError.value = err.message
  } finally {
    generatingScript.value = false
  }
}

const onApproveScript = () => {
  if (!scriptText.value.trim()) {
    actionError.value = 'Script is empty. Generate a script first.'
    return
  }
  uiStep.value = 2
  updateInCache(projectId, { status: ProjectStatus.UserReview })
}

const onGenerateAudio = async ({ provider, voiceModelId }: { provider: string, voiceModelId: string }) => {
  actionError.value = ''
  generatingAudio.value = true
  try {
    await generateAudio(projectId, { provider, scriptText: scriptText.value, voiceModelId })
    audioQueued.value = true
    updateInCache(projectId, { status: ProjectStatus.GeneratingAudio })
    if (!pollingInterval) pollingInterval = setInterval(fetchProject, 3000)
  } catch (err: any) {
    actionError.value = err.message
  } finally {
    generatingAudio.value = false
  }
}

const onApproveAudio = () => {
  if (!audioUrl.value) {
    actionError.value = 'Generate audio first.'
    return
  }
  uiStep.value = 3
}

const onGenerateVideo = async () => {
  actionError.value = ''
  generatingVideo.value = true
  try {
    await generateVideo(projectId)
    videoQueued.value = true
    updateInCache(projectId, { status: ProjectStatus.GeneratingVideo })
    if (!pollingInterval) pollingInterval = setInterval(fetchProject, 3000)
  } catch (err: any) {
    actionError.value = err.message
  } finally {
    generatingVideo.value = false
  }
}

const deleteProject = async () => {
  deletingProject.value = true
  try {
    await deleteById(projectId)
    // Cache cleanup is handled inside the dashboard when it mounts, 
    // but we can also do it here for better experience if we wanted.
    router.push('/dashboard')
  } catch (err: any) {
    actionError.value = err.message
    showDeleteModal.value = false
  } finally {
    deletingProject.value = false
  }
}

onMounted(fetchProject)
onUnmounted(() => { if (pollingInterval) clearInterval(pollingInterval) })
</script>

<style scoped>
.slide-fade-enter-active { transition: all 0.45s cubic-bezier(.4,0,.2,1); }
.slide-fade-enter-from { opacity: 0; transform: translateY(16px); }
</style>
