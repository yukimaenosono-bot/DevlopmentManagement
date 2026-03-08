import { useState } from "react"
import { toast } from "sonner"
import { Skeleton } from "@/components/ui/skeleton"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ConfirmDialog } from "@/components/composite/ConfirmDialog"
import { WorkOrderStatusBadge } from "./WorkOrderStatusBadge"
import {
  useWorkOrder,
  useStartWorkOrder,
  useCompleteWorkOrder,
} from "../hooks/useWorkOrder"

interface TabletWorkOrderViewProps {
  id: string
}

export function TabletWorkOrderView({ id }: TabletWorkOrderViewProps) {
  const [completeOpen, setCompleteOpen] = useState(false)

  const { data: wo, isPending, isError } = useWorkOrder(id)
  const { mutate: start,    isPending: isStarting   } = useStartWorkOrder(id)
  const { mutate: complete, isPending: isCompleting } = useCompleteWorkOrder(id)

  if (isPending) {
    return (
      <div className="flex flex-col gap-4 p-4">
        <Skeleton className="h-12 w-3/4" />
        <Skeleton className="h-8 w-1/2" />
        <Skeleton className="h-32 w-full" />
      </div>
    )
  }

  if (isError || !wo) {
    return (
      <div className="p-8 text-center text-muted-foreground">
        製造指示が見つかりません。
      </div>
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
    <div className="flex flex-col gap-6 p-4">
      <ConfirmDialog
        open={completeOpen}
        onOpenChange={setCompleteOpen}
        title="製造指示を完了しますか？"
        description="完了後は変更できません。"
        confirmLabel="完了する"
        onConfirm={handleComplete}
        isPending={isCompleting}
      />

      {/* ヘッダー情報 */}
      <div className="flex flex-col gap-2">
        <p className="text-sm text-muted-foreground">製造指示番号</p>
        <p className="text-3xl font-bold">{wo.workOrderNumber}</p>
        <WorkOrderStatusBadge status={wo.status} />
      </div>

      {/* 品名・数量 */}
      <Card>
        <CardContent className="flex flex-col gap-3 pt-6">
          <div>
            <p className="text-sm text-muted-foreground">品名</p>
            <p className="text-2xl font-semibold">{wo.itemName}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">製品コード</p>
            <p className="text-lg">{wo.itemCode}</p>
          </div>
          <div>
            <p className="text-sm text-muted-foreground">製造数量</p>
            <p className="text-xl font-semibold">{wo.quantity.toLocaleString()}</p>
          </div>
          {wo.lotNumber && (
            <div>
              <p className="text-sm text-muted-foreground">ロット番号</p>
              <p className="text-lg">{wo.lotNumber}</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* 作業指示 */}
      {wo.workInstruction && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm font-medium text-muted-foreground mb-2">作業指示</p>
            <p className="text-base whitespace-pre-wrap">{wo.workInstruction}</p>
          </CardContent>
        </Card>
      )}

      {/* アクションボタン */}
      <div className="flex flex-col gap-3 mt-2">
        {wo.status === 1 && (
          <Button
            className="min-h-12 text-lg"
            onClick={handleStart}
            disabled={isStarting}
          >
            {isStarting ? "処理中..." : "着手する"}
          </Button>
        )}
        {wo.status === 2 && (
          <Button
            className="min-h-12 text-lg"
            onClick={() => setCompleteOpen(true)}
            disabled={isCompleting}
          >
            完了する
          </Button>
        )}
        {wo.status === 3 && (
          <p className="text-center text-muted-foreground py-4">この製造指示は完了済みです。</p>
        )}
      </div>
    </div>
  )
}
