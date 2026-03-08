// SCR-MO-003 製造指示詳細・編集
import { useParams } from "react-router-dom"
import { WorkOrderDetailView } from "@/features/manufacturing-order/components/WorkOrderDetailView"

export function WorkOrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  if (!id) return null
  return <WorkOrderDetailView id={id} />
}
