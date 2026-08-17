import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

export interface ProblemDetails {
  title: string
  status: number
  errors?: Record<string, string[]>
}
