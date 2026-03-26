<template>
  <div class="flex items-center justify-center min-h-[85vh]">
    <div class="relative w-full max-w-lg">
      <div class="absolute -inset-1 bg-gradient-to-r from-purple-500 to-indigo-500 rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-1000 group-hover:duration-200"></div>
      
      <div class="relative w-full bg-slate-900/80 backdrop-blur-xl border border-white/10 rounded-2xl shadow-2xl p-8 sm:p-10">
        <div class="text-center mb-8">
          <h2 class="text-3xl font-bold text-white tracking-tight">Join Content AI</h2>
          <p class="text-slate-400 text-sm mt-2">Get started with unlimited faceless video generation</p>
        </div>
        
        <form @submit.prevent="handleRegister" class="space-y-5">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
            <div class="space-y-1">
              <label class="block text-sm font-medium text-slate-300">First Name</label>
              <input v-model="firstName" type="text" required placeholder="John"
                     class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
            </div>
            <div class="space-y-1">
              <label class="block text-sm font-medium text-slate-300">Last Name</label>
              <input v-model="lastName" type="text" required placeholder="Doe"
                     class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
            </div>
          </div>
          
          <div class="space-y-1">
            <label class="block text-sm font-medium text-slate-300">Email Address</label>
            <input v-model="email" type="email" required placeholder="you@example.com"
                   class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
          </div>
          
          <div class="space-y-1">
            <label class="block text-sm font-medium text-slate-300">Password</label>
            <input v-model="password" type="password" required placeholder="••••••••"
                   class="w-full px-4 py-3 bg-slate-950/50 border border-slate-800 rounded-lg text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all" />
          </div>
          
          <button type="submit" :disabled="loading" 
                  class="w-full relative overflow-hidden group py-3 px-4 mt-2 border border-transparent rounded-lg font-bold text-white bg-indigo-600 hover:bg-indigo-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-slate-900 focus:ring-indigo-500 transition-all disabled:opacity-70 disabled:cursor-not-allowed">
            <span class="absolute right-0 w-8 h-32 -mt-12 transition-all duration-1000 transform translate-x-12 bg-white opacity-10 rotate-12 group-hover:-translate-x-96 ease"></span>
            {{ loading ? 'Creating Account...' : 'Create Account' }}
          </button>
          
          <p v-if="errorMsg" class="text-red-400 text-center text-sm font-medium">{{ errorMsg }}</p>
        </form>
        
        <div class="mt-8 pt-6 border-t border-slate-800/50 text-center">
          <p class="text-sm text-slate-400">
            Already have an account? 
            <NuxtLink to="/login" class="font-medium text-indigo-400 hover:text-indigo-300 transition-colors">Sign in</NuxtLink>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')
const router = useRouter()

const handleRegister = async () => {
  loading.value = true
  errorMsg.value = ''
  try {
    const response = await fetch('http://localhost:5239/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, password: password.value, firstName: firstName.value, lastName: lastName.value })
    })
    
    if (!response.ok) {
      const errs = await response.json()
      throw new Error(errs[0]?.description || 'Registration failed')
    }
    
    // Auto-login after registration
    const loginRes = await fetch('http://localhost:5239/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, password: password.value })
    })
    if (loginRes.ok) {
      const loginData = await loginRes.json()
      localStorage.setItem('token', loginData.token)
    }
    router.push('/dashboard')
  } catch(e) {
    errorMsg.value = e.message || 'Registration failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>
