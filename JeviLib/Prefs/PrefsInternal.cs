using BoneLib.BoneMenu;
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Prefs;

internal static class PrefsInternal
{
    // like a dictinoary, but doesnt stop GC of its PrefEntries values
    static readonly ConditionalWeakTable<string, PrefEntries> nameToEntries = new();
    static readonly Dictionary<Type, MethodInfo> genericCreateEnumElements = new();
    static readonly MethodInfo baseCreateEnumElement = typeof(Page).GetMethodEasy(nameof(Page.CreateEnum));
    static readonly object[] NoParameters = Array.Empty<object>();
    //public static void RegisterPreferences<T>(string categoryName, bool prefSubcategory, Color categoryColor, string filePath) => RegisterPreferences(typeof(T), categoryName, prefSubcategory, categoryColor, filePath);

    public static PrefEntries RegisterPreferences(Type type, string categoryName, bool prefSubcategory, Color categoryColor, string filePath)
    {
        PrefEntries ret = nameToEntries.GetValue(categoryName, (key) => new PrefEntries(MelonPreferences.CreateCategory(key), MenuManager.CreateCategory(key, categoryColor)));
        MelonPreferences_Category mpCat = ret.MelonPrefsCategory; // interally first checks for GetCategory
        Page methodCategory = ret.BoneMenuCategory;
        Page fieldCategory = prefSubcategory ? ret.BoneMenuCategory.CreateCategory(Preferences.prefSubcategoryName, categoryColor) : ret.BoneMenuCategory;

        if (!filePath.EndsWith("MelonPreferences.cfg")) // only set file path if its not MP.cfg
            mpCat.SetFilePath(filePath, true, false); // actually get the values

        RegisterPreferences(type, mpCat, fieldCategory, methodCategory);
        
        return ret;
    }

    public static void RegisterPreferences(Type type, MelonPreferences_Category mpCat, Page fieldCategory, Page methodCategory)
    {
        FieldInfo[] staticFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

#if DEBUG
        FieldInfo[] instanceFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        FieldInfo[] instancePrefs = instanceFields.Where(f => f.GetCustomAttribute<Pref>() != null).ToArray();
        FieldInfo[] instanceRanges = instanceFields.Where(f => f.GetCustomAttribute<RangePref>() != null).ToArray();
        MethodInfo[] instancedMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.GetCustomAttribute<Pref>() != null).ToArray();

