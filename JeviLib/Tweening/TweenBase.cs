using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// The base class that all tweens derive from.
/// </summary>
public class TweenBase
{
    /// <summary>
    /// Displays whether this Tween has finished or is still in progress.
    /// <para>This will be <see langword="false"/> when <see cref="IsCancelled"/> is <see langword="true"/></para>
    /// </summary>
    public bool Active { get; internal set; }

    /// <summary>
    /// If this Tween is realtime, this returns the ingame seconds this Tween has remaining.
    /// <br>Otherwise, this returns the realtime seconds this Tween has remaining.</br>
    /// <code>Example Tween w/ length of 1 sec, 0.5f seconds passed since its start, Time.timeScale is 0.125f (slowmo level 3): 
    /// -- Realtime => (1 - 0.5f) * 0.125f = 0.0625f game seconds remaining
    /// -- Not realtime => (1 - 0.5f) / 0.125f = 4f realtime seconds remaining
    /// </code>
    /// See also: <seealso cref="UnscaledTimeRemaining">UnscaledTimeRemaining</seealso>
    /// </summary> 
    public float TimeRemaining => IsRealtime ? UnscaledTimeRemaining * Time.timeScale : UnscaledTimeRemaining / Time.timeScale;

    /// <summary>
    /// Displays the time remaining, unaffected by <see cref="Time.timeScale"/>.
    /// </summary>
    public float UnscaledTimeRemaining => Mathf.Clamp(length - timeSinceStart, 0, length);

    /// <summary>
    /// The value, from <c>0</c> to <c>1</c> representing how complete this Tween is.
    /// </summary>
    public float Completion => timeSinceStart / length;

    /// <summary>
    /// Displays whether the Tween should be ran in realtime or be 
    /// </summary>
    public bool IsRealtime { get; internal set; }
    /// <summary>
    /// Displays the <see cref="UnityEngine.Object"/> that the tween will check every time it updates.
    /// <br>The tween will cancel immediately if this is <see langword="null"/> or destroyed within the IL2CPP domain.</br>
    /// </summary>
    public UnityEngine.Object CancelWith { get; internal set; }

    /// <summary>
    /// Displays whether the tween was cancelled because <see cref="CancelWith"/> was either <see langword="null"/> or destroyed.
    /// </summary>
    public bool IsCancelled { get; internal set; }


    internal float timeSinceStart;
    internal float startTime;
    internal float length;
    internal string name;
    internal Action invokeAfter;
    internal bool forceInvokeAll;

    internal void StartInternal()
    {
        Active = true;
        startTime = Time.realtimeSinceStartup;
        timeSinceStart = 0;
        Start();
    }

    internal void UpdateInternal(float increment)
    {
        timeSinceStart += increment;
        Update(timeSinceStart / length);
    }

    internal void FinishInternal()
    {
        Active = false;
        if (!forceInvokeAll) invokeAfter?.Invoke();
        else invokeAfter.InvokeSafeParallel();
        Finish();
    }

    /// <summary>
    /// Called when a Tween is started (or restarted)
    /// </summary>
    protected virtual void Start() { }
    /// <summary>
    /// Called every frame on Update.
    /// </summary>
    /// <param name="completion">How complete the Tween is. From 0 to 1.</param>
    protected virtual void Update(float completion) { }
    /// <summary>
    /// Called when a Tween finishes, usually used to call <see cref="Tween{T}.setter"/> with the ending value.
    /// </summary>
    protected virtual void Finish() { }
}
