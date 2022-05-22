using StressLevelZero.Rig;
using StressLevelZero.VRMK;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// A cached reference to <see cref="BodyVitals"/>
    /// </summary>
    public static RigManager Player_RigManager { get; internal set; }
    /// <summary>
    /// A cached reference to <see cref="BodyVitals"/>
    /// </summary>
    public static Player_Health Player_Health { get; internal set; }
    /// <summary>
    /// A cached reference to <see cref="BodyVitals"/>
    /// </summary>
    public static PhysBody Player_PhysBody { get; internal set; }



    /// <summary>
    /// A cached reference to the spectator camera, regardless of if it is the active camera (Use <c><see cref="Camera.main"/></c> for that)
    /// </summary>
    public static Camera SpectatorCam { get; internal set; }
    /// <summary>
    /// Every camera in the currently loaded scene.
    /// </summary>
    public static Camera[] Cameras { get; internal set; }



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
    /// The active <see cref="Data_Manager"/> instance in the scene. Use this to reload the scene, or something.
    /// <para>I'm not 100% sure why you'd need this, but I put it here because I already use it to find the audio mixers, so I might as well cache it for others to use.</para>
    /// </summary>
    public static Data_Manager DataManager { get; internal set; }

    /// <summary>
    /// A Unity Object that should never be unloaded or destroyed.
    /// </summary>
    public static UnityEngine.Object NeverCancel { get; internal set; }
}
