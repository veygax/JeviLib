using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Patching;

/// <summary>
/// A class to make postfixes easier to create.
/// <para>This will just run your hook when the patch runs, you cannot modify the return value or anything like that.</para>
/// <para>It is important to use the Debug build of JeviLib when testing anything using <see cref="Hook"/> or <see cref="Redirect"/> because debug builds have more specific errors.</para>
/// </summary>
public static class Hook
{
    private static int redirections;

    // should be clear to use because internals SHOULD be visible to the dynamically created assembly called "JeviLib Dynamic Patch Assembly Host"
    internal static List<Delegate> hookDelegates = new();
    internal static Delegate GetDelegate(int idx) => hookDelegates[idx]; // didnt feel like learning how to have an expression get 
    private static readonly MethodInfo GetDelegate_Info = typeof(Hook).GetMethod(nameof(GetDelegate), BindingFlags.Static | BindingFlags.NonPublic);

    /// <summary>
    /// Get all methods dynamically created from <see cref="OntoMethod{TDelegate}(MethodBase, TDelegate)"/> and <see cref="OntoDelegate{TDelegateSource, TDelegateDest}(TDelegateSource, TDelegateDest)"/>.
    /// </summary>
    public static IEnumerable<MethodInfo> GetAllHookDynamicMethods()
        => DynTools.DynamicModuleBuilder.GetTypes()
                                        .SelectMany(t => t.GetMethods(Const.AllBindingFlags))
                                        .Where(m => m.Name.Contains("Hook_"));

    /// <summary>
    /// Redirection to <see cref="OntoMethod{TDelegate}(MethodBase, TDelegate)"/>. See that method's summary and remarks unless you value 'trial and error' and 'fucking around and finding out' over your time.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// </summary>
    /// <typeparam name="TDelegateSource"></typeparam>
    /// <typeparam name="TDelegateDest"></typeparam>
    /// <param name="toBeHooked">A delegate to be redirected</param>
    /// <param name="toBeRan"></param>
    /// <example>
    /// Hook.OntoDelegate(Physics.ClosestPoint, OnClosestPointRan);
    /// </example>
    public static void OntoDelegate<TDelegateSource, TDelegateDest>(TDelegateSource toBeHooked, TDelegateDest toBeRan) where TDelegateSource : Delegate where TDelegateDest : Delegate
        => OntoMethod(toBeHooked.Method, toBeRan);

    /// <summary>
    /// Hooks onto an object's initialization method, like <see cref="MonoBehaviour"/><c>.Awake()</c> or <see cref="MonoBehaviour"/><c>.Start()</c>
    /// </summary>
    /// <typeparam name="TDelegate">Something invokable like <see cref="Action"/></typeparam>
    /// <param name="componentType">The component type. Must inherit <see cref="MonoBehaviour"/> (Native Unity types like <see cref="Rigidbody"/> will not work)</param>
    /// <param name="toBeRan">The invokable callback that will be ran when the object initializes. Can be a lambda.</param>
    /// <param name="hookOnEnable">
    /// Hooks onto the object's OnEnable method instead of looking for Awake or Start.
    /// <para>This means the callback may be called multiple times per component as its <see cref="Component.gameObject"/> is disabled and reenabled.</para>
    /// </param>
    /// <exception cref="ArgumentNullException">Any parameter is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="componentType"/> is not a <see cref="MonoBehaviour"/> component.</exception>
    /// <exception cref="MissingMethodException"><paramref name="componentType"/> does not have an Awake or Start (or OnEnable if <paramref name="hookOnEnable"/> is <see langword="true"/>) method.</exception>
    public static void OntoComponentInit<TDelegate>(Type componentType, TDelegate toBeRan, bool hookOnEnable = false) where TDelegate : Delegate
    {
        if (componentType == null) throw new ArgumentNullException(nameof(componentType));
        if (toBeRan == null) throw new ArgumentNullException(nameof(toBeRan));
        if (!componentType.IsSubclassOf(typeof(MonoBehaviour)))
        {
#if DEBUG
            if (componentType.IsSubclassOf(typeof(Component)))
                throw new ArgumentException("Cannot hook onto initialization of Unity native type (such as Rigidbody or Transform), only MonoBehaviours. Given type was " + componentType.FullName, nameof(componentType));
            else
#endif
                throw new ArgumentException($"Given type {componentType.FullName} is not a MonoBehaviour component.", nameof(componentType));
        }

#if DEBUG
        ParameterInfo[] parameters = toBeRan.Method.GetParameters();
        if (parameters.Length > 1)
            throw new ArgumentException("Component initialization callback can only have, at most, one parameter.", nameof(toBeRan));
        else if (parameters.Length == 1 && parameters[0].ParameterType != componentType)
            throw new ArgumentException("Component initialization callback parameter must match component type, as this will pass the component instance to the callback.", nameof(toBeRan));
#endif

        MethodInfo initMethod;
        if (hookOnEnable)
            initMethod = componentType.GetMethod("OnEnable");
        else
            initMethod = componentType.GetMethod("Awake") ?? componentType.GetMethod("Start");

#if DEBUG
        if (initMethod == null)
        {
            if (hookOnEnable)
                throw new MissingMethodException($"Given type {componentType.FullName} has no OnEnable method");
            else
                throw new MissingMethodException($"Given type {componentType.FullName} has no Start method or Awake method");
        }

        if (initMethod.ReturnType != typeof(void))
            Log($"Method {componentType.FullName}.{initMethod.Name}() does not not return void, it returns {initMethod.ReturnType.FullName}. If it is an IEnumerator, it may take longer to fully initialize than you expect.");
#endif

        Log($"Hooking to initializing method {componentType.FullName}.{initMethod.Name}()");

        OntoMethod(initMethod, toBeRan);
    }

