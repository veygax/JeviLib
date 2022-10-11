using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Jevil.JevilBuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Jevil.JevilBuildInfo.Company)]
[assembly: AssemblyProduct(Jevil.JevilBuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Jevil.JevilBuildInfo.Author)]
[assembly: AssemblyTrademark(Jevil.JevilBuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(Jevil.JevilBuildInfo.Version)]
[assembly: AssemblyFileVersion(Jevil.JevilBuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo(Jevil.Patching.DynTools.dynamicAsmName)]
[assembly: MelonInfo(typeof(Jevil.JeviLib), Jevil.JevilBuildInfo.Name, Jevil.JevilBuildInfo.Version, Jevil.JevilBuildInfo.Author, Jevil.JevilBuildInfo.DownloadLink)]
[assembly: MelonPriority(-20925)] // rb rd

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
