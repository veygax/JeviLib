namespace Jevil;

internal static class JevilBuildInfo
{
    public const string Name = "JeviLib"; // Name of the Mod.  (MUST BE SET)
    public const string Author = "extraes"; // Author of the Mod.  (Set as null if none)
    public const string Company = null; // Company that made the Mod.  (Set as null if none)
    public const string Version = "1.1.0"; // Version of the Mod.  (MUST BE SET)
    public const string DownloadLink = "https://bonelab.thunderstore.io/package/extraes/JeviLibBL/"; // Download Link for the Mod.  (Set as null if none)
    public const bool Debug
#if DEBUG
        = true;
#else
        = false;
#endif
}
