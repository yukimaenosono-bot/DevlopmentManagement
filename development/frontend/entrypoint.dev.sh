#!/bin/sh
set -e

# pnpm ストアをプロジェクトディレクトリ外に置く（.pnpm-store/ が /app に作られるのを防ぐ）
export PNPM_HOME=/root/.local/share/pnpm
export PATH="$PNPM_HOME:$PATH"
pnpm config set store-dir /root/.local/share/pnpm/store

echo "==> pnpm install を実行中..."
pnpm install --frozen-lockfile=false

echo "==> Vite 開発サーバーを起動中..."
exec pnpm dev --host 0.0.0.0 --port 3000
