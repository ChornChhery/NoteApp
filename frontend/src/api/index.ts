import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000/api'
})

// Auto-attach JWT token to every request
api.interceptors.request.use(cfg => {
  const token = localStorage.getItem('token')

  if (token) {
    cfg.headers.Authorization = `Bearer ${token}`
  }

  return cfg
})

export interface Note {
  id: number
  title: string
  content?: string
  createdAt: string
  updatedAt: string
}

export const notesApi = {
  getAll: (search?: string, sort = 'newest') =>
    api.get<Note[]>('/notes', {
      params: { search, sort }
    }),

  create: (d: { title: string; content?: string }) =>
    api.post<Note>('/notes', d),

  update: (id: number, d: { title: string; content?: string }) =>
    api.put(`/notes/${id}`, d),

  delete: (id: number) =>
    api.delete(`/notes/${id}`)
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<{ token: string }>('/auth/login', {
      email,
      password
    }),

  register: (username: string, email: string, password: string) =>
    api.post<{ token: string }>('/auth/register', {
      username,
      email,
      password
    })
}