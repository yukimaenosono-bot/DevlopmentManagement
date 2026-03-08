import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { MasterSelect } from "@/components/composite/MasterSelect"
import { useGetApiItems } from "@/generated/items/items"
import type { CreateWorkOrderFormValues } from "../schemas/workOrderSchema"
import { createWorkOrderSchema } from "../schemas/workOrderSchema"

interface ItemOption {
  id: string
  code: string
  name: string
  unit: string
}

interface WorkOrderFormProps {
  onSubmit: (values: CreateWorkOrderFormValues) => void
  isPending?: boolean
  onCancel: () => void
}

export function WorkOrderForm({ onSubmit, isPending, onCancel }: WorkOrderFormProps) {
  const { data: itemsRaw, isPending: isItemsLoading } = useGetApiItems({ activeOnly: true })
  const items = (itemsRaw as unknown as ItemOption[] | undefined) ?? []

  const itemOptions = items.map((it) => ({
    value: it.id,
    label: `${it.code} - ${it.name}`,
  }))

  const {
    register,
    control,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<CreateWorkOrderFormValues>({
    resolver: zodResolver(createWorkOrderSchema),
    defaultValues: {
      itemId: "",
      quantity: undefined,
      plannedStartDate: "",
      plannedEndDate: "",
      lotNumber: null,
      productionPlanNumber: null,
      workInstruction: null,
    },
  })

  const selectedItemId = watch("itemId")
  const selectedItem = items.find((it) => it.id === selectedItemId)

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
      {/* 品目選択 */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="flex flex-col gap-1.5">
          <Label>
            製品コード <span className="text-red-500">*</span>
          </Label>
          <Controller
            name="itemId"
            control={control}
            render={({ field }) => (
              <MasterSelect
                value={field.value}
                onValueChange={field.onChange}
                options={itemOptions}
                placeholder="品目を選択"
                isLoading={isItemsLoading}
              />
            )}
          />
          {errors.itemId && (
            <p className="text-xs text-red-500">{errors.itemId.message}</p>
          )}
        </div>
        <div className="flex flex-col gap-1.5">
          <Label>品名</Label>
          <Input value={selectedItem?.name ?? ""} readOnly className="bg-muted" />
        </div>
      </div>

      {/* 数量・ロット */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <div className="flex flex-col gap-1.5">
          <Label>
            製造数量 <span className="text-red-500">*</span>
          </Label>
          <Input
            type="number"
            inputMode="numeric"
            min={1}
            {...register("quantity", { valueAsNumber: true })}
          />
          {errors.quantity && (
            <p className="text-xs text-red-500">{errors.quantity.message}</p>
          )}
        </div>
        <div className="flex flex-col gap-1.5">
          <Label>単位</Label>
          <Input value={selectedItem?.unit ?? ""} readOnly className="bg-muted" />
        </div>
        <div className="flex flex-col gap-1.5">
          <Label>製造ロット番号</Label>
          <Input
            placeholder="LOT-20260308-0001（省略時は自動採番）"
            {...register("lotNumber")}
          />
        </div>
      </div>

      {/* 日付 */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="flex flex-col gap-1.5">
          <Label>
            製造開始予定日 <span className="text-red-500">*</span>
          </Label>
          <Input type="date" {...register("plannedStartDate")} />
          {errors.plannedStartDate && (
            <p className="text-xs text-red-500">{errors.plannedStartDate.message}</p>
          )}
        </div>
        <div className="flex flex-col gap-1.5">
          <Label>
            完了予定日 <span className="text-red-500">*</span>
          </Label>
          <Input type="date" {...register("plannedEndDate")} />
          {errors.plannedEndDate && (
            <p className="text-xs text-red-500">{errors.plannedEndDate.message}</p>
          )}
        </div>
      </div>

      {/* 生産計画番号 */}
      <div className="flex flex-col gap-1.5">
        <Label>生産計画番号</Label>
        <Input
          placeholder="PP-20260308-0001（任意）"
          {...register("productionPlanNumber")}
        />
      </div>

      {/* 作業指示コメント */}
      <div className="flex flex-col gap-1.5">
        <Label>作業指示コメント</Label>
        <Textarea
          placeholder="現場への指示・注意事項を入力してください"
          rows={4}
          {...register("workInstruction")}
        />
      </div>

      <div className="flex justify-end gap-2">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isPending}>
          キャンセル
        </Button>
        <Button type="submit" disabled={isPending}>
          {isPending ? "発行中..." : "製造指示を発行"}
        </Button>
      </div>
    </form>
  )
}
