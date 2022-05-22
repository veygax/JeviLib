using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// The class that holds various extension methods related to initiaiting a Tween. See also <seealso cref="Tweener"/>
/// </summary>
public static class TweenExtensions
{
    /// <summary>
    /// Tweens the given transform's world space position.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static PositionTween TweenPosition(this Transform t, Vector3 target, float length)
    {
        PositionTween tween = new(t, target, length, false);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tweens the given transform's position in local space.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static PositionTween TweenLocalPosition(this Transform t, Vector3 target, float length)
    {
        PositionTween tween = new(t, target, length, true);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tweens the given transform's local space position.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static ScaleTween TweenLocalScale(this Transform t, Vector3 target, float length)
    {
        ScaleTween tween = new(t, target, length);
        Tweener.AddTween(tween);
        return tween;
    }
}
