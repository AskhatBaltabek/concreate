<template>
  <div class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
       :class="isActive ? 'border-pink-500/40 shadow-lg shadow-pink-500/5' : 'border-white/5 opacity-60'">
    
    <div v-if="isActive" class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-pink-500 rounded-br-lg"></div>
    
    <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
      <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border bg-pink-500/20 text-pink-400 border-pink-500/30">3</span>
      AI Video Rendering
    </h3>

    <template v-if="!isQueued && !videoUrl">
      <p class="text-slate-400 mb-6 max-w-2xl">Audio confirmed. The generation task will run in the background via RabbitMQ consumer.</p>
      <button @click="$emit('generate')" :disabled="loading || !isActive"
              class="px-7 py-3.5 bg-pink-600 font-bold rounded-lg hover:bg-pink-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
        <span v-if="loading" class="flex items-center gap-2">
          <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
          Sending to queue...
        </span>
        <span v-else>🎬 Start Background Render</span>
      </button>
    </template>

    <div v-else-if="isQueued && !videoUrl" class="text-center py-8">
      <div class="inline-block w-16 h-16 border-4 border-pink-500/30 border-t-pink-500 rounded-full animate-spin mb-4"></div>
      <p class="text-pink-400 font-medium animate-pulse mb-2">Rendering in background...</p>
      <p class="text-slate-500 text-sm mb-6">The background worker is processing your video. Auto-refreshing...</p>
      <button @click="$emit('refresh')" class="px-4 py-2 border border-slate-700 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors text-sm">↻ Refresh now</button>
    </div>

    <template v-else-if="videoUrl">
      <p class="text-emerald-400 font-medium text-xl mb-4">🎉 Your video is ready!</p>
      <video controls class="w-full aspect-video rounded-xl shadow-2xl border border-white/10 mb-6 bg-black">
        <source :src="fullVideoUrl" type="video/mp4">
      </video>
      <div class="flex justify-center">
        <a :href="fullVideoUrl" download 
           class="px-8 py-4 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full font-bold text-white shadow-lg shadow-indigo-500/25 hover:scale-105 transition-transform">⬇ Download Video</a>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  videoUrl: string
  isQueued: boolean
  isActive: boolean
  loading: boolean
  apiBase: string
}>()

defineEmits(['generate', 'refresh'])

const fullVideoUrl = computed(() => {
  if (!props.videoUrl) return ''
  return props.videoUrl.startsWith('http') ? props.videoUrl : `${props.apiBase}${props.videoUrl}`
})
</script>
