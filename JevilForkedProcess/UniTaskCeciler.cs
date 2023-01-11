using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Jevil.Patching;

internal static class UniTaskCeciler
{
    public static Action<string> Log;
    public static Action<string> Error;

    public static void Execute(string unitaskAsmPath, string il2cppMscorlibPath, string unhollowerBaselibPath)
    {
#if DEBUG
        if (Log == null || Error == null) 
            throw new NullReferenceException("Logging callbacks cannot be null.");
#endif
        string backupsFolder = Path.Combine(Path.GetDirectoryName(unitaskAsmPath), "..", "..", "UserData", "JevilSM", "Backups");
        string backupFile = Path.Combine(backupsFolder, "UniTask.bak.dll");

        if (!Directory.Exists(backupsFolder))
        {
            Directory.CreateDirectory(backupsFolder);
            Log("Created backup directory " + backupsFolder);
        }
        Log("Backing up current UniTask.dll to " + backupFile);
        File.Copy(unitaskAsmPath, backupFile, true);
        Log("Successfully wrote UniTask.dll backup.");

        // Added copious log statements
        Log("Creating temporary file for modified UniTask.dll");
        string tempPath = Path.GetTempFileName();
        FileStream tempStream = new FileStream(tempPath, FileMode.Open);
        Log("Success! Temporary file location: " + tempPath);

        Log("Reading UnhollowerBaseLib for type reference...");
        TypeDefinition il2cppObjectRef = AssemblyDefinition.ReadAssembly(unhollowerBaselibPath).MainModule.GetType("UnhollowerBaseLib.Il2CppObjectBase");
        Log("Success!");

        Log("Reading Il2Cppmscorlib for type references...");
        AssemblyDefinition il2Mscorlib = AssemblyDefinition.ReadAssembly(il2cppMscorlibPath);
        TypeDefinition il2ActionGeneric = il2Mscorlib.MainModule.GetType("Il2CppSystem.Action`1");
        TypeDefinition il2ActionUntyped = il2Mscorlib.MainModule.GetType("Il2CppSystem.Action");
        Log("Success!");

        Log("Getting conversion methods from Il2Cppmscorlib...");
        MethodDefinition monoActionToIl2ActionGenericDef = il2ActionGeneric.Methods.First(m => m.Name == "op_Implicit");
        MethodDefinition monoActionToIl2ActionUntypedDef = il2ActionUntyped.Methods.First(m => m.Name == "op_Implicit");
        Log("Success!");

        Log("Reading .NET Framework mscorlib for type references...");
        AssemblyDefinition mscorlib = AssemblyDefinition.ReadAssembly(typeof(INotifyCompletion).Assembly.Location);
        TypeDefinition notifyCompletionDef = mscorlib.MainModule.GetType(typeof(INotifyCompletion).FullName);
        TypeDefinition voidDef = mscorlib.MainModule.GetType(typeof(void).FullName);
        TypeDefinition actionGenericDef = mscorlib.MainModule.GetType(typeof(Action).FullName + "`1");
        TypeDefinition actionUntypedDef = mscorlib.MainModule.GetType(typeof(Action).FullName);
        Log("Success!");

        Log("Loading UniTask.dll to modify...");
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(unitaskAsmPath);
        Log("Success!");

        Log("Importing references for UniTask...");
        MethodReference monoActionToIl2ActionGenericRef = asm.MainModule.ImportReference(monoActionToIl2ActionGenericDef);
        MethodReference monoActionToIl2ActionUntypedRef = asm.MainModule.ImportReference(monoActionToIl2ActionUntypedDef);
        TypeReference voidRef = asm.MainModule.ImportReference(voidDef);
        TypeReference actionGenericRef = asm.MainModule.ImportReference(actionGenericDef);
        TypeReference actionUntypedRef = asm.MainModule.ImportReference(actionUntypedDef);
        TypeReference notifyCompletionRef = asm.MainModule.ImportReference(notifyCompletionDef);
        Log("Success!");


        Log("Looking for UniTask Awaiter type definitions...");
        TypeDefinition uniTaskAwaiterGeneric = asm.MainModule.GetType("Cysharp.Threading.Tasks.UniTask`1/Awaiter");
        TypeDefinition uniTaskAwaiterUntyped = asm.MainModule.GetType("Cysharp.Threading.Tasks.UniTask/Awaiter");
        Log("Success!");

        Log("Looking for UniTask Awaiter OnCompleted method definitions...");
        MethodDefinition uniTaskAwaiterOnCompletedGeneric = uniTaskAwaiterGeneric.Methods.First(m => m.Name == nameof(INotifyCompletion.OnCompleted));
        MethodDefinition uniTaskAwaiterOnCompletedUntyped = uniTaskAwaiterUntyped.Methods.First(m => m.Name == nameof(INotifyCompletion.OnCompleted));
        Log("Success!");

        // Clear previous runs
        Log("Checking for evidence of previous runs & undoing them...");
        if (uniTaskAwaiterGeneric.Interfaces.Any(i => i.InterfaceType == null) || uniTaskAwaiterGeneric.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            uniTaskAwaiterGeneric.Interfaces.Clear();
        if (uniTaskAwaiterUntyped.Interfaces.Any(i => i.InterfaceType == null) || uniTaskAwaiterUntyped.Interfaces.Count(i => i.InterfaceType.Name == nameof(INotifyCompletion)) >= 1)
            uniTaskAwaiterUntyped.Interfaces.Clear();
        var md1 = uniTaskAwaiterGeneric.Methods.FirstOrDefault(m => m.Name == "OnCompleted" && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        var md2 = uniTaskAwaiterUntyped.Methods.FirstOrDefault(m => m.Name == "OnCompleted" && !m.Parameters.FirstOrDefault().ParameterType.FullName.ToLower().Contains("il2"));
        if (md1 != null) uniTaskAwaiterGeneric.Methods.Remove(md1);
        if (md2 != null) uniTaskAwaiterUntyped.Methods.Remove(md2);
        Log("Success!");

        // Create new methods to host action conversion
        Log("Creating new method definitions for UniTask Awaiters...");
        // Copy method attributes from System.Runtime.CompilerServices.TaskAwaiter.OnCompleted.
        // If this is not done, using UniTask.Awaiter (generic and untyped) will throw "System.TypeLoadException: VTable setup of type Cysharp.Threading.Tasks.UniTask+Awaiter failed"
        MethodAttributes onCompletedAttr = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
        MethodDefinition notifyCompletionGeneric = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef);
        MethodDefinition notifyCompletionUntyped = new(nameof(INotifyCompletion.OnCompleted), onCompletedAttr, voidRef);
        TypeReference genericActionRefMatchesTask = actionGenericRef.MakeGenericInstanceType(uniTaskAwaiterGeneric.GenericParameters[0]);
        notifyCompletionGeneric.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, genericActionRefMatchesTask));
        notifyCompletionUntyped.Parameters.Add(new ParameterDefinition("jevilSaysHi", ParameterAttributes.None, actionUntypedRef));
        Log("Success!");

