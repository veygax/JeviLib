using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jevil.Patching;

/// <summary>
/// A class to make prefixes easier to create.
/// <para>This is functionally different from <see cref="Hook"/> due to the use of Prefixes instead of Postfixes. If you want to modify some fields before a method runs or replace a method entirely, use this class.</para>
/// </summary>
public static class Redirect
{
    delegate ParameterExpression ParamExpMake(Type type, string name, bool isByRef);

    static readonly ParamExpMake ParameterExpression_Make = (ParamExpMake)typeof(ParameterExpression).GetMethod("Make", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(ParamExpMake));
    static int redirections;
    /// <summary>
    /// The length, in ms, to wait to acquire the threaded spinlock. Lock logging statements do not abide by <see cref="DisableLogging"/>.
    /// <para>Defaults to <c>1000</c>ms, aka 1 second.</para>
    /// <br>If it fails to acquire a lock within the given time, it will queue another call.</br>
    /// </summary>
    public static int lockTimeout = 1000;
    static SpinLock spinLock = new();

    // should be clear to use because internals SHOULD be visible to the dynamically created assembly called "JeviLib Dynamic Patch Assembly Host"
    internal static List<Delegate> redirectionDelegates = new();
    internal static Delegate GetDelegate(int idx) => redirectionDelegates[idx]; // didnt feel like learning how to have an expression get 
    private static readonly MethodInfo GetDelegate_Info = typeof(Redirect).GetMethod(nameof(GetDelegate), BindingFlags.Static | BindingFlags.NonPublic);
    /// <summary>
    /// Get all methods dynamically created from <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> and <see cref="FromDelegate{TDelegateSource, TDelegateDest}(TDelegateSource, TDelegateDest, bool)"/>.
    /// </summary>
    public static IEnumerable<MethodInfo> GetConditionalDisableMethods()
        => DynTools.DynamicModuleBuilder.GetTypes()
                                        .Select(t => t.GetMethods(Const.AllBindingFlags))
                                        .Flatten()
                                        .Where(m => m.Name.Contains("Redirect_"));

    // todo: support reference parameters for changing params

    /// <summary>
    /// Redirection to <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/>. See that method's summary and remarks unless you value 'trial and error' and 'fucking around and finding out' over your time.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// </summary>
    /// <typeparam name="TDelegateSource"></typeparam>
    /// <typeparam name="TDelegateDest"></typeparam>
    /// <param name="toBeRedirected"></param>
    /// <param name="toBeRan"></param>
    /// <param name="skipOriginal"></param>
    /// <example>
    /// Redirect.FromDelegate(Physics.ClosestPoint, ClosestPointButBetter, true);
    /// Redirect.FromDelegate(Physics.ClosestPoint, ClosestPointChangePreState, false);
    /// </example>
    public static void FromDelegate<TDelegateSource, TDelegateDest>(TDelegateSource toBeRedirected, TDelegateDest toBeRan, bool skipOriginal = false) where TDelegateSource : Delegate where TDelegateDest : Delegate
        => FromMethod(toBeRedirected.Method, toBeRan, skipOriginal);

