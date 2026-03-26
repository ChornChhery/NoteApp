import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../api'

export const useAuthStore = defineStore('auth', () => {
  // read token from localStorage so the user stays logged in after a page refresh
  const token = ref<string | null>(localStorage.getItem('token'))

  // any component can check this without knowing about the token format
  const isLoggedIn = computed(() => !!token.value)

  async function login(email: string, password: string) {
    const res = await authApi.login(email, password)

    token.value = res.data.token
    localStorage.setItem('token', res.data.token) // persist across refreshes
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