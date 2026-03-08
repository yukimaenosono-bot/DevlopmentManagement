import { NavLink } from 'react-router-dom'
import {
  ClipboardList,
  CalendarDays,
  Package,
  FlaskConical,
  GitFork,
  Truck,
  Database,
  Settings,
  ChevronLeft,
  Factory,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { useUiStore } from '@/stores/uiStore'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'

const navItems = [
  { label: '製造指示', to: '/manufacturing-orders', icon: ClipboardList },
  { label: '生産計画', to: '/production-plans', icon: CalendarDays },
  { label: '在庫管理', to: '/inventory', icon: Package },
  { label: '品質管理', to: '/quality-control', icon: FlaskConical },
  { label: '工程進捗', to: '/processes', icon: GitFork },
  { label: '出荷管理', to: '/shipments', icon: Truck },
  { label: 'マスタ管理', to: '/master', icon: Database },
  { label: 'システム管理', to: '/system', icon: Settings },
]

export function Sidebar() {
  const { isSidebarOpen, toggleSidebar } = useUiStore()

  return (
    <aside
      className={cn(
        'relative flex flex-col border-r bg-sidebar text-sidebar-foreground transition-[width] duration-200',
        isSidebarOpen ? 'w-56' : 'w-14',
      )}
    >
      {/* ロゴ */}
      <div className="flex h-14 items-center gap-2 px-3">
        <Factory className="size-6 shrink-0 text-sidebar-primary" />
        {isSidebarOpen && (
          <span className="truncate text-base font-semibold tracking-tight">Synapse</span>
        )}
      </div>

      <Separator />

      {/* ナビゲーション */}
      <nav className="flex-1 overflow-y-auto py-2">
        <ul className="space-y-0.5 px-2">
          {navItems.map(({ label, to, icon: Icon }) => (
            <li key={to}>
              <NavLink
                to={to}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 rounded-md px-2 py-2 text-sm transition-colors',
                    'hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                    isActive
                      ? 'bg-sidebar-accent font-medium text-sidebar-accent-foreground'
                      : 'text-sidebar-foreground',
                  )
                }
              >
                <Icon className="size-4 shrink-0" />
                {isSidebarOpen && <span className="truncate">{label}</span>}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      {/* 折りたたみボタン */}
      <div className="border-t p-2">
        <Button
          variant="ghost"
          size="icon"
          onClick={toggleSidebar}
          className="w-full"
          aria-label={isSidebarOpen ? 'サイドバーを閉じる' : 'サイドバーを開く'}
        >
          <ChevronLeft
            className={cn('size-4 transition-transform duration-200', !isSidebarOpen && 'rotate-180')}
          />
        </Button>
      </div>
    </aside>
  )
}