    /// <summary>
    /// Redirect the given method to the given delegate, attempting to automatically resolve parameter types. See remarks on parameters to learn more on how this method operates.
    /// <para>This is one of the few JeviLib methods that will actually <see langword="throw"/> when something is amiss, so make sure you have your ducks in a row.</para>
    /// <para><b>THIS IS IRREVERSIBLE!!!</b> MelonLoader does not support unpatching on IL2CPP games!</para>
    /// </summary>
    /// <typeparam name="TDelegate">Any delegate, can be a <see cref="Func{T, TResult}"/> or an <see cref="Action{T}"/>.</typeparam>
    /// <param name="toBeRedirected">The method to be skipped and have its execution passed to the other method.</param>
    /// <param name="toBeRan">Any delegate. If it is a <see cref="Func{TResult}"/> (that is, it returns a value) it have its value post-execution automatically assigned to the original's return value.</param>
    /// <param name="skipOriginal">Whether or not to skip the original method. Most recommended if you </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// <para><b>Remarks:</b></para>
    /// <para>This method dymanically creates methods which it attaches to an internal class. That method calls your delegate, passing in every parameter you specify.</para>
    /// <para>It determines the parameters to pass in by matching types in order. If <paramref name="toBeRedirected"/> is declared with parameters like so, <c>(string goofBall, bool bazingoid, Pancake[] hotcakes)</c>, and your delegate <paramref name="toBeRan"/> is declared with the parameters <c>(string someShit, Pancake[] wowza)</c>, this method will resolve that when determining shared parameters and their types, and will call it as expected. <i>It does not look for parameter names.</i></para>
    /// <para>If your delegate's return type isn't <see langword="void"/> and matches <paramref name="toBeRedirected"/>'s return type, an extra parameter will be added to the dynamically created method, called <c>__result</c>. When a Harmony patch is created, Harmony looks through the parameters and sees that, and knows that any value assigned to it will become the return value of the patched method.</para>
    /// <para>If your delegate takes a parameter that is the same as <paramref name="toBeRedirected"/>.DeclaringType, it will add a parameter called <c>__instance</c> and pass that to your redirection.</para>
    /// <para>If there's a parameter your redirect has that isn't the instance type or whose type isn't in the normal parameter list, this method will likely fail.</para>
    /// </remarks>
    public static void FromMethod<TDelegate>(MethodInfo toBeRedirected, TDelegate toBeRan, bool skipOriginal = false) where TDelegate : Delegate
    {
        bool lockTaken = false;
#if DEBUG
        if (spinLock.IsHeld) JeviLib.QueueLog("Redirect will have to wait, the spin lock is presently held by another thread. Timing out in " + lockTimeout + ".");
#endif
        try
        {
            spinLock.TryEnter(lockTimeout, ref lockTaken);
            // cover my ass to make sure shit doesnt break while messing around in such a critical field
            if (toBeRedirected == null) throw new ArgumentNullException(nameof(toBeRedirected));
            if (toBeRan == null) throw new ArgumentNullException(nameof(toBeRan));

            // have a redirection-specific identifier
            int thisRedirNum = redirections++;
            int thisDelegateIdx = redirectionDelegates.Count;
            redirectionDelegates.Add(toBeRan);
            // keep the parameterinfo's of the source method, but only the ones that are used. hopefully kept in order
            List<ParameterInfo> parameters = DynTools.RemoveUnmatchedParameters(toBeRedirected, toBeRan.Method);
            // convert to parameterexpressions for Linq.Expressions
            List<ParameterExpression> paramExps = DynTools.GetMethodParameters(parameters, toBeRedirected.DeclaringType, toBeRedirected.IsStatic);

            // get it, bob the builder, har har
            TypeBuilder tb = DynTools.GetTypeBuilder("Redirect_" + thisRedirNum);
            MethodBuilder bob = tb.DefineMethod(toBeRedirected.Name + " _Redirect_" + thisRedirNum,
                                                MethodAttributes.Public | MethodAttributes.Static,
                                                CallingConventions.Any,
                                                typeof(bool),
                                                paramExps.Select(pi => pi.Type).ToArray());

            // used by callExp, dont need to pass into Expression.Block
            // TDelegate dele = (TDelegate)Hook.GetDelegate(<idx>);
            Expression delegateExp = Expression.Convert(Expression.Call(GetDelegate_Info, Expression.Constant(thisDelegateIdx, typeof(int))), typeof(TDelegate));
            // dele(param1, param2, param3, ...);
            Expression callExp = Expression.Invoke(delegateExp, paramExps);
            LabelTarget retLabel = Expression.Label(typeof(bool), "RetLabel");
            // return <!skipOriginal>;
            Expression retExp = Expression.Return(retLabel, Expression.Constant(!skipOriginal));
            Expression retLabelExp = Expression.Label(retLabel, Expression.Constant(!skipOriginal));

            // its fine to "pollute" paramExps now because Expression.Call copies the IEnumerable to a read-only variant, so the call expression won't change.
            if (toBeRan.Method.ReturnType != typeof(void) && toBeRan.Method.ReturnType == toBeRedirected.ReturnType)
            {
                // __result = dele(param1, param2, param3, ...);
                ParameterExpression resultParamExp = ParameterExpression_Make(toBeRedirected.ReturnType, "__result", true);
                paramExps.Add(resultParamExp);
                callExp = Expression.Assign(resultParamExp, callExp);
                Log("Redirect's return type matches original return type, added __result ref param");
            }
            Log("Created expressions");

            BlockExpression bexp = Expression.Block(callExp, retExp, retLabelExp);
            LambdaExpression lexp = Expression.Lambda(bexp, paramExps);
            lexp.CompileToMethod(bob);

            Type createdType = tb.CreateType();
            Log($"Compiled patch method and created runtime type!");
            Log($"Created: <asm={createdType.Assembly.GetName().Name}> <module={createdType.Module.Name}> {createdType.FullName}");


            //MethodBase dynInfo = MethodBase.GetMethodFromHandle(bob.MethodHandle);
            //SymbolExtensions.GetMethodInfo(dynInfo);
            HarmonyMethod hPrefix = new(bob.GetMethodInfo());

            JeviLib.instance.HarmonyInstance.Patch(toBeRedirected, prefix: hPrefix);
        }
        finally
        {
            if (lockTaken) spinLock.Exit();
            else
            {
                JeviLib.QueueLog("FAILURE!!! Redirect failed to acquire the lock after " + lockTimeout + "ms! Queueing call after 500ms!");
                Waiting.CallDelayed.CallAction(() => FromMethod(toBeRedirected, toBeRan, skipOriginal), 0.5f, true);
            } 
        }
    }

