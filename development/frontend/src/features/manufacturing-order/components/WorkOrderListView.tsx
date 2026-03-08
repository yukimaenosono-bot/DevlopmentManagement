import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { toast } from "sonner"
import { PageHeader } from "@/components/composite/PageHeader"
import { Button } from "@/components/ui/button"
import { Alert } from "@/components/ui/alert"
import { WorkOrderFilterForm, type WorkOrderFilterValues } from "./WorkOrderFilterForm"
import { WorkOrderTable } from "./WorkOrderTable"
import { useWorkOrders } from "../hooks/useWorkOrders"
import type { WorkOrderDto } from "../types"

const DEFAULT_FILTERS: WorkOrderFilterValues = {
  workOrderNumber: "",
  keyword: "",
  status: "all",
  from: "",
  to: "",
}

export function WorkOrderListView() {
  const navigate = useNavigate()
  const [filters, setFilters] = useState<WorkOrderFilterValues>(DEFAULT_FILTERS)
  const [activeParams, setActiveParams] = useState<Record<string, string | number>>({})

  const { data = [], isPending, isError } = useWorkOrders(
    Object.keys(activeParams).length > 0 ? activeParams : undefined
  )

  const handleSearch = () => {
    const params: Record<string, string | number> = {}
    if (filters.status && filters.status !== "all") params.status = Number(filters.status)
    if (filters.from) params.from = filters.from
    if (filters.to)   params.to   = filters.to
    setActiveParams(params)
  }

  const handleReset = () => {
    setFilters(DEFAULT_FILTERS)
    setActiveParams({})
  }

  const handleRowClick = (row: WorkOrderDto) => {
    navigate(`/manufacturing-orders/${row.id}`)
  }

  if (isError) {
    toast.error("製造指示一覧の取得に失敗しました")
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="製造指示一覧"
        description="SCR-MO-001"
        actions={
          <Button onClick={() => navigate("/manufacturing-orders/new")}>
            新規発行
          </Button>
        }
      />

      <WorkOrderFilterForm
        values={filters}
        onChange={setFilters}
        onSearch={handleSearch}
        onReset={handleReset}
        isPending={isPending}
      />

      {isError && (
        <Alert>データの取得に失敗しました。再読み込みしてください。</Alert>
      )}

      <div className="rounded-md border">
        <WorkOrderTable
          data={data}
          isLoading={isPending}
          onRowClick={handleRowClick}
        />
      </div>
    </div>
  )
}
