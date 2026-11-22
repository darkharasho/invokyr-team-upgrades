using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace InvokyrTeamUpgrades
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGuid = "com.darkharasho.invokyr.teamupgrades";
        public const string PluginName = "InvokyrTeamUpgrades";
        public const string PluginVersion = "1.0.0";

        /// <summary>The game's upgrades as of writing, so they're in the config file before the first game.</summary>
        private static readonly string[] KnownUpgrades = { "up_health", "up_stamina", "up_speed", "up_inventory", "up_doublejump" };

        internal static new ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> SyncOnLoad;
        private static ConfigFile _config;
        private static readonly Dictionary<string, ConfigEntry<bool>> Shared = new Dictionary<string, ConfigEntry<bool>>();

        public override void Load()
        {
            Log = base.Log;
            _config = Config;
            Enabled = Config.Bind("General", "Enabled", true, new ConfigDescription(
                "Upgrades one player buys go to the whole team. Only the host needs the mod.",
                null, new ConfigurationManagerAttributes { DispName = "Enabled", Order = 2 }));
            SyncOnLoad = Config.Bind("General", "SyncOnLoad", true, new ConfigDescription(
                "When a save loads, raise every character to the highest level any teammate has in each shared upgrade (catches up saves from before the mod).",
                null, new ConfigurationManagerAttributes { DispName = "Sync on load", Order = 1 }));
            foreach (var alias in KnownUpgrades)
                IsShared(alias);

            TeamUpgrades.Apply(new Harmony(PluginGuid));
            Log.LogInfo($"{PluginName} {PluginVersion} loaded");
        }

        /// <summary>Whether an upgrade is shared ([Shared] in the config, true by default; new upgrades are added as seen).</summary>
        internal static bool IsShared(string alias)
        {
            var key = Sharing.ConfigKey(alias);
            if (key == null)
                return false;
            if (!Shared.TryGetValue(key, out var entry))
            {
                entry = _config.Bind("Shared", key, true, $"true: buying {key} upgrades the whole team. false: it stays with the buyer.");
                Shared[key] = entry;
            }
            return entry.Value;
        }
    }
}
