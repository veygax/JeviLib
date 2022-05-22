using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Prefs;

/// <summary>
/// Define a color for the BoneMenu category. Without this, the BoneMenu category defaults to white. 
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PreferencesColor : Attribute
{
    internal readonly Color color;

    /// <summary>
    /// why rgba? cause <see cref="Color"/> has an overload for it.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    public PreferencesColor(float r, float g, float b, float a)
    {
        color = new Color(r, g, b, a);
    }

    /// <summary>
    /// why rgb? cause <see cref="Color"/> has an overload for it.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    public PreferencesColor(float r, float g, float b)
    {
        color = new Color(r, g, b);
    }
}
