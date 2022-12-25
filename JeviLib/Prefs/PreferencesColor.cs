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
    /// <param name="r">Red component of RGBA</param>
    /// <param name="g">Green component of RGBA</param>
    /// <param name="b">Blue component of RGBA</param>
    /// <param name="a">Alpha component of RGBA</param>
    public PreferencesColor(float r, float g, float b, float a)
    {
        color = new Color(r, g, b, a);
    }

    /// <summary>
    /// why rgb? cause <see cref="Color"/> has an overload for it.
    /// </summary>
    /// <param name="r">Red component of RGB</param>
    /// <param name="g">Green component of RGB</param>
    /// <param name="b">Blue component of RGB</param>
    public PreferencesColor(float r, float g, float b)
    {
        color = new Color(r, g, b);
    }

    /// <summary>
    /// Assigns a color to a prefernces entry.
    /// </summary>
    /// <param name="defaultColor">
    /// A unity default color. These usually have all their components either maxed or zeroed.
    /// <br>If you want more control over the color, use another overload.</br>
    /// </param>
    public PreferencesColor(UnityDefaultColor defaultColor)
    {
        color = PrefsInternal.EnumToColor(defaultColor);
    }
}
