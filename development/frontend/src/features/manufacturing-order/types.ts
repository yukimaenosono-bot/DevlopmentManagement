import type { IsoDateString, IsoDateTimeString } from "@/types/api-primitives"

/**
 * バックエンドの WorkOrderDto と 1:1 対応する型。
 * generated/ のレスポンス型が void のため、暫定的にここで定義する。
 * OpenAPI にレスポンス型が追加され pnpm generate が実行されたら削除して generated/ を使うこと。
 */
export interface WorkOrderDto {
  id: string
  workOrderNumber: string
  productionPlanNumber: string | null
  itemId: string
  itemCode: string
  itemName: string
  lotNumber: string | null
  quantity: number
  plannedStartDate: IsoDateString
  plannedEndDate: IsoDateString
  /** WorkOrderStatus: Issued=1 / InProgress=2 / Completed=3 */
  status: number
  workInstruction: string | null
  createdByUserId: string
  createdAt: IsoDateTimeString
  updatedAt: IsoDateTimeString
}
