import { useAuthStore } from '@/stores/authStore'

/** 認証状態・ユーザー情報へのアクセスを提供するフック */
export function useAuth() {
  const { user, isAuthenticated, logout } = useAuthStore()
  return { user, isAuthenticated, logout }
}
