using System;
using UnityEngine;

namespace Jevil.Prefs;

//todo: support loading preferences (maybe using delegates?) from OnPreferencesLoaded

/// <summary>
/// Designates a static member (private or public) as a preference. The predefined value will be treated as the default and will be overridden by BoneMenu or MelonPreferences values.
/// <para>Used for strings and enums.</para>
/// <para>Can be used on methods to generate a BoneLib menu function element when BoneLib has a menu implemented.</para>
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
    /// Declares a member as a preference with no MelonPreferences description, or places a method in a submenu.
    /// <para>Examples:</para>
    /// <example>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToOpen() { } </c> <br/>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToClose() { } </c> <para/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void HowToOpen() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void thingy() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/")] private static void HowToOpen() { } </c> <br/>
    /// </example>
    /// </summary>
    public Pref(string descOrPath) { desc = descOrPath; }

    /// <summary>
    /// Declare a field as a preference with a description in MelonPreferences or (if used on a Method) directions to a submenu to be used/created. Color defaults to white. <br/>
    /// The 'submenu path' will be the 'folder' your method is placed in.
    /// <para>Examples:</para>
    /// <example>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToOpen() { } </c> <br/>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToClose() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void HowToOpen() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void thingy() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/")] private static void HowToOpen() { } </c> <br/>
    /// </example>
    /// </summary>
    /// <param name="descOrPath">MelonPrefs descOrPath. Enums cannot have descriptions, as their descriptions will be used to display the names of the enum values. Functions will use this instead of the friendly-ified method name if this is present.</param>
    /// <param name="colorR">The R (red) component of the Pref's BoneMenu element</param>
    /// <param name="colorG">The G (green) component of the Pref's BoneMenu element</param>
    /// <param name="colorB">The B (blue) component of the Pref's BoneMenu element</param>
    public Pref(string descOrPath, float colorR = 1f, float colorG = 1f, float colorB = 1f) { desc = descOrPath; color = new(colorR, colorG, colorB); }

    /// <summary>
    /// Declare a member as a preference with a description in MelonPreferences or (if used on a Method) directions to a submenu to be used/created. <br/>
    /// The 'submenu path' will be the 'folder' your method is placed in. See examples for details.
    /// <para>Examples:</para>
    /// <example>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToOpen() { } </c> <br/>
    /// DO: <c> [Pref("Actions/Learn/Menu")] private static void HowToClose() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void HowToOpen() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/How To Open")] private static void thingy() { } </c> <br/>
    /// DONT: <c> [Pref("Actions/Learn/Menu/")] private static void HowToOpen() { } </c> <br/>
    /// </example>
    /// </summary>
    /// <param name="descOrPath">MelonPrefs descOrPath. Enums cannot have descriptions, as their descriptions will be used to display the names of the enum values. Functions will use this instead of the friendly-ified method name if this is present.</param>
    /// <param name="boneMenuColor">The color of the BoneLib menu element.</param>
    public Pref(string descOrPath, UnityDefaultColor boneMenuColor) { desc = descOrPath; color = PrefsInternal.EnumToColor(boneMenuColor); }
}
