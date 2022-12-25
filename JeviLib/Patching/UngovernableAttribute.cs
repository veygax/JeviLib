using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jevil.Patching;

// todo: do this
/// <summary>
/// Used to identify an assembly that JeviLib should autopatch with a transpiler.
/// <para>If <see cref="UngovernableAttribute"/> is found on a loaded assembly, all its <see langword="async"/> methods will have all <see langword="await"/>s suffixed with a call to <see cref="Utilities.ReturnToMainThread"/></para>
/// </summary>
/// <remarks></remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public class UngovernableAttribute : Attribute 
{
    internal readonly UngovernableType ungovernableType;
    /// <summary>
    /// Makes an assembly ungovernable.
    /// </summary>
    /// <param name="whyYouAreUngovernable">Specifies how an ungovernable assembly will have parts of it be modified.</param>
    public UngovernableAttribute(UngovernableType whyYouAreUngovernable = UngovernableType.ALL)
    {
        ungovernableType = whyYouAreUngovernable;
    }
}
