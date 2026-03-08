import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { ConfirmDialog } from '../ConfirmDialog'
import { FIGMA } from './figma-links'

const meta: Meta<typeof ConfirmDialog> = {
  title: 'Composite/ConfirmDialog',
  component: ConfirmDialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    ...(FIGMA.ConfirmDialog && {
      design: { type: 'figma', url: FIGMA.ConfirmDialog },
    }),
  },
}
export default meta

type Story = StoryObj<typeof ConfirmDialog>

function DialogDemo(props: Partial<React.ComponentProps<typeof ConfirmDialog>>) {
  const [open, setOpen] = useState(false)
  return (
    <>
      <Button variant={props.variant === 'destructive' ? 'destructive' : 'outline'} onClick={() => setOpen(true)}>
        ダイアログを開く
      </Button>
      <ConfirmDialog
        open={open}
        onOpenChange={setOpen}
        title={props.title ?? '確認'}
        description={props.description ?? 'この操作を実行しますか？'}
        variant={props.variant ?? 'default'}
        confirmLabel={props.confirmLabel}
        onConfirm={() => setOpen(false)}
      />
    </>
  )
}

export const Default: Story = {
  render: () => (
    <DialogDemo title="ステータス変更の確認" description="この製造指示を「完了」に変更しますか？" />
  ),
}

export const Destructive: Story = {
  render: () => (
    <DialogDemo
      title="削除の確認"
      description="この操作は取り消せません。本当に削除しますか？"
      variant="destructive"
      confirmLabel="削除する"
    />
  ),
}
