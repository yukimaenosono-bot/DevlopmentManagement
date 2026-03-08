import axios from 'axios'
import type { AxiosRequestConfig } from 'axios'

// ベースURL は環境変数から取得する（.env.local で VITE_API_URL=http://localhost:5000 を設定する）
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000',
})

// JWT トークンをすべてのリクエストに自動付与する
axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// 401 レスポンス時はトークンを削除してログイン画面へリダイレクトする
axiosInstance.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      localStorage.removeItem('access_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

/**
 * orval のミューテーター関数。
 * 生成された API クライアントはこの関数を通じてリクエストを送信する。
 */
export const apiClient = <T>(config: AxiosRequestConfig): Promise<T> => {
  return axiosInstance(config).then((res) => res.data as T)
}
