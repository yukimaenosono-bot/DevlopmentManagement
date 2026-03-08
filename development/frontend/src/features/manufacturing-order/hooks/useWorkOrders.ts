import { useGetApiWorkOrders } from "@/generated/work-orders/work-orders"
import type { GetApiWorkOrdersParams } from "@/generated/model"
import type { WorkOrderDto } from "../types"
import type { UseQueryResult } from "@tanstack/react-query"

/**
 * 製造指示一覧取得フック。
 * generated/ のレスポンス型が void のため型アサーションを行う。
 * OpenAPI にレスポンス型が追加されたらアサーションを削除すること。
 */
export function useWorkOrders(params?: GetApiWorkOrdersParams) {
  return useGetApiWorkOrders(params) as UseQueryResult<WorkOrderDto[]>
}
