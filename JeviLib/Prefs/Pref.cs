using ModThatIsNotMod.BoneMenu;
using System;
using UnityEngine;

namespace Jevil.Prefs;

//todo: support loading preferences (maybe using delegates?) from OnPreferencesLoaded

/// <summary>
/// Designates a static member (private or public) as a preference. The predefined value will be treated as the default and will be overridden by BoneMenu or MelonPreferences values.
/// <para>Used for strings and enums.</para>
/// <para>Can be used on methods to generate a <see cref="FunctionElement"/>.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class Pref : Attribute
{
    /// <summary>
    /// THE DESCRIPTION, DUMBASS, WHAT ELSE LOL
    /// </summary>
    public readonly string desc = "";
    /// <summary>
    /// The color of the BoneMenu element.
    /// </summary>
    public readonly Color color = Color.white;

    /// <summary>
    /// Declares a member as a preference with no MelonPreferences description.
    /// </summary>
    public Pref() { }

    /// <summary>
    /// Declare a field as a preference with a description in MelonPreferences. Color defaults to white.
    /// </summary>
    /// <param name="description">MelonPrefs description. Enums cannot have descriptions, as their descriptions will be used to display the names of the enum values. Functions will use this instead of the friendly-ified method name if this is present.</param>
    /// <param name="colorR">The R (red) component of the Pref's BoneMenu element</param>
    /// <param name="colorG">The G (green) component of the Pref's BoneMenu element</param>
    /// <param name="colorB">The B (blue) component of the Pref's BoneMenu element</param>
    public Pref(string description = "", float colorR = 1f, float colorG = 1f, float colorB = 1f) { desc = description; color = new(colorR, colorG, colorB); }

    /// <summary>
    /// Declare a member as a preference with a description in MelonPreferences. Color defaults to white.
    /// </summary>
    /// <param name="description">MelonPrefs description. Enums cannot have descriptions, as their descriptions will be used to display the names of the enum values. Functions will use this instead of the friendly-ified method name if this is present.</param>
    /// <param name="boneMenuColor">The <see cref="MenuElement"/> color.</param>
    public Pref(string description = "", UnityDefaultColor boneMenuColor = UnityDefaultColor.WHITE) { desc = description; color = PrefsInternal.EnumToColor(boneMenuColor); }
}
