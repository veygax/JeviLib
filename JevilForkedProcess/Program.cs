using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Mono.Cecil;
using System.Runtime.CompilerServices;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Reflection;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;

namespace Jevil
{
    internal class Program
    {
        static StreamWriter logFile;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Error(e.ExceptionObject.ToString());

            if (args.Length == 0)
            {
                Console.Error.WriteLine("Need path to ML UserData folder!");
                return;
            }

            System.Threading.Thread.Sleep(50); // Wait for parent to close
            Process me = Process.GetCurrentProcess();
            Process parent = ParentProcessUtilities.GetParentProcess();
            if (parent.ProcessName.Contains("BONELAB"))
            {
                Console.WriteLine("Current process is a child of BONELAB! Starting new process to fork.");
                Process newProc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = string.Join(" ", args),
#if DEBUG
                        CreateNoWindow = false,
#else
                        CreateNoWindow = true,
#endif
                        UseShellExecute = true,
                        FileName = Assembly.GetExecutingAssembly().Location,
                    }
                };
                newProc.Start();
                Environment.Exit(0);
            }

#if DEBUG
            if (!Debugger.IsAttached) Debugger.Launch();
#endif

            string userDataFolder = string.Join(" ", args);
            string jsmPath = Path.Combine(userDataFolder, "JevilSM");
            string logPath = Path.Combine(jsmPath, "FixerLog.log");
            string newSmPath = Path.Combine(jsmPath, "Il2Cpp.dll");
            string il2SmPath = Path.Combine(userDataFolder, "..", "MelonLoader", "Dependencies", "SupportModules", "Il2Cpp.dll");
            string unitaskPath = Path.Combine(userDataFolder, "..", "MelonLoader", "Managed", "UniTask.dll");
            string il2MscorlibPath = Path.Combine(userDataFolder, "..", "MelonLoader", "Managed", "Il2Cppmscorlib.dll");
            string unhollowerBasePath = Path.Combine(userDataFolder, "..", "MelonLoader", "Managed", "UnhollowerBaseLib.dll");
            if (!Directory.Exists(jsmPath)) Directory.CreateDirectory(jsmPath);
            if (File.Exists(logPath)) File.Delete(logPath);
            logFile = File.CreateText(logPath);
            Log("Logging to " + logPath);


            Log($"Current support module path: {il2SmPath}");
            Log($"Going to write support module to: {newSmPath}");
            Log($"UniTask assembly path: {unitaskPath}");
            Log($"Unhollowed IL2CPP mscorlib assembly path: {il2MscorlibPath}");
            Log($"UnhollowerBaseLib assembly path: {unhollowerBasePath}");

            using (FileStream dllWriteStream = File.Create(il2SmPath))
            {
                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JevilForkedProcess.Resources.Il2Cpp.dll");
                stream.CopyTo(dllWriteStream);
            }
            Log("Wrote support module to disk in UserData (see above for path).");


            WaitForBonelabExit();
            Log("BONELAB exited, continuing execution.");

            if (File.Exists(il2SmPath))
            {
                File.Delete(il2SmPath);
                Log("Deleted current IL2CPP support module.");
            }
            
            File.Copy(newSmPath, il2SmPath);
            Log("Copied new IL2CPP support module.");

            CecilAssembly(unitaskPath, il2MscorlibPath, unhollowerBasePath);
            Log("Successfully replaced IL2CPP support module and patched UniTask.dll!");
            Log("For users: You should now be able to use any mod that requires JeviLib v1.2.0 or higher");
            Log("For developers: Your coroutines can now yield other coroutines or yield WaitForSeconds/WaitForSecondsRealtime, and it will work as expected. You can also await a UniTask and UniTask<T> from a Task");
            Log("              - (AsyncOperations do not work unfortunately. If it is a big enough deal, ping me, extraes#2048, about it and I'll see what I can do)");
        }

        static void WaitForBonelabExit()
        {
            Process bonelab = Process.GetProcessesByName("BONELAB_Steam_Windows64").FirstOrDefault() 
                ?? Process.GetProcessesByName("BONELAB_Oculus_Windows64").FirstOrDefault();

            if (bonelab == null)
            {
                Log("Couldn't find BONELAB process. Continuing with fixes.");
                return;
            }
            Log("Waiting for BONELAB to exit. PID = " + bonelab.Id);
            bonelab.WaitForExit();
        }

        static void CecilAssembly(string unitaskAsmPath, string il2cppMscorlibPath, string unhollowerBaselibPath)
        {
            // Added copious
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

        #region https://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way
        /// <summary>
        /// A utility class to determine a process parent.
        /// </summary>
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

            /// <summary>
            /// Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                ParentProcessUtilities pbi = new ParentProcessUtilities();
                int returnLength;
                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                    throw new Win32Exception(status);

                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }

        #endregion

        #region Logging
        static void Log(string str)
        {
            Console.WriteLine(str);
            logFile.WriteLine(str);
            logFile.Flush();
        }

        static void Error(string str) 
        { 
            // try-catch to prevent stackoverflow caused if logfile is null
            try
            {
                Log("ERROR: " + str);
            }
            catch{ }
        }
        #endregion
    }
}
