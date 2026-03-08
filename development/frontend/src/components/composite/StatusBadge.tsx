import { cn } from "@/lib/utils"

type WorkOrderStatus = "Draft" | "Released" | "InProgress" | "Completed" | "Cancelled"
type QCResult = "Pass" | "Fail" | "Pending"
type ShipmentStatus = "Pending" | "Picking" | "Shipped" | "Cancelled"

interface StatusBadgeProps {
  status: WorkOrderStatus | QCResult | ShipmentStatus | string
}

const statusStyleMap: Record<string, string> = {
  Draft: "border border-border text-muted-foreground bg-transparent",
  Pending: "border border-border text-muted-foreground bg-transparent",
  Released: "border border-blue-200 text-blue-600 bg-blue-50",
  InProgress: "border border-yellow-200 text-yellow-600 bg-yellow-50",
  Picking: "border border-yellow-200 text-yellow-600 bg-yellow-50",
  Completed: "border border-green-200 text-green-600 bg-green-50",
  Pass: "border border-green-200 text-green-600 bg-green-50",
  Shipped: "border border-green-200 text-green-600 bg-green-50",
  Cancelled: "border border-red-200 text-red-600 bg-red-50",
  Fail: "border border-red-200 text-red-600 bg-red-50",
}

const statusLabelMap: Record<string, string> = {
  Draft: "下書き",
  Released: "発行済",
  InProgress: "進行中",
  Completed: "完了",
  Cancelled: "キャンセル",
  Pass: "合格",
  Fail: "不合格",
  Pending: "未処理",
  Picking: "ピッキング中",
  Shipped: "出荷済",
}

function StatusBadge({ status }: StatusBadgeProps) {
  const style = statusStyleMap[status] ?? "border border-border"
  const label = statusLabelMap[status] ?? status

  return (
    <span
      className={cn(
        "inline-flex h-5 w-fit shrink-0 items-center justify-center rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap",
        style
      )}
    >
      {label}
    </span>
  )
}

export { StatusBadge }
export type { StatusBadgeProps, WorkOrderStatus, QCResult, ShipmentStatus }
