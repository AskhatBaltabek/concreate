<template>
  <div class="p-8 border rounded-2xl bg-slate-900/50 backdrop-blur-xl relative transition-all duration-300"
       :class="isActive ? 'border-indigo-500/40 shadow-lg shadow-indigo-500/5' : 'border-white/5 opacity-60'">
    
    <div v-if="isActive" class="hidden sm:block absolute top-0 left-0 w-1 h-12 bg-indigo-500 rounded-br-lg"></div>
    
    <h3 class="text-xl font-bold mb-6 flex items-center gap-3">
      <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold border"
            :class="isCompleted ? 'bg-emerald-500 border-emerald-400 text-white' : 'bg-indigo-500/20 text-indigo-400 border-indigo-500/30'">
        <span v-if="isCompleted">✓</span><span v-else>1</span>
      </span>
      AI Script Generation
      <span v-if="isCompleted" class="text-sm font-normal text-emerald-400">Script approved</span>
    </h3>

    <template v-if="!modelValue">
      <p class="text-slate-400 mb-6 max-w-2xl">We'll send your topic to the AI to generate a full visual and voiceover script.</p>
      
      <div class="flex items-center gap-3 mb-8 p-1.5 bg-slate-950/50 border border-white/5 rounded-xl w-fit">
        <button v-for="p in providers" :key="p.id"
                @click="selectedProvider = p.id" 
                class="px-4 py-2 rounded-lg text-sm font-semibold transition-all flex items-center gap-2"
                :class="selectedProvider === p.id ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20' : 'text-slate-500 hover:text-slate-300'">
          <span class="w-2 h-2 rounded-full" :class="p.color" v-if="selectedProvider === p.id"></span>
          {{ p.name }}
        </button>
      </div>

      <button @click="onGenerate" :disabled="loading || !isActive"
              class="px-7 py-3.5 bg-indigo-600 font-bold rounded-lg hover:bg-indigo-500 transition-all disabled:opacity-50 disabled:cursor-not-allowed">
        <span v-if="loading" class="flex items-center gap-2">
          <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/></svg>
          Generating...
        </span>
        <span v-else>✨ Generate Script</span>
      </button>
    </template>

    <template v-else>
      <label class="block text-sm font-medium text-emerald-400 mb-2">
        {{ isActive ? 'Review and edit your script before approving' : 'Approved script' }}
      </label>
      <textarea :value="modelValue" @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)" 
                rows="9" :readonly="!isActive"
                class="w-full px-4 py-3 bg-slate-950/80 border border-slate-700 rounded-lg text-slate-300 font-mono text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/40 resize-y transition-all read-only:opacity-60 read-only:cursor-default"></textarea>
      
      <div v-if="isActive" class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 pt-4 mt-4 border-t border-slate-800">
        <button @click="onGenerate" :disabled="loading" 
                class="text-sm text-slate-400 hover:text-white py-2 px-4 border border-slate-700 rounded-lg hover:bg-slate-800 transition-colors disabled:opacity-50">
          ↺ Regenerate
        </button>
        <button @click="$emit('approve')"
                class="px-7 py-3 bg-emerald-600 rounded-lg hover:bg-emerald-500 font-bold text-sm transition-all hover:scale-105 shadow-lg shadow-emerald-600/20">
          Approve & Continue →
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  modelValue: string
  isActive: boolean
  isCompleted: boolean
  loading: boolean
}>()

const emit = defineEmits(['update:modelValue', 'generate', 'approve'])

const selectedProvider = ref('claude')
const providers = [
  { id: 'claude', name: 'Claude 3.5', color: 'bg-orange-400' },
  { id: 'gemini', name: 'Gemini 1.5', color: 'bg-blue-400' }
]

const onGenerate = () => {
  emit('generate', selectedProvider.value)
}
</script>
