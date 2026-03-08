import { defineConfig } from 'orval'

export default defineConfig({
  synapse: {
    input: {
      // バックエンドビルド時に development/ へ出力される openapi.json を参照する
      target: '../openapi.json',
    },
    output: {
      // コントローラー（タグ）ごとにファイルを分割して src/generated/ へ出力する
      mode: 'tags-split',
      target: 'src/generated',
      schemas: 'src/generated/model',
      client: 'react-query',
      // MSW ハンドラーと Faker によるダミーデータ生成を有効化
      mock: true,
      override: {
        // 共通の axios インスタンスをミューテーターとして使用する
        mutator: {
          path: 'src/services/apiClient.ts',
          name: 'apiClient',
        },
        query: {
          useQuery: true,
          useSuspenseQuery: false,
          useMutation: true,
          signal: true,
        },
      },
    },
  },
  // NOTE: Zod スキーマ自動生成は orval が Zod v4 に未対応のため無効化。
  // フォームバリデーション用 Zod スキーマは features/[domain]/schemas/ に手書きで管理する。
  // zod: { ... }
})
