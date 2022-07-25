using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.IMGUI;

/// <summary>
/// Contains methods to draw things onto the IMGUI. Will only draw things in a DEBUG build.
/// </summary>
public static class DebugDraw
{
#if DEBUG
    internal static List<GUIToken> tokens = new();
#endif

    /// <summary>
    /// Draws a box with the text <paramref name="text"/> inside, at <paramref name="position"/> on screen.
    /// </summary>
    /// <param name="text">The text to appear in the box.</param>
    /// <param name="position">The position of the text.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken Text(string text, GUIPosition position)
    {
        GUIToken ret = new(text);
#if DEBUG
        ret.position = position;
        tokens.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Tracks a variable using a getter you provide and the ToString provided by that value.
    /// </summary>
    /// <param name="varName">The variable's name</param>
    /// <param name="position">The element's position on the screen</param>
    /// <param name="getter">The variable getter. Will be called in Update.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken TrackVariable<T>(string varName, GUIPosition position, Func<T> getter)
    {
        Func<object> boxedGetter = () => getter;
        GUIToken ret = new(varName + ": " + getter().ToString(), boxedGetter);
#if DEBUG
        ret.position = position;
        tokens.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draws a button at <paramref name="position"/> on screen, labeled with <paramref name="text"/>, calling <paramref name="call"/> when pressed.
    /// </summary>
    /// <param name="text">The button's label</param>
    /// <param name="position">The position onscreen to draw the IMGUI element.</param>
    /// <param name="call">The call to invoke when the button is pressed.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken Button(string text, GUIPosition position, Action call)
    {
        GUIToken ret = new(text, call);
#if DEBUG
        ret.position = position;
        tokens.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draw a text field with a button beside it, that automatically resizes based on whats inside it.
    /// </summary>
    /// <param name="startingText">The starting text inside the text area.</param>
    /// <param name="buttonText">The text in the button.</param>
    /// <param name="position">The position on screen to draw the IMGUI elements.</param>
    /// <param name="call">A delegate that will have the text in the text field passed into it when called.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken TextButton(string startingText, string buttonText, GUIPosition position, Action<string> call)
    {
        GUIToken ret = new(startingText, buttonText, call);
#if DEBUG
        ret.position = position;
        tokens.Add(ret);
#endif
        return ret;
    }

    /// <summary>
    /// Draw a text field with a button beside it, that automatically resizes based on whats inside it. Button text is just "CALL".
    /// </summary>
    /// <param name="startingText">The starting text inside the text area.</param>
    /// <param name="position">The position on screen to draw the IMGUI elements.</param>
    /// <param name="call">A delegate that will have the text in the text field passed into it when called.</param>
    /// <returns>A <see cref="GUIToken"/> that can be sent to <see cref="Dont(GUIToken)"/> if you no longer wish for it to be drawn.</returns>
    public static GUIToken TextButton(string startingText, GUIPosition position, Action<string> call) 
        => TextButton(startingText, "CALL", position, call);

    /// <summary>
    /// Stops drawing a token.
    /// </summary>
    /// <param name="token">Any token, can be inactive, just means nothing will be done.</param>
    public static void Dont(GUIToken token)
    {
#if DEBUG
        int idx = tokens.IndexOf(token);
        if (idx != -1) tokens.RemoveAt(idx);
#endif
    }
}
