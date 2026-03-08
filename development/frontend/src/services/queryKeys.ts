/**
 * TanStack Query のクエリキーを一元管理する。
 * 階層構造 { all, lists, list, details, detail } で定義し、
 * invalidateQueries の粒度を統一する。
 */

export const workOrderKeys = {
  all:     () => ["work-orders"] as const,
  lists:   () => [...workOrderKeys.all(), "list"] as const,
  list:    (params: object) => [...workOrderKeys.lists(), params] as const,
  details: () => [...workOrderKeys.all(), "detail"] as const,
  detail:  (id: string) => [...workOrderKeys.details(), id] as const,
}

export const inventoryKeys = {
  all:   () => ["inventory"] as const,
  lists: () => [...inventoryKeys.all(), "list"] as const,
  list:  (params: object) => [...inventoryKeys.lists(), params] as const,
}

export const itemKeys = {
  all:     () => ["items"] as const,
  lists:   () => [...itemKeys.all(), "list"] as const,
  list:    (params: object) => [...itemKeys.lists(), params] as const,
  details: () => [...itemKeys.all(), "detail"] as const,
  detail:  (id: string) => [...itemKeys.details(), id] as const,
}

export const processKeys = {
  all:   () => ["processes"] as const,
  lists: () => [...processKeys.all(), "list"] as const,
  list:  (params: object) => [...processKeys.lists(), params] as const,
}

export const equipmentKeys = {
  all:   () => ["equipments"] as const,
  lists: () => [...equipmentKeys.all(), "list"] as const,
  list:  (params: object) => [...equipmentKeys.lists(), params] as const,
}

export const warehouseKeys = {
  all:   () => ["warehouses"] as const,
  lists: () => [...warehouseKeys.all(), "list"] as const,
  list:  (params: object) => [...warehouseKeys.lists(), params] as const,
}

export const bomKeys = {
  all:   () => ["bom"] as const,
  lists: () => [...bomKeys.all(), "list"] as const,
  list:  (params: object) => [...bomKeys.lists(), params] as const,
}

export const userKeys = {
  all:   () => ["users"] as const,
  lists: () => [...userKeys.all(), "list"] as const,
  list:  (params: object) => [...userKeys.lists(), params] as const,
}
