using ModThatIsNotMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil;

/// <summary>
/// Contains extension methods to act on various different types of object.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Resets a transform - that is, setting its scale to <see cref="Vector3.one"/> and setting its (local) position and rotation to <see cref="Vector3.zero"/> and <see cref="Quaternion.identity"/>, respectively.
    /// </summary>
    /// <param name="transform"></param>
    public static void Reset(this Transform transform)
    {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Returns whether or not a Transform is within the hierarchy of another
    /// </summary>
    /// <param name="t"></param>
    /// <param name="parentName"></param>
    /// <returns>Whether or not the GameObject is or is not in the hierarchy of another.</returns>
    public static bool InHierarchyOf(this Transform t, string parentName)
    {
        if (t.name == parentName)
            return true;

        if (t.parent == null)
            return false;

        t = t.parent;

        return InHierarchyOf(t, parentName);
    }

    /// <summary>
    /// Determines if a Transform is within the hierarchy of the RigManager - This is useful because the Hover Junkers sanfbox doesn't set the RigManager as the root of its hierarchy.
    /// <para>Use this to determine whether or not a GameObject is a part of the player.</para>
    /// </summary>
    /// <param name="t"></param>
    /// <returns>Whether or not a Transform is in the Hierarchy of a RigManager</returns>
    public static bool IsChildOfRigManager(this Transform t) => InHierarchyOf(t, "[RigManager (Default Brett)]");

    /// <summary>
    /// literal foreach.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="fun">the lambda of what to do</param>
    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> fun)
    {
        foreach (T item in sequence) fun(item);
    }


    /// <summary>
    /// literal foreach. in a try-catch though. debug builds log the error if one occurs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="fun">the lambda of what to do</param>
    public static void ForEachSafe<T>(this IEnumerable<T> sequence, Action<T> fun)
    {
        foreach (T item in sequence) 
        {
            try 
            { 
                fun(item); 
            }
#if !DEBUG
            catch { }
#else
            catch (Exception ex)
            {
                JeviLib.Warn("Exception whilst safe foreaching - still continuing though. The exception is below.");
                JeviLib.Warn(ex);
            }
#endif
        }
    }

    /// <summary>
    /// Redirect to the other override of Interpolate.
    /// </summary>
    public static float Interpolate(KeyValuePair<float, float> kvp, float time) => Interpolate((kvp.Key, kvp.Value), time);

    /// <summary>
    /// Smoothly interpolates between two values, clamping <paramref name="time"/> between 0 and 1.
    /// </summary>
    /// <param name="tuple">Tuple's Item1 will be the start value, Item2 will be the end value.</param>
    /// <param name="time">The position in time you want to get.</param>
    /// <param name="ease">Whether or not to ease in and out. Defaults to <see langword="true"/>.</param>
    /// <returns>A cosine-smoothed interpolation between Item1 and Item2, unless <paramref name="ease"/> is false.</returns>
    public static float Interpolate(this (float, float) tuple, float time, bool ease = true)
    {
        float clampTime = Mathf.Clamp(time, 0, 1);
        float diff = tuple.Item2 - tuple.Item1;
        if (ease)
        {
            float correctedCos = -(Mathf.Cos(clampTime * Const.FPI) - 1) / 2;
            return tuple.Item1 + (diff * correctedCos);
        }
        else return tuple.Item1 + (diff * clampTime);
    }

    /// <summary>
    /// <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/> as an extension method.
    /// </summary>
    /// <param name="go">The GameObject to be destroyed.</param>
    public static void Destroy(this GameObject go) => GameObject.Destroy(go);

    /// <summary>
    /// Takes the embedded resource out of the Assembly <paramref name="assembly"/> from the manifest path <paramref name="resourcePath"/> and passes it into the function <paramref name="whatToDoWithResource"/>
    /// </summary>
    /// <param name="assembly">The assembly containing the resource</param>
    /// <param name="resourcePath">The resource path. Debug builds check for this path in the assembly and throw a more descriptive Exception. Release builds will throw <see cref="NullReferenceException"/></param>
    /// <param name="whatToDoWithResource">The method or lambda to run, taking in the parameter of the raw bytes of the resource</param>
    /// <exception cref="System.IO.FileNotFoundException">The resource path does not point to a valid resource</exception>
    public static void UseEmbeddedResource(this System.Reflection.Assembly assembly, string resourcePath, Action<byte[]> whatToDoWithResource)
    {
#if DEBUG
        string[] paths = assembly.GetManifestResourceNames();
        if (!paths.Any(s => s == resourcePath))
        {
            foreach (string path in paths) JeviLib.Log(path);
            throw new System.IO.FileNotFoundException($"The specified resource was not found in the Assembly {assembly.GetName().Name}. All resource paths have been logged, use these to get an existent resource path.", resourcePath);
        }
#endif

        whatToDoWithResource(GetEmbeddedResource(assembly, resourcePath));
    }

    /// <summary>
    /// Takes the embedded resource out of the Assembly <paramref name="assembly"/> from the manifest path <paramref name="resourcePath"/> and returns it as a byte array.
    /// </summary>
    /// <param name="assembly">The assembly containing the resource</param>
    /// <param name="resourcePath">The resource path. Debug builds check for this path in the assembly and throw a more descriptive Exception. Release builds will throw <see cref="NullReferenceException"/></param>
    /// <exception cref="System.IO.FileNotFoundException">The resource path does not point to a valid resource</exception>
    public static byte[] GetEmbeddedResource(this System.Reflection.Assembly assembly, string resourcePath)
    {
#if DEBUG
        string[] paths = assembly.GetManifestResourceNames();
        if (!paths.Any(s => s == resourcePath))
        {
            foreach (string path in paths) JeviLib.Log(path);
            throw new System.IO.FileNotFoundException($"The specified resource was not found in the Assembly {assembly.GetName().Name}. All resource paths have been logged, use these to get an existent resource path.", resourcePath);
        }
#endif

        using System.IO.Stream stream = assembly.GetManifestResourceStream(resourcePath);
        using System.IO.MemoryStream mStream = new((int)stream.Length); // Don't overallocate memory to the mstream (?).
        // Copy the stream to a memorystream. Why? Don't know, ask .NET 4.7.2 designers.
        stream.CopyTo(mStream);
        return mStream.ToArray();
    }

    /// <summary>
    /// Returns a random element from the given enumerable <paramref name="sequence"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence">Source enumerable</param>
    /// <returns>A random element from <paramref name="sequence"/></returns>
    public static T Random<T>(this IEnumerable<T> sequence)
    {
        return sequence.ElementAt(UnityEngine.Random.Range(0, sequence.Count()));
    }

    // Random w/ IEnumerable doesn't check for arrays, only ICollection
    /// <summary>
    /// Returns a random element from the given array <paramref name="sequence"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence">Source array</param>
    /// <returns>A random element from <paramref name="sequence"/></returns>
    public static T Random<T>(this T[] sequence)
    {
        return sequence[UnityEngine.Random.Range(0, sequence.Length)];
    }

    /// <summary>
    /// Get the full path to the Transform <paramref name="t"/>, can be used with <see cref="Encoding.GetBytes(string)"/> to serialize and sync Transform paths.
    /// </summary>
    /// <param name="t"></param>
    /// <returns>A string containing the path to the Transform, like "/Sign example/Canvas/TextMeshPro" (this works with <see cref="GameObject.Find(string)"/>)</returns>
    public static string GetFullPath(this Transform t)
    {
        StringBuilder sb = new();
        sb.Append('/');
        sb.Append(t.name);
        while (t.parent != null)
        {
            t = t.parent;
            sb.Insert(0, t.name);
            sb.Insert(0, '/');
        }
        return sb.ToString();
    }

    /// <summary>
    /// Convert a Vector3 into an array of bytes, for serialization as part of a larger operation and/or Entanglement syncing.
    /// </summary>
    /// <param name="vec">The Vector3 to be serialized. Can be a Quaternion's Euler angles</param>
    /// <returns>A byte representation of the Vector <paramref name="vec"/></returns>
    public static byte[] ToBytes(this Vector3 vec)
    {
        byte[][] bytess = new byte[][]
        {
            BitConverter.GetBytes(vec.x),
            BitConverter.GetBytes(vec.y),
            BitConverter.GetBytes(vec.z)
        };
        return bytess.Flatten();
    }

    /// <summary>
    /// Joins a series of elements one after the other, without regard for reproducibility.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arrarr"></param>
    /// <returns></returns>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> arrarr)
    { //                                                         DAMN DANIEL ^
        IEnumerable<T> flatted = new List<T>(); // use ienumerable to avoid recasting constantly
        foreach (IEnumerable<T> item in arrarr)
        {
            flatted = flatted.Concat(item);
        }
        return flatted;
    }

    /// <summary>
    /// Joins an array of bytes directly one after another, without regard for reproduceability. Use this for things of fixed size, like byte representations of <see cref="float"/>s or <see cref="int"/>s.
    /// <para>This is used internally by <see cref="ToBytes(Vector3)"/>.</para>
    /// </summary>
    /// <param name="arrarr">The array of bytes to be concatenated.</param>
    /// <returns>A new array containing the elements of the source array, one after the other.</returns>
    public static byte[] Flatten(this byte[][] arrarr)
    {
        int totalToNow = 0;
        int sum = arrarr.Sum(arr => arr.Length);
        byte[] result = new byte[sum];

        for (int i = 0; i < arrarr.Length; i++)
        {
            Buffer.BlockCopy(arrarr[i], 0, result, totalToNow, arrarr[i].Length);
            totalToNow += arrarr[i].Length;
        }

        return result;
    }

    /// <summary>
    /// <see cref="string.Join(string, object[])"/> but as an extension method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="seq">The source collection, whose elements are to be joined.</param>
    /// <param name="delim">The string to be placed between each element. Defaults to ", ".</param>
    /// <returns>A string containing the strung (stringed?) elements separated by <paramref name="delim"/>, or <see cref="string.Empty"/> if <paramref name="seq"/> has no elements.</returns>
    public static string Join<T>(this IEnumerable<T> seq, string delim = ", ")
    {
        return string.Join(delim, seq);
    }

    /// <summary>
    /// Split <paramref name="source"/> into multiple IEnumerables with a maximum length of <paramref name="maxPerList"/>, however not all will be the max size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source collection, must not be null.</param>
    /// <param name="maxPerList">The maximum number of items to chunk by</param>
    /// <returns>The list of lists, with each inner list having a max Count of <paramref name="maxPerList"/></returns>
    public static IEnumerable<IEnumerable<T>> SplitList<T>(this IEnumerable<T> source, int maxPerList)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        IList<T> enumerable = source as IList<T> ?? source.ToList();
        if (!enumerable.Any())
        {
            return new List<IEnumerable<T>>();
        }
        return (new List<IEnumerable<T>>() { enumerable.Take(maxPerList) }).Concat(enumerable.Skip(maxPerList).SplitList<T>(maxPerList));
    }

    /// <summary>
    /// Serializes a Transform <paramref name="t"/>'s world space position and rotation.
    /// </summary>
    /// <param name="t">The transform to have its position and rotation serialized.</param>
    /// <returns>A byte array of length <c>24 (Vector3 size * 2)</c> containing, in order, the position and euler angles of the given Transform.</returns>
    public static byte[] SerializePosRot(this Transform t)
    {
        byte[] res = new byte[Const.SizeV3 * 2];

        t.position.ToBytes().CopyTo(res, 0);
        t.rotation.eulerAngles.ToBytes().CopyTo(res, Const.SizeV3);

        return res;
    }

    /// <summary>
    /// Applies a serialized world space position and rotation, from <see cref="SerializePosRot(Transform)"/>, to a Transform <paramref name="t"/>
    /// </summary>
    /// <param name="t">The transform </param>
    /// <param name="serializedPosRot">The byte array containing the serialized position and rotation</param>
    /// <param name="dontWarn">Only used in debug builds. Disables the warning about <c><paramref name="serializedPosRot"/>.Length</c> not being equal to the size of two Vector3's (6)</param>
    public static void DeserializePosRot(this Transform t, byte[] serializedPosRot, bool dontWarn = false)
    {
#if DEBUG
        if (!dontWarn) if (serializedPosRot.Length != Const.SizeV3 * 2) JeviLib.Warn("Deserializing posrot of unexpected length " + serializedPosRot.Length + "!!! This could be bad!!!");
#endif
        Vector3 pos = Utilities.DebyteV3(serializedPosRot);
        Quaternion rot = Quaternion.Euler(Utilities.DebyteV3(serializedPosRot, Const.SizeV3));
        t.SetPositionAndRotation(pos, rot);
    }

    /// <summary>
    /// A shorter way to call <see cref="Encoding.GetString(byte[], int, int)"/>, because the third parameter is annoying to write out.
    /// </summary>
    /// <param name="enc">The encoding to be used. My encoder of choice is <see cref="Encoding.ASCII"/>, because it's fast.</param>
    /// <param name="bytes">The byte array to decode into a string.</param>
    /// <param name="startOffset">The index to start reading bytes from.</param>
    /// <returns><paramref name="enc"/>.GetString(<paramref name="bytes"/>, <paramref name="startOffset"/>, <paramref name="bytes"/>.Length - <paramref name="startOffset"/>)</returns>
    public static string GetString(this Encoding enc, byte[] bytes, int startOffset)
    {
        return enc.GetString(bytes, startOffset, bytes.Length - startOffset);
    }

    /// <summary>
    /// Returns an enumerator for every child that the given GameObject <paramref name="go"/> has.
    /// <para>Debug builds throw an <see cref="InvalidOperationException"/> if the child count changes mid-iteration.</para>
    /// </summary>
    /// <param name="go">The GameObject to have its transforms children enumerated.</param>
    /// <returns>An IEnumerator for the GameObject's children that can be used with a foreach loop.</returns>
    public static IEnumerator<Transform> GetEnumerator(this GameObject go)
    {
        Transform t = go.transform;
        int childCount = t.childCount;
        for (int i = 0; i < childCount; i++)
        {
#if DEBUG
            if (t.childCount != childCount) throw new InvalidOperationException("The child count of a transform should not be modified while it is being enumerated!");
#endif
            yield return t.GetChild(i);
        }
    }

    /// <summary>
    /// A quicker way to use <see cref="Enumerable.Zip{TFirst, TSecond, TResult}(IEnumerable{TFirst}, IEnumerable{TSecond}, Func{TFirst, TSecond, TResult})"/>.
    /// <para>Throws if counts are not the same if <paramref name="throwOnUnequalCounts"/> is true (which it defaults to).</para>
    /// </summary>
    /// <typeparam name="T1">The first element in the tuple.</typeparam>
    /// <typeparam name="T2">The second element in the tuple.</typeparam>
    /// <param name="sequence">The sequence providing the first elements.</param>
    /// <param name="otherSeq">The sequence providing the second elements.</param>
    /// <param name="throwOnUnequalCounts">Whether to throw if <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/> is unequal.</param>
    /// <returns>A tuple enumerable with Item1 coming from <paramref name="sequence"/> and Item2 coming from <paramref name="otherSeq"/>.</returns>
    /// <exception cref="IndexOutOfRangeException"><see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/> is not the same for both.</exception>
    public static IEnumerable<(T1, T2)> Zip<T1,T2>(this IEnumerable<T1> sequence, IEnumerable<T2> otherSeq, bool throwOnUnequalCounts = true)
    {
        if (throwOnUnequalCounts && sequence.Count() != otherSeq.Count()) throw new IndexOutOfRangeException("Zipped enumerables must have the same length otherwise the operation will go out of bounds!");
        return sequence.Zip(otherSeq, (x, y) => (x, y));
    }

    /// <summary>
    /// An array version of <see cref="Zip{T1, T2}(IEnumerable{T1}, IEnumerable{T2}, bool)"/>, using .Length instead of .Count(). Uses a manual copy via a for-loop.
    /// <para>Throws if lengths are not the same.</para>
    /// </summary>
    /// <typeparam name="T1">The first element in the tuple.</typeparam>
    /// <typeparam name="T2">The second element in the tuple.</typeparam>
    /// <param name="arr"></param>
    /// <param name="otherArr"></param>
    /// <returns>A tuple array with the tuple's Item1 coming from <paramref name="arr"/> and Item2 coming from <paramref name="otherArr"/>.</returns>
    public static (T1, T2)[] Zip<T1, T2>(this T1[] arr, T2[] otherArr)
    {
        if (arr.Length != otherArr.Length) throw new IndexOutOfRangeException("Zipped arrays must have the same length otherwise the operation will go out of bounds!");
        (T1, T2)[] res = new (T1, T2)[arr.Length];
        
        for (int i = 0; i < arr.Length; i++)
        {
            res[i] = (arr[i], otherArr[i]);
        }
        return res;
    }

    /// <summary>
    /// Filters out <see langword="null"/>s from the given <paramref name="sequence"/> using the != operator, so it <i>should</i> also filter out garbage collected <see cref="UnityEngine.Object"/>s.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
    /// <param name="sequence"></param>
    /// <returns></returns>
    public static IEnumerable<T> NoNull<T>(IEnumerable<T> sequence)
    {
        return sequence.Where(o => o != null);
    }

    /// <summary>
    /// Splits a collection according to how many processors are in the system, assuming the system has hyperthreading. (Via <see cref="SystemInfo.processorCount"/>)
    /// <para>Not all elements of the resulting IEnumerable are guaranteed to have the same length. The last item may be smaller than the rest.</para>
    /// </summary>
    /// <typeparam name="T">Any.</typeparam>
    /// <param name="sequence">The sequence to be split.</param>
    /// <param name="onePerCore">Divides processorCount by 2 so there's one per core instead of one per thread.</param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> SplitByProcessors<T>(this IEnumerable<T> sequence, bool onePerCore = false)
    {
        int splitCount = SystemInfo.processorCount;
        if (onePerCore) splitCount /= 2;
        int maxCount = Mathf.CeilToInt(sequence.Count() / (float)splitCount);

        return sequence.SplitList(maxCount);
    }

    /// <summary>
    /// Returns the first element from <paramref name="sequence"/> whose <paramref name="selector"/> result matches the result of <see cref="Enumerable.Min(IEnumerable{int})"/>.
    /// </summary>
    /// <typeparam name="T">Any. This Is A Threat.</typeparam>
    /// <param name="sequence">The sequence to be .Min'd and .First'd</param>
    /// <param name="selector">The Func to pass into <see cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int})"/> and to use to compare against the result in <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>.</param>
    /// <returns>The first element in the sequence that matches the result of <see cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int})"/></returns>
    public static T FirstMin<T>(this IEnumerable<T> sequence, Func<T, int> selector)
    {
        int min = sequence.Min(selector);
        return sequence.First(t => selector(t) == min);
    }

    /// <summary>
    /// If <paramref name="paramTypes"/> is null, attempt to resolve conflicts between methods by filtering the results of <see cref="Type.GetMethods(BindingFlags)"/>, returning a method with the smallest number of parameters.
    /// <para>Otherwise, <see cref="Type.GetMethod(string, BindingFlags, Binder, CallingConventions, Type[], ParameterModifier[])"/> with the BindingFlags parameter set to <see cref="Const.AllBindingFlags"/>.</para>
    /// </summary>
    /// <param name="type">The type with a potentially overridden method named <paramref name="methodName"/>.</param>
    /// <param name="methodName">The name of the method. Can be overridden.</param>
    /// <param name="paramTypes">In case you want more specificity, you can specify an array of types correspoding to the parameter types.</param>
    /// <returns>An instance of <see cref="MethodInfo"/> if there's at least one method matching the criteria, or <see langword="null"/>.</returns>
    public static MethodInfo GetMethodEasy(this Type type, string methodName, Type[] paramTypes = null)
    {
        if (paramTypes == null)
        {
            MethodInfo[] minfs = type.GetMethods(Const.AllBindingFlags).Where(m => m.Name == methodName).ToArray();

            if (minfs.Length == 0)
                return null;        

            return minfs.FirstMin(m => m.GetParameters().Length);
        }
        else return type.GetMethod(methodName, Const.AllBindingFlags, null, CallingConventions.Any, paramTypes, null);
    }

    /// <summary>
    /// Get all overrides for a given method name.
    /// </summary>
    /// <param name="type">The type with the method(s)</param>
    /// <param name="methodName">The name of the method overridden</param>
    /// <returns>All methods on the given type that have the given name.</returns>
    public static MethodInfo[] GetMethods(this Type type, string methodName)
    {
        return (from MethodInfo method in type.GetMethods(Const.AllBindingFlags)
                where method.Name == methodName
                select method).ToArray();
    }

    /// <summary>
    /// i didnt like the extra parentheses so heres an easier way to add items to a tuple list.
    /// </summary>
    /// <typeparam name="T1">Type of <paramref name="item1"/>.</typeparam>
    /// <typeparam name="T2">Type of <paramref name="item2"/>.</typeparam>
    /// <param name="list">The tuple list.</param>
    /// <param name="item1">Tuple's first item.</param>
    /// <param name="item2">Tuple's second item.</param>
    public static void Add<T1, T2>(this List<(T1, T2)> list, T1 item1, T2 item2) => list.Add((item1, item2));

    /// <summary>
    /// Uses <see cref="MemberInfo.DeclaringType"/> and <see cref="Type.GetMethod(string, BindingFlags)"/> with <see cref="Const.AllBindingFlags"/> to get an instance of <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="mb">Any MethodBase declared by a Type.</param>
    /// <returns>An instnace of <see cref="MethodInfo"/>, or <see langword="null"/> for whatever reason. Should always return a methodinfo tho.</returns>
    public static MethodInfo ToInfo(this MethodBase mb)
    {
        Type type = mb.DeclaringType;
        return type.GetMethod(mb.Name, Const.AllBindingFlags);
    } 

    /// <summary>
    /// Waits for a <see cref="NotificationData"/> to disappear.
    /// </summary>
    /// <param name="notif">Any notification data</param>
    /// <returns>A yield awaitable enumerator that waits for the notification to finish.</returns>
    public static IEnumerator WaitForEnd(this NotificationData notif)
    {
        while (notif.timeRemaining > 0) yield return null;
    }

    /// <summary>
    /// <b>I</b>s <b><see langword="N"/></b><see langword="ull"/> <b>O</b>r <b>C</b>ollected
    /// </summary>
    /// <param name="obj">Any Unity object.</param>
    /// <returns></returns>
    public static bool INOC(this UnityEngine.Object obj)
    {
        return obj is null || obj.WasCollected || obj == null;
    }
}
