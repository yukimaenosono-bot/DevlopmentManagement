# CLAUDE.md — 製造管理システム（Synapse）

## プロジェクト概要

製造業向けの生産管理システム（MES）。製造指示・生産計画・在庫・品質・工程・出荷を一元管理する。

## 技術スタック

### バックエンド
- **言語/フレームワーク**: .NET 10 / ASP.NET Core (Synapse.API)
- **アーキテクチャ**: クリーンアーキテクチャ（Domain / Application / Infrastructure / API）
- **主要ライブラリ**: MediatR（CQRS）、AutoMapper、EF Core、Dapper、Serilog、Hangfire、JWT認証、SignalR
- **DB**: PostgreSQL
- **テスト**: xUnit、Moq

### フロントエンド
- **言語/フレームワーク**: React 19 + TypeScript + Vite
- **パッケージマネージャ**: pnpm

### インフラ
- **ローカル**: Docker Compose（`development/docker-compose.yml`）
- **本番**: GCP

## ディレクトリ構成

```
DevlopmentManagement/
├── development/
│   ├── backend/
│   │   ├── src/
│   │   │   ├── Synapse.Domain/        # エンティティ・値オブジェクト・ドメインロジック
│   │   │   ├── Synapse.Application/   # ユースケース（CQRS: Commands/Queries）
│   │   │   ├── Synapse.Infrastructure/ # DB・外部サービス実装
│   │   │   └── Synapse.API/           # コントローラ・DI設定・エントリポイント
│   │   └── tests/
│   │       └── Synapse.Tests/
│   └── frontend/                      # React アプリ
├── requirements/                      # 要件定義書
├── basic-design/                      # 基本設計書（画面・ER図・インフラ）
├── detailed-design/                   # 詳細設計書（API仕様・テスト仕様）
└── Makefile                           # Docker操作コマンド
```

## よく使うコマンド

```bash
# Docker操作
make up           # 全サービス起動
make down         # 全サービス停止
make build        # イメージビルド
make logs         # ログ表示
make shell-api    # APIコンテナシェル
make shell-db     # DBシェル（psql）

# バックエンド（development/backend/）
dotnet build      # ビルド
dotnet test       # テスト実行

# フロントエンド（development/frontend/）
pnpm install      # 依存インストール
pnpm dev          # 開発サーバ起動
pnpm build        # 本番ビルド
pnpm lint         # ESLint実行
```

## コーディング規約

### バックエンド共通
- **クリーンアーキテクチャ厳守**: ドメイン層は他レイヤーに依存しない
- **CQRS**: 読み取りはQuery、書き込みはCommand。MediatRで実装
- **命名**: クラス名はPascalCase、メソッド名はPascalCase、変数はcamelCase
- **Nullable有効**: `null`非許容型を基本とし、`?`は明示的に使用
- **例外**: ドメイン例外は`Synapse.Domain`に定義し、HTTPステータスへのマッピングはAPIレイヤーで行う
- **ログ**: Serilog使用。構造化ログを書く（文字列補間禁止）

### フロントエンド共通
- **TypeScript strict**: 型を省略しない
- **コンポーネント**: 関数コンポーネント + Hooks を使用
- **ESモジュール**: CommonJS (`require`) は使用しない

## ドメイン用語対照

| 日本語 | 英語（コード上の表記） |
|--------|----------------------|
| 製造指示 | WorkOrder / ManufacturingOrder (MO) |
| 生産計画 | ProductionPlan (PP) |
| 工程 | Process / Operation |
| ルーティング | Routing |
| BOM（部品表） | BillOfMaterials |
| ロット | Lot |
| 引当 | Allocation |
| 棚卸 | PhysicalInventory |
| 特採 | ConditionalAcceptance |
| 安全在庫 | SafetyStock |
| 出荷指示 | ShipmentOrder (SH) |
| 品質管理 | QualityControl (QC) |

用語の詳細: `requirements/07_用語集.md`

## ドキュメント索引

| ドキュメント | パス |
|-------------|------|
| 機能一覧 | `basic-design/03_機能一覧.md` |
| 画面遷移・詳細 | `basic-design/01_画面遷移図.md`, `basic-design/02_画面詳細.md` |
| ER図 | `basic-design/04_ER図.md` |
| データフロー | `basic-design/05_データフロー図.md` |
| API仕様 | `detailed-design/05_API仕様書.md` |
| テスト仕様 | `detailed-design/06_テスト仕様書.md` |
| バックエンドテスト方針 | `detailed-design/09_バックエンドテスト方針.md` |
| 機能要件 | `requirements/03_機能要件/` |
| 非機能要件 | `requirements/04_非機能要件.md` |
| データ要件 | `requirements/06_データ要件.md` |

## 重要な注意事項

- **DBマイグレーション**: EF Core Migrations を使用。`dotnet ef migrations add` でマイグレーション追加後、必ず内容を確認してからApplyする
- **認証**: JWTトークン認証。シークレットキーは環境変数から取得し、コードにハードコードしない
- **リアルタイム通信**: 工程進捗表示にSignalRを使用（`PROC-003`）
- **バックグラウンドジョブ**: Hangfireで管理。長時間処理はジョブとして実装する
- **テスト**: 新規ユースケース（Command/Query）には必ずxUnitテストを追加する
