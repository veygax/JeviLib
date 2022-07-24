using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Waiting;

/// <summary>
/// Waits until the scene index changes on scene intialization.
/// </summary>
public class WaitForSceneInit : IEnumerator
{
    /// <summary>
    /// literally <see langword="null"/>
    /// </summary>
    public object Current => null;
    internal static int currSceneIdx;
    readonly int sceneIdx;

    /// <summary>
    /// Whether the current scene index is teh same as the scene index this <see cref="WaitForSceneInit"/> started with.
    /// </summary>
    /// <returns>The value of <c>waiter()</c></returns>
    public bool MoveNext()
    {
        return sceneIdx == currSceneIdx;
    }

    /// <summary>
    /// stub => https://cdn.discordapp.com/attachments/587792632986730507/985659335088697454/trim.5C5AD44D-3A16-467D-8B16-2DBB397C205D.mov
    /// </summary>
    public void Reset() { }

    /// <summary>
    /// Create a new instance of <see cref="WaitForSceneInit"/>
    /// </summary>
    public WaitForSceneInit()
    {
        sceneIdx = currSceneIdx;
    }
}
