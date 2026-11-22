using System;
using System.Collections.Generic;
using Entities.Player;
using HarmonyLib;
using Mirror;
using EPlayerCharacter = User.UserInstancesManager.EPlayerCharacter;

namespace InvokyrTeamUpgrades
{
    /// <summary>
    /// Upgrades live per character slot in <see cref="CharacterUpgradesManager"/> (one Mirror SyncList each), and the
    /// shop adds a level with <c>SrvAddUpgradeToPlayerCharacter</c>. After a level is bought, every other character is
    /// raised to the buyer's level through that same method, so the game's own sync, stat effects and saving apply.
    /// Host only: clients receive the new levels through the SyncLists.
    /// </summary>
    internal static class TeamUpgrades
    {
        private static readonly EPlayerCharacter[] Characters =
        {
            EPlayerCharacter.Red, EPlayerCharacter.Specs, EPlayerCharacter.Tank, EPlayerCharacter.Ziggy,
            EPlayerCharacter.Carl, EPlayerCharacter.Bobby, EPlayerCharacter.Susan, EPlayerCharacter.Lily,
        };

        /// <summary>Set while we add levels ourselves, so our own calls aren't shared again.</summary>
        private static bool _sharing;

        public static void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(CharacterUpgradesManager), nameof(CharacterUpgradesManager.SrvAddUpgradeToPlayerCharacter)),
                postfix: new HarmonyMethod(typeof(TeamUpgrades), nameof(AddPostfix)));
            harmony.Patch(AccessTools.Method(typeof(CharacterUpgradesManager), nameof(CharacterUpgradesManager.SrvApplySave)),
                postfix: new HarmonyMethod(typeof(TeamUpgrades), nameof(ApplySavePostfix)));
        }

        private static void AddPostfix(CharacterUpgradesManager __instance, EPlayerCharacter __0, int __1, bool __result)
        {
            if (!__result || _sharing || !Plugin.Enabled.Value || !NetworkServer.active || __instance == null || __instance._applyingSave)
                return;
            try
            {
                var upgrade = Find(__instance, __1);
                if (upgrade == null || !Plugin.IsShared(upgrade.Alias))
                    return;
                var target = __instance.GetUpgradeLevel(__0, __1);
                var raised = RaiseAll(__instance, upgrade, target);
                if (raised > 0)
                    Plugin.Log.LogInfo($"{__0} bought {upgrade.Alias} level {target}; raised {raised} other characters");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Sharing an upgrade failed: {e}");
            }
        }

        private static void ApplySavePostfix(CharacterUpgradesManager __instance)
        {
            if (!Plugin.Enabled.Value || !Plugin.SyncOnLoad.Value || !NetworkServer.active || __instance == null)
                return;
            try
            {
                var upgrades = __instance._upgrades;
                for (var i = 0; upgrades != null && i < upgrades.Count; i++)
                {
                    var upgrade = upgrades[i];
                    if (upgrade == null || !Plugin.IsShared(upgrade.Alias))
                        continue;
                    var levels = new List<int>();
                    foreach (var character in Characters)
                        levels.Add(__instance.GetUpgradeLevel(character, upgrade.UID));
                    var target = Sharing.Highest(levels);
                    if (target <= 0)
                        continue;
                    var raised = RaiseAll(__instance, upgrade, target);
                    if (raised > 0)
                        Plugin.Log.LogInfo($"Save loaded: raised {raised} characters to {upgrade.Alias} level {target}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Syncing upgrades on load failed: {e}");
            }
        }

        /// <summary>Raises every character below <paramref name="target"/> to it. Returns how many characters changed.</summary>
        private static int RaiseAll(CharacterUpgradesManager manager, CharacterUpgrade upgrade, int target)
        {
            var uid = upgrade.UID;
            var max = upgrade.MaxLevel;
            var changed = 0;
            _sharing = true;
            try
            {
                foreach (var character in Characters)
                {
                    var missing = Sharing.LevelsToAdd(manager.GetUpgradeLevel(character, uid), target, max);
                    var added = 0;
                    while (added < missing && manager.SrvAddUpgradeToPlayerCharacter(character, uid))
                        added++;
                    if (added > 0)
                        changed++;
                }
            }
            finally
            {
                _sharing = false;
            }
            return changed;
        }

        private static CharacterUpgrade Find(CharacterUpgradesManager manager, int uid)
        {
            var upgrades = manager._upgrades;
            for (var i = 0; upgrades != null && i < upgrades.Count; i++)
            {
                var upgrade = upgrades[i];
                if (upgrade != null && upgrade.UID == uid)
                    return upgrade;
            }
            return null;
        }
    }
}
