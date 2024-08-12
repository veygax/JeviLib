using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BoneLib;
using BoneLib.RandomShit;
using Cysharp.Threading.Tasks;
using Jevil.IMGUI;
using Jevil.Patching;
using Jevil.Prefs;
using Jevil.Spawning;
using Jevil.Tweening;
using MelonLoader;
using MelonLoader.Assertions;
using Il2CppSLZ.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow;
using UnityEngine.Playables;

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
    internal static event Action onUpdateCallback;
    internal static event Action onNamespaceAssembliesCompleted;
    internal static int unityMainThread;

    static readonly ConcurrentQueue<string> toLog = new();
    static Stopwatch mainThreadInvokeTimer = new();

#if DEBUG
    private readonly int GuiGap = Utilities.IsPlatformQuest() ? 15 : 5;
    private readonly int GuiCornerDist = Utilities.IsPlatformQuest() ? 350 : 20;


    // 0     1
    // 2     3
    private int[] pagination = new int[4]; // TL, TR, BL, BR
    private GUIToken[] paginateTokens = new GUIToken[12];
    private bool[] paginates = new bool[] { true, true, true, true };
    private int[] pageIdx = new int[4];

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
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Remove JeviLib Debug tokens", GUIPosition.TOP_RIGHT, this.ClearStandardTokens));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Spawn cube", GUIPosition.TOP_LEFT, () => { this.tweenTarget = GameObject.CreatePrimitive(PrimitiveType.Cube); this.tweenTarget.GetComponent<Renderer>().material.shader = Shader.Find(Const.UrpLitName); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Pos -> V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenPosition(Vector3.one, 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Pos -> -V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenPosition(-Vector3.one, 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Scl -> V3.One", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenLocalScale(Vector3.one, 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Scl -> V3.Zero", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenLocalScale(Vector3.zero, 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Rot -> Euler(V3.Zero)", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 0, 0), 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Rot -> Euler(0,180,0)", GUIPosition.TOP_LEFT, () => { this.tweenTarget.transform.TweenRotation(Quaternion.Euler(0, 180, 0), 1); }));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Test spawning", GUIPosition.TOP_LEFT, TestSpawning));
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("Test UniTask async", GUIPosition.TOP_LEFT, TestUniTaskAsync));
        
        this.standardJevilTokens.Add(Jevil.IMGUI.DebugDraw.Button("PBM.CNSPU", GUIPosition.TOP_RIGHT, () => { PopupBoxManager.CreateNewShibePopup(); }));

        for (int i = 0; i < paginateTokens.Length / 3; i++)
        {
            int _i = i;
            paginateTokens[i * 3] = new GUIToken("Paginate", void () => paginates[_i] = !paginates[_i]);
            paginateTokens[i * 3 + 1] = new GUIToken("Pg++", void () => pagination[_i]++);
            paginateTokens[i * 3 + 2] = new GUIToken("Pg--", void () => pagination[_i]--);
        }

        Stopwatch submoduleInitSW = Stopwatch.StartNew();
#endif

        if (Utilities.IsPlatformQuest())
            AndroidAsyncUnfucker.Init();

        HarmonyInstance.PatchAll();

        Barcodes.Init();

        PlayerPrefsExceptionUses.Init();

#if DEBUG
        submoduleInitSW.Stop();
        LoggerInstance.Msg(ConsoleColor.Blue, $"JeviLib submodules initialized in {submoduleInitSW.ElapsedMilliseconds}ms");
#endif

        Task.Run(this.GetNamespaces);

        Hooking.OnLevelLoaded += (li) => { OnSceneWasInitialized(-1, li.barcode); };

        sw.Stop();
        LoggerInstance.Msg(ConsoleColor.Blue, $"Pre-initialized {nameof(JeviLib)} v{JevilBuildInfo.VERSION}{(JevilBuildInfo.DEBUG ? " Debug (Development)" : "")} in {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Initializes JeviLib
    /// </summary>
    public override void OnInitializeMelon()
    {
        Stopwatch sw = Stopwatch.StartNew();

#if DEBUG
        Log("This version of " + nameof(JeviLib) + " has been built with the DEBUG compiler flag!");
        Log("Functionality will remain in tact for the most part, however there will be extra log points to warn you if there is anything worrying about your usage of the library.");
        Log("You should only be using this build if you create code mods, and not if you simply use mods. Do not rely on the extra checks in this build, or require the use of a debug build for your production code.");
#endif

        try
        {
            AssemblyPatcher.Init();
        }
        catch (Exception ex)
        {
            Error("Fixes for UniTasks, Coroutines, or IL2CPP ToString methods may be unavailable. See below for more information.");
            Error("Exception while initializing fixes: " + ex);
        }

        // Initialize NeverCollect/NeverCancel for generic Tweens
        GameObject go = new(nameof(NeverCollect));
        NeverCollect nc = go.AddComponent<NeverCollect>();
        go.Persist();
        nc.Persist();
        Instances.NeverCancel = go;

        LoggerInstance.Msg(ConsoleColor.Blue, $"Completed initialization of {nameof(JeviLib)} v{JevilBuildInfo.VERSION}{(JevilBuildInfo.DEBUG ? " Debug" : "")} in {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Update things that are always changing, like Tweens.
    /// </summary>
    public override void OnUpdate()
    {
        onUpdateCallback.InvokeActionSafe();
        Tweener.UpdateAll();

        while (AsyncUtilities.mainThreadCallbacks.TryDequeue(out var mte))
        {
            try
            {
                mte.execute();
            }
            catch (Exception ex)
            {
                Error("Exception while executing a main-thread callback");
                Error(ex);
                if (mte.completer.UnsafeGetStatus() != UniTaskStatus.Pending) continue;

                mte.completer.exception = new(Il2CppSystem.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(new Il2CppSystem.Exception(ex.ToString())));
            }

            mte.completer.TrySetResult();
        }

        mainThreadInvokeTimer.Restart();
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
            GameObject.FindObjectOfType<Il2CppSLZ.Bonelab.BodyVitals>();
        if (Instances.Player_BodyVitals.INOC())
            return;
        Instances.Player_RigManager =
            GameObject.FindObjectOfType<Il2CppSLZ.Marrow.RigManager>();
        Instances.Player_PhysicsRig =
            GameObject.FindObjectOfType<Il2CppSLZ.Marrow.PhysicsRig>();
        Instances.Player_Health =
            GameObject.FindObjectOfType<Player_Health>();
        Instances.Audio_Manager =
            GameObject.FindObjectOfType<Audio3dManager>();
        Instances.MusicMixer = //todo: get mixer names from runtime game
            Instances.Audio_Manager.audioMixer.FindMatchingGroups("Music").First();
        Instances.SFXMixer =
            Instances.Audio_Manager.audioMixer.FindMatchingGroups("SFX").First();
        // Separate cameras because it's better this way, I think. It's more distinguishable even if it requires two lines to keep the two "in sync"
        Instances.RigCameras =
            GameObject.FindObjectsOfType<Camera>().Where(c => c.transform.IsChildOfRigManager()).ToArray();
        Instances.SpectatorCam =
            Instances.RigCameras.FirstOrDefault(c => c.name == "Spectator Camera");
        Instances.InHeadsetCam =
            Instances.RigCameras.FirstOrDefault(c => c.name == "Head");

        Transform pHead = Player.Head;

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

#if DEBUG
    /// <summary>
    /// Draw <see cref="GUIToken"/>s from <see cref="DebugDraw"/>.
    /// </summary>
    public override void OnGUI()
    {
        try
        {
            List<GUIToken> tokens = Jevil.IMGUI.DebugDraw.GetTokens();

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

            // assumed 12px for a button. default is 10 but im tryna make it a bit roomy
            int pxForOneButton = (GuiGap * 2) + 15;
            int drawnPerColumn = ((Screen.height - GuiCornerDist) / 2 / pxForOneButton) - 4;

            // these are only being enumerated once, so keeping them in an IEnumerable instead of ToArraying isn't a concern
            IEnumerable<GUIToken> topLeft = tokens.Where(t => t.position == GUIPosition.TOP_LEFT);
            IEnumerable<GUIToken> topRight = tokens.Where(t => t.position == GUIPosition.TOP_RIGHT);
            IEnumerable<GUIToken> bottomLeft = tokens.Where(t => t.position == GUIPosition.BOTTOM_LEFT);
            IEnumerable<GUIToken> bottomRight = tokens.Where(t => t.position == GUIPosition.BOTTOM_RIGHT);

            //if (paginate)
            //{
            //    topLeft = topLeft.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    topRight = topRight.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    bottomLeft = bottomLeft.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn);
            //    bottomRight = bottomRight.Skip(pageIdx * drawnPerColumn).Take(drawnPerColumn - paginateTokens.Length).Prepend(paginateTokens);
            //}
            
            // Draw top left
            int screenCorner = 0;
            int maxWidth = 0;
            int xStart = GuiCornerDist;
            int yStart = GuiCornerDist;
            if (paginates[screenCorner])
                topLeft = topLeft.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (Jevil.IMGUI.DebugDraw.IsActive) topLeft = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(topLeft);
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
            screenCorner++;
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = GuiCornerDist;
            if (paginates[screenCorner])
                topRight = topRight.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (Jevil.IMGUI.DebugDraw.IsActive) topRight = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(topRight);
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
            screenCorner++;
            maxWidth = 0;
            xStart = GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            if (paginates[screenCorner])
                bottomLeft = bottomLeft.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (Jevil.IMGUI.DebugDraw.IsActive) bottomLeft = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(bottomLeft);
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
            screenCorner++;
            maxWidth = 0;
            xStart = Screen.width - GuiCornerDist;
            yStart = Screen.height - GuiCornerDist;
            if (paginates[screenCorner])
                bottomRight = bottomRight.Skip(pagination[screenCorner] * drawnPerColumn).Take(drawnPerColumn);
            if (Jevil.IMGUI.DebugDraw.IsActive) bottomRight = paginateTokens.Skip(screenCorner * 3).Take(3).Concat(bottomRight);
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
        Log("Getting namespaces from assemblies now");
        Stopwatch sw = Stopwatch.StartNew();

        Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
        Assembly[][] assemblies = asms.SplitByProcessors().Select(e => e.ToArray()).ToArray();
#if DEBUG
        Log($"Getting namespaces from " + assemblies.Sum(a => a.Length) + " assemblies, split across " + assemblies.Length + " threads");
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
        Log($"Split list & started threads in {sw.ElapsedMilliseconds} ms.");
#endif
        while (threads.Any(t => t.ThreadState != System.Threading.ThreadState.Stopped)) await Task.Yield();

        sw.Stop();
        Log($"Cached all {namespaceAssemblies.Count} namespaces and their respective assemblies in {sw.ElapsedMilliseconds}ms");

        DoneMappingNamespacesToAssemblies = true;
        onNamespaceAssembliesCompleted.InvokeSafeParallel();
    }

    private void PopulateDictionary_ThreadStart(object obj) => this.PopulateDictionary((Assembly[])obj);

    private void PopulateDictionary(Assembly[] section)
    {
        string currAsmTitle = string.Empty;
        try
        {
            for (int i = 0; i < section.Length; i++)
            {
                Assembly currentAsm = section[i];
                currAsmTitle = currentAsm.FullName;
                if (currAsmTitle.Contains("JeviLib")) continue; // causes quest enumerator wrapper to fail
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
                }
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            // apparently jevilib is throwing System.TypeLoadException and IDFK why
            Error("Caught exception while populating namespace dictionary for assembly " + currAsmTitle, ex);
#endif
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

    #region MelonLogger replacements

    internal static void Log(string str) => instance.LoggerInstance.Msg(str);
    internal static void Log(object obj) => instance.LoggerInstance.Msg(obj?.ToString() ?? "null");
    internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
    internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
    internal static void Error(string str) => instance.LoggerInstance.Error(str);
    internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");
    internal static void Error(string str, Exception ex) => instance.LoggerInstance.Error(str ?? "null", ex);

    #endregion

#if DEBUG
    private void ClearStandardTokens()
    {
        foreach (GUIToken token in this.standardJevilTokens)
        {
            Jevil.IMGUI.DebugDraw.Dont(token);
        }

        this.standardJevilTokens.Clear();
    }

    private void TestSpawning()
    {
        Barcodes.SpawnAsync(JevilBarcode.MP5, Vector3.zero, Quaternion.identity);
    }

    private async void TestUniTaskAsync()
    {
        Log("Waiting 2sec");
        await UniTask.Delay((int)Il2CppSystem.TimeSpan.FromSeconds(2).TotalMilliseconds, true, PlayerLoopTiming.Update, new CancellationToken());
        Log("Hello after waiting 2sec!");
        Log("Are we still on the main thread?");
        GameObject.CreatePrimitive(PrimitiveType.Cube);
        Log("If we're still here, then YES we are on the main thread! UniTask and JeviLib did its job!");
        Log("Testing UniTask patches. Waiting 3sec on each.");
        await UniTask.Delay(3000, true);
        Log("Check-in 1");
        await UniTask.Delay(3000, DelayType.UnscaledDeltaTime);
        Log("Check-in 2");
        await UniTask.Delay((int)Il2CppSystem.TimeSpan.FromSeconds(3).TotalMilliseconds, true);
        Log("Check-in 3! All checks passed!");
    }

    private void NotifiedLowRAM()
    {
        JeviLib.Warn("BONELAB has been notified that your system has very little free RAM left! Double check that! Consider uninstalling some items!");
    }
#endif
}