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

    /// <summary>
    /// The <see cref="AssetBundle"/> of the most recently loaded Custom Map.
    /// <para>This will be <see langword="null"/> if more than 90 frames have passed since any map has loaded.</para>
    /// </summary>
    public static AssetBundle recentlyLoadedMap;
    /// <summary>
    /// Informs JeviLib that it should not unload the map bundle after 90 frames elapses.
    /// <para>If you set this to <see langword="true"/>, you must call <see cref="AssetBundle.Unload(bool)"/> on <see cref="recentlyLoadedMap"/></para>
    /// </summary>
    public static bool dontUnload;

    static readonly MethodInfo unloadMethod = typeof(AssetBundle).GetMethod(nameof(AssetBundle.Unload), Const.AllBindingFlags);
    static FieldInfo mapBundleInfo;
    static bool isPSPExec;

    // DO NOT RUN THIS CODE UNLESS THERE IS EVER A REASON TO DO SO
    // THAT MEANS: NOT UNTIL SOMEONE PROVES DOING THIS FIXES VIDEOPLAYERS
    internal static void FixUnload()
    {
        mapBundleInfo = Utilities.GetTypeFromString("CustomMaps", "MapLoader")?.GetField("mapBundle");
        MethodInfo psp = Utilities.GetMethodFromString("CustomMaps", "MapLoader", "PostScenePass");
        if (psp == null) return;

        Redirect.FromMethod(psp, PrePSP);
        Hook.OntoMethod(psp, PostPSP);
        Disable.When(() => isPSPExec, unloadMethod);
    }

    static void PostPSP()
    {
        isPSPExec = false;
    }

    static void PrePSP()
    {
        isPSPExec = true;
        JeviLib.Log("CustomMaps PostScenePass redirection was called!");
        AssetBundle mapBundle = (AssetBundle)mapBundleInfo.GetValue(null);
        recentlyLoadedMap = mapBundle;
        MelonLoader.MelonCoroutines.Start(CoUnload());
        JeviLib.Log("Going to attempt to unload " + mapBundle.name + " in 90 frames");
    }

    static IEnumerator CoUnload()
    {
        for (int i = 0; i < 90; i++)
            yield return null;
        
        if (dontUnload)
        {
            JeviLib.Log("Not unloading map bundle because it was requested. Assuming another mod or the map itself will unload the bundle.");
            dontUnload = false;
        }
        else
        {
            JeviLib.Log("Now unloading: " + recentlyLoadedMap.name);
            recentlyLoadedMap.Unload(false);
            recentlyLoadedMap = null;
        }
    }
}
