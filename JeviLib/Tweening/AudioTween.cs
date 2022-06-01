using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Tween that acts on an <see cref="AudioSource"/>. An instance of this is returned by <see cref="TweenExtensions.TweenVolume(AudioSource, float, float)"/>
/// </summary>
public sealed class AudioTween : Tween<float>
{
    (float start, float end) interp;

    internal AudioTween(AudioSource player, float vol, float length)
        : base(vol,
               length,
               player,
               () => player.volume, // getter
               (v) => player.volume = v) // setter
    {
        interp = (getter(), vol);
    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float newVal = interp.Interpolate(completion);
        setter(newVal);
    }
}
