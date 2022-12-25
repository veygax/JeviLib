using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace Jevil.Prefs;

/// <summary>
/// Class to mark register MelonPreferences and BoneMenu categories automatically, with minimal effort. This attribute is inherited for potential base-class referencing .
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class Preferences : Attribute
{
    /// <summary>
    /// The name of the BoneMenu subcategory created when <c>prefSubcategory</c> is set to True in your Preferences attribute.
    /// </summary>
    public static readonly string prefSubcategoryName = "Preferences";
    readonly string categoryName;
    readonly bool prefSubcategory;

    /// <summary>
    /// Used to designate the static fields of a class as preferences, so long as they have the requisite attributes.
    /// </summary>
    /// <param name="categoryName">The BoneMenu and MelonPreferences category name</param>
    /// <param name="prefSubcategory">
    /// Determines whether the BoneMenu category will hold the preferences directly, or if there will be a subcategory called "Preferences" holding the preferences.
    /// Use this if you want to use your BoneMenu category for other things, like running methods.
    /// <code>
    /// BoneMenu Example
    /// true:
    ///     ModName
    ///         - Preferences
    ///             * Toggle Enabled
    ///             * Other Option
    /// false:
    ///     ModName
    ///         * Toggle Enabled
    ///         * Other Option
    /// </code>
    /// </param>
    public Preferences(string categoryName, bool prefSubcategory) : base() 
    {
        this.categoryName = categoryName;
        this.prefSubcategory = prefSubcategory;
    }

    /// <summary>
    /// Register the preferences of the given class.
    /// <para>Debug builds have a check for instanced fields and will warn (but not <c>throw</c>) if it finds one. Release builds will ignore them.</para>
    /// </summary>
    /// <typeparam name="T">The class containing the preferences. It must be decorated with the Preferences attribute.</typeparam>
    /// <returns>A <see cref="PrefEntries"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The specified type is not decorated with Preferences.</exception>
    public static PrefEntries Register<T>()
        => Register(typeof(T));

    /// <summary>
    /// Register the preferences of the given class.
    /// <para>Debug builds have a check for instanced fields and will warn (but not <c>throw</c>) if it finds one. Release builds will ignore them.</para>
    /// </summary>
    /// <param name="type">The class containing the preferences. It must be decorated with the Preferences attribute.</param>
    /// <param name="overrideName">Overrides the name provided by <see cref="Preferences.categoryName"/>. Useful for subclasses.</param>
    /// <returns>A <see cref="PrefEntries"/> instance.</returns>
    /// <exception cref="InvalidOperationException">The specified type is not decorated with Preferences.</exception>
    public static PrefEntries Register(Type type, string overrideName = null)
    {
        Preferences attrib = (Preferences)type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(Preferences))
            ?? throw new InvalidOperationException($"Type {type.Namespace ?? "<Root>"}.{type.Name} doesn't have the [{nameof(Preferences)}] attribute! This is required to register preferences!");

        PreferencesColor pCol = (PreferencesColor)type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(PreferencesColor));

        PreferencesFile pFile = (PreferencesFile)type.GetCustomAttributes(false).FirstOrDefault(a => a.GetType() == typeof(PreferencesFile));
        string qualifiedPath = Path.Combine(MelonLoader.MelonUtils.UserDataDirectory, pFile?.path ?? "MelonPreferences.cfg");

        PrefEntries pentries = Register(type, attrib, pCol?.color ?? Color.white, qualifiedPath, overrideName ?? attrib.categoryName);

        foreach (Type inheritingType in type.Assembly.GetTypes().Where(t => t.IsSubclassOf(type)))
        {
            Register(inheritingType, attrib, pCol?.color ?? Color.white, qualifiedPath, overrideName ?? attrib.categoryName);
        }

        return pentries;
    }

    private static PrefEntries Register(Type type, Preferences attribute, Color color, string filePath, string name)
    {
        return PrefsInternal.RegisterPreferences(type, name, attribute.prefSubcategory, color, filePath);
    }
}
