using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Jevil.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Jevil.BuildInfo.Company)]
[assembly: AssemblyProduct(Jevil.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Jevil.BuildInfo.Author)]
[assembly: AssemblyTrademark(Jevil.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(Jevil.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Jevil.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(Jevil.JeviLib), Jevil.BuildInfo.Name, Jevil.BuildInfo.Version, Jevil.BuildInfo.Author, Jevil.BuildInfo.DownloadLink)]
[assembly: MelonPriority(-20925)] // rb rd

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONEWORKS")] 