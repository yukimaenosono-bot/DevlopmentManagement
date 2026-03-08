import type { Meta, StoryObj } from '@storybook/react'
import { Button } from '@/components/ui/button'
import { PageHeader } from '../PageHeader'
import { FIGMA } from './figma-links'

const meta: Meta<typeof PageHeader> = {
  title: 'Composite/PageHeader',
  component: PageHeader,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
    ...(FIGMA.PageHeader && {
      design: { type: 'figma', url: FIGMA.PageHeader },
    }),
  },
}
export default meta

type Story = StoryObj<typeof PageHeader>

export const WithActions: Story = {
  args: {
    title: '製造指示一覧',
    description: '製造指示の確認・発行・ステータス管理を行います',
    actions: <Button size="sm">+ 新規発行</Button>,
  },
}

export const TitleOnly: Story = {
  args: {
    title: '在庫一覧',
  },
}

export const TitleAndDescription: Story = {
  args: {
    title: '品質検査一覧',
    description: '各ロットの検査実績を確認します',
  },
}
