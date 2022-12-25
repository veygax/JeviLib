using System;

namespace Jevil.Patching;

/// <summary>
/// Determines exactly how <see cref="UngovernableAttribute"/> will be handled.
/// </summary>
[Flags]
public enum UngovernableType
{
    /// <summary>
    /// Specifies that all flags be used.
    /// </summary>
    ALL = ~0,
    /// <summary>
    /// NOT YET IMPLEMENTED; Ensures that all async methods will return to the main (Unity) thread after they resume execution.
    /// </summary>
    ENSURE_ASYNC_METHODS_MAIN_THREAD = 1 << 0,
    /// <summary>
    /// Replaces all creations of WaitForSeconds and WaitForSecondsRealtime
    /// </summary>
    I_FUCKING_SAID_WAIT = 1 << 1,
}
