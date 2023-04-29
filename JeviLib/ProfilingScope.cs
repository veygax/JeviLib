using MelonLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Apple;

namespace Jevil;

/// <summary>
/// When <see cref="Dispose"/> is called (automatically done by the compiler), a message is logged (defaults to JeviLib's logger instance) informing you of some debug stats, like execution time.
/// <para>Intended for use with C# 8 and above's "simple 'using' statement" syntax.</para>
/// <para>Only works in debug builds of JeviLib.</para>
/// <br><see href="https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0063"/></br>
/// <br><see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/using"/></br>
/// </summary>
public class ProfilingScope : IDisposable
{
    /// <summary>
    /// Indicates what kinds of things will be logged when debug stats are logged.
    /// </summary>
    [Flags]
    public enum ProfilingType
    {
        /// <summary>
        /// Indicates that nothing will be logged. I'm not sure why you'd use this.
        /// </summary>
        NONE = 0, 
        /// <summary>
        /// Indicates that a stopwatch's elapsed time (in milliseconds) will be logged when <see cref="Dispose"/> or <see cref="Log"/> are called.
        /// </summary>
        STOPWATCH_EXECUTION_TIME = 1 << 0,
        /// <summary>
        /// Indicates that a frame count difference will be logged when <see cref="Dispose"/> or <see cref="Log"/> are called.
        /// <para>Useful for asynchronous methods that use <see cref="Cysharp.Threading.Tasks.UniTask.Yield()"/> and <see cref="AsyncUtilities"/></para>
        /// </summary>
        FRAME_COUNT = 1 << 1,
        /// <summary>
        /// Indicates that all available statistics are to be logged.
        /// </summary>
        ALL = ~0,
    }

#if DEBUG
    MelonLogger.Instance logger;
    Stopwatch sw;
    int frameStart;
    ProfilingType profilingType;
    string logTag;
#endif

    /// <summary>
    /// Retrieves the number of milliseconds the stopwatch has measured.
    /// <para>This will return 0 in release builds.</para>
    /// </summary>
    public float ElapsedMilliseconds
#if DEBUG
        => sw.ElapsedTicks / 10000f;
#else
        => default;
#endif
    /// <summary>
    /// Retrieves the number of frames since this <see cref="ProfilingScope"/> started measureing time.
    /// </summary>
    public float ElapsedFrames
#if DEBUG
        => Time.frameCount - frameStart;
#else
        => default;
#endif

    /// <summary>
    /// Creates a <see cref="ProfilingScope"/> that, when it goes out of scope, will log its stats.
    /// </summary>
    /// <param name="whatToLog">Determines what stats will be printed when <see cref="Log"/> is called.</param>
    /// <param name="logTag">Any log tag. It will appear before any log statement from this instance. It will be prefxied with your assembly name</param>
    public ProfilingScope(ProfilingType whatToLog = ProfilingType.STOPWATCH_EXECUTION_TIME, [CallerMemberName] string logTag = null)
    {
#if DEBUG
        this.logger = JeviLib.instance.LoggerInstance;
        this.profilingType = whatToLog;
        this.logTag = Assembly.GetCallingAssembly().GetName().Name + " -> " + logTag;


        Restart();
#endif
    }

    /// <summary>
    /// Creates a <see cref="ProfilingScope"/> that, when it goes out of scope, will log its stats.
    /// </summary>
    /// <param name="logger">Uses your logger when <see cref="Log"/> is called, instead of using JeviLib's.</param>
    /// <param name="whatToLog">Determines what stats will be printed when <see cref="Log"/> is called.</param>
    /// <param name="logTag">Any log tag. It will appear before any log statement from this instance. It will be prefxied with your assembly name</param>
    public ProfilingScope(MelonLogger.Instance logger, ProfilingType whatToLog = ProfilingType.STOPWATCH_EXECUTION_TIME, [CallerMemberName] string logTag = null)
    {
#if DEBUG
        this.logger = logger;
        this.profilingType = whatToLog;
        this.logTag = Assembly.GetCallingAssembly().GetName().Name + " -> " + logTag;

        Restart();
#endif
    }

    /// <summary>
    /// Restarts the stopwatch and resets the frame counter.
    /// </summary>
    public void Restart()
    {
#if DEBUG
        sw ??= Stopwatch.StartNew();
        sw.Restart();
        frameStart = Time.frameCount;
#endif
    }

    /// <summary>
    /// Calls <see cref="Log"/> and then <see cref="Restart"/>.
    /// </summary>
    public void LogRestart()
    {
        Log();
        Restart();
    }

    /// <summary>
    /// Logs debug stats.
    /// </summary>
    public void Log()
    {
#if DEBUG
        if (profilingType.HasFlag(ProfilingType.STOPWATCH_EXECUTION_TIME))
            logger.Msg($"{logTag}: executed for {ElapsedMilliseconds} milliseconds");
        if (profilingType.HasFlag(ProfilingType.FRAME_COUNT))
            logger.Msg($"{logTag}: executed for {ElapsedFrames} frames");
#endif
    }

    /// <summary>
    /// Logs debug stats.
    /// </summary>
    public void Dispose() => Log();
}
