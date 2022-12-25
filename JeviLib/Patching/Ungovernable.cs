using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Jevil.Waiting;
using MelonLoader;
using Mono.Cecil;
using UnityEngine;

namespace Jevil.Patching;

internal static class Ungovernable
{
    public delegate void VoidParameterless();
    static VoidParameterless returnDelegate;
    static HarmonyMethod returnPatch = Utilities.AsInfo(Utilities.ReturnToMainThread).ToNewHarmonyMethod();
    static HarmonyLib.Harmony harmony = new HarmonyLib.Harmony(JeviLib.instance.HarmonyInstance.Id + ".Ungovernable");
    static HarmonyMethod mainThreadReturnsTranspiler = Utilities.AsInfo(AddMainThreadReturns).ToNewHarmonyMethod();
    static HarmonyMethod coroutinePreMoveNext = Utilities.AsInfo(CoroutinePreMoveNext).ToNewHarmonyMethod();
    //static HarmonyMethod coroutinePostMoveNext = Utilities.AsInfo(CoroutinePostMoveNext).ToNewHarmonyMethod();
    static HarmonyMethod waitTranspiler = Utilities.AsInfo(ChangeWaiters).ToNewHarmonyMethod();
    static Dictionary<IEnumerator, IEnumerator> waitingCoroutineToJevilAlternative = new();

    public static void Init()
    {
        returnDelegate = Utilities.AsInfo(Utilities.ReturnToMainThread).CreateDelegate(typeof(VoidParameterless)) as VoidParameterless;

        Type waitSecType = typeof(WaitSeconds);
        Type waitSecRealType = typeof(WaitSecondsReal);
        //AssemblyDefinition jevilAsm = AssemblyDefinition.ReadAssembly(typeof(Ungovernable).Assembly.Location);
        //ModuleDefinition modDef = jevilAsm.MainModule;
        //waitSecondsRef = modDef.Types.First(td => td.FullName == waitSecType.FullName);
        //waitSecondsRealRef = modDef.Types.First(td => td.FullName == waitSecRealType.FullName);

    }

    internal static void TranspileAssembly(Assembly asm, UngovernableType govt)
    {
#if DEBUG

#endif
        MethodInfo[] methods = asm.GetTypes()
                                  .SelectMany(t => t.GetMethods(Const.AllBindingFlags))
                                  .ToArray();

        IteratorStateMachineAttribute[] stateMachineAttrs = methods.Select(m => m.GetCustomAttribute<IteratorStateMachineAttribute>())
                                                                              .NoNull()
                                                                              .ToArray();
        MethodInfo[] enumeratorMoveNext = stateMachineAttrs.Select(stateMachine => stateMachine.StateMachineType.GetMethod("MoveNext", Const.AllBindingFlags))
                                                                      .ToArray();
        (IteratorStateMachineAttribute attr, MethodInfo moveNext)[] enumeratorsZipped = stateMachineAttrs.Zip(enumeratorMoveNext);



        IEnumerable<MethodInfo> asyncMoveNexts = methods.Select(m => m.GetCustomAttribute<AsyncStateMachineAttribute>())
                                                   .NoNull()
                                                   .Select(stateMachine => stateMachine.StateMachineType.GetMethod("MoveNext", Const.AllBindingFlags));

        if (govt.HasFlag(UngovernableType.I_FUCKING_SAID_WAIT))
        {
            JeviLib.Log($"Transpiling {asm.GetName().Name} to get coroutine waiting to function properly.");
            foreach ((IteratorStateMachineAttribute isam, MethodInfo moveNext) in enumeratorsZipped)
            {
#if DEBUG
                JeviLib.Log($"Transpiling MoveNext on {moveNext.DeclaringType.Name} to switch to waiters controlled by the mono domain.");
#endif
                //harmony.Patch(moveNext, transpiler: transpiler);
                harmony.Patch(moveNext, prefix: coroutinePreMoveNext, /*postfix: coroutinePostMoveNext,*/ transpiler: waitTranspiler);
            }
        }
    }

    static IEnumerable<CodeInstruction> AddMainThreadReturns(IEnumerable<CodeInstruction> instructions)
    {
        return instructions;
    }

    static bool CoroutinePreMoveNext(IEnumerator __instance, ref bool __result)
    {
#if DEBUG
        JeviLib.Log($"Coro type = {__instance.GetType().FullName}", ConsoleColor.White);
#endif
        if (__instance.Current is WaitSeconds waitSeconds)
        {
            bool ret = waitSeconds.MoveNext(); // returns true if more waiting is needed (aka: if the original coroutine should continue to be stalled)
#if DEBUG
            JeviLib.Log($"Coroutine's current is a {nameof(WaitSeconds)} set for {waitSeconds.length}, that has elapsed {waitSeconds.timeElapsed} seconds. Ret={ret}", ConsoleColor.Gray);
#endif
            __result = ret;
            return ret;
        }
        else if (__instance.Current is WaitSecondsReal waitSecondsReal)
        {
            bool ret = waitSecondsReal.MoveNext();
#if DEBUG
            JeviLib.Log($"Coroutine's current is a {nameof(WaitSecondsReal)} set for {waitSecondsReal.length}, that has elapsed {waitSecondsReal.timeElapsed} seconds. Ret={ret}", ConsoleColor.Gray);
#endif
            __result = ret;
            return ret;
        }
        return true;
    }

    static IEnumerable<CodeInstruction> ChangeWaiters(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Newobj)
            {
                // UnityEngine.WaitForSecondsRealtime
                //JeviLib.Log($"Instruction {instruction} of type {instruction.GetType()} creates a new object (opcode {instruction.opcode}, opctype={instruction.opcode.OpCodeType}, oprtype={instruction.opcode.OperandType}) with operand {instruction.operand} of type {instruction.operand.GetType().FullName} (def'd in {instruction.operand.GetType().AssemblyQualifiedName})");

                // jankily (shittily) identify instructions that create WFSRT's and WFS's
                string str = instruction.ToString();
                if (str.StartsWith("newobj void UnityEngine.WaitForSecondsRealtime::.ctor(float time)")) instruction.operand = WaitSecondsReal.ctor; // this is such a SHIT idea
                if (str.StartsWith("newobj void UnityEngine.WaitForSeconds::.ctor(float time)")) instruction.operand = WaitSeconds.ctor; // this is also such a SHIT idea
            }
            yield return instruction;
        }
    }
}