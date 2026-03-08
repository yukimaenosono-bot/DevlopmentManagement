import type { Meta, StoryObj } from '@storybook/react'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { FilterPanel } from '../FilterPanel'
import { FIGMA } from './figma-links'

const meta: Meta<typeof FilterPanel> = {
  title: 'Composite/FilterPanel',
  component: FilterPanel,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
    ...(FIGMA.FilterPanel && {
      design: { type: 'figma', url: FIGMA.FilterPanel },
    }),
  },
}
export default meta

type Story = StoryObj<typeof FilterPanel>

export const Default: Story = {
  render: () => (
    <FilterPanel onSearch={() => {}} onReset={() => {}}>
      <Input placeholder="MO番号・品目名で検索" className="w-64" />
      <Select>
        <SelectTrigger className="w-36">
          <SelectValue placeholder="ステータス" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">すべて</SelectItem>
          <SelectItem value="InProgress">進行中</SelectItem>
          <SelectItem value="Released">発行済</SelectItem>
          <SelectItem value="Completed">完了</SelectItem>
        </SelectContent>
      </Select>
    </FilterPanel>
  ),
}

export const Loading: Story = {
  render: () => (
    <FilterPanel onSearch={() => {}} onReset={() => {}} isPending>
      <Input placeholder="検索中..." className="w-64" disabled />
    </FilterPanel>
  ),
}
