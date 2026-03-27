<template>
  <div class="py-12 text-white min-h-[85vh]">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between mb-10 gap-4">
      <div>
        <h1 class="text-4xl font-extrabold tracking-tight mb-1">My Projects</h1>
        <p class="text-slate-400 text-sm">Manage and track all your AI-generated video projects.</p>
      </div>
      <NuxtLink to="/projects/create"
        class="flex items-center gap-2 px-6 py-3 rounded-xl bg-indigo-600 hover:bg-indigo-500 font-semibold text-white shadow-lg shadow-indigo-600/30 transition-all duration-200 hover:scale-105 whitespace-nowrap self-start sm:self-auto">
        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4" /></svg>
        New Project
      </NuxtLink>
    </div>

    <!-- Loading skeleton -->
    <div v-if="loading && displayedProjects.length === 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 3" :key="i" class="p-6 rounded-2xl border border-white/5 bg-slate-900/50 animate-pulse h-48"></div>
    </div>

    <!-- Empty state -->
    <div v-else-if="displayedProjects.length === 0" class="flex flex-col items-center justify-center py-28 text-center">
      <div class="w-20 h-20 rounded-2xl bg-indigo-500/10 flex items-center justify-center mb-6 border border-indigo-500/20">
        <svg class="w-10 h-10 text-indigo-400 opacity-60" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M15 10l4.553-2.069A1 1 0 0121 8.82V17a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h11l-1 5z" /></svg>
      </div>
      <h3 class="text-2xl font-bold text-slate-200 mb-2">No projects yet</h3>
      <p class="text-slate-500 max-w-xs mb-6">Start your first AI-powered faceless video project in seconds.</p>
      <NuxtLink to="/projects/create" class="px-6 py-3 rounded-xl bg-indigo-600 hover:bg-indigo-500 font-semibold text-white transition-all duration-200">
        Create your first project →
      </NuxtLink>
    </div>

    <!-- Projects grid -->
    <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <!-- Loading overlay indicator -->
      <div v-if="loading" class="col-span-full flex items-center gap-2 text-slate-500 text-sm mb-2">
        <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
        Refreshing from server...
      </div>

      <NuxtLink
        v-for="project in displayedProjects"
        :key="project.id"
        :to="`/projects/${project.id}`"
        class="group flex flex-col p-6 rounded-2xl border bg-slate-900/50 hover:bg-slate-800/70 backdrop-blur-xl shadow-xl transition-all duration-300 cursor-pointer relative overflow-hidden"
        :class="project._cached ? 'border-indigo-500/20 hover:border-indigo-500/40' : 'border-white/8 hover:border-indigo-500/30'"
      >
        <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/0 to-purple-500/0 group-hover:from-indigo-500/5 group-hover:to-purple-500/5 transition-all duration-300 rounded-2xl pointer-events-none"></div>

        <div class="flex items-start justify-between mb-4">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-indigo-500/20 to-purple-500/20 flex items-center justify-center border border-indigo-500/20 flex-shrink-0">
            <svg class="w-5 h-5 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.069A1 1 0 0121 8.82V17a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h11l-1 5z" /></svg>
          </div>
          <div class="flex items-center gap-2">
            <span class="px-2.5 py-1 rounded-full text-[10px] font-bold border" :class="getStatusBadgeClass(project.status)">
              {{ getStatusLabel(project.status) }}
            </span>
            <button @click.prevent="confirmDelete(project.id)" class="p-1.5 rounded-lg text-slate-500 hover:text-red-400 hover:bg-red-500/10 transition-all z-20" title="Delete Project">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
            </button>
          </div>
        </div>

        <h3 class="text-lg font-bold text-slate-100 group-hover:text-white mb-1 line-clamp-1 transition-colors">{{ project.title }}</h3>
        <p class="text-sm text-slate-500 group-hover:text-slate-400 mb-4 flex-grow line-clamp-2 transition-colors">{{ project.description || 'No description provided.' }}</p>

        <!-- Progress bar -->
        <div class="flex items-center gap-1.5 mt-auto pt-3 border-t border-slate-800">
          <div class="flex-1 h-1 rounded-full" :class="project.status >= 1 ? 'bg-indigo-500' : 'bg-slate-700'"></div>
          <div class="flex-1 h-1 rounded-full" :class="project.status >= 2 ? 'bg-purple-500' : 'bg-slate-700'"></div>
          <div class="flex-1 h-1 rounded-full" :class="project.status >= 3 ? 'bg-pink-500' : 'bg-slate-700'"></div>
          <div class="flex-1 h-1 rounded-full" :class="project.status >= 5 ? 'bg-emerald-500' : 'bg-slate-700'"></div>
        </div>
      </NuxtLink>
    </div>

    <!-- Delete Confirmation Modal -->
    <AppConfirmModal
      v-if="projectToDelete"
      title="Delete Project?"
      description="This will permanently delete the project and all generated AI assets. This action cannot be undone."
      confirm-text="Delete"
      @confirm="deleteProject"
      @cancel="projectToDelete = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import type { Project } from '~/types/project'

const router = useRouter()
const { getAll, deleteById, loadCache, saveCache, getStatusLabel, getStatusBadgeClass } = useProjects()

const projects = ref<Project[]>([])
const cachedProjects = ref<Project[]>([])
const loading = ref(false)
const fetchError = ref(false)
const projectToDelete = ref<string | null>(null)

const displayedProjects = computed(() => {
  return projects.value.length > 0 ? projects.value : cachedProjects.value
})

const confirmDelete = (id: string) => {
  projectToDelete.value = id
}

const deleteProject = async () => {
  if (!projectToDelete.value) return
  const id = projectToDelete.value

  try {
    await deleteById(id)
    projects.value = projects.value.filter(p => p.id !== id)
    cachedProjects.value = cachedProjects.value.filter(p => p.id !== id)
    saveCache(projects.value.length > 0 ? projects.value : cachedProjects.value)
  } catch (err) {
    console.error('Failed to delete project:', err)
  } finally {
    projectToDelete.value = null
  }
}

const fetchProjects = async () => {
  loading.value = true
  fetchError.value = false
  try {
    const data = await getAll()
    if (data) {
      projects.value = data
      saveCache(data)
      cachedProjects.value = data
    }
  } catch {
    fetchError.value = true
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  cachedProjects.value = loadCache()
  fetchProjects()
})
</script>
