import { cn } from "@/lib/utils"
import { WORK_ORDER_STATUS } from "../constants"

interface WorkOrderStatusBadgeProps {
  status: number
}

const colorClassMap: Record<string, string> = {
  blue:   "border border-blue-200 text-blue-600 bg-blue-50",
  yellow: "border border-yellow-200 text-yellow-600 bg-yellow-50",
  green:  "border border-green-200 text-green-600 bg-green-50",
  gray:   "border border-border text-muted-foreground bg-transparent",
}

export function WorkOrderStatusBadge({ status }: WorkOrderStatusBadgeProps) {
  const def = WORK_ORDER_STATUS[status as keyof typeof WORK_ORDER_STATUS]
  const label = def?.label ?? String(status)
  const colorClass = colorClassMap[def?.color ?? "gray"] ?? colorClassMap.gray

  return (
    <span
      className={cn(
        "inline-flex h-5 w-fit shrink-0 items-center justify-center rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap",
        colorClass
      )}
    >
      {label}
    </span>
  )
}
