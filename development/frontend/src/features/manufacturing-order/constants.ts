/**
 * 製造指示ドメインの定数定義。
 * バックエンドの WorkOrderStatus enum と 1:1 で対応させること。
 * Issued=1 / InProgress=2 / Completed=3
 */

export const WORK_ORDER_STATUS = {
  1: { label: "発行済",   color: "blue"  },
  2: { label: "着手中",   color: "yellow" },
  3: { label: "完了",     color: "green" },
} as const

export type WorkOrderStatusValue = keyof typeof WORK_ORDER_STATUS

export const WORK_ORDER_STATUS_OPTIONS = [
  { value: "1", label: "発行済" },
  { value: "2", label: "着手中" },
  { value: "3", label: "完了" },
] as const
