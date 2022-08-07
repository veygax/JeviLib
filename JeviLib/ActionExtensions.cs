using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// Class containing mass-extensions for <see cref="Action"/>s.
/// </summary>
public static class ActionExtensions
{
    /// <summary>
    /// Calls each invoker in a in a <see langword="try"/>-<see langword="catch"/> to call all invokers. If the results of one invoker depend on another, use <see cref="InvokeSafeSync(Action)"/>.
    /// </summary>
    /// <param name="a">An action. Can be null or have no registered invokers.</param>
    /// <remarks><b>WARNING:</b> This uses <see cref="Parallel"/>, so you may not be able to access Unity methods or accessors.</remarks>
    public static void InvokeSafeParallel(this Action a)
    {
        if (a == null) return;
        Delegate[] invocations = a.GetInvocationList();
        if (invocations.Length == 0) return;

        Parallel.For(0, invocations.Length, i =>
        {
            try
            {
                ((Action)invocations[i])();
            }
            catch (Exception e)
            {
                JeviLib.Error("Exception while parallel safe-invoking on iteration " + (i + 1) + " of " + invocations.Length + "!");
                JeviLib.Error(e);
            }
        });
    }


    /// <summary>
    /// Calls all invokers in a try block.
    /// </summary>
    /// <param name="a">An action. Can be null or have no registered invokers.</param>
    public static void InvokeSafeSync(this Action a)
    {
        if (a == null) return;
        Delegate[] invocations = a.GetInvocationList();
        if (invocations.Length == 0) return;

        for (int i = 0; i < invocations.Length; i++)
        {
            try
            {
                ((Action)invocations[i])();
            }
            catch (Exception e)
            {
                JeviLib.Error("Errored while safe-invoking!");
                JeviLib.Error(e);
            }

        }
    }

    /// <summary>
    /// Calls all invokers using the given. If the results of one invoker depend on another, use <see cref="InvokeSafeSync(Action)"/>.
    /// </summary>
    /// <param name="a">An action. Can be null or have no registered invokers.</param>
    /// <param name="param">The argument to be passed to the invokers.</param>
    /// <remarks><b>WARNING:</b> This uses <see cref="Parallel"/>, so you may not be able to access Unity methods or accessors.</remarks>
    public static void InvokeSafeParallel<T>(this Action<T> a, T param)
    {
        if (a == null) return;
        Delegate[] invocations = a.GetInvocationList();
        if (invocations.Length == 0) return;

        Parallel.For(0, invocations.Length, i =>
        {
            try
            {
                ((Action<T>)invocations[i])(param);
            }
            catch (Exception e)
            {
                JeviLib.Error("Errored while parallel safe-invoking!");
                JeviLib.Error(e);
            }
        });
    }

    /// <summary>
    /// Calls all invokers in a try block.
    /// </summary>
    /// <param name="a">An action. Can be null or have no registered invokers.</param>
    /// <param name="param">The parameter to be passed into the calls</param>
    public static void InvokeSafeSync<T>(this Action<T> a, T param)
    {
        if (a == null) return;
        Delegate[] invocations = a.GetInvocationList();
        if (invocations.Length == 0) return;

        for (int i = 0; i < invocations.Length; i++)
        {
            try
            {
                ((Action<T>)invocations[i])(param);
            }
            catch (Exception e)
            {
                JeviLib.Error("Errored while safe-invoking!");
                JeviLib.Error(e);
            }

        }
    }
}
