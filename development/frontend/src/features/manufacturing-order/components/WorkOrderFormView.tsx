import { useNavigate } from "react-router-dom"
import { toast } from "sonner"
import { PageHeader } from "@/components/composite/PageHeader"
import { Card, CardContent } from "@/components/ui/card"
import { WorkOrderForm } from "./WorkOrderForm"
import { useCreateWorkOrder } from "../hooks/useWorkOrder"
import type { CreateWorkOrderFormValues } from "../schemas/workOrderSchema"

export function WorkOrderFormView() {
  const navigate = useNavigate()
  const { mutate, isPending } = useCreateWorkOrder()

  const handleSubmit = (values: CreateWorkOrderFormValues) => {
    mutate(
      {
        data: {
          itemId: values.itemId,
          quantity: values.quantity,
          plannedStartDate: values.plannedStartDate,
          plannedEndDate: values.plannedEndDate,
          lotNumber: values.lotNumber ?? null,
          productionPlanNumber: values.productionPlanNumber ?? null,
          workInstruction: values.workInstruction ?? null,
        },
      },
      {
        onSuccess: () => {
          toast.success("製造指示を発行しました")
          navigate("/manufacturing-orders")
        },
        onError: () => {
          toast.error("製造指示の発行に失敗しました")
        },
      }
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="製造指示発行"
        description="SCR-MO-002"
      />
      <Card>
        <CardContent className="pt-6">
          <WorkOrderForm
            onSubmit={handleSubmit}
            isPending={isPending}
            onCancel={() => navigate("/manufacturing-orders")}
          />
        </CardContent>
      </Card>
    </div>
  )
}
