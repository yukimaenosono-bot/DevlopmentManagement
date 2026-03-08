#!/bin/sh
set -e

export PNPM_HOME=/root/.local/share/pnpm
export PATH="$PNPM_HOME:$PATH"
pnpm config set store-dir /root/.local/share/pnpm/store

echo "==> pnpm install を実行中..."
pnpm install --frozen-lockfile=false

echo "==> Storybook を起動中..."
exec pnpm storybook
