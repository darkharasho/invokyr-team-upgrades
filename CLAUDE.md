## Project Context

A BepInEx 6 (BepInEx.Unity.IL2CPP, be.755) mod for Invokyr (Unity 6000.3, IL2CPP, Mirror): upgrades bought by one player go to the whole team. Game code is in `vhs.dll`. Packaged Thunderstore-style under `darkharasho` and released on GitHub. Build setup is shared with `invokyr-admin-menu` (see its CLAUDE.md for refs, packaging and the flat zip layout HML needs).

## How it works (`TeamUpgrades.cs`)
- `Entities.Player.CharacterUpgradesManager` keeps upgrades per character slot (`EPlayerCharacter` Red..Lily, one `SyncList<Vector2Int>` each; Vector2Int is uid, level). The shop calls `SrvAddUpgradeToPlayerCharacter(character, uid)`, one level per call.
- Postfix on that method: when it succeeded and we aren't already sharing (re-entrancy guard) or loading (`_applyingSave`), raise every other character to the buyer's level by calling the same method, capped at `CharacterUpgrade.MaxLevel`.
- Postfix on `SrvApplySave`: with `SyncOnLoad`, raise each shared upgrade to the highest level across characters.
- `[Shared] <alias>` config per upgrade, bound lazily (`Plugin.IsShared`); the five known aliases are bound at load.
- Host only (`NetworkServer.active`). The decompile has no method bodies, so check the log ("raised N other characters") when testing.

## Building
`scripts/package.sh` → `dist/darkharasho-InvokyrTeamUpgrades-<version>.zip`; `hml install nexus-invokyr <zip> --profile Dev`. Tests: `dotnet test tests/InvokyrTeamUpgrades.Tests`. Run builds with `nice -n 19 taskset -c 0-3`. Release: bump `Plugin.cs`, csproj, manifest, CHANGELOG; push tag `v<version>`.