    /// <summary>
    /// Generates a type and method at runtime to host your patch and execute your Action. The same rules apply as <see cref="Redirect.FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> except for skipping and result changing.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate. Should be an Action, as the return value of a Func will be ignored and be useless.</typeparam>
    /// <param name="toBeHooked">The method to be hooked onto via a Harmony postfix.</param>
    /// <param name="toBeRan">The delegate to be ran after <paramref name="toBeHooked"/> executes.</param>
    /// <exception cref="ArgumentNullException">The method or delegate is null.</exception>
    public static void OntoMethod<TDelegate>(MethodBase toBeHooked, TDelegate toBeRan) where TDelegate : Delegate
    {
        // cover my ass to make sure shit doesnt break while messing around in such a critical field
        if (toBeHooked == null) throw new ArgumentNullException(nameof(toBeHooked));
        if (toBeRan == null) throw new ArgumentNullException(nameof(toBeRan));
#if DEBUG
        if (toBeRan.Method.ReturnType != typeof(void)) Log("You should really use an Action and not a Func when hooking something. The return value won't be used. It's wasted.");
#endif

        if (toBeRan.Method.IsStatic && toBeRan.Method.ReturnType == typeof(void) && toBeRan.Method.GetParameters().Length == 0)
        {
            Log("Hook callback returns void, is parameterless, and is static. Able to do direct harmony patch without dynamic method creation.");
            JeviLib.instance.HarmonyInstance.Patch(toBeHooked, postfix: toBeRan.Method.ToNewHarmonyMethod());
        }

        // have a redirection-specific identifier
        int thisRedirNum = redirections++;
        int thisDelegateIdx = hookDelegates.Count;
        hookDelegates.Add(toBeRan);
        // keep the parameterinfo's of the source method, but only the ones that are used. hopefully kept in order
        List<ParameterInfo> parameters = DynTools.RemoveUnmatchedParameters(toBeHooked, toBeRan.Method);
        // convert to parameterexpressions for Linq.Expressions
        List<ParameterExpression> paramExps = DynTools.GetMethodParameters(parameters, toBeHooked.DeclaringType, toBeHooked.IsStatic);

        // get it, bob the builder, har har
        TypeBuilder tb = DynTools.GetTypeBuilder("Hook_" + thisRedirNum);
        MethodBuilder bob = tb.DefineMethod(toBeHooked.Name + " _Hook_" + thisRedirNum,
                                            MethodAttributes.Public | MethodAttributes.Static,
                                            CallingConventions.Any,
                                            typeof(void),
                                            paramExps.Select(pi => pi.Type).ToArray());

        // used by callExp, dont need to pass into Expression.Block
        // TDelegate dele = (TDelegate)Hook.GetDelegate(<idx>);
        Expression delegateExp = Expression.Convert(Expression.Call(GetDelegate_Info, Expression.Constant(thisDelegateIdx, typeof(int))), typeof(TDelegate));
        // dele(param1, param2, param3, ...);
        Expression callExp = Expression.Invoke(delegateExp, paramExps);

        // its fine to "pollute" paramExps now because Expression.Call copies the IEnumerable to a read-only variant, so the call expression won't change.
        Log("Created expressions");

        BlockExpression bexp = Expression.Block(callExp);
        LambdaExpression lexp = Expression.Lambda(bexp, paramExps);
        lexp.CompileToMethod(bob);

        Type createdType = tb.CreateType();
        Log($"Compiled patch method and created runtime type!");
        Log($"Created: <asm={createdType.Assembly.GetName().Name}> <module={createdType.Module.Name}> {createdType.FullName}");


        //MethodBase dynInfo = MethodBase.GetMethodFromHandle(bob.MethodHandle);
        //SymbolExtensions.GetMethodInfo(dynInfo);
        HarmonyMethod hPostfix = new(bob.GetMethodInfo());

        JeviLib.instance.HarmonyInstance.Patch(toBeHooked, postfix: hPostfix);
    }


    #region Logging
    /// <summary>
    /// Whether to allow the (frankly copious amounts of) log statements in <see cref="OntoMethod{TDelegate}(MethodBase, TDelegate)"/> to output to the log file.
    /// <para>Debug builds default to false, release builds default to true.</para>
    /// </summary>
    public static bool DisableLogging { get => DynTools.disableLogging; set { DynTools.disableLogging = value; } }
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!DynTools.disableLogging) JeviLib.Log("HOOKING -> " + str, ConsoleColor.DarkGray);
    }
    #endregion
}
