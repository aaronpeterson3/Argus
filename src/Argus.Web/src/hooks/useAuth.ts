import create from 'zustand'
import { AuthState, LoginRequest, SystemInitRequest } from '@/types/auth'
import axios from 'axios'

const API_URL = '/api'

interface AuthStore extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
  initialize: (request: SystemInitRequest) => Promise<void>;
  checkSystem: () => Promise<boolean>;
}

export const useAuth = create<AuthStore>((set, get) => ({
  user: null,
  token: localStorage.getItem('token'),
  isAuthenticated: !!localStorage.getItem('token'),

  login: async (credentials) => {
    try {
      const response = await axios.post(`${API_URL}/auth/login`, credentials)
      const { token, user } = response.data

      localStorage.setItem('token', token)
      set({ token, user, isAuthenticated: true })

      // Set default Authorization header for future requests
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
    } catch (error) {
      throw error
    }
  },

  logout: () => {
    localStorage.removeItem('token')
    delete axios.defaults.headers.common['Authorization']
    set({ token: null, user: null, isAuthenticated: false })
  },

  initialize: async (request) => {
    try {
      await axios.post(`${API_URL}/tenant/initialize`, request)
      await get().login({
        email: request.adminEmail,
        password: request.adminPassword
      })
    } catch (error) {
      throw error
    }
  },

  checkSystem: async () => {
    try {
      const response = await axios.get(`${API_URL}/system/status`)
      return response.data.initialized
    } catch (error) {
      return false
    }
  }
}))

// Initialize axios interceptor for token
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Add response interceptor for handling unauthorized requests
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuth.getState().logout()
    }
    return Promise.reject(error)
  }
)
