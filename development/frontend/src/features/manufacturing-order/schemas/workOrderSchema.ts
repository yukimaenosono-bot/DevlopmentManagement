import { z } from "zod"

/** 製造指示発行フォームのバリデーションスキーマ */
export const createWorkOrderSchema = z.object({
  itemId: z.string().min(1, "品目を選択してください"),
  quantity: z
    .number()
    .int("整数を入力してください")
    .positive("1以上の数値を入力してください"),
  plannedStartDate: z.string().min(1, "製造開始予定日を入力してください"),
  plannedEndDate: z.string().min(1, "完了予定日を入力してください"),
  lotNumber: z.string().nullable().optional(),
  productionPlanNumber: z.string().nullable().optional(),
  workInstruction: z.string().nullable().optional(),
})

export type CreateWorkOrderFormValues = z.infer<typeof createWorkOrderSchema>

/** 製造指示更新フォームのバリデーションスキーマ（着手中は数量変更不可） */
export const updateWorkOrderSchema = z.object({
  quantity: z
    .number()
    .int("整数を入力してください")
    .positive("1以上の数値を入力してください"),
  plannedStartDate: z.string().min(1, "製造開始予定日を入力してください"),
  plannedEndDate: z.string().min(1, "完了予定日を入力してください"),
  workInstruction: z.string().nullable().optional(),
})

export type UpdateWorkOrderFormValues = z.infer<typeof updateWorkOrderSchema>