    #region Logging
    /// <summary>
    /// Whether to allow the (frankly copious amounts of) log statements in <see cref="FromMethod{TDelegate}(MethodInfo, TDelegate, bool)"/> to output to the log file.
    /// <para>Debug builds default to false, release builds default to true.</para>
    /// </summary>
    public static bool DisableLogging { get => DynTools.disableLogging; set { DynTools.disableLogging = value; } }
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!DynTools.disableLogging) JeviLib.Log("REDIRECTOR -> " + str, ConsoleColor.DarkGray);
    }
    #endregion

#if DEBUG
    internal class Tester
    {
        private static Tester _testAgainst;
        internal static void TestRedirect()
        {
            _testAgainst = new();
            //MethodInfo noRet = typeof(Tester).GetMethod(nameof(NoReturn), Const.AllBindingFlags);
            FromDelegate(_testAgainst.NoReturn, RedirectNoReturn);

            _testAgainst.NoReturn("astrazenica", true);

            string pre = _testAgainst.ReturnString("soxon", new(0, 1, 2));
            FromDelegate(_testAgainst.ReturnString, RedirectReturnString);
            string post = _testAgainst.ReturnString("soxon", new(0, 1, 2));
            JeviLib.Log($"pre == post (want false)? {pre == post}; PRE: {pre}, POST: {post}");

            JeviLib.Log($"hooking noreturn");
            Hook.OntoDelegate(_testAgainst.NoReturn, HookNoReturn);
        }

        internal static void HookNoReturn(bool shitass, Tester farquaad, string balls)
        {
            JeviLib.Log($"Hooked {nameof(NoReturn)}, {nameof(shitass)}={shitass}, {nameof(farquaad)}={farquaad}, {nameof(balls)}={balls}");
        }

        internal static void RedirectNoReturn(bool fuckstick, string borzoi, Tester testyMcTesterson)
        {
            JeviLib.Log($"Successfully redirected {nameof(NoReturn)}; {nameof(fuckstick)}={fuckstick}, {nameof(borzoi)}={borzoi}, {nameof(testyMcTesterson)}={testyMcTesterson}, testester==testagainst? {testyMcTesterson == _testAgainst}");
        }

        internal static string RedirectReturnString(string bababooey, UnityEngine.Vector3 oneHundredVecs)
        {
            return oneHundredVecs.ToString() + " " + bababooey;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // no inlining cause its a managed method and would be inlined by the runtime
        internal void NoReturn(string strazinga, bool toggalog)
        {

        }

        [MethodImpl(MethodImplOptions.NoInlining)] // no inlining cause its a managed method and would be inlined by the runtime
        internal string ReturnString(string kazoingus, UnityEngine.Vector3 birkenstocks)
        {
            return kazoingus + " " + birkenstocks.ToString();
        }
    }
#endif
}
