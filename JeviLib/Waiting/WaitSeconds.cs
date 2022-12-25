using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Waiting;

/// <summary>
/// Replacement for <see cref="WaitForSeconds"/>, because that shit doesn't work for some stupid ass reason.
/// </summary>
public class WaitSeconds : IEnumerator
{
    internal static ConstructorInfo ctor = typeof(WaitSeconds).GetConstructor(new Type[] { typeof(float) });
    internal readonly float length;
    internal float timeElapsed;

    /// <summary>
    /// literally <see langword="null"/>
    /// </summary>
    public object Current => null;

    /// <summary>
    /// yeah basically
    /// </summary>
    /// <returns>The value of <c>waiter()</c></returns>
    public bool MoveNext()
    {
        // return true if more waiting is needed
        timeElapsed += Time.deltaTime;
        return timeElapsed > length;
    }

    /// <summary>
    /// stub => https://cdn.discordapp.com/attachments/587792632986730507/985659335088697454/trim.5C5AD44D-3A16-467D-8B16-2DBB397C205D.mov
    /// </summary>
    public void Reset() { }

    /// <summary>
    /// Create a new instance of <see cref="WaitSeconds"/>
    /// </summary>
    public WaitSeconds(float length)
    {
        this.length = length;
    }
}
