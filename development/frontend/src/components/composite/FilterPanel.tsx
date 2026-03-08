import * as React from "react"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

interface FilterPanelProps {
  onSearch: () => void
  onReset: () => void
  isPending?: boolean
  children: React.ReactNode
}

function FilterPanel({
  onSearch,
  onReset,
  isPending = false,
  children,
}: FilterPanelProps) {
  return (
    <Card>
      <CardContent className="flex flex-col gap-4">
        <div className="flex flex-wrap gap-4">{children}</div>
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={onReset} disabled={isPending}>
            リセット
          </Button>
          <Button onClick={onSearch} disabled={isPending}>
            検索
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

export { FilterPanel }
export type { FilterPanelProps }
