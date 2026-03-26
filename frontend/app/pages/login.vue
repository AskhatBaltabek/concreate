<template>
  <div class="flex items-center justify-center min-h-[85vh]">
    <div class="relative w-full max-w-md">
      <div class="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-purple-500 rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-1000 group-hover:duration-200"></div>
      
      <div class="relative w-full bg-slate-900/80 backdrop-blur-xl border border-white/10 rounded-2xl shadow-2xl p-8 sm:p-10">
        <div class="text-center mb-10">
          <h2 class="text-3xl font-bold text-white tracking-tight">Welcome back</h2>
          <p class="text-slate-400 text-sm mt-2">Sign in to your Creator Workspace</p>
        </div>
        
        <form @submit.prevent="handleLogin" class="space-y-6">
          <div class="space-y-1">
            <label class="block text-sm font-medium text-slate-300">Email Address</label>
            <input v-model="email" type="email" placeholder="you@example.com" required 
                   class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
          </div>
          
          <div class="space-y-1">
            <div class="flex justify-between">
              <label class="block text-sm font-medium text-slate-300">Password</label>
              <a href="#" class="text-xs text-indigo-400 hover:text-indigo-300">Forgot password?</a>
            </div>
            <input v-model="password" type="password" placeholder="••••••••" required 
                   class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
          </div>
          
          <button type="submit" :disabled="loading" 
                  class="w-full relative overflow-hidden group py-3 px-4 border border-transparent rounded-lg font-bold text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-slate-900 focus:ring-indigo-500 transition-all disabled:opacity-70 disabled:cursor-not-allowed">
            <span class="absolute right-0 w-8 h-32 -mt-12 transition-all duration-1000 transform translate-x-12 bg-white opacity-10 rotate-12 group-hover:-translate-x-96 ease"></span>
            {{ loading ? 'Authenticating...' : 'Sign In' }}
          </button>
          
          <p v-if="errorMsg" class="text-red-400 text-center text-sm font-medium">{{ errorMsg }}</p>
        </form>
        
        <div class="mt-8 pt-6 border-t border-slate-800/50 text-center">
          <p class="text-sm text-slate-400">
            Don't have an account? 
            <NuxtLink to="/register" class="font-medium text-indigo-400 hover:text-indigo-300 transition-colors">Create one now</NuxtLink>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')
const router = useRouter()

const handleLogin = async () => {
  loading.value = true
  errorMsg.value = ''
  try {
    const response = await fetch('http://localhost:5239/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, password: password.value })
    })
    
    if (!response.ok) throw new Error('Invalid email or password')
    
    const data = await response.json()
    localStorage.setItem('token', data.token)
    router.push('/dashboard')
  } catch(e) {
    errorMsg.value = e.message || 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>
