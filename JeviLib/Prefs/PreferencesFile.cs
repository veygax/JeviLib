using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

namespace Jevil.Prefs;

/// <summary>
/// Define where the preferences file should be saved, if not <c>MelonPreferences.cfg</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PreferencesFile : Attribute
{
    internal readonly string path;

    /// <summary>
    /// Applies the <see cref="PreferencesFile"/> attribute to your class.
    /// </summary>
    /// <param name="filePath">The file path, relative to <see cref="MelonUtils.UserDataDirectory"/>. <para>For example, to save in UserData/MelonPreferences.cfg, you would pass in <c>"./MelonPreferences.cfg"</c>.</para></param>
    public PreferencesFile(string filePath)
    {
        path = filePath;
    }
}
