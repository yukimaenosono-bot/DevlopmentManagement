/**
 * UIカタログページ
 * 開発中のコンポーネント確認用。本番ビルドには含めない想定。
 * URL: /ui-catalog
 */
import { useState } from 'react'
import { PageHeader } from '@/components/composite/PageHeader'
import { StatusBadge } from '@/components/composite/StatusBadge'
import { ConfirmDialog } from '@/components/composite/ConfirmDialog'
import { DataTable } from '@/components/composite/DataTable'
import { FilterPanel } from '@/components/composite/FilterPanel'
import { MasterSelect } from '@/components/composite/MasterSelect'
import { Button, buttonVariants } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Checkbox } from '@/components/ui/checkbox'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Skeleton } from '@/components/ui/skeleton'
import { Separator } from '@/components/ui/separator'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { Pagination, PaginationContent, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from '@/components/ui/pagination'
import { AlertCircle, CheckCircle2, Info, TriangleAlert } from 'lucide-react'
import { cn } from '@/lib/utils'

// ---- セクション共通レイアウト ----------------------------------------
function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="space-y-4">
      <div>
        <h2 className="text-lg font-semibold">{title}</h2>
        <Separator className="mt-1" />
      </div>
      {children}
    </section>
  )
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <span className="w-28 shrink-0 text-xs text-muted-foreground">{label}</span>
      {children}
    </div>
  )
}

