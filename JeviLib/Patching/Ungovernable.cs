using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace Jevil.Patching;

internal static class Ungovernable
{
    const int FIELD_INJECTOR_DEBUG
#if DEBUG
        = 99;
#else
        = 0;
#endif

    public delegate void SerializationHandler_Inject(Type t, int debugLevel);
    static SerializationHandler_Inject injectionDelegate;

    public static void Init()
    {
        MethodInfo serializationHandler_Inject = Utilities.GetMethodFromString("FieldInjector", "SerialisationHandler", "Inject", new string[] { nameof(Type), nameof(Int32) });
        if (serializationHandler_Inject == null) return;

        injectionDelegate = serializationHandler_Inject.CreateDelegate(typeof(SerializationHandler_Inject)) as SerializationHandler_Inject;
    }

    private static List<Type> InjectSerialization(Assembly redirectedAsm, bool injectScriptableObjects)
    {
        List<Type> ret = new();

        foreach (Type type in redirectedAsm.GetTypes())
        {
            bool doInject = type.IsSubclassOf(typeof(MonoBehaviour)) || (injectScriptableObjects && type.IsSubclassOf(typeof(ScriptableObject)));
            if (!doInject) continue;

            injectionDelegate(type, FIELD_INJECTOR_DEBUG);
            
            ret.Add(type);
        }

#if DEBUG
        JeviLib.Log($"Injected {ret.Count} types from assembly {redirectedAsm.GetName().Name}");
#endif
        return ret;
    }

    // must use harmony patch the old fashioned way because Disable.When cant specify that its delegate returns a bool while also taking arguments. i love C# but it would be nice if Delegate got some more attention
    //todo: retarget bonelib when/if it ever handles cmb's
    //[HarmonyPatch(typeof(CustomMonoBehaviourHandler), nameof(CustomMonoBehaviourHandler.RegisterAndReturnAllInAssembly))]
    public static class RegisterPatch
    {
        public static bool Prefix(Assembly assembly, ref Type[] __result)
        {
            UngovernableAttribute attribute = assembly.GetCustomAttribute<UngovernableAttribute>();
            if (attribute == null || injectionDelegate == null) return true;

#if DEBUG
            JeviLib.Log($"Redirected RegisterAndReturnAllInAssembly for assembly {assembly.GetName().Name}");
#endif

            __result = InjectSerialization(assembly, attribute.alsoInjectScriptableObjects).ToArray();
            return false;
        }
    }
}
