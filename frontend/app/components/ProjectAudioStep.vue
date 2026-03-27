<template>
  <div class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
       :class="isActive ? 'border-purple-500/40 shadow-lg shadow-purple-500/5' : 'border-white/5 opacity-60'">
    
    <div v-if="isActive" class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-purple-500 rounded-br-lg"></div>
    
    <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
      <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border"
            :class="isCompleted ? 'bg-emerald-500 border-emerald-400 text-white' : 'bg-purple-500/20 text-purple-400 border-purple-500/30'">
        <span v-if="isCompleted">✓</span><span v-else>2</span>
      </span>
      AI Voice Over
      <span v-if="isCompleted" class="text-sm font-normal text-emerald-400">Audio approved</span>
    </h3>

    <template v-if="!audioUrl && !isQueued">
      <p class="text-slate-400 mb-6 max-w-2xl">Script approved. Now we'll synthesize a human-like voice from your script.</p>

      <div class="mb-6">
        <label class="block text-xs font-semibold text-purple-400 uppercase tracking-wider mb-3">AI Audio Engine</label>
        <div class="flex items-center gap-3 p-1.5 bg-slate-950/50 border border-white/5 rounded-xl w-fit">
          <button v-for="p in providers" :key="p.id"
                  @click="selectProvider(p.id)" 
                  class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                  :class="selectedAudioProvider === p.id ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/20' : 'text-slate-500 hover:text-slate-300'">
            {{ p.name }}
          </button>
        </div>
      </div>

      <div class="mb-8">
        <label class="block text-xs font-semibold text-purple-400 uppercase tracking-wider mb-3">Select AI Voice</label>
        
        <div v-if="selectedAudioProvider !== 'gemini'" class="flex flex-wrap gap-3">
          <button v-for="v in currentVoices" :key="v.id"
                  @click="selectedVoice = v.id" 
                  class="px-5 py-2.5 rounded-xl border transition-all flex items-center gap-2 text-sm font-medium"
                  :class="selectedVoice === v.id ? 'bg-purple-600 border-purple-500 text-white shadow-lg shadow-purple-600/20' : 'bg-slate-950/40 border-white/5 text-slate-400 hover:border-white/10 hover:text-slate-300'">
            <div class="w-1.5 h-1.5 rounded-full" :class="selectedVoice === v.id ? 'bg-white' : 'bg-slate-600'"></div>
            {{ v.name }}
          </button>
        </div>
        <div v-else>
          <p class="text-slate-500 text-sm italic">Gemini uses a default system voice.</p>
        </div>
      </div>

      <button @click="onGenerate" :disabled="loading || !isActive"
              class="px-7 py-3.5 bg-purple-600 font-bold rounded-lg hover:bg-purple-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
        <span v-if="loading" class="flex items-center gap-2">
          <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
          Sending to queue...
        </span>
        <span v-else>🎙️ Generate Voice Over</span>
      </button>
    </template>

    <div v-else-if="isQueued && !audioUrl" class="text-center py-8">
      <div class="inline-block w-16 h-16 border-4 border-purple-500/30 border-t-purple-500 rounded-full animate-spin mb-4"></div>
      <p class="text-purple-400 font-medium animate-pulse mb-2">Generating audio in background...</p>
      <p class="text-slate-500 text-sm mb-6">The background worker is processing your audio. Auto-refreshing...</p>
      <button @click="$emit('refresh')" class="px-4 py-2 border border-slate-700 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors text-sm">↻ Refresh now</button>
    </div>

    <template v-else>
      <p class="text-emerald-400 font-medium mb-4">Voiceover generated!</p>
      <audio controls class="w-full rounded-lg mb-6 bg-slate-800">
        <source :src="fullAudioUrl" type="audio/mpeg">
      </audio>
      <div v-if="isActive" class="flex justify-end pt-4 border-t border-slate-800">
        <button @click="$emit('approve')"
                class="px-7 py-3 bg-emerald-600 rounded-lg hover:bg-emerald-500 font-bold text-sm transition-all hover:scale-105 shadow-lg shadow-emerald-600/20">
          Proceed to Video →
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  audioUrl: string
  isQueued: boolean
  isActive: boolean
  isCompleted: boolean
  loading: boolean
  apiBase: string
}>()

const emit = defineEmits(['generate', 'approve', 'refresh'])

const selectedAudioProvider = ref('genaipro')
const selectedVoice = ref('Kore')

const providers = [
  { id: 'genaipro', name: 'GenAiPro.vn (Best)', defaultVoice: 'Kore' },
  { id: 'elevenlabs', name: 'ElevenLabs', defaultVoice: '21m00Tcm4TlvDq8ikWAM' },
  { id: 'gemini', name: 'Gemini Flash', defaultVoice: 'default' }
]

const voicesMap: Record<string, {id: string, name: string}[]> = {
  genaipro: [{ id: 'airYK6ydeWdrJg6gyZA3', name: 'Titan' }, {id: 'Kore', name: 'Kore'}],
  elevenlabs: [
    { name: 'Rachel', id: 'JBFqnCBsd6RMkjVDRZzb' }, 
    { name: 'Clyde', id: '2EiwWnXFnvU5JabPnv8n' }, 
    { name: 'Alice', id: 'Xb7hH9SOfS4B99TOvtA1' }, 
    { name: 'George', id: 'JBFqnCBv7SttcaGoHh9n' }
  ]
}

const currentVoices = computed(() => voicesMap[selectedAudioProvider.value] || [])

const selectProvider = (id: string) => {
  selectedAudioProvider.value = id
  const provider = providers.find(p => p.id === id)
  if (provider) selectedVoice.value = provider.defaultVoice
}

const fullAudioUrl = computed(() => {
  if (!props.audioUrl) return ''
  return props.audioUrl.startsWith('http') ? props.audioUrl : `${props.apiBase}${props.audioUrl}`
})

const onGenerate = () => {
  emit('generate', {
    provider: selectedAudioProvider.value,
    voiceModelId: selectedVoice.value
  })
}
</script>
