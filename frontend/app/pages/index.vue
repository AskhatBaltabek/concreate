<template>
  <div class="flex flex-col items-center justify-center min-h-[85vh] text-center pt-10">
    <div class="inline-flex items-center space-x-2 px-3 py-1 rounded-full bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 text-sm font-medium mb-8 animate-pulse">
      <span class="w-2 h-2 rounded-full bg-indigo-500 border border-white"></span>
      <span>Generative AI Engine v2.0 is Live</span>
    </div>
    
    <h1 class="text-5xl md:text-7xl font-extrabold tracking-tight mb-6 text-white leading-tight">
      Automate Your <br />
      <span class="bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400">Faceless Content</span>
    </h1>
    
    <p class="text-lg md:text-xl text-slate-400 max-w-2xl mx-auto mb-10 leading-relaxed">
      Turn simple ideas into viral, fully edited videos with AI-generated scripts, human-like voiceovers, and captivating visuals. Everything you need, zero effort.
    </p>
    
    <div class="flex flex-col sm:flex-row items-center justify-center space-y-4 sm:space-y-0 sm:space-x-6 w-full">
      <NuxtLink :to="isLoggedIn ? '/projects/create' : '/register'" class="w-full sm:w-auto px-8 py-4 bg-white text-slate-900 font-bold rounded-full hover:scale-105 transition-transform duration-300 shadow-[0_0_30px_rgba(255,255,255,0.2)]">
        Start Creating for Free
      </NuxtLink>
      <NuxtLink :to="isLoggedIn ? '/dashboard' : '/login'" class="w-full sm:w-auto px-8 py-4 border border-slate-700 bg-slate-900/50 text-white font-semibold rounded-full hover:bg-slate-800 transition-colors duration-300 backdrop-blur-md">
        {{ isLoggedIn ? 'Go to Dashboard →' : 'View Dashboard' }}
      </NuxtLink>
    </div>
    
    <div class="mt-20 relative w-full max-w-4xl mx-auto hidden sm:block delay-500 animate-fade-in-up">
      <div class="absolute inset-0 bg-gradient-to-t from-slate-950 via-transparent to-transparent z-10 rounded-xl"></div>
      <div class="w-full h-64 border border-white/10 rounded-xl bg-slate-900/50 backdrop-blur-xl shadow-2xl relative overflow-hidden flex items-center justify-center">
        <div class="absolute top-4 left-4 w-20 h-4 bg-slate-800 rounded-full"></div>
        <div class="absolute top-4 left-28 w-12 h-4 bg-slate-800 rounded-full"></div>
        <div class="absolute bottom-4 left-4 right-4 h-32 bg-indigo-500/10 rounded-lg border border-indigo-500/20 blur-sm"></div>
        <p class="text-slate-600 font-mono text-sm">[ Premium Workspace UI ]</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fadeInUp 1s ease-out forwards;
}
</style>

<script setup>
import { ref, onMounted } from 'vue'
const isLoggedIn = ref(false)
onMounted(() => { isLoggedIn.value = !!localStorage.getItem('token') })
</script>
