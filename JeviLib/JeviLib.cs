﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BoneLib;
using BoneLib.RandomShit;
using Jevil.IMGUI;
using Jevil.Patching;
using Jevil.Tweening;
using MelonLoader;
using MelonLoader.Assertions;
using SLZ.Data;
using SLZ.Marrow.Pool;
using SLZ.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jevil;

/// <summary>
/// The JeviLib <see cref="MelonMod"/> class. There's not much of note here.
/// </summary>
public class JeviLib : MelonMod
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JeviLib"/> class.
    /// https://cdn.discordapp.com/attachments/646885826776793099/976272724324401172/IMG_2559.jpg
    /// </summary>
    public JeviLib() : base() => instance = this;
    internal static JeviLib instance;  
    internal readonly new Assembly Assembly = typeof(JeviLib).Assembly; // melonloader's favorite word is "Obsolete"

    internal static ConcurrentDictionary<string, ConcurrentBag<Assembly>> namespaceAssemblies = new();
    internal static Action onNamespaceAssembliesCompleted;

    static readonly ConcurrentQueue<string> toLog = new();
    static volatile bool somethingIsLogging;


#if DEBUG
    private readonly int GuiGap = Utilities.IsPlatformQuest() ? 15 : 5;
    private readonly int GuiCornerDist = Utilities.IsPlatformQuest() ? 350 : 20;

    private GameObject tweenTarget;
    private List<GUIToken> standardJevilTokens = new();
#endif

    /// <summary>
    /// Gets a value indicating whether the asynchronously built map of namespaces to assemblies is done being created.
    /// <para><see cref="Utilities.GetTypeFromString(string, string)"/> will fail if this is <see langword="false"/>, but <see cref="Patching.Disable.TryFromName(string, string, string, string[])"/>, which relies on GetTypeFromString, will queue an action to run when the map is finished being built.</para>
    /// </summary>
    public static bool DoneMappingNamespacesToAssemblies { get; private set; }

    /// <summary>
    /// https://media.discordapp.net/attachments/919014401187643435/958026151383691344/freeze-1.gif
    /// </summary>
    public override void OnEarlyInitializeMelon()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
#if DEBUG
        Log("This version of " + nameof(JeviLib) + " has been built with the DEBUG compiler flag!");
        Log("Functionality will remain in tact for the most part, however there will be extra log points to warn you if there is anything worrying about your usage of the library.");
        Log("You should only be using this build if you create code mods, and not if you simply use mods. Do not rely on the extra checks in this build, or require the use of a debug build for your production code.");

        this.standardJevilTokens.Add(DebugDraw.Button("Hide JeviLib tween debugger buttons", GUIPosition.TOP_RIGHT, this.ClearStandardTokens));
        this.standardJevilTokens.Add(DebugDraw.Button("Spawn cube", GUIPosition.TOP_LEFT, () => { this.tweenTarget = GameObject.CreatePrimitive(PrimitiveType.Cube); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Pos -> V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenPosition(Vector3.one, 1); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Pos -> -V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenPosition(-Vector3.one, 1); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Scl -> V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenLocalScale(Vector3.one, 1); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Scl -> V3.Zero", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenLocalScale(Vector3.zero, 1); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Rot -> Euler(V3.Zero)", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 0, 0), 1); }));
        this.standardJevilTokens.Add(DebugDraw.Button("Rot -> Euler(0,180,0)", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 180, 0), 1); }));

        this.standardJevilTokens.Add(DebugDraw.Button("PBM.CNSPU", GUIPosition.TOP_RIGHT, () => { PopupBoxManager.CreateNewShibePopup(); }));

        Stopwatch patchTimeSW = Stopwatch.StartNew();
#endif

        HarmonyInstance.PatchAll();

        Spawning.Barcodes.Init();

        MethodBase[] ctors = typeof(AssetPool).GetConstructors();

        foreach (MethodBase mb in ctors)
            Hook.OntoMethod(mb, CacheAssetPool);
#if DEBUG
        patchTimeSW.Stop();
        Log($"Took {patchTimeSW.ElapsedMilliseconds}ms to find & patch {ctors.Length} methods (constructors)", ConsoleColor.DarkGray);
