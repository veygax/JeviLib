using System.Collections.Generic;
using UnityEngine;

namespace Jevil;

/// <summary>
/// ComponentCache, but static, in the Mono domain, with helper methods, and, of course, documentation.
/// </summary>
/// <typeparam name="T">Any component, can be injected or native to the IL2CPP domain.</typeparam>
public static class Instances<T> where T : Component
{
    static readonly Dictionary<GameObject, T> cache = new();

    static Instances()
    {
#if DEBUG
        JeviLib.Log("Initialized Instances Cache of " + typeof(T).Name);
#endif
    }

    /// <summary>
    /// Attempts to get the component from the cache, get it using <see cref="GameObject.GetComponent{T}"/>, and if all else fails, add the component to the GameObject and add it to the cache.
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T GetOrAdd(GameObject go)
    {
        T ret = Get(go);

        if (ret == null)
        {
            ret = go.AddComponent<T>();
            cache.Add(go, ret);
        }

        return ret;
    }

    /// <summary>
    /// Add the component as attached to the GameObject <paramref name="go"/>.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> that the cache should check for.</param>
    /// <param name="component"></param>
    /// <returns>Whether there was already a component of type <typeparamref name="T"/> on the GameObject <paramref name="go"/>.
    /// <br><see langword="true"/> if <paramref name="go"/> already existed in the cache, <see langword="false"/> if not or if <paramref name="go"/> was <see langword="null"/>.</br>
    /// </returns>
    public static bool AddManual(GameObject go, T component)
    {
        if (go == null) return false;

        bool ret = cache.ContainsKey(go);
        cache[go] = component;
        return ret;
    }

    /// <summary>
    /// Gets the <typeparamref name="T"/> component on <typeparamref name="T"/>, first trying the cache, otherwise getting it via <see cref="GameObject.GetComponent{T}"/>.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to be checked for a component of type <typeparamref name="T"/>.</param>
    /// <returns>The component, or <see langword="null"/> if it wasn't in the cache or </returns>
    public static T Get(GameObject go)
    {
        if (cache.TryGetValue(go, out T obj)) return obj;

        T val = go.GetComponent<T>();
        if (val != null) cache[go] = val;
        return val;
    }

    /// <summary>
    /// Get the component from the cache using the Transform's <see cref="Component.gameObject"/>.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static T Get(Transform t) => Get(t.gameObject);

    /// <summary>
    /// Whether or not the cache has <paramref name="go"/> in it.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to check the cache for.</param>
    /// <returns><see langword="true"/> if cache[<paramref name="go"/>] exists, otherwise <see langword="false"/>.</returns>
    public static bool Has(GameObject go) => cache.ContainsKey(go);

    /// <summary>
    /// Finds the first Transform that is a parent of <paramref name="t"/> that has a component of the given type.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Transform HasUpwards(Transform t)
    {
        bool has = Has(t.gameObject);

        if (has)
            return t;

        if (t.parent == null)
            return null;

        return HasUpwards(t.parent);
    }

    private static T GetUpwardsImpl(Transform t)
    {
        T ret = Get(t);

        if (ret != null)
            return ret;

        if (t.parent == null)
            return null;

        return GetUpwardsImpl(t.parent);
    }

    /// <summary>
    /// Recursively checks if any of Transform <paramref name="t"/>'s parents have a component of type <typeparamref name="T"/>.
    /// <para>Does a first pass to see if anything above has a cached component using <see cref="HasUpwards(Transform)"/>.</para>
    /// <br>Because of this, a cached component will be given priority over one that is <i>technically</i> closer in the hierarchy to the given Transform.</br>
    /// </summary>
    /// <param name="t">The Transforms whose parents to check.</param>
    /// <returns>The first component found going upwards from <paramref name="t"/>, or null if there are none.</returns>
    public static T GetUpwards(Transform t)
    {
        Transform hup = HasUpwards(t);
        if (hup != null) return Get(hup.gameObject);
        else return GetUpwardsImpl(t);
    }


    /// <summary>
    /// Recursively checks if any of GameObject <paramref name="go"/>'s parents have a component of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="go">The GameObject whose parents to check.</param>
    /// <returns>The first component found going upwards from <paramref name="go"/>, or null if there are none.</returns>
    public static T GetUpwards(GameObject go) => GetUpwards(go.transform);
}
