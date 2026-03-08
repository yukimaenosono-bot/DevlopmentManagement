import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { cn } from "@/lib/utils"

interface MasterSelectOption {
  value: string
  label: string
}

interface MasterSelectProps {
  value: string
  onValueChange: (value: string) => void
  options: MasterSelectOption[]
  placeholder?: string
  isLoading?: boolean
  disabled?: boolean
  className?: string
}

function MasterSelect({
  value,
  onValueChange,
  options,
  placeholder = "選択してください",
  isLoading = false,
  disabled = false,
  className,
}: MasterSelectProps) {
  return (
    <Select value={value} onValueChange={(v) => v !== null && onValueChange(v)}>
      <SelectTrigger
        className={cn("w-full", className)}
        disabled={isLoading || disabled}
      >
        <SelectValue
          placeholder={isLoading ? "読み込み中..." : placeholder}
        />
      </SelectTrigger>
      <SelectContent>
        {options.map((option) => (
          <SelectItem key={option.value} value={option.value}>
            {option.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}

export { MasterSelect }
export type { MasterSelectProps, MasterSelectOption }
