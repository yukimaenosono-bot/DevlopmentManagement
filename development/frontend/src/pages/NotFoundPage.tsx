import { Link } from 'react-router-dom'
import { buttonVariants } from '@/components/ui/button'

export function NotFoundPage() {
  return (
    <div className="flex flex-col items-center justify-center gap-4 py-20">
      <p className="text-5xl font-bold text-muted-foreground">404</p>
      <p className="text-muted-foreground">ページが見つかりません</p>
      <Link to="/" className={buttonVariants({ variant: 'outline' })}>
        ホームへ戻る
      </Link>
    </div>
  )
}
