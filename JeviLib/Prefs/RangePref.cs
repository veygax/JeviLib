using System;

namespace Jevil.Prefs;

/// <summary>
/// Declare a preference within a range of values. It is your responsibility to check if the values are between <c>low</c> and <c>high</c> at runtime.
/// <para>The default value and upper and lower bounds are set as comments in MelonPreferences and as the limits in BoneMenu. Increment is used only in BoneMenu.</para>
/// <code>
/// Generated MelonPreferences entry example:
/// # Default: 0.025, 0 to 1
/// scrollSpeedX = 0.025000000372529
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class RangePref : Attribute
{
    /// <summary>
    /// Lower bound for BoneMenu and MelonPreferences comment
    /// </summary>
    public readonly float low;
    /// <summary>
    /// Upper bound for BoneMenu and MelonPreferences comment
    /// </summary>
    public readonly float high;
    /// <summary>
    /// Increment for BoneMenu presses.
    /// </summary>
    public readonly float inc;

    /// <summary>
    /// Declare a static int field as a range preference.
    /// </summary>
    /// <param name="lowerBound">Lower bound for BoneMenu and MelonPreferences comment</param>
    /// <param name="upperBound">Upper bound for BoneMenu and MelonPreferences comment</param>
    /// <param name="increment">The increment between button presses in BoneMenu</param>
    public RangePref(int lowerBound, int upperBound, int increment)
    {
        low = lowerBound;
        high = upperBound;
        inc = increment;
    }

    /// <summary>
    /// Declare a static float field as a range preference.
    /// </summary>
    /// <param name="lowerBound">Lower bound for BoneMenu and MelonPreferences comment</param>
    /// <param name="upperBound">Upper bound for BoneMenu and MelonPreferences comment</param>
    /// <param name="increment">The increment between button presses in BoneMenu</param>
    public RangePref(float lowerBound, float upperBound, float increment)
    {
        low = lowerBound;
        high = upperBound;
        inc = increment;
    }
}
