using MelonLoader;
using ModThatIsNotMod;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Jevil.Tweening;

namespace Jevil;

internal static class BuildInfo
{
    public const string Name = "JeviLib"; // Name of the Mod.  (MUST BE SET)
    public const string Author = "extraes"; // Author of the Mod.  (Set as null if none)
    public const string Company = null; // Company that made the Mod.  (Set as null if none)
    public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
    public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
}

/// <summary>
/// The JeviLib <see cref="MelonMod"/> class. There's not much of note here.
/// </summary>
public class JeviLib : MelonMod
{
    /// <summary>
    /// https://cdn.discordapp.com/attachments/646885826776793099/976272724324401172/IMG_2559.jpg
    /// </summary>
    public JeviLib() : base() => instance = this;
    internal static JeviLib instance;

    /// <summary>
    /// https://media.discordapp.net/attachments/919014401187643435/958026151383691344/freeze-1.gif
    /// </summary>
    public override void OnApplicationStart()
    {
        Log("Initialized " + nameof(JeviLib) + " v" + BuildInfo.Version);
#if DEBUG
        Log("This version of " + nameof(JeviLib) + " has been built with the DEBUG compiler flag!");
        Log("Functionality will remain in tact for the most part, however there will be extra log points to warn you if there is anything worrying about your usage of the library.");
        Log("You should only be using this build if you create code mods, and not if you simply use mods. Do not rely on the extra checks in this build, or require the use of a debug build for your production code.");
#endif

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
        Tweening.Tweener.UpdateAll();
    }

    /// <summary>
    /// Update references in <see cref="Instances"/>
    /// </summary>
    /// <param name="buildIndex">idk bro ask melonloader</param>
    /// <param name="sceneName">idk bro ask melonloader</param>
    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
#endif
        // Grab the necessary references when the scene starts. 
        Instances.Player_BodyVitals =
            GameObject.FindObjectOfType<StressLevelZero.VRMK.BodyVitals>();
        Instances.Player_RigManager =
            GameObject.FindObjectOfType<StressLevelZero.Rig.RigManager>();
        Instances.Player_Health =
            GameObject.FindObjectOfType<Player_Health>();
        Instances.Player_PhysBody =
            GameObject.FindObjectOfType<StressLevelZero.VRMK.PhysBody>();
        Instances.DataManager = 
            GameObject.FindObjectOfType<Data_Manager>();
        Instances.MusicMixer =
            Instances.DataManager.audioManager.audioMixer.FindMatchingGroups("Music").First();
        Instances.SFXMixer =
            Instances.DataManager.audioManager.audioMixer.FindMatchingGroups("SFX").First();
        // Separate cameras because it's better this way, I think. It's more distinguishable even if it requires two lines to keep the two "in sync"
        Instances.SpectatorCam =
            GameObject.Find("[RigManager (Default Brett)]/[SkeletonRig (GameWorld Brett)]/Head/FollowCamera").GetComponent<Camera>();
        Instances.Cameras =
            GameObject.FindObjectsOfType<Camera>().Where(c => c.name.StartsWith("Camera (")).ToArray();

        GameObject pHead = Player.GetPlayerHead();

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

#if DEBUG
        Log("Found our instances in " + sw.ElapsedMilliseconds + "ms.");
#endif
    }

//#if DEBUG
//    private GameObject tweenTarget;
//    /// <summary>
//    /// Tweener debugger
//    /// </summary>
//    public override void OnGUI()
//    {
//        if (GUI.Button(new(25, 25, 125, 20), "Spawn cube")) tweenTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
//        if (GUI.Button(new(25, 50, 125, 20), "Pos -> V3.One")) tweenTarget.transform.TweenPosition(Vector3.one, 1);
//        if (GUI.Button(new(25, 75, 125, 20), "Pos -> -V3.One")) tweenTarget.transform.TweenPosition(-Vector3.one, 1);
//        if (GUI.Button(new(25, 100, 125, 20), "Scl -> V3.One")) tweenTarget.transform.TweenLocalScale(Vector3.one, 1);
//        if (GUI.Button(new(25, 125, 125, 20), "Scl -> V3.Zero")) tweenTarget.transform.TweenLocalScale(Vector3.zero, 1);
//    }
//#endif

    #region MelonLogger replacements

    internal static void Log(string str) => instance.LoggerInstance.Msg(str);
    internal static void Log(object obj) => instance.LoggerInstance.Msg(obj?.ToString() ?? "null");
    internal static void Warn(string str) => instance.LoggerInstance.Warning(str);
    internal static void Warn(object obj) => instance.LoggerInstance.Warning(obj?.ToString() ?? "null");
    internal static void Error(string str) => instance.LoggerInstance.Error(str);
    internal static void Error(object obj) => instance.LoggerInstance.Error(obj?.ToString() ?? "null");

    #endregion
}