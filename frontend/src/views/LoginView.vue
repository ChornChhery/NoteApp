<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const isLogin = ref(true) // toggles between login and register mode
const username = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

// handles both login and register — isLogin decides which one to call to use
async function submit() {
  error.value = ''
  loading.value = true
  try {
    if (isLogin.value) {
      await auth.login(email.value, password.value)
    } else {
      await auth.register(username.value, email.value, password.value)
    }
    router.push('/') // go to notes page on success
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Something went wrong'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center px-4">
    <div class="bg-white rounded-2xl shadow-sm border border-gray-100 w-full max-w-md p-8">

      <!-- Title -->
      <h1 class="text-2xl font-bold text-gray-800 mb-1">
        {{ isLogin ? 'Welcome back' : 'Create account' }}
      </h1>
      <p class="text-sm text-gray-400 mb-6">
        {{ isLogin ? 'Login to see your notes' : 'Start writing your notes' }}
      </p>

      <!-- Error -->
      <div v-if="error" class="bg-red-50 text-red-600 text-sm rounded-lg px-4 py-3 mb-4">
        {{ error }}
      </div>

      <!-- Form -->
      <div class="flex flex-col gap-3">
        <input
          v-if="!isLogin"
          v-model="username"
          type="text"
          placeholder="Username"
          class="border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <input
          v-model="email"
          type="email"
          placeholder="Email"
          class="border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <input
          v-model="password"
          type="password"
          placeholder="Password"
          class="border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />

        <button
          @click="submit"
          :disabled="loading"
          class="bg-blue-600 text-white rounded-lg py-2.5 text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors"
        >
          {{ loading ? 'Please wait...' : isLogin ? 'Login' : 'Register' }}
        </button>
      </div>

      <!-- Switch mode -->
      <p class="text-sm text-center text-gray-400 mt-6">
        {{ isLogin ? "Don't have an account?" : 'Already have an account?' }}
        <button
          @click="isLogin = !isLogin; error = ''"
          class="text-blue-600 hover:underline ml-1"
        >
          {{ isLogin ? 'Register' : 'Login' }}
        </button>
      </p>

    </div>
  </div>
</template>