using System;
using System.Collections.Generic;

namespace InvokyrTeamUpgrades
{
    /// <summary>The arithmetic of sharing, kept apart from the game so it can be tested.</summary>
    internal static class Sharing
    {
        /// <summary>Levels to add to a character at <paramref name="current"/> to reach <paramref name="target"/>, never past the upgrade's max.</summary>
        public static int LevelsToAdd(int current, int target, int maxLevel)
        {
            var goal = maxLevel > 0 ? Math.Min(target, maxLevel) : target;
            return Math.Max(0, goal - current);
        }

        public static int Highest(IEnumerable<int> levels)
        {
            var best = 0;
            foreach (var level in levels)
                best = Math.Max(best, level);
            return best;
        }

        /// <summary>A config key for an upgrade alias: BepInEx keys can't hold = \n \t \ " ' [ ].</summary>
        public static string ConfigKey(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return null;
            var chars = alias.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if ("=\n\t\\\"'[]".IndexOf(chars[i]) >= 0)
                    chars[i] = '_';
            }
            return new string(chars);
        }
    }
}
