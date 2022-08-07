using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Jevil.Patching;

/// <summary>
/// Harmony for custom maps if you don't want to make your own Harmony instance.
/// </summary>
public static class CMaps
{
    private static readonly HarmonyLib.Harmony _cmapHarmony = new("Jevilib.CustomMapHarmony");
    /// <summary>
    /// A harmony instance for custom maps.
    /// </summary>
    public static HarmonyLib.Harmony CMHarmony => _cmapHarmony;


    static readonly MethodInfo unloadMethod = typeof(AssetBundle).GetMethod(nameof(AssetBundle.Unload), Const.AllBindingFlags);
    static FieldInfo mapBundleInfo;

    // DO NOT RUN THIS CODE UNLESS THERE IS EVER A REASON TO DO SO
    // THAT MEANS: NOT UNTIL SOMEONE PROVES DOING THIS FIXES VIDEOPLAYERS
    internal static void FixUnload()
    {
        mapBundleInfo = Utilities.GetTypeFromString("CustomMaps", "MapLoader")?.GetField("mapBundle");
        MethodInfo psp = Utilities.GetMethodFromString("CustomMaps", "MapLoader", "PostScenePass");
        if (psp == null) return;
        Redirect.FromMethod(psp, PostScenePassHook);
        Disable.WhenCalledFrom(unloadMethod, psp);
    }

    static void PostScenePassHook()
    {
        JeviLib.Log("CustomMaps PostScenePass redirections was called!");
        AssetBundle mapBundle = (AssetBundle)mapBundleInfo.GetValue(null);
        MelonLoader.MelonCoroutines.Start(CoUnload(mapBundle));
    }

    static IEnumerator CoUnload(AssetBundle toUnload)
    {
        yield return null;
        yield return null;
        toUnload.Unload(false);
    }
}
