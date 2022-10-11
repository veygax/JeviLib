using SLZ.Data;
using SLZ.Marrow.Pool;
using SLZ.Rig;
using SLZ.Utilities;
using SLZ.VRMK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Jevil;

/// <summary>
/// Cached references to commonly used components, most of them having single instances in the active scene.
/// <para>If you frequently use something and cache it yourself, feel free to suggest an addition</para>
/// </summary>
public static class Instances
{
    /// <summary>
    /// A cached reference to <see cref="BodyVitals"/>
    /// </summary>
    public static BodyVitals Player_BodyVitals { get; internal set; }
    /// <summary>
    /// A cached reference to <see cref="RigManager"/>
    /// </summary>
    public static RigManager Player_RigManager { get; internal set; }
    /// <summary>
    /// A cached reference to <see cref="global::Player_Health"/>
    /// </summary>
    public static Player_Health Player_Health { get; internal set; }



    /// <summary>
    /// A cached reference to the spectator camera, regardless of if it is the active camera (Use <c><see cref="Camera.main"/></c> for that)
    /// </summary>
    public static Camera SpectatorCam { get; internal set; }
    /// <summary>
    /// Every camera in the currently loaded scene.
    /// </summary>
    public static Camera[] Cameras { get; internal set; }

    /// <summary>
    /// The currently operating Audio_Manager.
    /// </summary>
    public static Audio_Manager Audio_Manager { get; internal set; }
    /// <summary>
    /// The AudioMixerGroup corresponding to the Music mixer in the game's audio mixer settings.
    /// </summary>
    public static AudioMixerGroup MusicMixer { get; internal set; }
    /// <summary>
    /// The AudioMixerGroup corresponding to the SFX mixer in the game's audio mixer settings.
    /// </summary>
    public static AudioMixerGroup SFXMixer { get; internal set; }
    /// <summary>
    /// An <see cref="AudioPlayer"/> already set to output to <see cref="MusicMixer"/>
    /// <para>Use <see cref="ModThatIsNotMod.Nullables.NullableMethodExtensions"/> to play things through it.</para>
    /// </summary>
    public static AudioPlayer MusicPlayer { get; internal set; }
    /// <summary>
    /// An <see cref="AudioPlayer"/> already set to output to <see cref="SFXMixer"/>
    /// <para>Use <see cref="ModThatIsNotMod.Nullables.NullableMethodExtensions"/> to play things through it.</para>
    /// </summary>
    public static AudioPlayer SFXPlayer { get; internal set; }

    /// <summary>
    /// A Unity Object that should never be unloaded or destroyed.
    /// </summary>
    public static UnityEngine.Object NeverCancel { get; internal set; }

    /// <summary>
    /// Holds a cache of all loaded namespaces and their corresponding assemblies.
    /// </summary>
    public static readonly Dictionary<string, Assembly> namespaceToAssembly = new();
    
    /// <summary>
    /// A read-only collection of all pools.
    /// </summary>
    public static IReadOnlyList<AssetPool> AllPools => allPools.AsReadOnly(); //todo: patch assetpool ctor

    internal static List<AssetPool> allPools = new();
    internal static List<IDictionary> instanceCachesToClear = new();
}
