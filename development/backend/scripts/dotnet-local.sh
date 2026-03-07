#!/usr/bin/env bash
set -euo pipefail

DOTNET_ROOT="${HOME}/.dotnet-clean"
DOTNET_CLI_HOME="${HOME}/.dotnet-cli-clean"

export DOTNET_ROOT
export DOTNET_CLI_HOME
export DOTNET_MULTILEVEL_LOOKUP=0
export PATH="${DOTNET_ROOT}:${PATH}"

exec "${DOTNET_ROOT}/dotnet" "$@"
