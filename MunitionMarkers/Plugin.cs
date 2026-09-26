using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MunitionMarkers
{
    /// <summary>
    /// Reveals bomb / rocket / RBU contacts on the tactical map.
    ///
    /// Sea Power hides these in <c>SeaPower.Vehicle.IsVisible()</c> and
    /// <c>SeaPower.Vehicle.GetMapType()</c>; the tactical map only plots entries
    /// from <c>taskforce.PlottingTable.Vehicles</c> that pass <c>IsVisible()</c>.
    /// Missiles, torpedoes and guided bombs are already visible - this plugin does
    /// not touch them.
    /// </summary>
    [BepInPlugin(Guid, "Munition Markers", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.hqdmi.munitionmarkers";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> ShowBombs;
        internal static ConfigEntry<bool> ShowRockets;
        internal static ConfigEntry<bool> ShowRbu;
        internal static ConfigEntry<bool> ShowBothSides;

        private void Awake()
        {
            Log = Logger;

            ShowBombs = Config.Bind(
                "General", "ShowBombs", true,
                "Show unguided bombs on the tactical map.");
            ShowRockets = Config.Bind(
                "General", "ShowRockets", true,
                "Show unguided aerial rockets on the tactical map.");
            ShowRbu = Config.Bind(
                "General", "ShowRbu", true,
                "Show RBU (anti-submarine rockets) on the tactical map.");
            ShowBothSides = Config.Bind(
                "General", "ShowBothSides", true,
                "true  = plot munitions from every taskforce.\n" +
                "false = plot only the player's own munitions.");

            var harmony = new Harmony(Guid);
            harmony.PatchAll();

            Logger.LogInfo("Munition Markers loaded.");
        }
    }
}
