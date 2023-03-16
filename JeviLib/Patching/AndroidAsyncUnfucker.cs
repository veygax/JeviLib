using Jevil.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jevil.Patching;

// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST
// FUCK OCULUS QUEST

internal static class AndroidAsyncUnfucker
{
    internal static void Init()
    {
        Redirect.FromMethod(typeof(SynchronizationContext).GetMethod("GetThreadLocalContext", Const.AllBindingFlags), FuckYourContext, true);
    }

    static SynchronizationContext FuckYourContext()
    {
        return null; // go fuck yourself
    }
}
