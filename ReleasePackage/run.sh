#!/bin/bash

set -euo pipefail

project_base="$1"
jar_file="$2"

pip --quiet install aiohttp

pkill --signal TERM --full run_sim.py || true

exec python3 run_sim.py "$project_base" "$jar_file"