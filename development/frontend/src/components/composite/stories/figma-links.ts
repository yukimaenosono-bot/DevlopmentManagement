/**
 * Figma コンポーネント URL 一覧
 *
 * Figma でコンポーネントを右クリック → "Copy link to selection" でコピーした
 * URL をここに貼り付けてください。
 *
 * URL 形式:
 *   https://www.figma.com/design/<FILE_ID>/<FILE_NAME>?node-id=<NODE_ID>
 *
 * 未設定のものは undefined のまま（Storybook の "Design" タブが非表示になります）
 */
export const FIGMA = {
  PageHeader: undefined as string | undefined,
  StatusBadge: undefined as string | undefined,
  ConfirmDialog: undefined as string | undefined,
  DataTable: undefined as string | undefined,
  FilterPanel: undefined as string | undefined,
  MasterSelect: undefined as string | undefined,
} satisfies Record<string, string | undefined>
