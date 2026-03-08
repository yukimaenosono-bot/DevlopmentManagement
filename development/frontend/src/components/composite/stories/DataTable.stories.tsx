import type { Meta, StoryObj } from '@storybook/react'
import { StatusBadge } from '../StatusBadge'
import { DataTable } from '../DataTable'
import { FIGMA } from './figma-links'

type WorkOrderRow = {
  id: string
  itemName: string
  quantity: number
  status: string
}

const meta: Meta<typeof DataTable<WorkOrderRow>> = {
  title: 'Composite/DataTable',
  component: DataTable,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
    ...(FIGMA.DataTable && {
      design: { type: 'figma', url: FIGMA.DataTable },
    }),
  },
}
export default meta

type Story = StoryObj<typeof DataTable<WorkOrderRow>>

const columns = [
  { key: 'id' as const, label: 'MO番号', className: 'w-28' },
  { key: 'itemName' as const, label: '品目名' },
  { key: 'quantity' as const, label: '数量', className: 'w-20 text-right' },
  {
    key: 'status' as const,
    label: 'ステータス',
    className: 'w-28',
    render: (row: WorkOrderRow) => <StatusBadge status={row.status} />,
  },
]

const data: WorkOrderRow[] = [
  { id: 'MO-2026-001', itemName: 'アルミフレーム A型', quantity: 100, status: 'InProgress' },
  { id: 'MO-2026-002', itemName: 'スチールシャフト B型', quantity: 50, status: 'Released' },
  { id: 'MO-2026-003', itemName: 'プラスチックカバー C', quantity: 200, status: 'Completed' },
]

export const Default: Story = {
  render: () => <DataTable columns={columns} data={data} />,
}

export const Loading: Story = {
  render: () => <DataTable columns={columns} data={[]} isLoading />,
}

export const Empty: Story = {
  render: () => <DataTable columns={columns} data={[]} emptyMessage="製造指示が見つかりませんでした" />,
}

export const Clickable: Story = {
  render: () => (
    <DataTable
      columns={columns}
      data={data}
      onRowClick={(row) => alert(`${row.id} をクリックしました`)}
    />
  ),
}
