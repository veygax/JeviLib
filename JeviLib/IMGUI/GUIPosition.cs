using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.IMGUI;

/// <summary>
/// Defines the corner of the screen the IMGUI element will be drawn in.
/// </summary>
public enum GUIPosition
{
    /// <summary>
    /// Draws an IMGUI element in the top left of the screen.
    /// <para>Drawn top to bottom.</para>
    /// </summary>
    TOP_LEFT,
    /// <summary>
    /// Draws an IMGUI element in the top right of the screen.
    /// <para>Drawn top to bottom.</para>
    /// </summary>
    TOP_RIGHT,
    /// <summary>
    /// Draws an IMGUI element in the bottom left of the screen.
    /// <para>Drawn bottom-up.</para>
    /// </summary>
    BOTTOM_LEFT,
    /// <summary>
    /// Draws an IMGUI element in the bottom right of the screen.
    /// <para>Drawn bottom-up.</para>
    /// </summary>
    BOTTOM_RIGHT,
}
