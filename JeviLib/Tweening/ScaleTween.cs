using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Tween that acts on a Transform's local scale. An instance of this is returned by <see cref="TweenExtensions.TweenLocalScale(Transform, Vector3, float)"/>
/// </summary>
public sealed class ScaleTween : Tween<Vector3>
{
    private (float start, float end) xComp;
    private (float start, float end) yComp;
    private (float start, float end) zComp;

    internal ScaleTween(Transform transform, Vector3 target, float length) 
        : base(target,
               length,
               transform,
               () => transform.localScale, // getter
               (scale) => transform.localScale = scale) // setter
    {
        Vector3 start = startValue;
        xComp = (start.x, target.x);
        yComp = (start.y, target.y);
        zComp = (start.z, target.z);
    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float x = xComp.Interpolate(completion);
        float y = yComp.Interpolate(completion);
        float z = zComp.Interpolate(completion);
        setter(new(x, y, z));
    }
}
