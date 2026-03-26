import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))

  const isLoggedIn = computed(() => !!token.value)

  async function login(email: string, password: string) {
    const res = await authApi.login(email, password)

    token.value = res.data.token
    localStorage.setItem('token', res.data.token)
  }

  async function register(username: string, email: string, password: string) {
    const res = await authApi.register(username, email, password)

    token.value = res.data.token
    localStorage.setItem('token', res.data.token)
  }

  function logout() {
    token.value = null
    localStorage.removeItem('token')
  }

  return {
    token,
    isLoggedIn,
    login,
    register,
    logout
  }
})