        if (instancePrefs.Length != 0)
        {
            JeviLib.Warn($"The type {type.FullName} declares instanced preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instancePrefs) JeviLib.Warn(" - " + field.Name);
        }

        if (instanceRanges.Length != 0)
        {
            JeviLib.Warn($"The type {type.FullName} declares instanced range preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instanceRanges) JeviLib.Warn(" - " + field.Name);
        }

        if (instancedMethods.Length != 0)
        {
            JeviLib.Warn($"The type {type.Namespace}.{type.Name} declares instanced method preferences, this is not allowed!");
            JeviLib.Warn($"These methods are: ");
            foreach (MethodInfo method in instancedMethods) JeviLib.Warn(" - " + method.Name);
        }
#endif

        RegisterPrefAttr(type, staticFields, mpCat, fieldCategory);
        RegisterRangeAttr(type, staticFields, mpCat, fieldCategory);
        RegisterMethods(type, staticMethods, methodCategory); // methods arent saved to melonprefs

#if DEBUG
        if (methodCategory == fieldCategory) 
            JeviLib.Log($"Created {mpCat.Entries.Count} entries in the MelonPrefs '{mpCat.DisplayName}' category and {fieldCategory.Elements.Count} elements in the BoneMenu '{fieldCategory.Name}' category");
        else
            JeviLib.Log($"Created {mpCat.Entries.Count} entries in the MelonPrefs '{mpCat.DisplayName}' category, {fieldCategory.Elements.Count} field elements in the BoneMenu '{fieldCategory.Name}' category, and {methodCategory.Elements.Count} method elements in '{methodCategory.Name}'");

#endif

        if (mpCat.Entries.Count != 0) mpCat.SaveToFile(false);

        //nameToEntries.Add(categoryName, ret); dont need, GetOrCreate works fine
    }

    private static MelonPreferences_Entry<T> SetEntry<T>(MelonPreferences_Category mpCategory, FieldInfo field, out T toSet, string desc = "")
    {
        T defaultValue = (T)field.GetValue(null);
        desc = string.Join(", ", "Default: " + defaultValue.ToString(), desc);
        var entry = mpCategory.HasEntry(field.Name) ? mpCategory.GetEntry<T>(field.Name) : mpCategory.CreateEntry<T>(field.Name, defaultValue, description: desc);
        T entryValue = entry.Value;
        toSet = defaultValue.Equals(entryValue) ? entryValue : entryValue; // if the two are different
        field.SetValue(null, defaultValue);
        return entry;
    }

    internal static Color EnumToColor(UnityDefaultColor udc) => udc switch
        {
            UnityDefaultColor.RED => Color.red,
            UnityDefaultColor.GREEN => Color.green,
            UnityDefaultColor.BLUE => Color.blue,
            UnityDefaultColor.WHITE => Color.white,
            UnityDefaultColor.BLACK => Color.black,
            UnityDefaultColor.YELLOW => Color.yellow,
            UnityDefaultColor.CYAN => Color.cyan,
            UnityDefaultColor.MAGENTA => Color.magenta,
            UnityDefaultColor.GRAY => Color.gray,
            _ => throw new ArgumentException($"Unrecognized {nameof(UnityDefaultColor)} value: {udc}"),
        };
    

    private static void RegisterPrefAttr(Type type, FieldInfo[] fields, MelonPreferences_Category mpCat, Page bmCat)
    {
        foreach (FieldInfo field in fields)
        {
            var ep = field.GetCustomAttribute<Pref>();
            if (ep == null) continue;
#if DEBUG
            JeviLib.Log($"Found preference on {type.Name}.{field.Name}");
#endif

            var fieldType = field.FieldType;
            var readableName = Utilities.GenerateFriendlyMemberName(field.Name);
            if (fieldType == typeof(string))
            {
                var entry = SetEntry(mpCat, field, out string toSet, ep.desc);
                field.SetValue(null, toSet);
#if DEBUG
                JeviLib.Warn($"BoneMenu does not support string elements. Preference field is {type.FullName}.{type.FullDescription()}");
#endif
                //fieldCategory.CreateStringElement(readableName, ep.color, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType == typeof(bool))
            {
                var entry = SetEntry(mpCat, field, out bool toSet, ep.desc);
                field.SetValue(null, toSet);
                bmCat.CreateBool(readableName, ep.color, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType == typeof(Color))
            {
                var entry = SetEntry(mpCat, field, out Color toSet, ep.desc);
                field.SetValue(null, toSet);
#if DEBUG
                JeviLib.Warn($"BoneMenu does not support color elements. Preference field is {type.FullName}.{type.FullDescription()}");
#endif
                //fieldCategory.CreateColorElement(readableName, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType.IsEnum)
            {
#if DEBUG
                if (!string.IsNullOrEmpty(ep.desc)) JeviLib.Warn($"Descriptions are not allowed for enum preferences - the description '{ep.desc}' on {type.FullName}.{field.Name} will not be put in MelonPreferences.cfg");
#endif
                if (!genericCreateEnumElements.TryGetValue(fieldType, out MethodInfo createEnumElement))
                {
                    createEnumElement = baseCreateEnumElement.MakeGenericMethod(fieldType);
                    genericCreateEnumElements[fieldType] = createEnumElement;
                }

                Enum dv = (Enum)field.GetValue(null);
                var entry = mpCat.CreateEntry<string>(field.Name, dv.ToString(), description: $"Default: {dv}; Options: {string.Join(", ", Enum.GetNames(fieldType))}");
                Enum toSet = dv; // if the two are different
                try
                {
                    toSet = (Enum)Enum.Parse(fieldType, entry.Value);
                }
                catch
                {
                    JeviLib.Warn($"Failed to parse '{entry.Value}' as a value in the enum {fieldType.Name} for the class {type.FullName}'s preference {field.Name}");
                    JeviLib.Warn("Replacing it with its default value of " + dv);
                    entry.Value = dv.ToString();
                }
                object[] parameters = { readableName, ep.color, (dynamic val) => { field.SetValue(null, val); entry.Value = val.ToString(); mpCat.SaveToFile(false); } };
                field.SetValue(null, toSet);

                try
                {
                    createEnumElement.Invoke(bmCat, parameters);
                }
                catch
#if DEBUG
                (Exception ex)
#endif
                {
#if DEBUG
                    JeviLib.Warn($"It seems that JeviLib is unable to create an enum element for {type.FullName}.{field.Name}; See more: {ex}");
#endif
                }
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.FullName}.{field.Name} is of un-bonemenu-able (or un-melonpreferences-able) type {fieldType.Name}! This is no good!");
            }
            JeviLib.Log($"Set {type.FullName}.{field.Name} to " + field.GetValue(null));
#endif
        }
    }

    private static void RegisterRangeAttr(Type type, FieldInfo[] fields, MelonPreferences_Category mpCat, Page bmCat)
    {
        foreach (var field in fields)
        {
            var rp = field.GetCustomAttribute<RangePref>();
            if (rp == null) continue;
#if DEBUG
            JeviLib.Log($"Found effect range preference on {type.Name}.{field.Name}");
#endif

            var readableName = Utilities.GenerateFriendlyMemberName(field.Name);
            var defaultValue = field.GetValue(null);
            if (field.FieldType == typeof(int))
            {
                var entry = SetEntry(mpCat, field, out int toSet, $"{rp.low} to {rp.high}");
                field.SetValue(null, toSet);
                bmCat.CreateInt(readableName, Color.white, toSet, (int)rp.inc, (int)rp.low, (int)rp.high, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (field.FieldType == typeof(float))
            {
                var entry = SetEntry(mpCat, field, out float toSet, $"{rp.low} to {rp.high}");
                field.SetValue(null, toSet);
                bmCat.CreateFloat(readableName, Color.white, toSet, rp.inc, rp.low, rp.high, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.Name}.{field.Name} is of un-range-able type {field.FieldType.Name}! This is no good!");
            }
            JeviLib.Log($"Set {type.FullName}.{field.Name} to " + field.GetValue(null));
            JeviLib.Log("Successfully created range preference");
#endif
        }
    }

    private static void RegisterMethods(Type type, MethodInfo[] methods, Page bmCat)
    {
        foreach (MethodInfo method in methods)
        {
            Pref pref = method.GetCustomAttribute<Pref>();
            if (pref == null) continue;
#if DEBUG
            if (method.GetParameters().Length != 0)
            {
                JeviLib.Warn($"Method {type.FullName}.{method.Name} takes parameters! This makes it ineligible to become a FunctionElement!");
                continue;
            }
#endif

            Action deleg8; // "delegate" is a keyword
            try
            {
                deleg8 = (Action)method.CreateDelegate(typeof(Action));
            }
            catch
            {
#if DEBUG
                JeviLib.Warn($"Failed to create FunctionElement delegate for {type.FullName}.{method.Name}, this is likely because it returns a value, which is fine. Falling back to the slower MethodInfo.Invoke");
#endif
                deleg8 = () => { method.Invoke(null, NoParameters); };
            }

            Page targetCategory = CreateSubmenu(bmCat, pref);

            targetCategory.CreateFunction(Utilities.GenerateFriendlyMemberName(method.Name), pref.color, deleg8);
#if DEBUG
            JeviLib.Log($"Successfully created FunctionElement for {type.FullName}.{method.Name}");
#endif
        }
    }

    private static Page CreateSubmenu(Page bmRoot, Pref pref)
    {
        if (string.IsNullOrWhiteSpace(pref.desc)) return bmRoot;
        string[] pathParts = pref.desc.Split('/');

        Page ret = bmRoot;
        for (int i = 0; i < pathParts.Length; i++)
        {
            Page target = ret.Elements.OfType<Page>().FirstOrDefault(mc => mc.Name == pathParts[i]);
            if (target == null)
            {
#if DEBUG
                JeviLib.Log($"Creating submenu '{pathParts[i]}' on Menu '{bmRoot.Name}' for part {i + 1} of submenu path {pref.desc}");
#endif
                target = bmRoot.CreatePage(pathParts[i], pref.color);
            }

            ret = target;
        }

        return ret;
    }
}
