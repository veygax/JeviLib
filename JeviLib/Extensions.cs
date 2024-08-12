using BoneLib;
using Il2CppCysharp.Threading.Tasks;
using Jevil.Spawning;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Rig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.SceneStreaming;
using System.Xml.Serialization;
using Il2CppSLZ.Marrow;
using Il2CppSystem;

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
    public static bool IsChildOfRigManager(this Transform t) => InHierarchyOf(t, Const.RigManagerName);

    /// <summary>
    /// literal foreach.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="fun">the lambda of what to do</param>
    public static void ForEach<T>(this IEnumerable<T> sequence, System.Action<T> fun)
    {
        foreach (T item in sequence) fun(item);
    }


    /// <summary>
    /// literal foreach. in a try-catch though. debug builds log the error if one occurs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="fun">the lambda of what to do</param>
    public static void ForEachSafe<T>(this IEnumerable<T> sequence, System.Action<T> fun)
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
            catch (System.Exception ex)
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
    public static void UseEmbeddedResource(this System.Reflection.Assembly assembly, string resourcePath, System.Action<byte[]> whatToDoWithResource)
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
    public static unsafe byte[] ToBytes(this Vector3 vec)
    {
        byte[] ret = new byte[Const.SizeV3];

        fixed(byte* arrayPtr = ret)
        {
            *(float*)arrayPtr = vec.x;
            *(float*)(arrayPtr + sizeof(float)) = vec.y;
            *(float*)(arrayPtr + sizeof(float) * 2) = vec.z;
        }
        
        return ret;
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
            System.Buffer.BlockCopy(arrarr[i], 0, result, totalToNow, arrarr[i].Length);
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
        if (source == null) throw new System.ArgumentNullException(nameof(source));
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
            if (t.childCount != childCount) throw new System.InvalidOperationException("The child count of a transform should not be modified while it is being enumerated!");
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
        if (throwOnUnequalCounts && sequence.Count() != otherSeq.Count()) throw new System.IndexOutOfRangeException("Zipped enumerables must have the same length otherwise the operation will go out of bounds!");
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
        if (arr.Length != otherArr.Length) throw new System.IndexOutOfRangeException("Zipped arrays must have the same length otherwise the operation will go out of bounds!");
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
    public static IEnumerable<T> NoNull<T>(this IEnumerable<T> sequence)
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
    /// <returns>An <see cref="IEnumerable{T}"/> with as many elements as there are in <see cref="SystemInfo.processorCount"/></returns>
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
    public static T FirstMin<T>(this IEnumerable<T> sequence, System.Func<T, int> selector)
    {
        int min = sequence.Min(selector);
        return sequence.First(t => selector(t) == min);
    }

    /// <summary>
    /// If <paramref name="paramTypes"/> is null, attempt to resolve conflicts between methods by filtering the results of <see cref="System.Type.GetMethods(BindingFlags)"/>, returning a method with the smallest number of parameters.
    /// <para>Otherwise, <see cref="System.Type.GetMethod(string, BindingFlags, Binder, CallingConventions, System.Type[], ParameterModifier[])"/> with the BindingFlags parameter set to <see cref="Const.AllBindingFlags"/>.</para>
    /// </summary>
    /// <param name="type">The type with a potentially overridden method named <paramref name="methodName"/>.</param>
    /// <param name="methodName">The name of the method. Can be overridden.</param>
    /// <param name="paramTypes">In case you want more specificity, you can specify an array of types correspoding to the parameter types.</param>
    /// <returns>An instance of <see cref="MethodInfo"/> if there's at least one method matching the criteria, or <see langword="null"/>.</returns>
    public static MethodInfo GetMethodEasy(this System.Type type, string methodName, System.Type[] paramTypes = null)
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
    public static MethodInfo[] GetMethods(this System.Type type, string methodName)
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
    /// Uses <see cref="MemberInfo.DeclaringType"/> and <see cref="System.Type.GetMethod(string, BindingFlags)"/> with <see cref="Const.AllBindingFlags"/> to get an instance of <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="mb">Any MethodBase declared by a Type.</param>
    /// <returns>An instnace of <see cref="MethodInfo"/>, or <see langword="null"/> for whatever reason. Should always return a methodinfo tho.</returns>
    public static MethodInfo ToInfo(this MethodBase mb)
    {
        System.Type type = mb.DeclaringType;
        return type.GetMethod(mb.Name, Const.AllBindingFlags);
    } 

    /// <summary>
    /// Returns whether an object in the IL2CPP domain <b>I</b>s <b><see langword="N"/></b><see langword="ull"/> <b>O</b>r <b>C</b>ollected.
    /// </summary>
    /// <param name="obj">Any Unity object.</param>
    /// <returns></returns>
    public static bool INOC(this UnityEngine.Object obj)
    {
        System.IntPtr ptr = System.IntPtr.Zero;
        try { ptr = obj.Pointer; }
        catch { }
        // This shit better fucking work this time
        return obj is null || ptr == System.IntPtr.Zero || obj.WasCollected || obj == null;
    }

    /// <summary>
    /// Shortcut to <see cref="Barcodes.ToBarcodeString(JevilBarcode)"/>
    /// </summary>
    /// <param name="jBarcode">Who the hell looks at the first parameter of extension methods? Weirdo.</param>
    /// <returns>A Barcode ID string corresponsing to the JevilBarcode value.</returns>
    public static string ToString(this JevilBarcode jBarcode)
    {
        return Barcodes.ToBarcodeString(jBarcode);
    }

    /// <summary>
    /// <i>Assuming the given barcode represents a spawnable</i>, spawns an instance of whatever the barcode represents.
    /// <para>Shortcut to <see cref="Barcodes.SpawnAsync(JevilBarcode, Vector3, Quaternion)"/></para>
    /// </summary>
    /// <param name="spawnableBarcode">A barcode. Assumed to represent a spawnable.</param>
    /// <param name="position">Worldspace position</param>
    /// <param name="rotation">Worldspace rotation</param>
    public static void Spawn(this JevilBarcode spawnableBarcode, Vector3 position, Quaternion rotation)
    {
        Barcodes.SpawnAsync(spawnableBarcode, position, rotation);
    }

    /// <summary>
    /// Adds a velocity change to all rigidbodies on the player.
    /// </summary>
    /// <param name="physRig">who looks at the first param of an extension method ??? take this https://www.youtube.com/watch?v=O-2n-Y-uSRg</param>
    /// <param name="velocity">A worldspace vector representing the change in velocity for each of the player's rigidbodies.</param>
    public static void AddVelocityChange(this PhysicsRig physRig, Vector3 velocity)
    {
        physRig.torso.rbChest.AddForce(velocity, ForceMode.VelocityChange);
        physRig.torso.rbPelvis.AddForce(velocity, ForceMode.VelocityChange);
        physRig.torso.rbSpine.AddForce(velocity, ForceMode.VelocityChange);
        physRig.torso.rbHead.AddForce(velocity, ForceMode.VelocityChange);
        physRig.rbFeet.AddForce(velocity, ForceMode.VelocityChange);
        physRig.rbKnee.AddForce(velocity, ForceMode.VelocityChange);
        
        // i recall the "rb" field being null in BW, so im just gonna cover my bases
        //todo: check if Hand.rb is null
        try
        {
            Player.LeftHand.rb.AddForce(velocity, ForceMode.VelocityChange);
            Player.RightHand.rb.AddForce(velocity, ForceMode.VelocityChange);
        }
        catch { }
    }

    /// <summary>
    /// Spawns whatever a crate represents.
    /// </summary>
    /// <param name="crate">oo ee oo aa aa ting tang walla walla bing bang</param>
    /// <param name="pos">Worldspace position to spawn the object</param>
    /// <param name="rot">Worldspace rotation to give the object</param>
    public static void Spawn(this SpawnableCrate crate, Vector3 pos, Quaternion rot)
    {
        Spawnable spawn = Barcodes.ToSpawnable(crate.Barcode.ID);
        AssetSpawner.Spawn(spawn, pos, rot, new Il2CppSystem.Nullable<Vector3>(), null, false, null, null);
    }

    /// <summary>
    /// Spawns whatever a crate represents.
    /// </summary>
    /// <param name="crate">oo ee oo aa aa ting tang walla walla bing bang</param>
    /// <param name="pos">Worldspace position to spawn the object</param>
    /// <param name="rot">Worldspace rotation to give the object</param>
    public static UniTask<Poolee> SpawnAsync(this SpawnableCrate crate, Vector3 pos, Quaternion rot)
    {
        Spawnable spawn = Barcodes.ToSpawnable(crate.Barcode.ID);
        return AssetSpawner.SpawnAsync(spawn, pos, rot, new Il2CppSystem.Nullable<Vector3>(), null, false, null, null);
    }

    /// <summary>
    /// Spawns whatever a crate represents.
    /// </summary>
    /// <param name="spawnable">oo ee oo aa aa ting tang walla walla bing bang</param>
    /// <param name="pos">Worldspace position to spawn the object</param>
    /// <param name="rot">Worldspace rotation to give the object</param>
    /// <param name="enableOnSpawn"></param>
    public static void Spawn(this Spawnable spawnable, Vector3 pos, Quaternion rot, bool? enableOnSpawn = null)
    {
        System.Action<GameObject> callback = null;
        if (enableOnSpawn.HasValue) 
            callback = new System.Action<GameObject>(go => go.SetActive(enableOnSpawn.Value));
        
        AssetSpawner.Spawn(spawnable, pos, rot, new Il2CppSystem.Nullable<Vector3>(), null, false, null, callback, null);
    }

    /// <summary>
    /// Spawns whatever a crate represents.
    /// </summary>
    /// <param name="spawnable">oo ee oo aa aa ting tang walla walla bing bang</param>
    /// <param name="pos">Worldspace position to spawn the object</param>
    /// <param name="rot">Worldspace rotation to give the object</param>
    public static UniTask<Poolee> SpawnAsync(this Spawnable spawnable, Vector3 pos, Quaternion rot)
    {
        return AssetSpawner.SpawnAsync(spawnable, pos, rot, new Il2CppSystem.Nullable<Vector3>(), null, false, null, null);
    }

    /// <summary>
    /// Spawns another <see cref="Poolee"/> from the given <see cref="Poolee"/>'s <see cref="Poolee"/>.
    /// </summary>
    /// <param name="asspoole">https://media.tenor.com/images/1af43e40653cb90765d776fedf0186cf/tenor.gif</param>
    /// <param name="pos">Worldspace position to spawn the object</param>
    /// <param name="rot">Worldspace rotation to give the object</param>
    public static void Dupe(this Poolee asspoole, Vector3 pos, Quaternion rot)
    {
        asspoole.SpawnableCrate.Spawn(pos, rot);
    }

    /// <summary>
    /// Allows you to use LINQ with IL2CPP dictionaries.
    /// </summary>
    /// <typeparam name="T1">Type 1</typeparam>
    /// <typeparam name="T2">Type 2</typeparam>
    /// <param name="enumerable">Any IL2CPP dictionary</param>
    /// <returns>An IEnumerable that can be used with normal <see cref="System.Linq"/></returns>
    public static IEnumerable<KeyValuePair<T1, T2>> MonoEnumerable<T1, T2>(this Il2CppSystem.Collections.Generic.Dictionary<T1, T2> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return new KeyValuePair<T1, T2>(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Allows you to use LINQ with IL2CPP lists.
    /// </summary>
    /// <typeparam name="T">Type 2</typeparam>
    /// <param name="enumerable">Any IL2CPP dictionary</param>
    /// <returns>An IEnumerable that can be used with normal <see cref="System.Linq"/></returns>
    public static IEnumerable<T> MonoEnumerable<T>(this Il2CppSystem.Collections.Generic.List<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item;
        }
    }

    /// <summary>
    /// A manual unstripping of <c><see cref="AndroidJavaObject"/>.GetStatic{<typeparamref name="FieldType"/>}(<see langword="string"/> <paramref name="fieldName"/>)</c>
    /// <para>Retrieves the value of a static field on an <see cref="AndroidJavaObject"/> (typically an <see cref="AndroidJavaClass"/>).</para>
    /// <para><i>ARRAYS ARE NOT SUPPORTED! If you wish to implement array support, you can do so by going to the JeviLib GitHub repository.</i></para>
    /// </summary>
    /// <typeparam name="FieldType">A blittable type (AKA: A primitive, like <see cref="int"/> or <see cref="long"/>), <see cref="AndroidJavaObject"/>, or <see cref="string"/>.</typeparam>
    /// <param name="ajo">The android java object to access the static field of.</param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    /// <exception cref="Il2CppConversionException">Attempted to fetch an array. This is not currently supported.</exception>
    /// <exception cref="Exception">Unknown field type. </exception>
    public static FieldType GetStatic<FieldType>(this AndroidJavaObject ajo, string fieldName)
    {
        System.IntPtr fieldID = AndroidJNIHelper.GetFieldID<FieldType>(ajo.m_jclass, fieldName, true);
        //if (Il2CppClassPointerStore<FieldType>.NativeClassPtr == IntPtr.Zero)
        //    throw new NotSupportedException($"The ");

        if (typeof(FieldType).IsPrimitive)
        {
            if (typeof(FieldType) == typeof(int))
                return (FieldType)(object)AndroidJNISafe.GetStaticIntField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(bool))
                return (FieldType)(object)AndroidJNISafe.GetStaticBooleanField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(byte))
            {
                JeviLib.Warn("Field type <Byte> for Java get field call is obsolete, use field type <SByte> instead");
                return (FieldType)(object)AndroidJNISafe.GetStaticSByteField(ajo.m_jclass, fieldID);
            }

            if (typeof(FieldType) == typeof(sbyte))
                return (FieldType)(object)AndroidJNISafe.GetStaticSByteField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(short))
                return (FieldType)(object)AndroidJNISafe.GetStaticShortField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(long))
                return (FieldType)(object)AndroidJNISafe.GetStaticLongField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(float))
                return (FieldType)(object)AndroidJNISafe.GetStaticFloatField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(double))
                return (FieldType)(object)AndroidJNISafe.GetStaticDoubleField(ajo.m_jclass, fieldID);

            if (typeof(FieldType) == typeof(char))
                return (FieldType)(object)AndroidJNISafe.GetStaticCharField(ajo.m_jclass, fieldID);

            return default;
        }

        if (typeof(FieldType) == typeof(string))
        {
            return (FieldType)(object)AndroidJNISafe.GetStaticStringField(ajo.m_jclass, fieldID);
        }

        if (typeof(FieldType) == typeof(AndroidJavaClass))
        {
            System.IntPtr staticObjectField = AndroidJNISafe.GetStaticObjectField(ajo.m_jclass, fieldID);
            
            return (staticObjectField == System.IntPtr.Zero) ? default(FieldType) : ((FieldType)(object)AndroidJavaObject.AndroidJavaObjectDeleteLocalRef(staticObjectField));
        }

        if (typeof(FieldType) == typeof(AndroidJavaObject))
        {
            System.IntPtr staticObjectField2 = AndroidJNISafe.GetStaticObjectField(ajo.m_jclass, fieldID);
            return (staticObjectField2 == System.IntPtr.Zero) ? default(FieldType) : ((FieldType)(object)AndroidJavaObject.AndroidJavaObjectDeleteLocalRef(staticObjectField2));
        }

        if (typeof(FieldType).IsAssignableFrom(typeof(System.Array)))
            throw new Il2CppConversionException("Arrays must be retrieved as their IL2CPP types, not their Mono domain types.");

        if (AndroidReflection.IsAssignableFrom(Il2CppType.Of<System.Array>(), Il2CppType.Of<FieldType>()))
        {
            throw new Il2CppConversionException("Arrays cannot be retrieved. Too much wonk. If you wish to get arrays, implement it and test it.");
            //IntPtr staticObjectField3 = AndroidJNISafe.GetStaticObjectField(ajo.m_jclass, fieldID);
            //return AndroidJavaObject.FromJavaArrayDeleteLocalRef<FieldType>(staticObjectField3);
        }

        throw new System.Exception("JNI: Unknown field type '" + typeof(FieldType).ToString() + "'");
    }

    /// <summary>
    /// Prevents an object from getting garbage collected using <see cref="HideFlags"/> and <see cref="UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object)"/>,
    /// </summary>
    /// <param name="unityObj">Any Unity object. Can be anything from a <see cref="Material"/> to a <see cref="AssetBundle"/>.</param>
    /// <param name="hide">Applies the <see cref="HideFlags.HideAndDontSave"/> flag. This is recommended for prefabs you wish to use later, but not have appear at (0, 0, 0).</param>
    public static void Persist(this UnityEngine.Object unityObj, bool hide = true)
    {
        unityObj.hideFlags = hide ? HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave : HideFlags.DontUnloadUnusedAsset;
        GameObject.DontDestroyOnLoad(unityObj);
    }
}