#endif

        Task.Run(LogFromQueue);

        Task.Run(this.GetNamespaces);

        Hooking.OnLevelInitialized += (li) => { OnSceneWasInitialized(-1, li.barcode); };

        sw.Stop();
        this.LoggerInstance.Msg(ConsoleColor.Blue, $"Initialized {nameof(JeviLib)} v{JevilBuildInfo.Version}{(JevilBuildInfo.Debug ? " Debug (Development)" : "")} in {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Initializes 
    /// </summary>
    public override void OnInitializeMelon()
    {
        // Initialize NeverCollect/NeverCancel for generic Tweens
        GameObject go = new(nameof(NeverCollect));
        go.AddComponent<NeverCollect>();
        go.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        GameObject.DontDestroyOnLoad(go);
        Instances.NeverCancel = go;
    }

    /// <summary>
    /// Update things that are always changing, like Tweens.
    /// </summary>
    public override void OnUpdate()
    {
        Tweener.UpdateAll();
    }

    ///// <summary>
    ///// kjr
    ///// </summary>
    ///// <param name="buildIndex"></param>
    ///// <param name="sceneName"></param>
    //public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    //{
    //    Log($"OSWL CALLED, PASSING TO OSWI: PARAMS: IDX={buildIndex}, NAME={sceneName}");
    //    OnSceneWasInitialized(buildIndex, sceneName);
    //}

    /// <summary>
    /// Update references in <see cref="Instances"/>
    /// </summary>
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        Log($"OSWI CALLED: PARAMS: IDX={buildIndex}, NAME={sceneName}");
        if (!Instances.Player_RigManager.INOC()) return;

        Waiting.WaitForSceneInit.currSceneIdx = SceneManager.GetActiveScene().buildIndex;
        foreach (IDictionary item in Instances.instanceCachesToClear)
        {
            item.Clear();
        }
#if DEBUG
        Log("Cleared instance caches!");

        LemonAssert.IsFalse(Instances.NeverCancel == null, "Instances.NeverCancel == null <- should be FALSE at all times!");

        Stopwatch sw = Stopwatch.StartNew();
#endif
        // Grab the necessary references when the scene starts. 
        Instances.Player_BodyVitals =
            GameObject.FindObjectOfType<SLZ.VRMK.BodyVitals>();
        if (Instances.Player_BodyVitals.INOC())
            return;
        Instances.Player_RigManager =
            GameObject.FindObjectOfType<SLZ.Rig.RigManager>();
        Instances.Player_PhysicsRig =
            GameObject.FindObjectOfType<SLZ.Rig.PhysicsRig>();
        Instances.Player_Health =
            GameObject.FindObjectOfType<Player_Health>();
        Instances.Audio_Manager =
            GameObject.FindObjectOfType<Audio_Manager>();
        Instances.MusicMixer = //todo: get mixer names from runtime game
            Instances.Audio_Manager.audioMixer.FindMatchingGroups("Music").First();
        Instances.SFXMixer =
            Instances.Audio_Manager.audioMixer.FindMatchingGroups("SFX").First();
        // Separate cameras because it's better this way, I think. It's more distinguishable even if it requires two lines to keep the two "in sync"
        Instances.Cameras =
            GameObject.FindObjectsOfType<Camera>().Where(c => c.transform.IsChildOfRigManager()).ToArray();
        Instances.SpectatorCam =
            Instances.Cameras.First(c => c.name == "Spectator Camera");

        Transform pHead = Player.playerHead;

        GameObject musicPlayer = new("JeviLib Music Player");
        musicPlayer.transform.parent = pHead.transform;
        Instances.MusicPlayer = musicPlayer.AddComponent<AudioPlayer>();
        Instances.MusicPlayer._source = musicPlayer.AddComponent<AudioSource>();
        Instances.MusicPlayer.source.outputAudioMixerGroup = Instances.MusicMixer;
        Instances.MusicPlayer._defaultVolume = 0.1f;
        Instances.MusicPlayer.source.volume = 0.1f;
        Instances.MusicPlayer.enabled = true;

        GameObject sfxPlayer = new("JeviLib SFX Player");
        sfxPlayer.transform.parent = pHead.transform;
        Instances.SFXPlayer = sfxPlayer.AddComponent<AudioPlayer>();
        Instances.SFXPlayer._source = sfxPlayer.AddComponent<AudioSource>();
        Instances.SFXPlayer.source.outputAudioMixerGroup = Instances.SFXMixer;
        Instances.SFXPlayer._defaultVolume = 0.25f;
        Instances.SFXPlayer.source.volume = 0.25f;
        Instances.SFXPlayer.enabled = true;

        Spawning.Ammo.Init();

#if DEBUG
        Log("Found our instances in " + sw.ElapsedMilliseconds + "ms.");
#endif
    }

    static void CacheAssetPool(AssetPool newPool)
    {
        Instances.allPools.Add(newPool);
    }

