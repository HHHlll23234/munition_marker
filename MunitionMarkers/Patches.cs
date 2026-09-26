using System;
using HarmonyLib;
using SeaPower;
using SeapowerUI;
using ObjectType = SeaPower.ObjectBase.ObjectType;

namespace MunitionMarkers
{
    internal static class MunitionRules
    {
        /// <summary>Which hidden munition categories the player currently wants plotted.</summary>
        internal static bool IsWanted(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Bomb:
                    return Plugin.ShowBombs.Value;
                case ObjectType.AerialRocket:
                    return Plugin.ShowRockets.Value;
                case ObjectType.RBU:
                    return Plugin.ShowRbu.Value;
                default:
                    return false;
            }
        }

        /// <summary>The map symbol to reuse for a munition (the XAML only has symbols for existing types).</summary>
        internal static MapVisualType SymbolFor(ObjectType type)
        {
            return type == ObjectType.RBU ? MapVisualType.Torpedo : MapVisualType.Missile;
        }

        internal static bool SideAllowed(Vehicle vehicle)
        {
            if (Plugin.ShowBothSides.Value)
            {
                return true;
            }

            var target = vehicle.BaseObject;
            return target != null && target._taskforce == Globals._playerTaskforce;
        }

        internal static bool ShouldReveal(Vehicle vehicle)
        {
            var target = vehicle?.BaseObject;
            if (target == null)
            {
                return false;
            }

            return IsWanted(target._type) && SideAllowed(vehicle);
        }
    }

    /// <summary>Un-hides the tracked munition so the tactical map plots it.</summary>
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.IsVisible))]
    internal static class Patch_Vehicle_IsVisible
    {
        private static void Postfix(Vehicle __instance, ref bool __result)
        {
            try
            {
                if (__result)
                {
                    return;
                }

                if (MunitionRules.ShouldReveal(__instance))
                {
                    __result = true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError(e);
            }
        }
    }

    /// <summary>Gives the now-visible munition a real map symbol instead of "Invisible".</summary>
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.GetMapType))]
    internal static class Patch_Vehicle_GetMapType
    {
        private static void Postfix(Vehicle __instance, ref MapVisualType __result)
        {
            try
            {
                if (__result != MapVisualType.Invisible)
                {
                    return;
                }

                if (MunitionRules.ShouldReveal(__instance))
                {
                    __result = MunitionRules.SymbolFor(__instance.BaseObject._type);
                }
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError(e);
            }
        }
    }
}
