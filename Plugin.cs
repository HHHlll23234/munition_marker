using System;
using System.Reflection;
using AnchorChain;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MunitionMarkers
{
    /// <summary>
    /// Reveals bomb / rocket / RBU contacts on the tactical map.
    ///
    /// Sea Power hides these in <see cref="SeaPower.Vehicle.IsVisible()"/> and
    /// <c>SeaPower.Vehicle.GetMapType()</c>; the tactical map only plots entries from
    /// <c>taskforce.PlottingTable.Vehicles</c> that pass <c>IsVisible()</c>. Missiles,
    /// torpedoes and guided bombs are already visible and are not touched.
    /// </summary>
    [ACPlugin(Guid, Name, Version)]
    public class Plugin : IAnchorChainMod
    {
        public const string Guid = "io.github.hhhlll23234.munition_markers";
        public const string Name = "Munition Markers";
        public const string Version = "1.0.0";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> ShowBombs;
        internal static ConfigEntry<bool> ShowRockets;
        internal static ConfigEntry<bool> ShowRbu;
        internal static ConfigEntry<bool> ShowBothSides;

        private static ConfigFile _config;
        private static Harmony _harmony;

        public void TriggerEntryPoint()
        {
            Log = BepInEx.Logging.Logger.CreateLogSource("MunitionMarkers");
            try
            {
                LoadConfig();
                _harmony = new Harmony(Guid);

                // Register patches one class at a time: a single stale/renamed target must
                // not take down every other patch (PatchAll throws on the first failure and
                // leaves the whole mod inert).
                int patched = 0;
                int failed = 0;
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (!IsHarmonyPatchType(type))
                    {
                        continue;
                    }
                    try
                    {
                        _harmony.CreateClassProcessor(type).Patch();
                        patched++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Log.LogError($"[MunitionMarkers] Failed to patch {type.FullName}: {ex}");
                    }
                }

                Log.LogInfo($"[MunitionMarkers] loaded. Patched {patched} classes, {failed} failed. " +
                            $"bombs={ShowBombs.Value}, rockets={ShowRockets.Value}, rbu={ShowRbu.Value}, " +
                            $"bothSides={ShowBothSides.Value}");
            }
            catch (Exception ex)
            {
                Log.LogError("[MunitionMarkers] Failed to initialize: " + ex);
            }
        }

        private static bool IsHarmonyPatchType(Type type)
        {
            if (type == null)
            {
                return false;
            }
            return type.GetCustomAttributes(typeof(HarmonyPatch), inherit: false).Length > 0
                || type.GetCustomAttributes(typeof(HarmonyPatchAll), inherit: false).Length > 0;
        }

        private static void LoadConfig()
        {
            string path = System.IO.Path.Combine(Paths.ConfigPath, "MunitionMarkers.cfg");
            _config = new ConfigFile(path, saveOnInit: true);

            ShowBombs = _config.Bind(
                "Markers", "ShowBombs", true,
                "Show unguided bombs on the tactical map.");

            ShowRockets = _config.Bind(
                "Markers", "ShowRockets", true,
                "Show unguided aerial rockets on the tactical map.");

            ShowRbu = _config.Bind(
                "Markers", "ShowRbu", true,
                "Show RBU (anti-submarine rockets) on the tactical map.");

            ShowBothSides = _config.Bind(
                "Markers", "ShowBothSides", true,
                "true  = plot munitions from every taskforce.\n" +
                "false = plot only the player's own munitions.");
        }
    }
}
