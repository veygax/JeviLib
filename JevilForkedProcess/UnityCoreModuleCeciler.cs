using Mono.Cecil;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

internal static class UnityCoreModuleCeciler
{
    public static Action<string> Log;
    public static Action<string> Error;

    public static void Execute(string unityCorePath, string userData)
    {
        Log("UnityEngine CoreModule path: " + unityCorePath);

        string backupsFolder = Path.Combine(userData, "JevilSM", "Backups");
        string backupFile = Path.Combine(backupsFolder, "UnityEngine.CoreModule.bak.dll");

        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current UnityEngine.CoreModule.dll to " + backupFile);
        File.Copy(unityCorePath, backupFile, true);
        Log("Successfully wrote UnityEngine.CoreModule.dll backup.");

        Log("Reading UnityEngine.CoreModule...");
        AssemblyDefinition unityAsm = AssemblyDefinition.ReadAssembly(unityCorePath);
        ModuleDefinition modDef = unityAsm.MainModule;
        Log("Read successfully!");

        Log("Iterating type definitions...");
        int count = 0;
        foreach (TypeDefinition type in modDef.Types)
        {

            foreach (MethodDefinition method in type.Methods)
            {
                if (method.Name == "ToString" && method.Parameters.Count == 0)
                {
                    Log("Setting virtual flag to true on method: " + method.FullName);
                    method.IsVirtual = true;
                    count++;
                }
            }
        }
        Log($"Finished iterating type definitions. Modified {count} types/methods.");

        Log("Writing assembly to memory...");
        using MemoryStream ms = new();
        unityAsm.Write(ms);
        unityAsm.Dispose();
        Log("Success! Now writing to disk...");
        File.WriteAllBytes(unityCorePath, ms.ToArray());
        Log("Success!");
    }
}
