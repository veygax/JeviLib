using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Jevil.Patching;

[HarmonyPatch(typeof(SocketAsyncEventArgs), "OnCompleted")]
internal static class FixSocketAsyncEventArgs
{
    public static bool Prefix()
    {
#if DEBUG
        JeviLib.Log($"Caught call to {nameof(SocketAsyncEventArgs)}.OnCompleted. On Quest this call will hang the game, so it is patched out.");
#endif
        // LemonLoader hangs on startup if this isn't patched out. IDK why.
        // For full trace when hang occurs, see: https://cdn.discordapp.com/attachments/716834164762083374/1053981933232738335/image.png
        return !Utilities.IsPlatformQuest();
    }
}
