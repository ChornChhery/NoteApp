import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(), // clean URLs, no # in the path
  routes: [
    {
      path: '/',
      component: () => import('../views/NotesView.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/login',
      component: () => import('../views/LoginView.vue')
    }
  ]
})

// Navigation guard: redirect to /login if not authenticated
router.beforeEach((to) => {
  const auth = useAuthStore()

  // not logged in and trying to access a protected page — send to login
  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return '/login'
  }

  // already logged in and going to /login — send to notes instead
  if (to.path === '/login' && auth.isLoggedIn) {
    return '/'
  }
})

export default router