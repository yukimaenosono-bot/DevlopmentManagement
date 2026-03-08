import type { Meta, StoryObj } from '@storybook/react'
import { MasterSelect } from '../MasterSelect'
import { FIGMA } from './figma-links'

const meta: Meta<typeof MasterSelect> = {
  title: 'Composite/MasterSelect',
  component: MasterSelect,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    ...(FIGMA.MasterSelect && {
      design: { type: 'figma', url: FIGMA.MasterSelect },
    }),
  },
}
export default meta

type Story = StoryObj<typeof MasterSelect>

const itemOptions = [
  { value: 'ITEM-001', label: 'アルミブロック A型 (ITEM-001)' },
  { value: 'ITEM-002', label: 'スチールシャフト B型 (ITEM-002)' },
  { value: 'ITEM-003', label: 'プラスチックカバー C (ITEM-003)' },
]

export const Default: Story = {
  args: {
    value: '',
    placeholder: '品目を選択してください',
    options: itemOptions,
    onValueChange: () => {},
  },
}

export const Selected: Story = {
  args: {
    value: 'ITEM-001',
    placeholder: '品目を選択してください',
    options: itemOptions,
    onValueChange: () => {},
  },
}

export const Loading: Story = {
  args: {
    value: '',
    placeholder: '品目を選択してください',
    options: [],
    isLoading: true,
    onValueChange: () => {},
  },
}

export const Disabled: Story = {
  args: {
    value: 'ITEM-002',
    options: itemOptions,
    disabled: true,
    onValueChange: () => {},
  },
}
