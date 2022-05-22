using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// A class representing an active Tween.
/// <para>Debug builds will notify you when an object has been destroyed mid-tween and check if length is 0 on instantiation</para>
/// </summary>
public class Tween<T> : TweenBase
{
    internal readonly T startValue;
    internal readonly T endValue;
    internal readonly Getter getter;
    internal readonly Setter setter;

    internal delegate T Getter();
    internal delegate void Setter(T val);

    internal Tween(T end, float length, UnityEngine.Object cancelWith, Getter getter, Setter setter)
    {
        this.startValue = getter();
        this.endValue = end;
        this.length = length;
        this.CancelWith = cancelWith;
        this.getter = getter;
        this.setter = setter;

#if DEBUG
        this.name = cancelWith != null ? cancelWith.name : "<STARTED NULL>";
        if (this.length == 0) throw new InvalidOperationException("A tween cannot be started with length 0! Attempted tween on object with name: " + name);
#endif
    }

    /// <summary>
    /// Sets the value to the ending value. Feel free to override.
    /// </summary>
    protected override void Finish()
    {
        setter(endValue);
    }
}
