using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Extension methods that act on instances of <see cref="Tween{T}"/>.
/// <para>These can be used to chain statements together, like so:</para>
/// <example>
/// <code>
/// timer.transform.TweenPosition(endPos)
///                .SetRealtime()
///                .RunOnFinish(EndGame);
/// </code>
/// </example>
/// </summary>
public static class TweenTweenExtensions
{
    /// <summary>
    /// Sets the value of <see cref="TweenBase.IsRealtime"/>. Best used immediately after Tween instantiation. Using mid-tween is not intended and the behavior is not guaranteed.
    /// </summary>
    /// <typeparam name="T">A type, where T extends <see cref="TweenBase"/>. Most likely extending <see cref="Tween{T}"/>.</typeparam>
    /// <param name="tween">The tween to make realtime or not realtime.</param>
    /// <param name="value">The value to assign to <see cref="TweenBase.IsRealtime"/></param>
    /// <returns>The original <paramref name="tween"/></returns>
    public static T SetRealtime<T>(this T tween, bool value = true) where T : TweenBase
    {
        tween.IsRealtime = value;
        return tween;
    }

    /// <summary>
    /// Makes actions invoke using <see cref="ActionExtensions.InvokeSafeSync(Action)"/>, blocking the main thread but allowing you to call Unity methods and properties.
    /// <para><b>!!!</b> This does <b>NOT</b> invoke all queued Actions. <b>!!!</b></para>
    /// </summary>
    /// <typeparam name="T">A type, where T extends <see cref="TweenBase"/>. Most likely extending <see cref="Tween{T}"/>.</typeparam>
    /// <param name="tween">The tween to make realtime or not realtime.</param>
    /// <param name="value">The value to assign to <see cref="TweenBase.IsRealtime"/></param>
    /// <returns>The original <paramref name="tween"/></returns>
    public static T ForceInvokeQueued<T>(this T tween, bool value = true) where T : TweenBase
    {
        tween.forceInvokeAll = value;
        return tween;
    }

    /// <summary>
    /// Queues an <see cref="Action"/> to run when <paramref name="tween"/> finishes.
    /// <br>Will be skipped if an Action that has already been queued throws an Exception, unless <see cref="ForceInvokeQueued"/> has been called.</br>
    /// </summary>
    /// <typeparam name="T">A type, where T extends <see cref="TweenBase"/>. Most likely extending <see cref="Tween{T}"/>.</typeparam>
    /// <param name="tween">The tween to be </param>
    /// <param name="fun">The lambda, method, or Action to be called upon finish.</param>
    /// <returns></returns>
    public static T RunOnFinish<T>(this T tween, Action fun) where T : TweenBase
    {
        tween.invokeAfter += fun;
        return tween;
    }

    /// <summary>
    /// Resets a Tween to the beginning. This does not clear the queued action to be called upon the Tween finishing.
    /// </summary>
    /// <typeparam name="T">A Tween</typeparam>
    /// <param name="tween">The Tween</param>
    /// <param name="setToStart">Whether to return the subject of the Tween to its original state, as retrieved at the time of the Tween's starting.</param>
    /// <returns>The Tween</returns>
    public static T Reset<T>(this T tween, bool setToStart = true) where T : TweenBase
    {
        if (!tween.Active) Tweener.AddTween(tween);
        tween.StartInternal();
        
        if (setToStart)
        {
            Tween<T> tt = tween as Tween<T>;
            tt.setter(tt.startValue);
        }
        return tween;
    }

    /// <summary>
    /// Stops a tween if it isn't already stopped.
    /// </summary>
    /// <typeparam name="T">A Tween</typeparam>
    /// <param name="tween">The Tween</param>
    /// <returns>The Tween</returns>
    public static T Stop<T>(this T tween) where T : TweenBase
    {
        if (tween.Active) Tweener.RemoveTween(tween);
        return tween;
    }

    /// <summary>
    /// Makes sure that this <paramref name="tween"/> is the only Tween acting on its Object.
    /// <para>Useful to make sure there are no conflicts and this Tween will go through no matter what.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tween"></param>
    /// <returns></returns>
    public static T Unique<T>(this T tween) where T : TweenBase
    {
        TweenBase[] competitors = Tweener.tweens.Where(t => t.CancelWith == tween.CancelWith && t != tween).ToArray();
        foreach (TweenBase twn in competitors) Tweener.RemoveTween(twn);
        return tween;
    }

    /// <summary>
    /// An IEnumerator that waits for <paramref name="tween"/> to end. Uses <c>yield return null;</c> so if you want to resume in Fixed/LateUpdate, you'll need to <see langword="yield return"/> a <c>WaitFor[...]</c> afterwards for them.
    /// <para>Use this as <c>yield return tweenName.WaitForEnd();</c> and your coroutine will yield until the tween finishes.</para>
    /// </summary>
    /// <typeparam name="T">A tween</typeparam>
    /// <param name="tween">A tween (no shit really?)</param>
    /// <returns>An IEnumerator that waits for <paramref name="tween"/> to end.</returns>
    public static IEnumerator WaitForEnd<T>(this T tween) where T : TweenBase
    {
        while (tween.Active) yield return null;
    }

    /// <summary>
    /// Makes the <paramref name="tween"/> not ease in and out.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tween"></param>
    /// <returns></returns>
    public static T DontEase<T>(this T tween) where T : TweenBase
    {
        tween.IsEasing = false;
        return tween;
    }

    /// <summary>
    /// Makes the <paramref name="tween"/> ease in and out. 
    /// <para>This is only necessary if the Tween has had <see cref="DontEase{T}(T)"/> already ran on it before, because all Tweens default to easing in and out.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tween"></param>
    /// <returns></returns>
    public static T DoEase<T>(this T tween) where T : TweenBase
    {
        tween.IsEasing = false;
        return tween;
    }
}
