namespace Jevil.Spawning;

/// <summary>
/// The type of EarlyExit to spawn. Units of measurement are anyone's guess.
/// </summary>
public enum ZombieType
{
    /// <summary>
    /// Speed = 1.8, Hp = 5; Throw = false
    /// </summary>
    FAST_NOTHROW,
    /// <summary>
    /// Speed = 1.8, Hp = 75; Throw = false
    /// </summary>
    FAST_NOTHROW_TANK,
    /// <summary>
    /// Speed = 1.8, Hp = 60; Throw = true; Cooldown and reload time are decreased (if those do anything)
    /// </summary>
    FAST_THROW,
    /// <summary>
    /// Speed = 1.68, Hp = 5; Throw = false
    /// </summary>
    MED_NOTHROW,
    /// <summary>
    /// Speed = 1.6, Hp = 5; Throw = true
    /// </summary>
    MED_THROW,
    /// <summary>
    /// Speed = 1.25, Hp = 5; Throw = true
    /// </summary>
    SLOW_THROW,
    /// <summary>
    /// Speed = 1.35, Hp = 60; Throw = true
    /// </summary>
    SLOW_THROW_TANK,
}
