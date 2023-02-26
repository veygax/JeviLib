using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// Occurs when there is a mismatch between the Mono mod domain and the Il2Cpp fake-Mono game domain.
/// </summary>
[System.Serializable]
public abstract class Il2CppMismatchException : Exception
{
    internal Il2CppMismatchException() { }
    internal Il2CppMismatchException(string message) : base(message) { }
    internal Il2CppMismatchException(string message, Exception inner) : base(message, inner) { }
    internal Il2CppMismatchException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}


/// <summary>
/// Occurs when a type cannot be converted to/from its IL2CPP equivalent.
/// <para>These errors may require more advanced programming to work around.</para>
/// </summary>
[Serializable]
public sealed class Il2CppConversionException : Il2CppMismatchException
{
    internal Il2CppConversionException(Type monoType, Type il2cppType) : this($"Mono type '{monoType.FullName}' cannot be converted between its IL2CPP equivalent '{il2cppType.FullName}'.") { }

    internal Il2CppConversionException() { }
    internal Il2CppConversionException(string message) : base(message) { }
    internal Il2CppConversionException(string message, Exception inner) : base(message, inner) { }
    internal Il2CppConversionException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}