#if DEBUG
    /// <summary>
    /// Draw <see cref="GUIToken"/>s from <see cref="DebugDraw"/>.
    /// </summary>
    public override void OnGUI()
    {
        try
        {
            List<GUIToken> tokens = DebugDraw.GetTokens();

            try
            {
                foreach (GUIToken tkn in tokens)
                {
                    try
                    {
                        if (tkn.type == GUIType.TRACKER) tkn.SetText(tkn.txtAlt + ": " + tkn.getter().ToString());
                    }
                    catch(Exception ex)
                    {
                        JeviLib.Error($"Exception while grabbing variable for IMGUI:\n\t\t{ex.GetType().FullName} '{ex.Message}'\n\t\t\t@ {ex.TargetSite.DeclaringType.FullName}.{ex.TargetSite.Name} (in {ex.Source})");
                    }
                }
            }
            catch (Exception ex)
            {
                JeviLib.Error($"Exception while enumerating {nameof(GUIType)}.{GUIType.TRACKER} tokens. Are you calling DebugDraw in a variable checker?");
                JeviLib.Error($"Exception information: Source = '{ex.Source}'; Exception = {ex}");
            }

            // these are only being enumerated once, so keeping them in an IEnumerable instead of ToArraying isn't a concern
            IEnumerable<GUIToken> topLeft = tokens.Where(t => t.position == GUIPosition.TOP_LEFT);
            IEnumerable<GUIToken> topRight = tokens.Where(t => t.position == GUIPosition.TOP_RIGHT);
            IEnumerable<GUIToken> bottomLeft = tokens.Where(t => t.position == GUIPosition.BOTTOM_LEFT);
            IEnumerable<GUIToken> bottomRight = tokens.Where(t => t.position == GUIPosition.BOTTOM_RIGHT);

            // Draw top left
            int maxWidth = 0;
            int xStart = GuiCornerDist;
            int yStart = GuiCornerDist;
            foreach (GUIToken token in topLeft)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart, yStart, maxWidth, token.height);
                this.DrawToken(rekt, token);
                yStart += token.height + GuiGap;
                
                if (yStart + token.height + GuiGap > Screen.height / 2)
                {
                    yStart = GuiCornerDist;
                    xStart += maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw top right
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = GuiCornerDist;
            foreach (GUIToken token in topRight)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart - maxWidth, yStart, maxWidth, token.height);
                this.DrawToken(rekt, token);
                yStart += token.height + GuiGap;

                if (yStart + token.height + GuiGap > Screen.height / 2)
                {
                    yStart = GuiCornerDist;
                    xStart -= maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw bottom left
            maxWidth = 0;
            xStart = GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            foreach (GUIToken token in bottomLeft)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart, yStart - token.height, maxWidth, token.height);
                this.DrawToken(rekt, token);
                yStart -= token.height + GuiGap;

                if (yStart - token.height - GuiGap < Screen.height / 2)
                {
                    yStart = Screen.height - GuiCornerDist;
                    xStart += maxWidth;
                    maxWidth = 0;
                }
            }

            // Draw bottom right
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            foreach (GUIToken token in bottomRight)
            {
                if (maxWidth < token.width) maxWidth = token.width;
                Rect rekt = new(xStart - maxWidth, yStart - token.height, maxWidth, token.height);
                this.DrawToken(rekt, token);
                yStart -= token.height + GuiGap;

                if (yStart - token.height - GuiGap < Screen.height / 2)
                {
                    yStart = Screen.height - GuiCornerDist;
                    xStart -= maxWidth;
                    maxWidth = 0;
                }
            }
        }
        catch (Exception ex)
        {
            Error($"Exception thrown while drawing IMGUI:\n\t{ex}");
        }
    }

    private void DrawToken(Rect rect, GUIToken token)
    {
        switch (token.type)
        {
            case GUIType.TRACKER:
            case GUIType.TEXT:
                GUI.Box(rect, token.txt);
                break;
            case GUIType.BUTTON:
                if (GUI.Button(rect, token.txt)) token.call();
                break;
            case GUIType.TEXT_BUTTON:
                token.SetText(GUI.TextField(rect, token.txt));

                Rect button = rect;
                button.width = token.txtAlt.Length * 7f + 12f;
                if (token.position == GUIPosition.TOP_LEFT || token.position == GUIPosition.BOTTOM_LEFT)
                    button.x += rect.width + GuiGap;
                else button.x -= + GuiGap + button.width;

                if (GUI.Button(button, token.txtAlt)) token.callStr(token.txt);
                break;
            default:
                break;
        }
    }
