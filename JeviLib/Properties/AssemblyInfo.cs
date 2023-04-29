using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Jevil.JevilBuildInfo.NAME)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Jevil.JevilBuildInfo.COMPANY)]
[assembly: AssemblyProduct(Jevil.JevilBuildInfo.NAME)]
[assembly: AssemblyCopyright("Created by " + Jevil.JevilBuildInfo.AUTHOR)]
[assembly: AssemblyTrademark(Jevil.JevilBuildInfo.COMPANY)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(Jevil.JevilBuildInfo.VERSION)]
[assembly: AssemblyFileVersion(Jevil.JevilBuildInfo.VERSION)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo(Jevil.Patching.DynTools.dynamicAsmName)]
[assembly: MelonInfo(typeof(Jevil.JeviLib), Jevil.JevilBuildInfo.NAME, Jevil.JevilBuildInfo.VERSION, Jevil.JevilBuildInfo.AUTHOR, Jevil.JevilBuildInfo.DOWNLOAD_LINK)]
[assembly: MelonPriority(-20925)] // rb rd

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

// Cecil.Rocks doesnt get automatically loaded by Monodroid, gfys.... either way, i only use it in the unitask ceciler, so i just load it manually before entering that method
[assembly: MelonOptionalDependencies("Mono.Cecil.Rocks")] 