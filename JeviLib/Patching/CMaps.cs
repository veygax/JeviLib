namespace Jevil.Patching;

/// <summary>
/// Harmony for custom maps if you don't want to make your own Harmony instance.
/// </summary>
public static class CMaps
{
    private static HarmonyLib.Harmony _cmapHarmony = new("Jevilib.CustomMapHarmony");
    /// <summary>
    /// A harmony instance for custom maps.
    /// </summary>
    public static HarmonyLib.Harmony CMHarmony => _cmapHarmony;
}
