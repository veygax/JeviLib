using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

/// <summary>
/// A class to make postfixes easier to create.
/// <para>This will just run your hook when the patch runs, you cannot modify the return value or anything like that.</para>
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
    public static IEnumerable<MethodInfo> GetConditionalDisableMethods()
        => DynTools.DynamicModuleBuilder.GetTypes()
                                        .Select(t => t.GetMethods(Const.AllBindingFlags))
                                        .Flatten()
                                        .Where(m => m.Name.Contains("Hook_"));

    /// <summary>
    /// Redirection to <see cref="OntoMethod{TDelegate}(MethodBase, TDelegate)"/>. See that method's summary and remarks unless you value 'trial and error' and 'fucking around and finding out' over your time.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// </summary>
    /// <typeparam name="TDelegateSource"></typeparam>
    /// <typeparam name="TDelegateDest"></typeparam>
    /// <param name="toBeRedirected"></param>
    /// <param name="toBeRan"></param>
    /// <example>
    /// Hook.OntoDelegate(Physics.ClosestPoint, OnClosestPointRan, true);
    /// </example>
    public static void OntoDelegate<TDelegateSource, TDelegateDest>(TDelegateSource toBeRedirected, TDelegateDest toBeRan) where TDelegateSource : Delegate where TDelegateDest : Delegate
        => OntoMethod(toBeRedirected.Method, toBeRan);

    /// <summary>
    /// Generates a type and method at runtime to host your patch and execute your Action. The same rules apply as <see cref="Redirect.FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> except for skipping and result changing.
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate. Should be an Action, as the return value of a Func will be ignored and be useless.</typeparam>
    /// <param name="toBeRedirected">The method to be hooked on to via a Harmony prefix</param>
    /// <param name="toBeRan">The delegate to be ran after toBeRedirected is executed.</param>
    /// <exception cref="ArgumentNullException">The method or delegate is null.</exception>
    public static void OntoMethod<TDelegate>(MethodBase toBeRedirected, TDelegate toBeRan) where TDelegate : Delegate
    {
        // cover my ass to make sure shit doesnt break while messing around in such a critical field
        if (toBeRedirected == null) throw new ArgumentNullException(nameof(toBeRedirected));
        if (toBeRan == null) throw new ArgumentNullException(nameof(toBeRan));
#if DEBUG
        if (toBeRan.Method.ReturnType != typeof(void)) Log("You should really use an Action and not a Func when hooking something. The return value won't be used. It's wasted.");
#endif

        // have a redirection-specific identifier
        int thisRedirNum = redirections++;
        int thisDelegateIdx = hookDelegates.Count;
        hookDelegates.Add(toBeRan);
        // keep the parameterinfo's of the source method, but only the ones that are used. hopefully kept in order
        List<ParameterInfo> parameters = DynTools.RemoveUnmatchedParameters(toBeRedirected, toBeRan.Method);
        // convert to parameterexpressions for Linq.Expressions
        List<ParameterExpression> paramExps = DynTools.GetMethodParameters(parameters, toBeRedirected.DeclaringType, toBeRedirected.IsStatic);

        // get it, bob the builder, har har
        TypeBuilder tb = DynTools.GetTypeBuilder("Hook_" + thisRedirNum);
        MethodBuilder bob = tb.DefineMethod(toBeRedirected.Name + " _Hook_" + thisRedirNum,
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

        JeviLib.instance.HarmonyInstance.Patch(toBeRedirected, postfix: hPostfix);
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
