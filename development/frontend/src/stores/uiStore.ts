import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface UiState {
  isSidebarOpen: boolean
  toggleSidebar: () => void
  setSidebarOpen: (open: boolean) => void
}

export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      isSidebarOpen: true,
      toggleSidebar: () => set((s) => ({ isSidebarOpen: !s.isSidebarOpen })),
      setSidebarOpen: (open) => set({ isSidebarOpen: open }),
    }),
    {
      name: 'synapse-ui',
      partialize: (state) => ({ isSidebarOpen: state.isSidebarOpen }),
    },
  ),
)
