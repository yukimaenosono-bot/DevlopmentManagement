import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { toast } from "sonner"
import { PageHeader } from "@/components/composite/PageHeader"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { ConfirmDialog } from "@/components/composite/ConfirmDialog"
import { formatDate, formatDateTime } from "@/lib/utils"
import { WorkOrderStatusBadge } from "./WorkOrderStatusBadge"
import {
  useWorkOrder,
  useCancelWorkOrder,
  useStartWorkOrder,
  useCompleteWorkOrder,
} from "../hooks/useWorkOrder"

interface WorkOrderDetailViewProps {
  id: string
}

export function WorkOrderDetailView({ id }: WorkOrderDetailViewProps) {
  const navigate = useNavigate()
  const [cancelOpen, setCancelOpen]     = useState(false)
  const [completeOpen, setCompleteOpen] = useState(false)

  const { data: wo, isPending, isError } = useWorkOrder(id)
  const { mutate: cancel,   isPending: isCancelling } = useCancelWorkOrder(id)
  const { mutate: start,    isPending: isStarting    } = useStartWorkOrder(id)
  const { mutate: complete, isPending: isCompleting  } = useCompleteWorkOrder(id)

  const isActioning = isCancelling || isStarting || isCompleting

  if (isPending) {
    return (
      <div className="flex flex-col gap-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-40 w-full" />
      </div>
    )
  }

  if (isError || !wo) {
    return <p className="text-muted-foreground">製造指示が見つかりません。</p>
  }

  const handleCancel = () => {
    cancel(
      { id },
      {
        onSuccess: () => { toast.success("キャンセルしました"); setCancelOpen(false) },
        onError:   () => toast.error("キャンセルに失敗しました"),
      }
    )
  }

  const handleStart = () => {
    start(
      { id },
      {
        onSuccess: () => toast.success("着手しました"),
        onError:   () => toast.error("着手に失敗しました"),
      }
    )
  }

  const handleComplete = () => {
    complete(
      { id },
      {
        onSuccess: () => { toast.success("完了しました"); setCompleteOpen(false) },
        onError:   () => toast.error("完了処理に失敗しました"),
      }
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <ConfirmDialog
        open={cancelOpen}
        onOpenChange={setCancelOpen}
        title="製造指示をキャンセルしますか？"
        description="キャンセルすると元に戻せません。"
        variant="destructive"
        confirmLabel="キャンセルする"
        onConfirm={handleCancel}
        isPending={isCancelling}
      />
      <ConfirmDialog
        open={completeOpen}
        onOpenChange={setCompleteOpen}
        title="製造指示を完了しますか？"
        description="完了後は変更できません。"
        confirmLabel="完了する"
        onConfirm={handleComplete}
        isPending={isCompleting}
      />

      <PageHeader
        title={wo.workOrderNumber}
        description="SCR-MO-003"
        actions={
          <div className="flex gap-2">
            {wo.status === 1 && (
              <>
                <Button
                  variant="outline"
                  onClick={() => setCancelOpen(true)}
                  disabled={isActioning}
                >
                  キャンセル
                </Button>
                <Button onClick={handleStart} disabled={isActioning}>
                  {isStarting ? "着手中..." : "着手"}
                </Button>
              </>
            )}
            {wo.status === 2 && (
              <Button onClick={() => setCompleteOpen(true)} disabled={isActioning}>
                完了
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => navigate("/manufacturing-orders")}
            >
              一覧へ戻る
            </Button>
          </div>
        }
      />

      <Card>
        <CardContent className="pt-6">
          <dl className="grid grid-cols-2 gap-x-8 gap-y-4 md:grid-cols-3">
            <InfoRow label="製造指示番号" value={wo.workOrderNumber} />
            <InfoRow label="ステータス"   value={<WorkOrderStatusBadge status={wo.status} />} />
            <InfoRow label="製品コード"   value={wo.itemCode} />
            <InfoRow label="品名"         value={wo.itemName} />
            <InfoRow label="製造数量"     value={wo.quantity.toLocaleString()} />
            <InfoRow label="ロット番号"   value={wo.lotNumber ?? "—"} />
            <InfoRow label="開始予定日"   value={formatDate(wo.plannedStartDate)} />
            <InfoRow label="完了予定日"   value={formatDate(wo.plannedEndDate)} />
            <InfoRow label="生産計画番号" value={wo.productionPlanNumber ?? "—"} />
            <InfoRow label="発行日時"     value={formatDateTime(wo.createdAt)} />
            <InfoRow label="更新日時"     value={formatDateTime(wo.updatedAt)} />
          </dl>
          {wo.workInstruction && (
            <div className="mt-6">
              <p className="text-sm font-medium text-muted-foreground mb-1">作業指示コメント</p>
              <p className="text-sm whitespace-pre-wrap rounded-md bg-muted p-3">
                {wo.workInstruction}
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <dt className="text-xs font-medium text-muted-foreground">{label}</dt>
      <dd className="text-sm">{value}</dd>
    </div>
  )
}
