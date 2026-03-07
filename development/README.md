# SYNAPSE

## 技術スタック

| レイヤー | 技術 |
|---------|------|
| フロントエンド | React 18 + TypeScript (Vite) |
| バックエンド | ASP.NET Core 10 Web API (C#) |
| データベース | PostgreSQL 16 (Cloud SQL) |
| インフラ | GCP (Cloud Run / Firebase Hosting) |
| CI/CD | GitHub Actions |

## ローカル起動

### 前提
- Docker Desktop
- Node.js 20 + pnpm
- .NET SDK 10

### 起動

```bash
# 全サービス起動（DB + API + Frontend）
docker compose up -d

# フロントエンドのみ開発モードで起動（HMR有効）
cd frontend && pnpm dev

# バックエンドのみ起動
cd backend && dotnet run --project src/ManufacturingSystem.API
```

### DBマイグレーション

```bash
cd backend
dotnet ef database update --project src/ManufacturingSystem.Infrastructure --startup-project src/ManufacturingSystem.API
```

## フォルダ構成

```
development/
├── frontend/          # React + TypeScript (Vite)
├── backend/           # ASP.NET Core 10 (.NET Solution)
│   ├── src/
│   │   ├── ManufacturingSystem.API/
│   │   ├── ManufacturingSystem.Application/
│   │   ├── ManufacturingSystem.Domain/
│   │   └── ManufacturingSystem.Infrastructure/
│   └── tests/
│       └── ManufacturingSystem.Tests/
├── .github/workflows/ # CI/CD (GitHub Actions)
├── docker-compose.yml # ローカル開発環境
└── README.md
```
