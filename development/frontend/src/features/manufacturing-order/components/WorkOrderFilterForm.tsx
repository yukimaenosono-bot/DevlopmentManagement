import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { FilterPanel } from "@/components/composite/FilterPanel"
import { WORK_ORDER_STATUS_OPTIONS } from "../constants"

interface WorkOrderFilterValues {
  workOrderNumber: string
  keyword: string
  status: string
  from: string
  to: string
}

interface WorkOrderFilterFormProps {
  values: WorkOrderFilterValues
  onChange: (values: WorkOrderFilterValues) => void
  onSearch: () => void
  onReset: () => void
  isPending?: boolean
}

export function WorkOrderFilterForm({
  values,
  onChange,
  onSearch,
  onReset,
  isPending,
}: WorkOrderFilterFormProps) {
  const set = <K extends keyof WorkOrderFilterValues>(key: K, val: WorkOrderFilterValues[K]) =>
    onChange({ ...values, [key]: val })

  return (
    <FilterPanel onSearch={onSearch} onReset={onReset} isPending={isPending}>
      <div className="flex flex-col gap-1 min-w-40">
        <Label className="text-xs">製造指示番号</Label>
        <Input
          placeholder="MO-20260308-0001"
          value={values.workOrderNumber}
          onChange={(e) => set("workOrderNumber", e.target.value)}
        />
      </div>
      <div className="flex flex-col gap-1 min-w-40">
        <Label className="text-xs">製品コード / 品名</Label>
        <Input
          placeholder="部分一致"
          value={values.keyword}
          onChange={(e) => set("keyword", e.target.value)}
        />
      </div>
      <div className="flex flex-col gap-1 min-w-36">
        <Label className="text-xs">ステータス</Label>
        <Select value={values.status} onValueChange={(v) => set("status", v ?? "all")}>
          <SelectTrigger>
            <SelectValue placeholder="すべて" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">すべて</SelectItem>
            {WORK_ORDER_STATUS_OPTIONS.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="flex flex-col gap-1 min-w-36">
        <Label className="text-xs">開始予定日（From）</Label>
        <Input
          type="date"
          value={values.from}
          onChange={(e) => set("from", e.target.value)}
        />
      </div>
      <div className="flex flex-col gap-1 min-w-36">
        <Label className="text-xs">開始予定日（To）</Label>
        <Input
          type="date"
          value={values.to}
          onChange={(e) => set("to", e.target.value)}
        />
      </div>
    </FilterPanel>
  )
}

export type { WorkOrderFilterValues }