// ---- カタログ本体 -------------------------------------------------------
export function UiCatalogPage() {
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [checked, setChecked] = useState(false)
  const [radio, setRadio] = useState('a')
  const [selectVal, setSelectVal] = useState('')

  return (
    <div className="mx-auto max-w-4xl space-y-12 py-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">UIカタログ</h1>
        <p className="mt-1 text-muted-foreground">
          Synapse で使用する共通コンポーネントの一覧です。開発専用ページです。
        </p>
      </div>

      {/* ===================== 1. Button ===================== */}
      <Section title="1. Button">
        <Row label="variant">
          <Button variant="default">Default</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="destructive">Destructive</Button>
          <Button variant="link">Link</Button>
        </Row>
        <Row label="size">
          <Button size="lg">Large</Button>
          <Button size="default">Default</Button>
          <Button size="sm">Small</Button>
          <Button size="xs">XSmall</Button>
          <Button size="icon">＋</Button>
        </Row>
        <Row label="disabled">
          <Button disabled>Disabled</Button>
          <Button variant="outline" disabled>Outline</Button>
        </Row>
        <Row label="loading">
          <Button disabled>
            <span className="animate-spin mr-2">⟳</span>処理中...
          </Button>
        </Row>
        <Row label="as link">
          <a href="/ui-catalog" className={buttonVariants({ variant: 'outline' })}>
            buttonVariants on &lt;a&gt;
          </a>
        </Row>
      </Section>

      {/* ===================== 2. Input / Label ===================== */}
      <Section title="2. Input / Label">
        <Row label="default">
          <div className="w-64 space-y-1">
            <Label htmlFor="ex1">ラベル</Label>
            <Input id="ex1" placeholder="テキストを入力" />
          </div>
        </Row>
        <Row label="disabled">
          <Input className="w-64" placeholder="disabled" disabled />
        </Row>
        <Row label="error">
          <div className="w-64 space-y-1">
            <Input className="w-full" placeholder="エラー状態" aria-invalid />
            <p className="text-xs text-destructive">必須入力です</p>
          </div>
        </Row>
        <Row label="textarea">
          <Textarea className="w-64" placeholder="複数行テキスト" rows={3} />
        </Row>
      </Section>

      {/* ===================== 3. Select ===================== */}
      <Section title="3. Select">
        <Row label="default">
          <Select value={selectVal} onValueChange={setSelectVal}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="選択してください" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="a">品目A</SelectItem>
              <SelectItem value="b">品目B</SelectItem>
              <SelectItem value="c">品目C</SelectItem>
            </SelectContent>
          </Select>
          {selectVal && <span className="text-sm">選択値: {selectVal}</span>}
        </Row>
        <Row label="disabled">
          <Select disabled>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="disabled" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="x">Item</SelectItem>
            </SelectContent>
          </Select>
        </Row>
      </Section>

      {/* ===================== 4. Checkbox / Radio ===================== */}
      <Section title="4. Checkbox / RadioGroup">
        <Row label="checkbox">
          <div className="flex items-center gap-2">
            <Checkbox
              id="chk1"
              checked={checked}
              onCheckedChange={(v) => setChecked(Boolean(v))}
            />
            <Label htmlFor="chk1">チェックボックス</Label>
          </div>
          <div className="flex items-center gap-2">
            <Checkbox id="chk2" disabled />
            <Label htmlFor="chk2">Disabled</Label>
          </div>
        </Row>
        <Row label="radio">
          <RadioGroup value={radio} onValueChange={setRadio} className="flex gap-4">
            {['a', 'b', 'c'].map((v) => (
              <div key={v} className="flex items-center gap-2">
                <RadioGroupItem value={v} id={`radio-${v}`} />
                <Label htmlFor={`radio-${v}`}>選択肢{v.toUpperCase()}</Label>
              </div>
            ))}
          </RadioGroup>
        </Row>
      </Section>

      {/* ===================== 5. Badge ===================== */}
      <Section title="5. Badge（ステータスカラー）">
        <Row label="status">
          <Badge variant="outline" className="border-gray-300 text-gray-500">下書き</Badge>
          <Badge variant="outline" className="border-blue-200 bg-blue-50 text-blue-600">発行済</Badge>
          <Badge variant="outline" className="border-yellow-200 bg-yellow-50 text-yellow-600">進行中</Badge>
          <Badge variant="outline" className="border-green-200 bg-green-50 text-green-600">完了</Badge>
          <Badge variant="outline" className="border-red-200 bg-red-50 text-red-600">キャンセル</Badge>
        </Row>
        <Row label="QC result">
          <Badge variant="outline" className="border-green-200 bg-green-50 text-green-600">合格</Badge>
          <Badge variant="outline" className="border-red-200 bg-red-50 text-red-600">不合格</Badge>
          <Badge variant="outline" className="border-gray-300 text-gray-500">未処理</Badge>
        </Row>
      </Section>

      {/* ===================== 6. Alert ===================== */}
      <Section title="6. Alert">
        <Alert>
          <Info className="size-4" />
          <AlertTitle>情報</AlertTitle>
          <AlertDescription>通常の情報メッセージです。</AlertDescription>
        </Alert>
        <Alert className="border-yellow-200 bg-yellow-50 text-yellow-800 [&>svg]:text-yellow-600">
          <TriangleAlert className="size-4" />
          <AlertTitle>警告</AlertTitle>
          <AlertDescription>安全在庫を下回っている品目があります。</AlertDescription>
        </Alert>
        <Alert className="border-red-200 bg-red-50 text-red-800 [&>svg]:text-red-600">
          <AlertCircle className="size-4" />
          <AlertTitle>エラー</AlertTitle>
          <AlertDescription>在庫数量が不足しています。出庫できません。</AlertDescription>
        </Alert>
        <Alert className="border-green-200 bg-green-50 text-green-800 [&>svg]:text-green-600">
          <CheckCircle2 className="size-4" />
          <AlertTitle>成功</AlertTitle>
          <AlertDescription>入庫登録が完了しました。</AlertDescription>
        </Alert>
      </Section>

      {/* ===================== 7. Card ===================== */}
      <Section title="7. Card">
        <div className="grid grid-cols-2 gap-4">
          <Card>
            <CardHeader>
              <CardTitle>KPIカード例</CardTitle>
              <CardDescription>本日の製造完了率</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">87.5%</p>
              <p className="text-sm text-green-600">▲ 前日比 +2.3%</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>在庫アラート</CardTitle>
              <CardDescription>安全在庫を下回った品目</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold text-red-600">3件</p>
              <p className="text-sm text-muted-foreground">要補充対応</p>
            </CardContent>
          </Card>
        </div>
      </Section>

      {/* ===================== 8. Table ===================== */}
      <Section title="8. Table">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>製造指示番号</TableHead>
              <TableHead>品目コード</TableHead>
              <TableHead>数量</TableHead>
              <TableHead>ステータス</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[
              { id: 'MO-2026-001', code: 'ITEM-001', qty: 100, status: 'InProgress' },
              { id: 'MO-2026-002', code: 'ITEM-002', qty: 50, status: 'Completed' },
              { id: 'MO-2026-003', code: 'ITEM-003', qty: 200, status: 'Released' },
            ].map((row) => (
              <TableRow key={row.id} className="cursor-pointer hover:bg-muted/50">
                <TableCell className="font-medium">{row.id}</TableCell>
                <TableCell>{row.code}</TableCell>
                <TableCell>{row.qty}</TableCell>
                <TableCell>
                  <Badge
                    variant="outline"
                    className={cn(
                      row.status === 'InProgress' && 'border-yellow-200 bg-yellow-50 text-yellow-600',
                      row.status === 'Completed' && 'border-green-200 bg-green-50 text-green-600',
                      row.status === 'Released' && 'border-blue-200 bg-blue-50 text-blue-600',
                    )}
                  >
                    {row.status === 'InProgress' ? '進行中' : row.status === 'Completed' ? '完了' : '発行済'}
                  </Badge>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Section>

      {/* ===================== 9. Skeleton ===================== */}
      <Section title="9. Skeleton（ローディング）">
        <Row label="text">
          <div className="space-y-2 w-64">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
          </div>
        </Row>
        <Row label="table row">
          <div className="w-full space-y-2">
            {[1, 2, 3].map((i) => (
              <div key={i} className="flex gap-4">
                <Skeleton className="h-8 w-32" />
                <Skeleton className="h-8 w-24" />
                <Skeleton className="h-8 w-16" />
                <Skeleton className="h-8 w-20" />
              </div>
            ))}
          </div>
        </Row>
      </Section>

      {/* ===================== 10. Tabs ===================== */}
      <Section title="10. Tabs">
        <Tabs defaultValue="list">
          <TabsList>
            <TabsTrigger value="list">一覧</TabsTrigger>
            <TabsTrigger value="detail">詳細</TabsTrigger>
            <TabsTrigger value="history">履歴</TabsTrigger>
          </TabsList>
          <TabsContent value="list" className="pt-4">
            <p className="text-sm text-muted-foreground">一覧タブのコンテンツ</p>
          </TabsContent>
          <TabsContent value="detail" className="pt-4">
            <p className="text-sm text-muted-foreground">詳細タブのコンテンツ</p>
          </TabsContent>
          <TabsContent value="history" className="pt-4">
            <p className="text-sm text-muted-foreground">履歴タブのコンテンツ</p>
          </TabsContent>
        </Tabs>
      </Section>

      {/* ===================== 11. Tooltip ===================== */}
      <Section title="11. Tooltip">
        <Row label="default">
          <Tooltip>
            <TooltipTrigger asChild>
              <Button variant="outline">ホバーしてください</Button>
            </TooltipTrigger>
            <TooltipContent>ツールチップのテキスト</TooltipContent>
          </Tooltip>
        </Row>
      </Section>

      {/* ===================== 12. Pagination ===================== */}
      <Section title="12. Pagination">
        <Pagination>
          <PaginationContent>
            <PaginationItem>
              <PaginationPrevious href="#" />
            </PaginationItem>
            {[1, 2, 3, 4, 5].map((p) => (
              <PaginationItem key={p}>
                <PaginationLink href="#" isActive={p === 2}>{p}</PaginationLink>
              </PaginationItem>
            ))}
            <PaginationItem>
              <PaginationNext href="#" />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      </Section>

      {/* ===================== 13. 複合コンポーネント ===================== */}
      <Section title="13. PageHeader">
        <PageHeader
          title="製造指示一覧"
          description="製造指示の確認・発行・ステータス管理を行います"
          actions={<Button size="sm">+ 新規発行</Button>}
        />
        <PageHeader title="タイトルのみ（description・actions なし）" />
      </Section>

      <Section title="14. StatusBadge">
        <Row label="WorkOrderStatus">
          <div className="flex flex-wrap gap-2">
            {(['Draft', 'Released', 'InProgress', 'Completed', 'Cancelled'] as const).map((s) => (
              <StatusBadge key={s} status={s} />
            ))}
          </div>
        </Row>
        <Row label="QCResult">
          <div className="flex gap-2">
            {(['Pass', 'Fail', 'Pending'] as const).map((s) => (
              <StatusBadge key={s} status={s} />
            ))}
          </div>
        </Row>
        <Row label="ShipmentStatus">
          <div className="flex flex-wrap gap-2">
            {(['Pending', 'Picking', 'Shipped', 'Cancelled'] as const).map((s) => (
              <StatusBadge key={s} status={s} />
            ))}
          </div>
        </Row>
      </Section>

      <Section title="15. ConfirmDialog">
        <Row label="default">
          <Button variant="outline" onClick={() => setConfirmOpen(true)}>
            確認ダイアログを開く
          </Button>
        </Row>
        <ConfirmDialog
          open={confirmOpen}
          onOpenChange={setConfirmOpen}
          title="削除の確認"
          description="この操作は取り消せません。本当に削除しますか？"
          variant="destructive"
          confirmLabel="削除する"
          onConfirm={() => setConfirmOpen(false)}
        />
      </Section>

      <Section title="16. DataTable">
        <DataTable
          columns={[
            { key: 'id', label: 'ID', className: 'w-20' },
            { key: 'name', label: '名称' },
            { key: 'status', label: 'ステータス', className: 'w-28' },
          ]}
          data={[
            { id: 'WO-001', name: 'サンプル製品A', status: '進行中' },
            { id: 'WO-002', name: 'サンプル製品B', status: '完了' },
          ]}
        />
        <p className="text-xs text-muted-foreground">ローディング状態:</p>
        <DataTable
          columns={[
            { key: 'id', label: 'ID', className: 'w-20' },
            { key: 'name', label: '名称' },
          ]}
          data={[]}
          isLoading
        />
      </Section>

      <Section title="17. FilterPanel">
        <FilterPanel onSearch={() => {}} onReset={() => {}}>
          <div className="flex gap-2">
            <Input placeholder="キーワード検索" className="w-48" />
            <Select>
              <SelectTrigger className="w-36">
                <SelectValue placeholder="ステータス" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">すべて</SelectItem>
                <SelectItem value="active">進行中</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </FilterPanel>
      </Section>

      <Section title="18. MasterSelect">
        <Row label="通常">
          <MasterSelect
            value=""
            placeholder="品目を選択"
            options={[
              { value: 'ITEM-001', label: 'アルミブロック A' },
              { value: 'ITEM-002', label: 'スチールシャフト B' },
            ]}
            onValueChange={() => {}}
          />
        </Row>
        <Row label="isLoading">
          <MasterSelect value="" placeholder="品目を選択" options={[]} onValueChange={() => {}} isLoading />
        </Row>
      </Section>

      {/* ===================== 19. デザイントークン ===================== */}
      <Section title="19. デザイントークン（カラー）">
        <div className="grid grid-cols-4 gap-2">
          {[
            { name: 'background', cls: 'bg-background border' },
            { name: 'foreground', cls: 'bg-foreground' },
            { name: 'primary', cls: 'bg-primary' },
            { name: 'secondary', cls: 'bg-secondary border' },
            { name: 'muted', cls: 'bg-muted' },
            { name: 'accent', cls: 'bg-accent border' },
            { name: 'destructive', cls: 'bg-destructive' },
            { name: 'border', cls: 'bg-border' },
          ].map(({ name, cls }) => (
            <div key={name} className="space-y-1">
              <div className={cn('h-10 rounded-md', cls)} />
              <p className="text-xs text-muted-foreground">{name}</p>
            </div>
          ))}
        </div>
        <div className="grid grid-cols-5 gap-2 mt-2">
          {['green', 'yellow', 'red', 'blue', 'gray'].map((color) => (
            <div key={color} className="space-y-1">
              <div className={cn('h-10 rounded-md border', `bg-${color}-50 border-${color}-200`)} />
              <p className="text-xs text-muted-foreground">{color}</p>
            </div>
          ))}
        </div>
      </Section>
    </div>
  )
}