#endif

    private async void GetNamespaces()
    {
        QueueLog("Getting namespaces from assemblies now");
        Stopwatch sw = Stopwatch.StartNew();

        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        Assembly[][] assemblies = asms.SplitByProcessors().Select(e => e.ToArray()).ToArray();
#if DEBUG
        QueueLog($"Getting namespaces from " + assemblies.Sum(a => a.Length) + " assemblies, split across " + assemblies.Length + " threads");
#endif

        Thread[] threads = new Thread[assemblies.Length];

        for (int i = 0; i < threads.Length; i++)
        {
            Thread dictThread = new(this.PopulateDictionary_ThreadStart)
            {
                IsBackground = true
            };
            dictThread.Start(assemblies[i]);
            threads[i] = dictThread;
        }
#if DEBUG
        QueueLog($"Split list & started threads in {sw.ElapsedMilliseconds} ms.");
#endif
        while (threads.Any(t => t.ThreadState != System.Threading.ThreadState.Stopped)) await Task.Yield();

        sw.Stop();
        QueueLog($"Cached all {namespaceAssemblies.Count} namespaces and their respective assemblies in {sw.ElapsedMilliseconds}ms");

        DoneMappingNamespacesToAssemblies = true;
        onNamespaceAssembliesCompleted.InvokeSafeParallel();
    }

    private void PopulateDictionary_ThreadStart(object obj) => this.PopulateDictionary((Assembly[])obj);

    private void PopulateDictionary(Assembly[] section)
    {
        try
        {
            for (int i = 0; i < section.Length; i++)
            {
                Assembly currentAsm = section[i];
                if (currentAsm.IsDynamic) continue; // avoid exceptions from Redirect and Hooking
                Type[] types = currentAsm.GetTypes();

                IEnumerable<string> namespaces = types.Select(t => t.Namespace).Distinct();

                foreach (string ns in namespaces)
                {
                    ConcurrentBag<Assembly> asms;
                    if (!namespaceAssemblies.TryGetValue(ns ?? "", out asms))
                    {
                        asms = new ConcurrentBag<Assembly>();
                        namespaceAssemblies.TryAdd(ns ?? "", asms);
                    }
                    asms.Add(currentAsm);
#if DEBUG
                    // dont need this log statement anymore, namespace collection works fine.
                    //QueueLog($"Associating namespace {ns ?? "<none>"} with assembly {currentAsm.GetName().Name}. It is associated with {asms.Count} assembly(ies) now.");
#endif
                }
            }
        }
        catch (Exception ex)
        {
            Error(ex);
        }
    }

    private static IEnumerable<MethodInfo> GetLogMethods()
    {
        IEnumerable<MethodInfo> logMethods = new List<MethodInfo>();
        string[] methodnames = { nameof(MelonLogger.Msg), nameof(MelonLogger.Warning), nameof(MelonLogger.Error), };
        Type[] types = { typeof(MelonLogger), typeof(MelonLogger.Instance) };

        foreach (Type type in types)
        {
            foreach (string method in methodnames)
            {
                logMethods = logMethods.Concat(type.GetMethods(method));
            }
        }
        return logMethods;
    }

    // this things existence is because i didnt want to interrupt another log call and i didnt want to be tied to Unity's main thread, so here we are, checks and locks, or i think thats a principle of asynchronous code
    private static async void LogFromQueue()
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
#endif
        foreach (MethodInfo method in GetLogMethods())
        {
            Redirect.FromMethod(method, LogPre, false);
            Hook.OntoMethod(method, LogPost);
        }

#if DEBUG
        sw.Stop();
        QueueLog($"Took {sw.ElapsedMilliseconds}ms to patch everything.");
#endif
        QueueLog("Started async logger thread");
        while (true)
        {
            await Task.Delay(1);
            while (!somethingIsLogging && toLog.TryDequeue(out string strLog))
            {
                Log(strLog);
            }
        }
    }

    // these *will* add execution time to every call to any melonmod logger, but given the fact that all they do is set a boolean, i think im fine
    private static void LogPre() => somethingIsLogging = true;

    private static void LogPost() => somethingIsLogging = false;

    #region MelonLogger replacements

    internal static void Log(string str, ConsoleColor conCol = ConsoleColor.Gray) => instance.LoggerInstance.Msg(conCol, str);
    internal static void Log(object obj, ConsoleColor conCol = ConsoleColor.Gray) => instance.LoggerInstance.Msg(conCol, obj?.ToString() ?? "null");
    internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
    internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
    internal static void Error(string str) => instance.LoggerInstance.Error(str);
    internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");

    #endregion

    /// <summary>
    /// Enqueues a statement to be logged when the MelonLoader console window is free.
    /// <para>Intended to be used off the main thread.</para>
    /// </summary>
    /// <param name="str">The string to be logged.</param>
    internal static void QueueLog(string str) => toLog.Enqueue(str);

#if DEBUG
    private void ClearStandardTokens()
    {
        foreach (GUIToken token in this.standardJevilTokens)
        {
            DebugDraw.Dont(token);
        }

        this.standardJevilTokens.Clear();
    }

    private void NotifiedLowRAM()
    {
        JeviLib.Warn("BONELAB has been notified that your system has very little free RAM left! Double check that! Consider uninstalling some items!");
    }
#endif
}