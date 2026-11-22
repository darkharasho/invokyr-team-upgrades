#!/usr/bin/env bash
# Copies the game's interop assemblies (BepInEx generates them in <game>/BepInEx/interop on launch) and the BepInEx
# core into ~/.cache/invokyr, then regenerates lib/refs: reference-only copies (public API, no method bodies) so CI can
# build without the game. Rerun after a game update, once BepInEx has regenerated the interop.
# Needs refasmer: dotnet tool install -g JetBrains.Refasmer.CliTool
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
project="$(basename "$(ls -d "$root"/src/*/ | head -1)")"
game="${GAME_PATH:-/var/mnt/data/SteamLibrary/steamapps/common/Invokyr}"
cache="${CACHE_PATH:-$HOME/.cache/invokyr}"
out="$root/lib/refs"
export DOTNET_ROOT="${DOTNET_ROOT:-/home/linuxbrew/.linuxbrew/opt/dotnet/libexec}" DOTNET_ROLL_FORWARD=Major

if [ -d "$game/BepInEx/interop" ]; then
  rm -rf "$cache/interop" "$cache/core"
  mkdir -p "$cache"
  cp -r "$game/BepInEx/interop" "$cache/interop"
  cp -r "$game/BepInEx/core" "$cache/core"
fi

# Every <Reference> in the csproj, from whichever folder has it.
mapfile -t names < <(grep -oP '<Reference Include="\K[^"]+' "$root/src/$project/$project.csproj")
rm -rf "$out"
mkdir -p "$out"
for name in "${names[@]}"; do
  for dir in "$cache/interop" "$cache/core"; do
    if [ -f "$dir/$name.dll" ]; then
      # Some interop DLLs (UnityEngine.CoreModule) make refasmer fail when it drops non-API members; keep them then.
      nice -n 19 taskset -c 0-3 ~/.dotnet/tools/refasmer -q -c --omit-non-api-members=true -O "$out" "$dir/$name.dll" > /dev/null 2>&1 || true
      [ -f "$out/$name.dll" ] || nice -n 19 taskset -c 0-3 ~/.dotnet/tools/refasmer -q -c --omit-non-api-members=false -O "$out" "$dir/$name.dll"
      [ -f "$out/$name.dll" ] || { echo "refasmer failed on $name.dll" >&2; exit 1; }
      break
    fi
  done
done
ls -l "$out"
