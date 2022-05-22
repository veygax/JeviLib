using System;

namespace Jevil.Prefs;

/// <summary>
/// Designates a static field (private or public) as a preference. The predefined value will be treated as the default and will be overridden by BoneMenu or MelonPreferences values.
/// <para>Used for strings and enums.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class Pref : Attribute
{
    /// <summary>
    /// THE DESCRIPTION, DUMBASS, WHAT ELSE LOL
    /// </summary>
    public readonly string desc = "";
    
    /// <summary>
    /// Declares a field as a preference with no MelonPreferences description.
    /// </summary>
    public Pref() { desc = ""; }

    /// <summary>
    /// Declare a field as a preference with a description in MelonPreferences.
    /// </summary>
    /// <param name="description">Enums cannot have descriptions, as their descriptions will be used to display the names of the enum values.</param>
    public Pref(string description) { desc = description; }
}
