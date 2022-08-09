using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        if (!MapUnloadWatcher.waitingToUnload && MapWantsBundlePersist()) 
            MapUnloadWatcher.WaitToUnloadBundle();
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
        }
        else
        {
            JeviLib.Log("Now unloading: " + recentlyLoadedMap.name);
            recentlyLoadedMap.Unload(false);
        }

        dontUnload = false;
        recentlyLoadedMap = null;
    }

    static bool MapWantsBundlePersist()
    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
        return SceneManager.GetActiveScene()
                           .GetRootGameObjects()
                           .Any(rgo => rgo.GetComponent<LocalizedText>()?.key == "JEVILIB_PERSIST_BUNDLE");
#pragma warning restore UNT0008 // Null propagation on Unity objects
    }

    /// <summary>
    /// Informs JeviLib when a scene is unloaded, should that scene be a custom map.
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class MapUnloadWatcher : MonoBehaviour
    {
        /// <summary>
        /// Used by IL2CPP to create a new instance of <see cref="MapUnloadWatcher"/>
        /// </summary>
        /// <param name="ptr"></param>
        public MapUnloadWatcher(IntPtr ptr) : base(ptr) { }

        private static AssetBundle currBundle;
        internal static bool waitingToUnload;

        void OnDestroy()
        {
#if DEBUG
            JeviLib.Log("Custom map scene was unloaded! Unloading its bundle now!");
#endif
            currBundle.Unload(false);
            currBundle = null;
            waitingToUnload = false;
        }

        /// <summary>
        /// Informs JeviLib of an intent to persist the map bundle until the scene unloads.
        /// <para>Calling this avoids the few ms it takes to find a <see cref="LocalizedText"/> with the text "JEVILIB_PERSIST_BUNDLE".</para>
        /// </summary>
        public static void WaitToUnloadBundle()
        {
#if DEBUG
            JeviLib.Log("The currently loaded map wishes to persist until the scene is unloaded.");
#endif
            waitingToUnload = true;
            GameObject gobi = new GameObject("JeviLib Map Unload Watcher");
            gobi.AddComponent<MapUnloadWatcher>();
            currBundle = recentlyLoadedMap;
            dontUnload = true;
        }
    }

}
