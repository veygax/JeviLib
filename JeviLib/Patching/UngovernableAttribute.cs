using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jevil.Patching;

/// <summary>
/// Used to identify an assembly that JeviLib should handle in some special way. Undecided what it'll be for yet.
/// <para>If <see cref="UngovernableAttribute"/> is found on the registering assembly, all its classes will be injected with FieldInjector instead of ClassInjector</para>
/// </summary>
/// <remarks>This will fall back to ClassInjector is FieldInjector isn't found.</remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class UngovernableAttribute : Attribute 
{
    /// <summary>
    /// If <see langword="false"/>, JeviLib will only inject serialization for classes deriving from <see cref="UnityEngine.MonoBehaviour"/>.
    /// </summary>
    public bool alsoInjectScriptableObjects;
}
