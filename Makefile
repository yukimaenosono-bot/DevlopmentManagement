# Variables
DOCKER_COMPOSE_DIR := development
DOCKER_COMPOSE := docker compose -f $(DOCKER_COMPOSE_DIR)/docker-compose.yml

.PHONY: help up down restart build logs ps shell-api shell-db shell-frontend db-migrate clean

help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

up: ## Start all services in background
	$(DOCKER_COMPOSE) up -d

down: ## Stop and remove all containers
	$(DOCKER_COMPOSE) down

restart: ## Restart all services
	$(DOCKER_COMPOSE) restart

build: ## Build or rebuild services
	$(DOCKER_COMPOSE) build

logs: ## Show logs for all services
	$(DOCKER_COMPOSE) logs -f

ps: ## List running containers
	$(DOCKER_COMPOSE) ps

shell-api: ## Open a shell in the API container
	$(DOCKER_COMPOSE) exec api /bin/sh

shell-db: ## Open a shell in the DB container
	$(DOCKER_COMPOSE) exec db psql -U postgres -d manufacturing_db

shell-frontend: ## Open a shell in the frontend container
	$(DOCKER_COMPOSE) exec frontend /bin/sh

clean: ## Remove containers, networks, and volumes
	$(DOCKER_COMPOSE) down -v --rmi local
