# Invokyr Team Upgrades

Upgrades one player buys go to the whole team. When someone buys a level of health, stamina, speed, inventory or double jump, every other character is raised to the same level.

## How it works
- Every character slot is raised, including ones nobody is playing, so a player who joins later starts with the team's upgrades.
- Characters already above the buyer's level are left alone, and nobody goes past an upgrade's max level.
- **Sync on load**: when a save loads, each shared upgrade is raised to the highest level anyone has. This catches up saves from before the mod.
- The money is spent once, by the buyer.

## Settings
In `BepInEx/config/com.darkharasho.invokyr.teamupgrades.cfg`:
- `[General] Enabled` and `SyncOnLoad`.
- `[Shared]` has one entry per upgrade (`up_health`, `up_stamina`, `up_speed`, `up_inventory`, `up_doublejump`, and any new ones once they're seen). `true` shares it with the team, `false` keeps it with the buyer.

## Notes
- Only the host needs the mod. Upgrades are kept by the host's game and sent to everyone, so teammates without it still get them.
- Needs BepInEx 6 for IL2CPP (be.755 or newer).