        // Create method bodies
        Log("Filling out method bodies for UniTask Awaiters...");
        Log(" - Creating bodies & getting IL Processors...");
        MethodBody genericBody = new(notifyCompletionGeneric);
        MethodBody untypedBody = new(notifyCompletionGeneric);
        ILProcessor genericProc = genericBody.GetILProcessor();
        ILProcessor untypedProc = untypedBody.GetILProcessor();
        Log(" - Success!");

        // pasting the IL from some dummy methods real quick
        Log(" - Filling in method body for the generic awaiter...");
        genericProc.Emit(OpCodes.Ldarg_0);
        genericProc.Emit(OpCodes.Ldarg_1); //ldarg1 because not static method
        genericProc.Emit(OpCodes.Call, monoActionToIl2ActionGenericRef);
        genericProc.Emit(OpCodes.Call, uniTaskAwaiterOnCompletedGeneric);
        genericProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Filling in method body for the non-generic awaiter...");
        untypedProc.Emit(OpCodes.Ldarg_0);
        untypedProc.Emit(OpCodes.Ldarg_1); //ldarg1 because not static method
        untypedProc.Emit(OpCodes.Call, monoActionToIl2ActionUntypedRef);
        untypedProc.Emit(OpCodes.Call, uniTaskAwaiterOnCompletedUntyped);
        untypedProc.Emit(OpCodes.Ret);
        Log(" - Success!");

        Log(" - Assigning and optimizing method bodies");
        notifyCompletionGeneric.Body = genericBody;
        notifyCompletionUntyped.Body = untypedBody;
        notifyCompletionGeneric.Body.Optimize();
        notifyCompletionUntyped.Body.Optimize();
        Log(" - Success!");

        Log(" - Adding new methods to UniTask Awaiters...");
        uniTaskAwaiterGeneric.Methods.Add(notifyCompletionGeneric);
        uniTaskAwaiterUntyped.Methods.Add(notifyCompletionUntyped);
        Log(" - Success!");

        Log("Success!");

        Log("Making UniTask Awaiters implement interface...");
        uniTaskAwaiterGeneric.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        uniTaskAwaiterUntyped.Interfaces.Add(new InterfaceImplementation(notifyCompletionRef));
        Log("Success!");

        Log("The moment of truth: Writing new UniTask assembly...");
        asm.Write(tempStream);
        Log("Success! The hard part's over, UniTask is now patched!");

        Log("Cleaning up temporary items...");

        Log(" - Flushing & closing file writer...");
        tempStream.Flush();
        tempStream.Dispose();
        Log(" - Success!");

        Log(" - Closing UniTask.dll (Disposing)...");
        asm.Dispose();
        Log(" - Success!");

        Log(" - Copying temporary file to UniTask.dll path...");
        File.Copy(tempPath, unitaskAsmPath, true);
        Log(" - Success!");

        Log(" - Deleting temporary file...");
        File.Delete(tempPath);
        Log(" - Success!");
        Log("Success!");
    }
}
