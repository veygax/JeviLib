using System;
using UnityEngine;

namespace Jevil.Prefs;

/// <summary>
/// Designates a class, when being registered by <see cref="Preferences.Register{T}"/>, will have every type in that assembly searched for preferences.
/// <para>Child types will be registered as if they are the game as whatever class this one is on, but they won't need the <c>[<see cref="Preferences"/>("blahblah", false)]</c> attribute</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PreferencesRegisterChildren : Attribute
{
    internal PrefEntries entries;
    internal readonly bool hookCtorForAutoRegister;

    /// <summary>
    /// Decorates a class with <see cref="PreferencesRegisterChildren"/>
    /// </summary>
    /// <param name="hookCtor">this feature is not yet implemented</param>
    public PreferencesRegisterChildren(bool hookCtor) 
    {
        hookCtorForAutoRegister = hookCtor;
    }

    internal void NewTypeLoaded()
    {

    }
}
