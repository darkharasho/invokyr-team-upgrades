#!/usr/bin/env bash
# Builds a Release DLL and zips a Thunderstore-style package into dist/. The layout is flat (manifest.json, icon.png,
# README.md and the DLL at the root): HML treats Invokyr as a game-folder (Nexus) game and copies a package as-is, except
# that a flat BepInEx plugin goes to BepInEx/plugins/<mod>/. A plugins/ folder would land in the game folder instead.
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
project="$(basename "$(ls -d "$root"/src/*/ | head -1)")"
manifest="$root/thunderstore/manifest.json"
stage="$root/dist/stage"

# Validate the manifest and check the version matches the plugin, the project and the changelog.
read -r name version < <(python3 - "$manifest" "$root" "$project" <<'PY'
import json, re, sys, pathlib
m = json.load(open(sys.argv[1]))
root = pathlib.Path(sys.argv[2]); project = sys.argv[3]
errors = []
if not re.fullmatch(r"[A-Za-z0-9_]+", m.get("name", "")):
    errors.append("name must be letters, digits and underscores")
if not re.fullmatch(r"\d+\.\d+\.\d+", m.get("version_number", "")):
    errors.append("version_number must be Major.Minor.Patch")
if len(m.get("description", "")) > 250:
    errors.append("description is over 250 characters")
v = m.get("version_number")
plugin = re.search(r'PluginVersion = "([^"]+)"', (root / f"src/{project}/Plugin.cs").read_text())
csproj = re.search(r"<Version>([^<]+)</Version>", (root / f"src/{project}/{project}.csproj").read_text())
if not plugin or plugin.group(1) != v:
    errors.append(f"Plugin.cs PluginVersion is {plugin and plugin.group(1)}, manifest is {v}")
if not csproj or csproj.group(1) != v:
    errors.append(f"{project}.csproj Version is {csproj and csproj.group(1)}, manifest is {v}")
if f"## {v}" not in (root / "CHANGELOG.md").read_text():
    errors.append(f"CHANGELOG.md has no '## {v}' entry")
if errors:
    sys.exit("manifest check failed:\n  " + "\n  ".join(errors))
print(m["name"], v)
PY
)
zipfile="$root/dist/darkharasho-$name-$version.zip"

dotnet build "$root/src/$project/$project.csproj" -c Release -m:2

rm -rf "$stage" "$zipfile"
mkdir -p "$stage"
cp "$manifest" "$root/thunderstore/icon.png" "$root/thunderstore/README.md" "$root/CHANGELOG.md" "$stage/"
cp "$root/src/$project/bin/Release/$project.dll" "$stage/"

(cd "$stage" && zip -qX "$zipfile" manifest.json icon.png README.md CHANGELOG.md "$project.dll")
rm -rf "$stage"
unzip -l "$zipfile"
echo "Package: $zipfile"
