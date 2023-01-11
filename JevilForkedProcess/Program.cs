﻿using System;
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

namespace JevilForkedProcess;

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

        WaitForBonelabExit();
        Log("BONELAB exited, continuing execution.");

        Log("Support module updating can now begin.");
        Jevil.Patching.SupportModuleOverwriter.isQuest = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows); // This could just be hardcoded false
        Jevil.Patching.SupportModuleOverwriter.Log = Log;
        Jevil.Patching.SupportModuleOverwriter.Error = Error;
        Jevil.Patching.SupportModuleOverwriter.Execute(newSmPath, il2SmPath);
        Log("Support module updating completed successfully.");

        Log("UniTask modification can now begin.");
        Jevil.Patching.UniTaskCeciler.Log = Log;
        Jevil.Patching.UniTaskCeciler.Error = Error;
        Jevil.Patching.UniTaskCeciler.Execute(unitaskPath, il2MscorlibPath, unhollowerBasePath);
        Log("Successfully replaced IL2CPP support module and patched UniTask.dll!");
        Log("For users: You should now be able to use any mod that requires JeviLib v2.0.0 or higher");
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
