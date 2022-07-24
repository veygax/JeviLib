using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Prefs;

/// <summary>
/// Represents a Unity standard color, like white, black, or pink
/// </summary>
public enum UnityDefaultColor
{
    /// <summary>
    /// R:1, G:0, B:0
    /// <br><see cref="Color.red"/></br>
    /// </summary>
    RED = 1,
    /// <summary>
    /// R:0, G:1, B:0
    /// <br><see cref="Color.green"/></br>
    /// </summary>
    GREEN = 2,
    /// <summary>
    /// R:0, G:0, B:1
    /// <br><see cref="Color.blue"/></br>
    /// </summary>
    BLUE = 3,
    /// <summary>
    /// R:1, G:1, B:1
    /// <br><see cref="Color.white"/></br>
    /// </summary>
    WHITE = 4,
    /// <summary>
    /// R:0, G:0, B:0
    /// <br><see cref="Color.black"/></br>
    /// </summary>
    BLACK = default,
    /// <summary>
    /// R:1, G:1, B:0
    /// <br><see cref="Color.yellow"/></br>
    /// </summary>
    YELLOW = 5,
    /// <summary>
    /// R:0, G:1, B:1
    /// <br><see cref="Color.cyan"/></br>
    /// </summary>
    CYAN = 6,
    /// <summary>
    /// R:1, G:0, B:1 (estimate)
    /// <br><see cref="Color.magenta"/></br>
    /// </summary>
    MAGENTA = 7,
    /// <summary>
    /// R:0.5, G:0.5, B:0.5 (estimate)
    /// <br><see cref="Color.gray"/></br>
    /// </summary>
    GRAY = 8,
    /// <summary>
    /// R:0.5, G:0.5, B:0.5 (estimate)
    /// <br><see cref="Color.grey"/></br>
    /// </summary>
    GREY = 8,
}
