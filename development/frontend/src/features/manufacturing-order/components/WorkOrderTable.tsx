import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Skeleton } from "@/components/ui/skeleton"
import { cn } from "@/lib/utils"
import { formatDate } from "@/lib/utils"
import type { WorkOrderDto } from "../types"
import { WorkOrderStatusBadge } from "./WorkOrderStatusBadge"

interface WorkOrderTableProps {
  data: WorkOrderDto[]
  isLoading?: boolean
  onRowClick?: (row: WorkOrderDto) => void
}

export function WorkOrderTable({ data, isLoading, onRowClick }: WorkOrderTableProps) {
  const headers = [
    "製造指示番号", "製品コード", "品名", "製造数量",
    "開始予定日", "完了予定日", "ステータス",
  ]

  if (isLoading) {
    return (
      <Table>
        <TableHeader>
          <TableRow>
            {headers.map((h) => <TableHead key={h}>{h}</TableHead>)}
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 3 }).map((_, i) => (
            <TableRow key={i}>
              {headers.map((h) => (
                <TableCell key={h}><Skeleton className="h-4 w-full" /></TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    )
  }

  if (data.length === 0) {
    return (
      <Table>
        <TableHeader>
          <TableRow>
            {headers.map((h) => <TableHead key={h}>{h}</TableHead>)}
          </TableRow>
        </TableHeader>
        <TableBody>
          <TableRow>
            <TableCell colSpan={headers.length} className="h-24 text-center text-muted-foreground">
              製造指示が見つかりません
            </TableCell>
          </TableRow>
        </TableBody>
      </Table>
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          {headers.map((h) => <TableHead key={h}>{h}</TableHead>)}
        </TableRow>
      </TableHeader>
      <TableBody>
        {data.map((row) => (
          <TableRow
            key={row.id}
            onClick={onRowClick ? () => onRowClick(row) : undefined}
            className={cn(onRowClick && "cursor-pointer hover:bg-muted/50")}
          >
            <TableCell className="font-mono text-sm">{row.workOrderNumber}</TableCell>
            <TableCell>{row.itemCode}</TableCell>
            <TableCell>{row.itemName}</TableCell>
            <TableCell className="text-right">{row.quantity.toLocaleString()}</TableCell>
            <TableCell>{formatDate(row.plannedStartDate)}</TableCell>
            <TableCell>{formatDate(row.plannedEndDate)}</TableCell>
            <TableCell><WorkOrderStatusBadge status={row.status} /></TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
