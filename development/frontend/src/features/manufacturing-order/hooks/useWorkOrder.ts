import { useQueryClient } from "@tanstack/react-query"
import type { UseQueryResult } from "@tanstack/react-query"
import {
  useGetApiWorkOrdersId,
  usePostApiWorkOrders,
  usePutApiWorkOrdersId,
  usePostApiWorkOrdersIdCancel,
  usePostApiWorkOrdersIdStart,
  usePostApiWorkOrdersIdComplete,
} from "@/generated/work-orders/work-orders"
import { workOrderKeys } from "@/services/queryKeys"
import type { WorkOrderDto } from "../types"

/** 製造指示1件取得フック */
export function useWorkOrder(id: string) {
  return useGetApiWorkOrdersId(id) as UseQueryResult<WorkOrderDto>
}

/** 製造指示発行 Mutation */
export function useCreateWorkOrder() {
  const queryClient = useQueryClient()
  return usePostApiWorkOrders({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: workOrderKeys.lists() })
      },
    },
  })
}

/** 製造指示更新 Mutation */
export function useUpdateWorkOrder(id: string) {
  const queryClient = useQueryClient()
  return usePutApiWorkOrdersId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: workOrderKeys.detail(id) })
        queryClient.invalidateQueries({ queryKey: workOrderKeys.lists() })
      },
    },
  })
}

/** 製造指示キャンセル Mutation */
export function useCancelWorkOrder(id: string) {
  const queryClient = useQueryClient()
  return usePostApiWorkOrdersIdCancel({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: workOrderKeys.detail(id) })
        queryClient.invalidateQueries({ queryKey: workOrderKeys.lists() })
      },
    },
  })
}

/** 製造指示着手 Mutation */
export function useStartWorkOrder(id: string) {
  const queryClient = useQueryClient()
  return usePostApiWorkOrdersIdStart({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: workOrderKeys.detail(id) })
        queryClient.invalidateQueries({ queryKey: workOrderKeys.lists() })
      },
    },
  })
}

/** 製造指示完了 Mutation */
export function useCompleteWorkOrder(id: string) {
  const queryClient = useQueryClient()
  return usePostApiWorkOrdersIdComplete({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: workOrderKeys.detail(id) })
        queryClient.invalidateQueries({ queryKey: workOrderKeys.lists() })
      },
    },
  })
}
