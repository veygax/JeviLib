using HarmonyLib;
using MelonLoader;
using ModThatIsNotMod;
using PuppetMasta;
using StressLevelZero.Interaction;
using StressLevelZero.Pool;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil;

/// <summary>
/// A collection of utility methods I felt the need for while making Chaos. This may get refactored into a bunch of smaller classes
/// </summary>
public static class Utilities
{
    static Utilities()
    {
        IsLegitCopy();
        IsLegitCopy();
        IsLegitCopy();
        // make sure the runtime inlines it (i think)
    }

    static readonly MethodInfo hashMethodInfo = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "ModThatIsNotMod")
        .GetType("ModThatIsNotMod.Internals.MelonHashChecking")
        .GetMethod("GetMelonHash", BindingFlags.Static | BindingFlags.Public);
    static FieldInfo discordUserIdInfo;
    static FieldInfo discordIntegrationCurrentUserInfo;
    static bool isEntanglementInstalled = true;
#if DEBUG
    static Type inspectorManager;
#endif

    /// <summary>
    /// Checks to see if the hash of the currently running executable is genuine. Use in combination with <see cref="IsSteamVersion"/>.
    /// </summary>
    /// <returns>If the hash of the game is equal to the 1.6 version of BONEWORKS bought from Steam</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // should hopefully prevent harmony patches from getting to this, in case any prospective pirates think theyre slick.
    public static bool IsLegitCopy()
    {
        return hashMethodInfo.Invoke(null, new object[] { MelonUtils.GetApplicationPath() }) as string == "1cf5b055a5dd6be6d15d6db9c0f994fb";
    }

    /// <summary>
    /// Tells whether the currently running game is installed from the Oculus store using a basic file check.
    /// </summary>
    /// <returns>Whether Boneworks_Oculus_Windows64.exe exists in the application path.</returns>
    public static bool IsSteamVersion()
    {
        return !System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "..", "Boneworks_Oculus_Windows64.exe"));
    }

    /// <summary>
    /// Looks for <paramref name="objectName"/> 
    /// </summary>
    /// <param name="objectName">name of the pool</param>
    /// <returns><see langword="null"/> if the pool was not found, otherwise the prefab.</returns>
    public static GameObject GetPrefabOfPool(string objectName)
    {
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, Pool> dynamicPool in PoolManager.DynamicPools)
        {
            if (dynamicPool.Key == objectName)
                return dynamicPool.Value.Prefab;
        }

        return null;
    }

    /// <summary>
    /// Uses <see cref="UnityEngine.Random.Range(int, int)"/> to determine what to return.
    /// </summary>
    /// <returns><see cref="Player.leftHand"/> or <see cref="Player.rightHand"/></returns>
    public static Hand GetRandomPlayerHand()
    {
        int randomNum = UnityEngine.Random.Range(0, 2);
        if (randomNum == 1)
            return Player.leftHand;
        else
            return Player.rightHand;
    }

    /// <summary>
    /// Spawns a <see cref="ModThatIsNotMod"/> Ad and then moves it in front of, and makes it face, the player.
    /// </summary>
    /// <param name="str">The text to be displayed on the Ad.</param>
    /// <returns></returns>
    public static GameObject SpawnAd(string str)
    {
        GameObject ad = ModThatIsNotMod.RandomShit.AdManager.CreateNewAd(str);
        MoveAndFacePlayer(ad);
        return ad;
    }

    /// <summary>
    /// <see cref="UnityEngine.Object.FindObjectsOfTypeAll(Il2CppSystem.Type)"/> except LINQ Select'd to Cast to the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A type extending <see cref="UnityEngine.Object"/></typeparam>
    /// <returns>The output of FindObjectsOfTypeAll, ran through Select. It is recommended you ToArray it to avoid running the selector every access.</returns>
    public static IEnumerable<T> FindAll<T>() where T : UnityEngine.Object
    {
        return GameObject.FindObjectsOfTypeAll(UnhollowerRuntimeLib.Il2CppType.Of<T>()).Select(obj => obj.Cast<T>());
    }

    /// <summary>
    /// Basically the <c>Split</c> method on <see cref="string"/>, but it returns a 2 element array instead. It's hard to describe without an example
    /// <code>
    /// Argsify("part1,part2,alsopart2", ','); // Returns { "part1", "part2,alsopart2" } 
    /// </code>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="split"></param>
    /// <returns>The first element alone, with the following elements joined in one string.</returns>
    public static string[] Argsify(string data, char split)
    {
        string[] args = data.Split(split);
        string arg = args[0];
        string argg = string.Join(split.ToString(), args.Skip(1));
        return new string[] { arg, argg };
    }

    /// <summary>
    /// Takes the byte representation of a <see cref="Vector3"/> and returns the Vector3 it represents. Useful for Entanglement.
    /// </summary>
    /// <param name="bytes">
    /// The byte representation of a Vector3, AKA the byte representation of 3 <see cref="float"/>s.
    /// <para>Debug builds have a check to warn in case the source array is too short for the given <paramref name="startIdx"/> or if it's not the correct size.</para>
    /// </param>
    /// <param name="startIdx">The index to start taking bytes from the <paramref name="bytes"/> array to represent as floats to construct a new <see cref="Vector3"/></param>
    /// <returns></returns>
    public static Vector3 DebyteV3(byte[] bytes, int startIdx = 0)
    {
#if DEBUG
        if (startIdx == 0 && bytes.Length != Const.SizeV3) JeviLib.Warn($"Trying to debyte a Vector3 of length {bytes.Length}, this is not the expected {Const.SizeV3} bytes!");
        if (startIdx + (sizeof(float) * 3) > bytes.Length) JeviLib.Warn($"{bytes.Length} is too short for the given index of {startIdx}");
#endif
        return new Vector3(
            BitConverter.ToSingle(bytes, startIdx),
            BitConverter.ToSingle(bytes, startIdx + sizeof(float)),
            BitConverter.ToSingle(bytes, startIdx + sizeof(float) * 2));
    }

    /// <summary>
    /// Takes the output of <see cref="SerializeMesh(Vector3[])"/> and converts it into vertices (that can be piped into <see cref="MeshFilter.mesh"/>)
    /// <para>Can also be used to serialize a bunch of positions and rotations, and then deserialize them. IDK what for, that one's for you to decide.</para>
    /// </summary>
    /// <param name="bytes">Byte representation of the mesh vertices</param>
    /// <returns>The vertices</returns>
    public static Vector3[] DeserializeMesh(byte[] bytes)
    {
        Vector3[] result = new Vector3[bytes.Length / Const.SizeV3];
#if DEBUG
        if (bytes.Length % Const.SizeV3 != 0) JeviLib.Warn("Malformed byte array - not divisible by the size of a vector3");
#endif
        for (int i = 0; i < bytes.Length; i += Const.SizeV3)
        {
            result[i / Const.SizeV3] = DebyteV3(bytes, i);
        }

        return result;
    }

    /// <summary>
    /// Serializes a mesh into bytes to be saved or networked.
    /// </summary>
    /// <param name="vectors">Mesh vertices</param>
    /// <returns>An array of the mesh vertices, represented as bytes.</returns>
    public static byte[] SerializeMesh(Vector3[] vectors)
    {
        byte[] bytes = new byte[vectors.Length * sizeof(float) * 3];
        for (int i = 0; i < vectors.Length; i++)
        {
            int offset = i * sizeof(float) * 3;
            byte[] _vecB = vectors[i].ToBytes();
            Buffer.BlockCopy(_vecB, 0, bytes, offset, sizeof(float) * 3);
        }
        return bytes;
    }

    /// <summary>
    /// Moves an object in front of the player and rotates it to face the player, with its up remaining the global up.
    /// </summary>
    /// <param name="obj">The object to move and rotate</param>
    public static void MoveAndFacePlayer(GameObject obj)
    {
        Transform phead = Instances.Player_PhysBody.rbHead.transform;
        Vector3 position = phead.position + phead.forward.normalized * 2;
        Quaternion rotation = Quaternion.LookRotation(obj.transform.position - phead.position, Vector3.up);
        obj.transform.SetPositionAndRotation(position, rotation);
    }

    /// <summary>
    /// So I basically made this method over the course of a few days in my Comp Sci class and I dont quite recall how it works aside from the rundown.
    /// <para>The first byte dictates how many arrays were in the source 2dArray. The following <c>[value of the first byte] * 2</c> bytes compose the rest of the header.</para>
    /// <para>The next pairs of bytes are used as ushorts that tell how long each corresponding array was</para>
    /// </summary>
    /// <param name="bytess">The source arrays to be joined together.</param>
    /// <returns>A single array consisting of a header and the values of the source arrays.</returns>
    /// <remarks>
    /// <para><b>Here's a rundown of how this method packs arrays. Note that ushorts are two bytes long, so this method will fail if one of your arrays is over 64KB in length.</b></para>
    /// <c>
    /// <br>Input: [</br>
    /// <br>....[1,2,3,4,5,6,7]</br>
    /// <br>....[2,3,4,5,6,7,8,9,10]</br>
    /// <br>]</br>
    /// <br>Output: [</br>
    /// <br>....2, (header, says how many things there are)</br>
    /// <br>....ushorts(7, 9)  (header, says lengths of elements, so method knows where to split)</br>
    /// <br>....1,2,3,4,5,6,7,</br>
    /// <br>....2,3,4,5,6,7,8,9,10</br>
    /// <br>]</br>
    /// </c></remarks>
    public static byte[] JoinBytes(byte[][] bytess)
    {
        byte arrayCount = (byte)bytess.Length;
        // bytes.Length - 1 because you dont put a delim at the end (unless youre weird i guess, but im not)
        List<ushort> indices = new(bytess.Length - 1);
        byte[] bytesJoined = new byte[(bytess.Length * 2 + 1) + bytess.Sum(b => b.Length)];
        // need to get the indices beforehand to compose the header
        foreach (byte[] arr in bytess)
        {
            indices.Add((ushort)arr.Length);
        }
        // Compose the header
        bytesJoined[0] = arrayCount;
        for (int i = 0; i < indices.Count; i++)
        {
            ushort idx = indices[i];
            BitConverter.GetBytes(idx).CopyTo(bytesJoined, i * sizeof(ushort) + 1);
        }

        ushort lastLen = 0;
        int index = arrayCount * 2 + 1;
        for (int i = 0; i < arrayCount; i++)
        {
            int thisLen = indices[i];
            Buffer.BlockCopy(bytess[i], 0, bytesJoined, index, thisLen);

            // set the next index
            lastLen = (ushort)thisLen;
            index += thisLen;
        }

        return bytesJoined;
    }

    /// <summary>
    /// Split an array of bytes recieved from <see cref="JoinBytes(byte[][])"/> into an array of arrays.
    /// </summary>
    /// <param name="bytes">The byte array with the proper header generated by JoinBytes This method will malfunction if the header is malformed.</param>
    /// <returns>A byte-for-byte copy of the original byte[][] before it was sent to <see cref="JoinBytes(byte[][])"/> and likely sent over Entanglement</returns>
    internal static byte[][] SplitBytes(byte[] bytes)
    {
        // Grab data from the header
        // get number of splits
        int splitsCount = bytes[0];
        // actually get the list of element sizes
        int[] splitIndices = new int[splitsCount];
        for (int i = 0; i < splitsCount; i++)
        {
            // add 1 to skip the byte that says how many split indicies there are
            ushort idx = BitConverter.ToUInt16(bytes, 1 + i * sizeof(ushort));
            splitIndices[i] = idx;
        }

        byte[][] res = new byte[splitsCount][];
        // start after the header
        int lastIdx = splitsCount * sizeof(ushort) + 1;
        for (int i = 0; i < res.Length; i++)
        {
            int len = splitIndices[i];
            // dont overallocate lest there be a dead byte
            byte[] arr = new byte[len];
            // copy the bytes
            Buffer.BlockCopy(bytes, lastIdx, arr, 0, len);


            // save our changes to the array 
            res[i] = arr;
            // perpetuate the nevereding cycle
            lastIdx += len;
        }
        return res;
    }

    /// <summary>
    /// Returns a user-friendly name from the name of a member. Only works with camelCase. WhateverCaseYouCallThis might work too. But not THIS_CASE or this_case.
    /// </summary>
    /// <param name="name">Member name, like "modToggleEnable"</param>
    /// <returns>Friendly name, like "Mod Enable Toggle"</returns>
    public static string GenerateFriendlyMemberName(string name)
    {
        StringBuilder builder = new(name.Length);
        builder.Append(char.ToUpper(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            // use char's cause theyre on the stack not heap, and i want to avoid allocating shit 
            char character = name[i];

            if (char.IsUpper(character))
            {
                builder.Append(' ');
                builder.Append(char.ToUpper(character));
            }
            else builder.Append(character);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Redirect to <see cref="Extensions.Flatten(byte[][])"/>
    /// <br>Joins an array of bytes directly one after another, without regard for reproduceability. Use this for things of fixed size, like byte representations of <see cref="float"/>s or <see cref="int"/>s.</br>
    /// </summary>
    /// <param name="arrays">The arrays to be concatenated one after another.</param>
    /// <returns>An array of bytes whose length is equal to the sum of the lengths of the input arrays.</returns>
    public static byte[] Flatten(params byte[][] arrays) => Extensions.Flatten(arrays);

    /// <summary>
    /// Takes an embedded resource from "Resources/EntanglementSync.dll" out of the caller (that's you!) and loads it if Entanglement is installed.
    /// </summary>
    /// <param name="rootFolderName">
    /// The root folder name for your resources. Will default to the name of your assembly, with spaces replaced by underscores. 
    /// <para><see cref="Assembly.GetManifestResourceNames()"/></para>
    /// </param>
    /// <returns>Whether or not your module attempted setup. Will be false if "Resources/EntanglementSync.dll" doesn't exist or if Entanglement isn't installed.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool LoadSyncAssembly(string rootFolderName = null)
    {
        Assembly caller = Assembly.GetCallingAssembly();
        try
        {
            string asmName = rootFolderName ?? caller.GetName().Name.Replace(' ', '_');
            byte[] asmRaw = caller.GetEmbeddedResource(asmName + ".Resources.EntanglementSync.dll");
            Assembly syncAsm = Assembly.Load(asmRaw);

            // First, check if Entanglement is loaded
            Assembly entanglementAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "Entanglement");

            // Then get the ModuleHandler dynamically
            Type moduleHandlerType = entanglementAssembly.GetType("Entanglement.Modularity.ModuleHandler");

            // Then try to get SetupModule()
            MethodInfo setupModuleMethod = moduleHandlerType.GetMethod("SetupModule", BindingFlags.Static | BindingFlags.Public);

            // Then call the setup method reflectively
            setupModuleMethod.Invoke(null, new object[] { syncAsm });

            JeviLib.Log("Loaded Entanglement module from " + caller.GetName().Name);

            return true;
        }
#if DEBUG
        catch (Exception ex)
        {
            JeviLib.Warn("Failed to load sync assembly, here's why. This likely doesn't matter to you, but here you go:\n" + ex);
            return false;
        }
#else
        catch { return false; }
#endif
    }

    /// <summary>
    /// Gets all rigidbodies from the given <paramref name="puppet"/>, using <see cref="PuppetMaster.muscles"/> and LINQ.
    /// </summary>
    /// <param name="puppet">The puppet to get muscles from.</param>
    /// <returns>The rigidbodies of the muscles. I'm not sure if they're assured to be not <see langword="null"/>, so you may want to run </returns>
    public static IEnumerable<Rigidbody> GetMuscleRigidbodies(PuppetMaster puppet)
    {
        return puppet.muscles.Select(s => s.rigidbody);
    }

    /// <summary>
    /// Attempts to retrieve a <see cref="Type"/> using only its name and namespace, and an asynchronously created cache.
    /// <para>If the cache isn't built yet, this method will likely fail. Check via <see cref="JeviLib.DoneMappingNamespacesToAssemblies"/>.</para>
    /// </summary>
    /// <param name="namezpaze">The name of the namespace, without a leading or trailing period.</param>
    /// <param name="clazz">The Type's name.</param>
    /// <returns>The <see cref="Type"/> represented by the combined namespace and class name, or <see langword="null"/> if it wasn't found.</returns>
    public static Type GetTypeFromString(string namezpaze, string clazz)
    {
#if DEBUG
        if (!JeviLib.DoneMappingNamespacesToAssemblies) JeviLib.Log("Not done mapping namespaces to assemblies! Getting type from string may falsely return null!");
#endif

        if (JeviLib.namespaceAssemblies.TryGetValue(namezpaze, out ConcurrentBag<Assembly> asms))
        {

#if DEBUG
            JeviLib.Log($"Found the namespace '{namezpaze}' in {asms.Count} assemblies, attempting to find the type {clazz}");
#endif

            foreach (Assembly asm in asms)
            {
                Type t = asm.GetType(namezpaze + "." + clazz);
                if (t != null) return t;
            }

#if DEBUG
            JeviLib.Warn($"Unable to find a Type named '{clazz}' in the namespace '{namezpaze}' in any loaded Assemblies");
#endif
        }
#if DEBUG
        else JeviLib.Warn($"Unable to find the namespace '{namezpaze}' in any loaded assemblies (This is probably fine/expected behavior)");
#endif
        return null;
    }

    /// <summary>
    /// Get the <see cref="MethodInfo"/> behind any delegate. It can be a lambda or an actual method. See examples for examples.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate type, can be an Action (returns nothing/void) or a Func (returns something).</typeparam>
    /// <param name="method">Any delegate, can be an Action (returns nothing/void) or a Func (returns something).</param>
    /// <returns>The underlying <see cref="MethodInfo"/>, gotten from <see cref="Delegate.Method"/>.</returns>
    /// <example>
    /// <c>AsInfo(MethodNameHere);</c> That's it.
    /// <c>AsInfo(() => { return "top 10 things to do in michigan\n1. leave"; });</c> That's it.
    /// </example>
    public static MethodInfo AsInfo<TDelegate>(TDelegate method) where TDelegate : Delegate
    {
        // this is a bit chickenshit i think
        // it feels too easy
        return method.Method;
    }

    /// <summary>
    /// Creates a new harmony method from the delegate's MethodInfo.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate type, can be Func or Action</typeparam>
    /// <param name="method">a method or delegate</param>
    /// <returns>A <see cref="HarmonyMethod"/> of the delegate.</returns>
    public static HarmonyMethod ToHarmony<TDelegate>(TDelegate method) where TDelegate : Delegate
    {
        return AsInfo(method).ToNewHarmonyMethod();
    }

    /// <summary>
    /// Clones an instance of <see cref="BaseEnemyConfig.HealthSettings"/> because it's not a <see cref="UnityEngine.Object"/>, thus <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> (cloning) doesn't work on it.
    /// </summary>
    /// <param name="hs">The health settings to be cloned.</param>
    /// <returns>A new instance of <see cref="BaseEnemyConfig.HealthSettings"/> with the values of the original.</returns>
    public static BaseEnemyConfig.HealthSettings CloneHealthSettings(BaseEnemyConfig.HealthSettings hs)
    {
        return new()
        {
            aggression = hs.aggression,
            irritability = hs.irritability,
            maxAppendageHp = hs.maxAppendageHp,
            maxHitPoints = hs.maxHitPoints,
            maxStunSeconds = hs.maxStunSeconds,
            minHeadImpact = hs.minHeadImpact,
            minLimbImpact = hs.minLimbImpact,
            minSpineImpact = hs.minSpineImpact,
            placability = hs.placability,
            stunRecovery = hs.stunRecovery,
            vengefulness = hs.vengefulness
        };
    }

    /// <summary>
    /// Creates a <see cref="Thread"/>, sets <see cref="Thread.IsBackground"/> to <see langword="true"/>, starts it, then returns it.
    /// </summary>
    /// <param name="call">An action to be invoked</param>
    /// <returns>A background thread.</returns>
    public static Thread StartBGThread(Action call)
    {
        Thread thread = new(() => { call(); });
        thread.IsBackground = true;
        thread.Start();
        return thread;
    }

    /// <summary>
    /// Creates a <see cref="Thread"/>, sets <see cref="Thread.IsBackground"/> to <see langword="true"/>, starts it, then returns it.
    /// </summary>
    /// <param name="call">An action to be invoked</param>
    /// <param name="param">The parameter to be passed into the call.</param>
    /// <returns>A background thread.</returns>
    public static Thread StartBGThread<T>(Action<T> call, T param)
    {
        Thread thread = new((object obj) => { call((T)obj); });
        thread.IsBackground = true;
        thread.Start(param);
        return thread;
    }

    /// <summary>
    /// Try-catches a get request for the user's Steam ID so you don't have to! This method will fail if Steamworks isn't yet initialized or if the game is an Oculus copy.
    /// </summary>
    /// <param name="steamID">A <see cref="Steamworks.CSteamID"/> that will be filled in with the result of <see cref="Steamworks.SteamUser.GetSteamID"/>. Will be <see langword="default"/> if the call fails.</param>
    /// <returns>Whether or not the call was successful.</returns>
    /// <remarks>Also useful for CMaps because Steamworks is in Asm C# Firstpass, so the classes containing the steamworks methods aren't present, as well as the ripper not putting method stubs in dummy classes.</remarks>
    public static bool TryGetSteamID(out Steamworks.CSteamID steamID)
    {
        try
        {
            steamID = Steamworks.SteamUser.GetSteamID();
            return true;
        }
        catch
#if DEBUG
            (Exception ex)
#endif
        {
#if DEBUG
            JeviLib.Warn("Failed to get Steam ID due to exception: " + ex.Message);
#endif
            steamID = default;
            return false;
        }   
    }

    /// <summary>
    /// Gets the user's Discord user ID, if Entanglement is installed.
    /// </summary>
    /// <param name="userID">The Discord user ID. Will be <see langword="default"/> if Entanglement isn't installed.</param>
    /// <returns>Whether or not the data getting via reflection was successful.</returns>
    public static bool TryGetDiscordID(out long userID)
    {
        if (isEntanglementInstalled && discordUserIdInfo == null)
        {
            discordUserIdInfo = GetTypeFromString("Discord", "User")?.GetField("Id", Const.AllBindingFlags);
            discordIntegrationCurrentUserInfo = GetTypeFromString("Entanglement.Network", "DiscordIntegration")?.GetField("currentUser", Const.AllBindingFlags);
            isEntanglementInstalled = discordUserIdInfo != null;
        }

        if(!isEntanglementInstalled)
        {
            userID = default;
            return false;
        }

        object currUser = discordIntegrationCurrentUserInfo.GetValue(null); // its static
        userID = (long)discordUserIdInfo.GetValue(currUser); // its not static
        return true;
    }

    /// <summary>
    /// Focuses something in a UnityExplorer inspector, if it is installed (and if this is in Debug mode).
    /// </summary>
    /// <param name="obj">Any object, can be a <see cref="Component"/>, <see cref="GameObject"/>, or even a normal Mono domain thing.</param>
    public static void InspectInUnityExplorer(object obj)
    {
#if DEBUG
        inspectorManager ??= GetTypeFromString("UnityExplorer", "InspectorManager");
        Type cobj = GetTypeFromString("UnityExplorer.CacheObject", "CacheObjectBase");
        // public static void Inspect(object obj, CacheObjectBase sourceCache = null)
        MethodInfo minf = inspectorManager?.GetMethod("Inspect", new Type[] { typeof(object), cobj });
        minf?.Invoke(null, new object[] { obj, null });
#endif
    }

    /// <summary>
    /// Focuses a <see cref="Type"/> in a UnityExplorer inspector using static reflection, if it is installed (and if this is in Debug mode).
    /// </summary>
    /// <param name="t">Any Type. Can be Mono domain or IL2CPP.</param>
    public static void InspectInUnityExplorer(Type t)
    {
#if DEBUG
        inspectorManager ??= GetTypeFromString("UnityExplorer", "InspectorManager");
        // public static void Inspect(object obj, CacheObjectBase sourceCache = null)
        MethodInfo minf = inspectorManager?.GetMethod("Inspect", new Type[] { typeof(Type) });
        minf?.Invoke(null, new object[] { t });
        
#endif
    }

    /// <summary>
    /// Gets a MethodInfo based entirely off of strings, so no direct assembly reference is required to interop with other mods.
    /// </summary>
    /// <param name="namezpaze">The Type's namespace. This will be used to look through assemblies with this namespace defined.</param>
    /// <param name="clazz">The Type's name.</param>
    /// <param name="method">The method's name.</param>
    /// <param name="paramTypes">The names of types of the parameters. <i>Do NOT include the namespaces in the names.</i></param>
    /// <returns></returns>
    public static MethodInfo GetMethodFromString(string namezpaze, string clazz, string method, string[] paramTypes = null)
    {
#if DEBUG
        if (!JeviLib.DoneMappingNamespacesToAssemblies)
        {
            JeviLib.Warn("Unlikely to get successfully get method from string if namespace mapping is incomplete!");
        }
#endif

        Type type = Utilities.GetTypeFromString(namezpaze, clazz);
        if (type == null)
        {
#if DEBUG
            JeviLib.Log($"Unable to find the namespace '{namezpaze}' in any loaded Assemblies.");
#endif
            return null;
        }

        MethodInfo minf = null;
        if (paramTypes == null) minf = type.GetMethodEasy(method);
        else
        {
            MethodInfo[] minfs = type.GetMethods(Const.AllBindingFlags).Where(m => m.Name == method).ToArray();
            foreach (MethodInfo meth in minfs)
            {
                string[] paramStrs = meth.GetParameters().Select(pi => pi.ParameterType.Name).ToArray();
                if (paramStrs.SequenceEqual(paramTypes))
                {
                    minf = meth;
                    break;
                }
            }
        }

#if DEBUG
        if (minf == null)
        {
            JeviLib.Log($"The type {namezpaze}.{clazz} (from assembly {type.Assembly.GetName().Name}) doesn't have a method named {method} {(paramTypes == null ? "" : $"that matches the given {paramTypes.Length} parameter(s).")}");
        }
#endif

        return minf;
    }
}
