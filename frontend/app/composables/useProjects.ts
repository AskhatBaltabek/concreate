import type { Project, ProjectStatus } from '~/types/project'
import { useApi } from '~/composables/useApi'

export const useProjects = () => {
  const { fetchWithAuth } = useApi()

  const getAll = async (): Promise<Project[]> => {
    return await fetchWithAuth('/api/projects')
  }

  const getById = async (id: string): Promise<Project> => {
    return await fetchWithAuth(`/api/projects/${id}`)
  }

  const deleteById = async (id: string) => {
    return await fetchWithAuth(`/api/projects/${id}`, { method: 'DELETE' })
  }

  const generateScript = async (id: string, provider: string): Promise<{ script: string }> => {
    return await fetchWithAuth(`/api/projects/${id}/script?provider=${provider}`, { method: 'POST' })
  }

  const generateAudio = async (id: string, options: { provider: string, scriptText: string, voiceModelId: string }) => {
    const { provider, ...body } = options
    return await fetchWithAuth(`/api/projects/${id}/audio?provider=${provider}`, {
      method: 'POST',
      body: JSON.stringify(body)
    })
  }

  const generateVideo = async (id: string) => {
    return await fetchWithAuth(`/api/projects/${id}/video`, { method: 'POST' })
  }

  const getStatusLabel = (status: number) => {
    const map: Record<number, string> = { 
      0: 'Draft', 
      1: 'Scripting', 
      2: 'Review', 
      3: 'Audio Ready', 
      4: 'Rendering', 
      5: 'Done' 
    }
    return map[status] ?? 'Unknown'
  }

  const getStatusBadgeClass = (status: number) => {
    switch (status) {
      case 0: return 'bg-slate-800 text-slate-400 border-slate-700'
      case 1: case 4: return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30'
      case 2: return 'bg-blue-500/20 text-blue-400 border-blue-500/30'
      case 3: return 'bg-purple-500/20 text-purple-400 border-purple-500/30'
      case 5: return 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30'
      default: return 'bg-slate-800 text-slate-500 border-slate-700'
    }
  }

  // Local storage cache management (for dashboard instant load)
  const CACHE_KEY = 'cached_projects'
  
  const loadCache = (): Project[] => {
    try {
      return JSON.parse(localStorage.getItem(CACHE_KEY) || '[]')
    } catch { return [] }
  }

  const saveCache = (projects: Project[]) => {
    try {
      localStorage.setItem(CACHE_KEY, JSON.stringify(projects))
    } catch {}
  }

  const updateInCache = (id: string, updates: Partial<Project>) => {
    const cached = loadCache()
    const idx = cached.findIndex(p => p.id === id)
    if (idx !== -1) {
      cached[idx] = { ...cached[idx], ...updates, _cached: false } as Project
      saveCache(cached)
    }
  }

  const create = async (project: { title: string, description: string, lengthSeconds: number, format: string }): Promise<{ projectId: string }> => {
    return await fetchWithAuth('/api/projects', {
      method: 'POST',
      body: JSON.stringify(project)
    })
  }

  return {
    getAll,
    getById,
    create,
    deleteById,
    generateScript,
    generateAudio,
    generateVideo,
    getStatusLabel,
    getStatusBadgeClass,
    loadCache,
    saveCache,
    updateInCache
  }
}
