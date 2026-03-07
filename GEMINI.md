# プロジェクトのルールとコンテキスト: DevlopmentManagement (Synapse)

## プロジェクト概要
製造管理システム（Synapse）。Reactフロントエンド、.NET APIバックエンド、PostgreSQLデータベースで構成されています。

## 技術スタック
- **フロントエンド:** React (TypeScript), Vite, Tailwind CSS, Shadcn UI.
- **バックエンド:** .NET 8/9, C#, ASP.NET Core API.
- **データベース:** PostgreSQL 16.
- **インフラ:** ローカル開発用に Docker Compose を使用。

## 開発フロー
- **Docker 管理:** すべての Docker 操作にはルートディレクトリの `Makefile` を使用してください。
  - `make up`: すべてのサービスを起動。
  - `make down`: サービスを停止。
  - `make logs`: ログを表示。
- **ディレクトリ構造:**
  - `development/frontend`: React アプリケーション。
  - `development/backend`: .NET ソリューションおよびプロジェクト。
  - `development/docker-compose.yml`: メインのオーケストレーション。

## コーディング標準
- バックエンドには慣用的な C# パターンを使用してください。
- フロントエンドには関数コンポーネントとフックを使用してください。
- `replace` ツールを使用して、ピンポイントな修正を優先してください。
- 変更を加えた後は、必ず `Makefile` または関連するテストを実行して検証してください。
