import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from '@/components/layout/AppLayout'
import { AuthLayout } from '@/components/layout/AuthLayout'
import { TabletLayout } from '@/components/layout/TabletLayout'
import { LoginPage } from '@/pages/LoginPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { UiCatalogPage } from '@/pages/UiCatalogPage'
import { useAuth } from '@/hooks/useAuth'

// 製造指示
import { WorkOrderListPage } from '@/pages/manufacturing-order/WorkOrderListPage'
import { WorkOrderDetailPage } from '@/pages/manufacturing-order/WorkOrderDetailPage'
import { WorkOrderNewPage } from '@/pages/manufacturing-order/WorkOrderNewPage'
import { TabletWorkOrderPage } from '@/pages/manufacturing-order/TabletWorkOrderPage'

// 生産計画
import { ProductionPlanListPage } from '@/pages/production-plan/ProductionPlanListPage'
import { ProductionPlanFormPage } from '@/pages/production-plan/ProductionPlanFormPage'

// 工程
import { ProcessMonitoringPage } from '@/pages/process/ProcessMonitoringPage'
import { TabletProcessInputPage } from '@/pages/process/TabletProcessInputPage'

// 在庫
import { InventoryListPage } from '@/pages/inventory/InventoryListPage'
import { InventoryReceiptPage } from '@/pages/inventory/InventoryReceiptPage'
import { InventoryReleasePage } from '@/pages/inventory/InventoryReleasePage'
import { InventoryCountPage } from '@/pages/inventory/InventoryCountPage'

// 品質管理
import { InspectionListPage } from '@/pages/quality-control/InspectionListPage'
import { InspectionResultPage } from '@/pages/quality-control/InspectionResultPage'
import { DefectRegistrationPage } from '@/pages/quality-control/DefectRegistrationPage'
import { QualityAnalysisPage } from '@/pages/quality-control/QualityAnalysisPage'

// 出荷
import { ShipmentListPage } from '@/pages/shipment/ShipmentListPage'
import { ShipmentDetailPage } from '@/pages/shipment/ShipmentDetailPage'
import { ShipmentResultPage } from '@/pages/shipment/ShipmentResultPage'
import { DeliveryNotePage } from '@/pages/shipment/DeliveryNotePage'

// マスタ
import { ItemMasterPage } from '@/pages/master/ItemMasterPage'
import { BOMPage } from '@/pages/master/BOMPage'
import { ProcessMasterPage } from '@/pages/master/ProcessMasterPage'
import { RoutingPage } from '@/pages/master/RoutingPage'
import { EquipmentMasterPage } from '@/pages/master/EquipmentMasterPage'
import { WarehouseMasterPage } from '@/pages/master/WarehouseMasterPage'
import { InspectionStandardPage } from '@/pages/master/InspectionStandardPage'

// システム管理
import { UserManagementPage } from '@/pages/system/UserManagementPage'
import { RoleManagementPage } from '@/pages/system/RoleManagementPage'
import { AuditLogPage } from '@/pages/system/AuditLogPage'

/** 未認証ユーザーをログイン画面へリダイレクトする */
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth()
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <Routes>
      {/* 認証画面 */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>

      {/* 開発用UIカタログ（認証不要） */}
      <Route path="/ui-catalog" element={<UiCatalogPage />} />

      {/* PC向け認証済み画面 */}
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        {/* ダッシュボード */}
        <Route index element={<DashboardPage />} />

        {/* 製造指示 */}
        <Route path="manufacturing-orders" element={<WorkOrderListPage />} />
        <Route path="manufacturing-orders/new" element={<WorkOrderNewPage />} />
        <Route path="manufacturing-orders/:id" element={<WorkOrderDetailPage />} />

        {/* 生産計画 */}
        <Route path="production-plans" element={<ProductionPlanListPage />} />
        <Route path="production-plans/new" element={<ProductionPlanFormPage />} />
        <Route path="production-plans/:id/edit" element={<ProductionPlanFormPage />} />

        {/* 工程モニタリング */}
        <Route path="process-monitoring" element={<ProcessMonitoringPage />} />

        {/* 在庫管理 */}
        <Route path="inventory" element={<InventoryListPage />} />
        <Route path="inventory/receipt" element={<InventoryReceiptPage />} />
        <Route path="inventory/release" element={<InventoryReleasePage />} />
        <Route path="inventory/count" element={<InventoryCountPage />} />

        {/* 品質管理 */}
        <Route path="quality/inspections" element={<InspectionListPage />} />
        <Route path="quality/inspections/:id" element={<InspectionResultPage />} />
        <Route path="quality/defects/new" element={<DefectRegistrationPage />} />
        <Route path="quality/analysis" element={<QualityAnalysisPage />} />

        {/* 出荷 */}
        <Route path="shipments" element={<ShipmentListPage />} />
        <Route path="shipments/:id" element={<ShipmentDetailPage />} />
        <Route path="shipments/:id/result" element={<ShipmentResultPage />} />
        <Route path="shipments/:id/delivery-note" element={<DeliveryNotePage />} />

        {/* マスタ管理 */}
        <Route path="master/items" element={<ItemMasterPage />} />
        <Route path="master/bom" element={<BOMPage />} />
        <Route path="master/processes" element={<ProcessMasterPage />} />
        <Route path="master/routings" element={<RoutingPage />} />
        <Route path="master/equipment" element={<EquipmentMasterPage />} />
        <Route path="master/warehouses" element={<WarehouseMasterPage />} />
        <Route path="master/inspection-standards" element={<InspectionStandardPage />} />

        {/* システム管理 */}
        <Route path="system/users" element={<UserManagementPage />} />
        <Route path="system/roles" element={<RoleManagementPage />} />
        <Route path="system/audit-logs" element={<AuditLogPage />} />
      </Route>

      {/* タブレット向け画面（認証済み） */}
      <Route
        element={
          <ProtectedRoute>
            <TabletLayout />
          </ProtectedRoute>
        }
      >
        <Route path="tablet/work-orders/:id" element={<TabletWorkOrderPage />} />
        <Route path="tablet/process-input" element={<TabletProcessInputPage />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}
