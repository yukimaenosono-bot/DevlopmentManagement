// SCR-MO-004 現場端末用製造指示
import { useParams } from "react-router-dom"
import { TabletWorkOrderView } from "@/features/manufacturing-order/components/TabletWorkOrderView"

export function TabletWorkOrderPage() {
  const { id } = useParams<{ id: string }>()
  if (!id) return null
  return <TabletWorkOrderView id={id} />
}
