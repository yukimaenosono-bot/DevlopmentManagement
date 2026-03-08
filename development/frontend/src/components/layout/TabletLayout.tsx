import { Outlet } from 'react-router-dom'
import { Factory } from 'lucide-react'

/** タブレット・現場端末用レイアウト（サイドバーなし、タッチ最適化） */
export function TabletLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="flex h-14 items-center gap-3 border-b px-4 shadow-sm">
        <Factory className="h-6 w-6 text-primary" aria-hidden />
        <span className="text-lg font-semibold tracking-tight">Synapse</span>
        <span className="ml-auto text-sm text-muted-foreground">現場端末</span>
      </header>
      <main className="flex-1 overflow-auto p-4">
        <Outlet />
      </main>
    </div>
  )
}
