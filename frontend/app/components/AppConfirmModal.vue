<template>
  <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
    <div class="bg-slate-900 border border-white/10 rounded-2xl shadow-2xl p-8 max-w-sm w-full animate-fade-in-up">
      <h3 class="text-xl font-bold text-white mb-4 text-center">{{ title }}</h3>
      <p class="text-slate-400 text-sm mb-8 text-center">{{ description }}</p>
      <div class="flex gap-4">
        <button 
          @click="$emit('cancel')" 
          class="flex-1 px-4 py-3 rounded-xl bg-slate-800 text-white font-semibold hover:bg-slate-700 transition-colors"
        >
          Cancel
        </button>
        <button 
          @click="$emit('confirm')" 
          :disabled="loading"
          class="flex-1 px-4 py-3 rounded-xl bg-red-600 text-white font-semibold hover:bg-red-500 transition-colors disabled:opacity-50"
        >
          <span v-if="loading">Processing...</span>
          <span v-else>{{ confirmText }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  description: string
  confirmText?: string
  loading?: boolean
}>()

defineEmits(['confirm', 'cancel'])
</script>

<style scoped>
@keyframes fadeInUp { 
  from { opacity: 0; transform: translateY(12px); } 
  to { opacity: 1; transform: translateY(0); } 
}
.animate-fade-in-up {
  animation: fadeInUp 0.3s ease-out;
}
</style>
