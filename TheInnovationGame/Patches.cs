using System;
using System.Collections.Generic;
using HarmonyLib;

public class RandManager
{
    public static Dictionary<int, int> seeds = new Dictionary<int, int>();
    public static Dictionary<int, CrossPlatformRandom> instances = new Dictionary<int, CrossPlatformRandom>();

    public static CrossPlatformRandom getInstance(int id)
    {
        if (!instances.ContainsKey(id))
            instances.Add(id, new CrossPlatformRandom(seeds[id]));
        return instances[id];
    }
    public static Harmony harmony = new Harmony("TheInnovationGameMod");
    public static void ApplyPatches()
    {
        if(!Harmony.HasAnyPatches("TheInnovationGameMod"))
            harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(System.Random))]
class Patches
{
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { })]
    static void Postfix(System.Random __instance)
    {
        var id = __instance.GetHashCode();
        if (!RandManager.seeds.ContainsKey(id))
            RandManager.seeds.Add(id, id);
    }

    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(int) })]
    static void Postfix(System.Random __instance, object[] __args)
    {
        var id = __instance.GetHashCode();
        if (!RandManager.seeds.ContainsKey(id))
            RandManager.seeds.Add(id, (int)__args[0]);
    }

    [HarmonyPatch("Next")]
    [HarmonyPatch(new Type[] { typeof(int) })]
    static bool Prefix(System.Random __instance, ref int __result, int maxValue)
    {
        __result = RandManager.getInstance(__instance.GetHashCode()).Next(maxValue);
        return false;
    }

    [HarmonyPatch("Next")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
    static bool Prefix(System.Random __instance, ref int __result, int minValue, int maxValue)
    {
        __result = RandManager.getInstance(__instance.GetHashCode()).Next(minValue, maxValue);
        return false;
    }

    [HarmonyPatch("NextBytes")]
    [HarmonyPatch(new Type[] { typeof(byte[]) })]
    static bool Prefix(System.Random __instance, byte[] buffer)
    {
        RandManager.getInstance(__instance.GetHashCode()).NextBytes(buffer);
        return false;
    }

    [HarmonyPatch("NextDouble")]
    [HarmonyPatch(new Type[] { })]
    static bool Prefix(System.Random __instance, ref double __result)
    {
        __result = RandManager.getInstance(__instance.GetHashCode()).NextDouble();
        return false;
    }
}
