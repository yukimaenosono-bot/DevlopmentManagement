import { useGetApiWorkOrders } from "@/generated/work-orders/work-orders"
import type { GetApiWorkOrdersParams } from "@/generated/model"

/** 製造指示一覧取得フック */
export function useWorkOrders(params?: GetApiWorkOrdersParams) {
  return useGetApiWorkOrders(params)
}
