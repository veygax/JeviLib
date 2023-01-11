using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Jevil.Patching;

internal static class SupportModuleOverwriter
{
    public static bool isQuest;
    public static Action<string> Log;
    public static Action<string> Error;

    public static void Execute(string newSmPath, string currSmPath)
    {
#if DEBUG
        if (Log == null || Error == null)
            throw new NullReferenceException("Logging callbacks cannot be null.");
#endif

        string backupsFolder = Path.Combine(Path.GetDirectoryName(newSmPath), "Backups");
        string backupFile = Path.Combine(backupsFolder, "Il2Cpp.bak.dll");
        using (FileStream dllWriteStream = File.Create(newSmPath))
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string resourcePath = asm.GetManifestResourceNames().First(p => p.EndsWith("Resources.Il2Cpp.dll"));
            using Stream stream = asm.GetManifestResourceStream(resourcePath);
            stream.CopyTo(dllWriteStream);
        }

        Log("Wrote new support module to disk in working directory at " + newSmPath);
        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current support module to " + backupFile);
        File.Copy(newSmPath, backupFile, true);
        Log("Successfully wrote support module backup.");

        Log("Replacing enumerator wrapper...");
        Stream newSupportModule = CecilNewEnumeratorIfNeeded(newSmPath);
        Log("New support module successfully replaced and written to in-memory stream.");
        
        Log("Deleting old support module.");
        File.Delete(currSmPath);
        Log("Old support module deleted. Creating new file in its place and opening stream to write.");
        using (FileStream currSmOverwritten = File.OpenWrite(currSmPath))
        {
            newSupportModule.CopyTo(currSmOverwritten);
            currSmOverwritten.Flush();
        }
        Log("Wrote new IL2CPP support module.");
    }

    private static Stream CecilNewEnumeratorIfNeeded(string newSmPath)
    {
        MemoryStream memoryStream = new();
        if (!isQuest)
        {
            Log("Executing platform is Windows - able to use precompiled support module without modifications");
            File.OpenRead(newSmPath).CopyTo(memoryStream);
            return memoryStream;
        }

        Log("Executing platform is Android - New support module must have one method removed.");
        Log("Reading new support module...");
        using AssemblyDefinition newSm = AssemblyDefinition.ReadAssembly(newSmPath);
        Log("Success!");

        // Manually ret MelonLoader.Support.Main.ClearConsole() - It gets returned from immediately in LL, see https://github.com/LemonLoader/MelonLoader/blob/lemon/Dependencies/SupportModules/Il2Cpp/Main.cs#L60
        Log("Finding MelonLoader.Support.Main.ClearConsole()");
        MethodDefinition smMain = newSm.MainModule.GetType("MelonLoader.Support.Main").Methods.First(m => m.Name == "ClearConsole");
        Log("Success!");

        Log("Stripping ClearConsole()'s method body...");
        MethodBody smMainBody = smMain.Body;
        smMainBody.ExceptionHandlers.Clear();
        smMainBody.Variables.Clear();
        ILProcessor ilProc = smMainBody.GetILProcessor();

        ilProc.Emit(OpCodes.Ret);
        Log("Success!");

        Log("Writing new support module to RAM");
        newSm.Write(memoryStream);
        Log("Success! Returning back to main execution.");
        return memoryStream;
    }
}
