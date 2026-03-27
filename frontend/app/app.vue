<template>
  <div class="min-h-screen bg-slate-950 text-slate-100 selection:bg-indigo-500/30 font-sans flex flex-col items-center">
    <div class="fixed inset-0 z-[-1] bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-indigo-900 via-slate-950 to-black opacity-80"></div>
    
    <header class="w-full border-b border-white/10 bg-slate-950/50 backdrop-blur-lg sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 flex justify-between items-center">
        <NuxtLink to="/" class="flex items-center space-x-2 group">
          <div class="w-8 h-8 rounded-lg bg-gradient-to-tr from-indigo-500 to-purple-500 flex items-center justify-center shadow-lg shadow-indigo-500/20 group-hover:shadow-indigo-500/40 transition-all duration-300">
            <svg class="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" /></svg>
          </div>
          <span class="text-xl font-extrabold tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-indigo-400 to-purple-400">Content AI</span>
        </NuxtLink>
        
        <div class="space-x-6 flex items-center">
          <template v-if="!isLoggedIn">
            <NuxtLink to="/login" class="text-sm font-medium text-slate-300 hover:text-white transition-colors">Sign In</NuxtLink>
            <NuxtLink to="/register" class="text-sm font-medium relative px-5 py-2 rounded-full bg-indigo-500/10 text-indigo-400 hover:bg-indigo-500 hover:text-white border border-indigo-500/30 transition-all duration-300 shadow-[0_0_15px_rgba(99,102,241,0.2)] hover:shadow-[0_0_20px_rgba(99,102,241,0.4)]">Get Started</NuxtLink>
          </template>
          <template v-else>
            <NuxtLink to="/dashboard" class="text-sm font-medium text-indigo-400 hover:text-indigo-300 transition-colors pr-2">Dashboard</NuxtLink>
            <div class="relative group">
              <button class="flex items-center space-x-2 text-sm font-medium text-slate-300 hover:text-white transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:ring-offset-slate-950 rounded-full">
                <div class="w-8 h-8 rounded-full bg-slate-800 border border-slate-700 flex items-center justify-center">
                  <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                </div>
              </button>
              <div class="absolute right-0 mt-2 w-48 rounded-xl shadow-2xl py-2 bg-slate-800/90 backdrop-blur-md ring-1 ring-white/10 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50 transform origin-top-right group-hover:scale-100 scale-95 border border-slate-700">
                <div class="px-4 py-2 border-b border-slate-700 mb-1">
                  <p class="text-xs text-slate-400">Signed in as</p>
                  <p class="text-sm font-medium text-white truncate break-all">User</p>
                </div>
                <a href="#" @click.prevent="logout" class="block px-4 py-2 text-sm text-red-400 hover:bg-slate-700 hover:text-red-300 transition-colors mx-1 rounded-md">Log out</a>
              </div>
            </div>
          </template>
        </div>
      </div>
    </header>
    
    <main class="flex-grow w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative">
      <NuxtPage />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const isLoggedIn = ref(false)

const checkAuth = () => {
  if (import.meta.client) {
    isLoggedIn.value = !!localStorage.getItem('token')
  }
}

onMounted(() => {
  checkAuth()
  
  // Re-evaluate on route changes
  router.afterEach(() => {
    checkAuth()
  })
})

const logout = () => {
  localStorage.removeItem('token')
  isLoggedIn.value = false
  router.push('/login')
}
</script>

<style>
.page-enter-active,
.page-leave-active {
  transition: all 0.4s ease-out;
}
.page-enter-from,
.page-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>
