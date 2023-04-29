using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Patching;

internal static class PlayerPrefsExceptionUses
{
    // PlayerPrefsException was stripped, so Unhollower trying to instantiate (and throw) them throws a MissingMethodException.
    // Reimplement functionality of affected methods.
    public static void Init()
    {
        // todo: crashes game on startup. find out why/try to find workaround.
        // optionally just harmony transpile the managed interop to throw a normal exception
        return;
        Redirect.FromDelegate(PlayerPrefs.SetInt, SetIntButBetter, true);
        Redirect.FromDelegate(PlayerPrefs.SetFloat, SetFloatButBetter, true);
        Redirect.FromDelegate(PlayerPrefs.SetString, SetStringButBetter, true);
    }

    static void SetIntButBetter(string key, int value)
    {
        if (!PlayerPrefs.TrySetInt(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }

    static void SetFloatButBetter(string key, float value)
    {
        if (!PlayerPrefs.TrySetFloat(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }

    static void SetStringButBetter(string key, string value)
    {
        if (!PlayerPrefs.TrySetSetString(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }
}
