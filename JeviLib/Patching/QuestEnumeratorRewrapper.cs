using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace Jevil.Patching;

internal static class QuestEnumeratorRewrapper
{
    // basically a paste of the code i used to write the new enumerator wrapper
    private class JevilWrapper : IEnumerator
    {
        Stack<IEnumerator> enumerators = new(1);
        bool useScaledTime;
        float waitTime;

        public JevilWrapper(IEnumerator enumerator)
        {
            enumerators.Push(enumerator);
        }

        public object Current
        {
            get
            {
                return enumerators.Peek().Current switch
                {
                    IEnumerator next => new JevilWrapper(next), // this switch case will likely never get called due to enumerator stack
                    Il2CppSystem.Object il2obj => il2obj,
                    null => null,
                    _ => throw new NotSupportedException($"{enumerators.Peek().GetType().FullName}: Unsupported type {enumerators.Peek().Current.GetType().FullName}")
                };
            }
        }

        public bool MoveNext()
        {
            try
            {
                if (waitTime <= 0)
                {
                    bool ret = enumerators.Peek().MoveNext();
                    object curr = enumerators.Peek().Current;

                    // Run post-execution checks to restore functionality of WFS and WFSRT in newer unity versions (e.g. 2021.3.5f1 BONELAB)
                    switch (curr)
                    {
                        case WaitForSeconds wfs:
                            useScaledTime = true;
                            waitTime = wfs.m_Seconds;
                            break;
                        case WaitForSecondsRealtime wfsrt:
                            useScaledTime = false;
                            waitTime = wfsrt.waitTime;
                            break;
                        case IEnumerator subCoro:
                            enumerators.Push(subCoro);
                            break;
                        default:
                            break;
                    }

                    return ret;
                }

                waitTime -= useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
                return true;
            }
            catch (Exception e)
            {
                var melon = MelonUtils.GetMelonFromStackTrace(new System.Diagnostics.StackTrace(e), true);

                MelonLogger.Instance logger;

                if (melon != null)
                {
                    logger = melon.LoggerInstance;
                    logger.Error("Unhandled exception in coroutine. It will not continue executing.", e);
                }
                else
                {
                    logger = JeviLib.instance.LoggerInstance;
                    logger.Error("[Error: Could not identify source] Unhandled exception in coroutine. It will not continue executing.", e);
                }

                if (enumerators.Count > 1)
                {
                    logger.Error("Coroutine call stack unwind: ");
                    IEnumerator called = enumerators.Pop();
                    IEnumerator calledFrom = enumerators.Pop();
                    while (enumerators.Count != 0)
                    {
                        logger.Error($"\t{called.GetType().FullName} was called from {calledFrom.GetType().FullName}");
                        calledFrom = called;
                        called = enumerators.Pop();
                    }
                }

                return false;
            }
        }

        public void Reset()
        {
            for (int i = enumerators.Count - 1; i >= 1; i--)
            {
                // I don't think it matters whether I reset the ones I pop, but I'm just making sure.
                enumerators.Pop().Reset();
            }
            enumerators.Peek().Reset();
        }
    }

    public static void Init()
    {
        // use harmony directly because i cant remember if jevil patching supports ref parameters/changing parameters, and i cannot be fucked to check
        JeviLib.instance.HarmonyInstance.Patch(typeof(MelonCoroutines).GetMethod(nameof(MelonCoroutines.Start)), prefix: Utilities.ToHarmony(WrapIncomingEnumerator));
    }

    static void WrapIncomingEnumerator(ref IEnumerator routine)
    {
        routine = new JevilWrapper(routine);
    }

    // PatchShield also checks for PatchShield on Types and Methods but i cannot be fucked to find what i need to patch an abstract method
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetCustomAttributes), typeof(Type), typeof(bool))]
    private static class PatchUnshielder
    {
#if DEBUG
        static bool alreadyProsthelytized;
#endif
        public static bool Prefix(Type attributeType, ref object[] __result)
        {
            if (attributeType == typeof(PatchShield))
            {
#if DEBUG
                if (alreadyProsthelytized)
                    JeviLib.Log("Assembly PatchShield denied.");
                else
                    JeviLib.Log("I have a vendetta against the MelonLoader PatchShield. This PatchShield disabler only has an explicit use for JeviLib on Android, but it pisses me off so much I'm keeping it enabled for any platform.");
                alreadyProsthelytized = true;
#endif
                __result = Array.Empty<object>();
                return false;
            }

            return true;
        }
    }
}
