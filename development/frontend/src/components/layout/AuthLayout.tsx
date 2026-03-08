import { Outlet } from 'react-router-dom'
import { Factory } from 'lucide-react'

export function AuthLayout() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/40">
      <div className="w-full max-w-sm space-y-6 rounded-xl border bg-card p-8 shadow-sm">
        <div className="flex flex-col items-center gap-2">
          <Factory className="size-8 text-primary" />
          <h1 className="text-xl font-semibold tracking-tight">Synapse</h1>
          <p className="text-sm text-muted-foreground">製造管理システム</p>
        </div>
        <Outlet />
      </div>
    </div>
  )
}
