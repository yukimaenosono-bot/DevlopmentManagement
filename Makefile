# ==============================================================================
# Synapse - 製造管理システム Makefile
# ==============================================================================

DOCKER_COMPOSE_DIR := development
DOCKER_COMPOSE     := docker compose -f $(DOCKER_COMPOSE_DIR)/docker-compose.yml
BACKEND_DIR        := $(DOCKER_COMPOSE_DIR)/backend
FRONTEND_DIR       := $(DOCKER_COMPOSE_DIR)/frontend

# カラー定義
CYAN  := \033[36m
GREEN := \033[32m
YELLOW := \033[33m
RESET := \033[0m

.PHONY: help \
        up down restart build logs ps clean fresh \
        api-build api-test api-shell \
        db-shell db-migrate db-migrate-add db-migrate-revert \
        fe-install fe-dev fe-build fe-lint fe-shell \
        check-ports

# デフォルトターゲット
.DEFAULT_GOAL := help

# ==============================================================================
# ヘルプ
# ==============================================================================
help: ## このヘルプを表示
	@echo ""
	@echo "$(CYAN)Synapse - 製造管理システム$(RESET)"
	@echo ""
	@echo "$(GREEN)[Docker]$(RESET)"
	@grep -E '^(up|down|restart|build|logs|ps|clean|fresh):.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  $(CYAN)%-20s$(RESET) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(GREEN)[バックエンド]$(RESET)"
	@grep -E '^api-[a-z-]+:.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  $(CYAN)%-20s$(RESET) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(GREEN)[データベース]$(RESET)"
	@grep -E '^db-[a-z-]+:.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  $(CYAN)%-20s$(RESET) %s\n", $$1, $$2}'
	@echo ""
	@echo "$(GREEN)[フロントエンド（ローカル）]$(RESET)"
	@grep -E '^fe-[a-z-]+:.*?## ' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "  $(CYAN)%-20s$(RESET) %s\n", $$1, $$2}'
	@echo ""

# ==============================================================================
# Docker 操作
# ==============================================================================
up: check-ports ## 全サービスをバックグラウンドで起動
	@echo "$(GREEN)Starting all services...$(RESET)"
	$(DOCKER_COMPOSE) up -d
	@echo "$(GREEN)Done! API: http://localhost:5000  Frontend: http://localhost:3000$(RESET)"

down: ## 全コンテナを停止・削除
	@echo "$(YELLOW)Stopping all services...$(RESET)"
	$(DOCKER_COMPOSE) down

restart: ## 全サービスを再起動
	$(DOCKER_COMPOSE) restart

build: ## イメージをビルド（コード変更後に実行）
	$(DOCKER_COMPOSE) build

logs: ## 全サービスのログをストリーミング表示
	$(DOCKER_COMPOSE) logs -f

logs-api: ## APIのログのみ表示
	$(DOCKER_COMPOSE) logs -f api

logs-fe: ## フロントエンドのログのみ表示
	$(DOCKER_COMPOSE) logs -f frontend

ps: ## 起動中のコンテナ一覧を表示
	$(DOCKER_COMPOSE) ps

fresh: down build up ## コンテナを停止→ビルド→起動（クリーンな再起動）

clean: ## コンテナ・ネットワーク・ボリュームを全削除（DBデータも消える）
	@echo "$(YELLOW)WARNING: This will delete all containers, networks, and volumes (including DB data).$(RESET)"
	@read -p "続けますか？ [y/N]: " confirm && [ "$$confirm" = "y" ] || (echo "Aborted." && exit 1)
	$(DOCKER_COMPOSE) down -v --rmi local

# ==============================================================================
# バックエンド（dotnet）
# ==============================================================================
api-build: ## バックエンドをビルド
	cd $(BACKEND_DIR) && dotnet build

api-test: ## バックエンドのテストを実行
	cd $(BACKEND_DIR) && dotnet test --logger "console;verbosity=normal"

api-shell: ## APIコンテナのシェルを開く
	$(DOCKER_COMPOSE) exec api /bin/sh

# ==============================================================================
# データベース（EF Core）
# ==============================================================================
db-shell: ## DBシェル（psql）を開く
	$(DOCKER_COMPOSE) exec db psql -U postgres -d manufacturing_db

db-migrate: ## 未適用のマイグレーションをDBに適用
	cd $(BACKEND_DIR)/src/Synapse.API && dotnet ef database update \
		--project ../Synapse.Infrastructure

db-migrate-add: ## マイグレーションを追加（使い方: make db-migrate-add NAME=AddXxx）
	@[ -n "$(NAME)" ] || (echo "$(YELLOW)Usage: make db-migrate-add NAME=AddXxx$(RESET)" && exit 1)
	cd $(BACKEND_DIR)/src/Synapse.API && dotnet ef migrations add $(NAME) \
		--project ../Synapse.Infrastructure
	@echo "$(YELLOW)マイグレーションファイルを確認してから apply してください$(RESET)"

db-migrate-revert: ## 直前のマイグレーションをロールバック
	@echo "$(YELLOW)WARNING: 直前のマイグレーションをロールバックします$(RESET)"
	@read -p "続けますか？ [y/N]: " confirm && [ "$$confirm" = "y" ] || (echo "Aborted." && exit 1)
	cd $(BACKEND_DIR)/src/Synapse.API && dotnet ef migrations remove \
		--project ../Synapse.Infrastructure

# ==============================================================================
# フロントエンド（ローカル実行 / pnpm）
# ==============================================================================
fe-install: ## フロントエンドの依存パッケージをインストール
	cd $(FRONTEND_DIR) && pnpm install

fe-dev: ## フロントエンド開発サーバーをローカルで起動
	cd $(FRONTEND_DIR) && pnpm dev

fe-build: ## フロントエンドを本番ビルド
	cd $(FRONTEND_DIR) && pnpm build

fe-lint: ## ESLintを実行
	cd $(FRONTEND_DIR) && pnpm lint

fe-shell: ## フロントエンドコンテナのシェルを開く（Docker使用時）
	$(DOCKER_COMPOSE) exec frontend /bin/sh

# ==============================================================================
# 内部ユーティリティ
# ==============================================================================
check-ports: ## 必要なポートの空きを確認（5000, 3000, 5432）
	@for port in 5000 3000 5432; do \
		if lsof -i :$$port -sTCP:LISTEN -t > /dev/null 2>&1; then \
			echo "$(YELLOW)WARNING: ポート $$port が使用中です。競合する可能性があります。$(RESET)"; \
		fi; \
	done
