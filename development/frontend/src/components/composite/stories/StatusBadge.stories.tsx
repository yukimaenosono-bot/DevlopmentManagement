import type { Meta, StoryObj } from '@storybook/react'
import { StatusBadge } from '../StatusBadge'
import { FIGMA } from './figma-links'

const meta: Meta<typeof StatusBadge> = {
  title: 'Composite/StatusBadge',
  component: StatusBadge,
  tags: ['autodocs'],
  parameters: {
    ...(FIGMA.StatusBadge && {
      design: { type: 'figma', url: FIGMA.StatusBadge },
    }),
  },
  argTypes: {
    status: {
      control: 'select',
      options: [
        'Draft', 'Released', 'InProgress', 'Completed', 'Cancelled',
        'Pass', 'Fail', 'Pending',
        'Picking', 'Shipped',
      ],
    },
  },
}
export default meta

type Story = StoryObj<typeof StatusBadge>

export const WorkOrderDraft: Story = { args: { status: 'Draft' } }
export const WorkOrderReleased: Story = { args: { status: 'Released' } }
export const WorkOrderInProgress: Story = { args: { status: 'InProgress' } }
export const WorkOrderCompleted: Story = { args: { status: 'Completed' } }
export const WorkOrderCancelled: Story = { args: { status: 'Cancelled' } }

export const QCPass: Story = { args: { status: 'Pass' } }
export const QCFail: Story = { args: { status: 'Fail' } }
export const QCPending: Story = { args: { status: 'Pending' } }

export const ShipmentPicking: Story = { args: { status: 'Picking' } }
export const ShipmentShipped: Story = { args: { status: 'Shipped' } }

export const AllStatuses: Story = {
  render: () => (
    <div className="flex flex-wrap gap-2">
      {['Draft', 'Released', 'InProgress', 'Completed', 'Cancelled',
        'Pass', 'Fail', 'Pending', 'Picking', 'Shipped'].map((s) => (
        <StatusBadge key={s} status={s} />
      ))}
    </div>
  ),
}
