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
})
