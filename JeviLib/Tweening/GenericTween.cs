using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Tweening;

/// <summary>
/// A Tween that can act on any value. use this if there's no extension method for a tween you want to get and you don't feel like .
/// <para>You will need to extend this class and override <see cref="TweenBase.Update(float)"/></para>
/// </summary>
/// <typeparam name="T">The type to get and set, not the type to act upon.</typeparam>
public abstract class GenericTween<T> : Tween<T>
{

    /// <summary>
    /// Create a new <see cref="GenericTween{T}"/>. This does not activate the tween, so you will need to call <see cref="Play{TTween}"/>.
    /// </summary>
    /// <param name="getter">A lambda or method to retrieve a value.</param>
    /// <param name="setter">A lambda or method to set a value.</param>
    /// <param name="endingValue">The value to end with.</param>
    /// <param name="length">How long you want the tween to last</param>
    /// <param name="cancelWith">What the Tween should cancel with. Defaults to <see cref="Instances.NeverCancel"/></param>
#pragma warning disable UNT0007 // Null coalescing on Unity objects
    protected GenericTween(Func<T> getter,
                           Action<T> setter,
                           T endingValue,
                           float length,
                           UnityEngine.Object cancelWith = null) 
        : base(endingValue,
               length,
               cancelWith ?? Instances.NeverCancel,
               () => getter(),
               (val) => setter(val))
#pragma warning restore UNT0007 // Null coalescing on Unity objects
    {

    }

    /// <summary>
    /// Update the Tween according to its progress.
    /// <para>You will need to call <see cref="TweenBase.interpolator"/> by yourself, if you called <see cref="TweenTweenExtensions.UseCustomInterpolator{T}(T, Func{float, float})"/></para>
    /// </summary>
    /// <param name="completion">A value from 0 to 1 representing how complete the Tween is.</param>
    protected abstract override void Update(float completion);

    /// <summary>
    /// Start the created tween.
    /// </summary>
    /// <returns>The tween</returns>
    public TTween Play<TTween>() where TTween : GenericTween<T>
    {
        Tweener.AddTween(this);
        return (TTween)this;
    }
}
