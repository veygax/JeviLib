using System.Reflection;

namespace Jevil;

/// <summary>
/// Constant values used in multiple places throughout this library.
/// <br>They may be useful in serialization or other calculations.</br>
/// </summary>
public static class Const
{
    /// <summary>
    /// The size of a <see cref="UnityEngine.Vector3"/> (3 <see cref="float"/>s).
    /// <para>Used for serialization, for offsets in byte arrays.</para>
    /// </summary>
    public const int SizeV3 = sizeof(float) * 3;
    /// <summary>
    /// <see cref="System.Math.PI"/> pre-casted into <see cref="float"/>.
    /// <para>Used extensively in <see cref="Tweening"/> to make interpolation smooth.</para>
    /// </summary>
    public const float FPI = (float)System.Math.PI;
    /// <summary>
    /// All relevant binding flags (Public, NonPublic, Instance, Static)
    /// </summary>
    public const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    /// <summary>
    /// The name of the GameObject <see cref="Instances.Player_RigManager"/> is on.
    /// <para>Useful with <see cref="Extensions.InHierarchyOf(UnityEngine.Transform, string)"/></para>
    /// </summary>
    public const string RigManagerName = "[RigManager (Default Brett)]";
}