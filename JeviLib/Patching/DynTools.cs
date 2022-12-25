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

internal static class DynTools
{
    public const string dynamicAsmName = "JeviLib Dynamic Patch Assembly Host";
    public static readonly ModuleBuilder DynamicModuleBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(dynamicAsmName), AssemblyBuilderAccess.RunAndSave)
                                                                      .DefineDynamicModule("Modulo");
    internal static bool disableLogging =
#if DEBUG
        false;
#else
        true;
#endif
    public static TypeBuilder GetTypeBuilder(string extraIdentifier)
    {
        return DynamicModuleBuilder.DefineType("DynamicMethodHost_" + extraIdentifier);
    }

    internal static MethodInfo GetMethodInfo(this MethodBuilder builder)
    {
        return builder.DeclaringType.GetMethod(builder.Name, Const.AllBindingFlags);
    }


    internal static List<ParameterInfo> RemoveUnmatchedParameters(MethodBase source, MethodInfo dest)
    {
        List<ParameterInfo> srcParams = source.GetParameters().ToList();
        ParameterInfo[] destParams = dest.GetParameters();
        List<ParameterInfo> ret = new();
        bool alreadyInstanced = false;

        foreach (ParameterInfo param in destParams)
        {
            if (!alreadyInstanced && param.ParameterType == source.DeclaringType)
            {
                Log($"Skipping delegate parameter {param.ParameterType} {param.Name} because it's the first parameter that's the same type as the type that's going to be patched.");
                alreadyInstanced = true;
                ret.Add(param); // should work methinks
                continue;
            }

            ParameterInfo pinf = srcParams.FirstOrDefault(p => p.ParameterType == param.ParameterType);
            if (pinf is null)
            {
                throw new InvalidHarmonyPatchArgumentException($"Patch parameter has no matching paramter in original method! {param.ParameterType.FullName} {param.Name}", source, dest);
            }
            srcParams.Remove(pinf);
            ret.Add(pinf);
        }
        Log("Patch will have " + ret.Count + " parameters passed from patch to redirect.");

        return ret;
    }

    internal static List<ParameterExpression> GetMethodParameters(IEnumerable<ParameterInfo> infos, Type instanceType, bool isStatic)
    {
        List<ParameterExpression> ret = new(infos.Count());
        bool alreadyInstanced = isStatic;
        foreach (ParameterInfo pinf in infos)
        {
            string name = pinf.Name;

            if (!alreadyInstanced && pinf.ParameterType == instanceType)
            {
                Log($"Replacing parameter name '{pinf.Name}' with '__instance'");
                name = "__instance";
                alreadyInstanced = true;
            }

            ret.Add(Expression.Parameter(pinf.ParameterType, name));
        }
        return ret;
    }

    #region Logging
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!disableLogging) JeviLib.Log("DYNAMICTOOLS -> " + str, ConsoleColor.DarkGray);
    }
    #endregion
}
