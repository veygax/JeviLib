using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// The class that controls Tweens. 
/// <para>Tweens are initiated via extenison methods. There doesn't seem to be anything for external use in this class.</para>
/// </summary>
public static class Tweener
{
    internal static readonly List<TweenBase> tweens = new(32);

    internal static void UpdateAll()
    {
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            TweenBase tween = tweens[i];
            // check using the equality operator because then it checks if its destroyed.
            if (tween.CancelWith == null)
            {
#if DEBUG
                JeviLib.Log($"Cancelling {tween.name}; Its corresponding UnityEngine.Object was destroyed or null.");
#endif
                tween.Active = false;
                tween.IsCancelled = true;
                tweens.RemoveAt(i);
            }
            else if (tween.UnscaledTimeRemaining == 0)
            {
#if DEBUG
                JeviLib.Log($"Removing {tween.name}; It is done.");
#endif
                tween.FinishInternal();
                tweens.RemoveAt(i);
            }

            float increment;
            if (tween.IsRealtime) increment = Time.unscaledDeltaTime;
            else increment = Time.deltaTime;

            tween.UpdateInternal(increment);
        }
    }

    internal static void AddTween(TweenBase tween)
    {
#if DEBUG
        if (tween.IsCancelled || tween.CancelWith == null)
        {
            JeviLib.Warn("Don't try to add a cancelled Tween or a Tween with a null CancelWith into the list of Tweens! FYI, the Tween was acting on " + tween.name);
            JeviLib.Warn("This check is exclusive to debug builds, do not rely on this check in your production code!");
            return;
        }
#endif
        if (tweens.Contains(tween))
        {
#if DEBUG
            JeviLib.Warn("Can't double-add a Tween to the list of acive Tweens! FYI, the Tween was acting on " + tween.name);
#endif
            return;
        }

        tweens.Add(tween);

#if DEBUG
        JeviLib.Log("Adding Tween " + tween.name + " to the list of active Tweens.");
#endif
    }

    internal static void RemoveTween(TweenBase tween)
    {
#if DEBUG
        JeviLib.Log("Removing tween " + tween.name);
#endif

        tweens.Remove(tween);
    }
}
