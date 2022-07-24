using MelonLoader;
using ModThatIsNotMod.BoneMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Prefs;

internal static class PrefsInternal
{
    public static void RegisterPreferences<T>(string categoryName, bool prefSubcategory, Color categoryColor) => RegisterPreferences(typeof(T), categoryName, prefSubcategory, categoryColor);

    public static PrefEntries RegisterPreferences(Type type, string categoryName, bool prefSubcategory, Color categoryColor)
    {
        MelonPreferences_Category mpCat = MelonPreferences.CreateCategory(categoryName);
        MenuCategory bmCat = MenuManager.CreateCategory(categoryName, categoryColor);
        PrefEntries ret = new(mpCat, bmCat);
        if (prefSubcategory) bmCat = bmCat.CreateSubCategory(Preferences.prefSubcategoryName, categoryColor);

#if DEBUG
        var instanceFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        var instancePrefs = instanceFields.Where(f => f.GetCustomAttribute<Pref>() != null);
        if (instancePrefs.Count() != 0)
        {
            JeviLib.Warn($"The type {type.Namespace}.{type.Name} declares instanced preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instancePrefs) JeviLib.Warn(" - " + field.Name);
        }
#endif

        FieldInfo[] staticFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        if (staticFields.Length == 0) return ret;

        foreach (FieldInfo field in staticFields)
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
                //field.SetValue(entry.)
                bmCat.CreateStringElement(readableName, ep.color, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType == typeof(bool))
            {
                var entry = SetEntry(mpCat, field, out bool toSet, ep.desc);
                bmCat.CreateBoolElement(readableName, ep.color, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType == typeof(Color))
            {
                var entry = SetEntry(mpCat, field, out Color toSet, ep.desc);
                bmCat.CreateColorElement(readableName, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); });
            }
            else if (fieldType.IsEnum)
            {
#if DEBUG
                if (!string.IsNullOrEmpty(ep.desc)) JeviLib.Warn($"Descriptions are not allowed for enum preferences - the description '{ep.desc}' on {type.Namespace}.{type.Name}.{field.Name} will not be put in MelonPreferences.cfg");
#endif
                Enum dv = (Enum)field.GetValue(null);
                var entry = mpCat.CreateEntry<string>(field.Name, dv.ToString(), description: $"Options: {string.Join(", ", Enum.GetNames(fieldType))}");
                Enum toSet = dv; // if the two are different
                try
                {
                    toSet = (Enum)Enum.Parse(fieldType, entry.Value);
                }
                catch
                {
                    JeviLib.Warn($"Failed to parse '{entry.Value}' as a value in the enum {type.Name} for the class {type.Name}'s preference {field.Name}");
                    JeviLib.Warn("Replacing it with its default value of " + dv);
                    entry.Value = dv.ToString();
                }
                bmCat.CreateEnumElement(readableName, ep.color, toSet, val => { field.SetValue(null, val); entry.Value = val.ToString(); mpCat.SaveToFile(false); });
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.Name}.{field.Name} is of un-bonemenu-able (or un-melonpreferences-able) type {type.Name}! This is no good!");
            }
#endif
        }


#if DEBUG
        var instanceRanges = instanceFields.Where(f => f.GetCustomAttribute<RangePref>() != null);
        if (instanceRanges.Count() != 0)
        {
            JeviLib.Warn($"The type {type.Namespace}.{type.Name} declares instanced range preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instanceRanges) JeviLib.Warn(" - " + field.Name);
        }
#endif

        foreach (var field in staticFields)
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
                bmCat.CreateIntElement(readableName, Color.white, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); }, (int)rp.inc, (int)rp.low, (int)rp.high);
            }
            else if (field.FieldType == typeof(float))
            {
                var entry = SetEntry(mpCat, field, out float toSet, $"{rp.low} to {rp.high}");
                bmCat.CreateFloatElement(readableName, Color.white, toSet, val => { field.SetValue(null, val); entry.Value = val; mpCat.SaveToFile(false); }, rp.inc, rp.low, rp.high);
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.Name}.{field.Name} is of un-range-able type {field.FieldType.Name}! This is no good!");
            }
            JeviLib.Log("Successfully created range preference");
#endif
        }

#if DEBUG
        MethodInfo[] instancedMethods = type.GetMethods(Const.AllBindingFlags | BindingFlags.Static);
        JeviLib.Warn($"The type {type.Namespace}.{type.Name} declares instanced method preferences, this is not allowed!");
        JeviLib.Warn($"These methods are: ");
        foreach (MethodInfo method in instancedMethods) JeviLib.Warn(" - " + method.Name);
#endif

        MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
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
                deleg8 = () => { method.Invoke(null, new object[0]); };
            }
#if DEBUG

#endif
            bmCat.CreateFunctionElement(Utilities.GenerateFriendlyMemberName(method.Name), pref.color, deleg8);
#if DEBUG
            JeviLib.Log($"Successfully created FunctionElement for {type.FullName}.{method.Name}");
#endif
        }

#if DEBUG
        JeviLib.Log($"Created {mpCat.Entries.Count} entries in the preference category for {type.Name}");
#endif
        if (mpCat.Entries.Count != 0) mpCat.SaveToFile(false);

        return ret;
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
    
}
