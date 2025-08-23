using System;

namespace Codout.Framework.Domain.Base;

/// <summary>
///     Facilitates indicating which property(s) describe the unique signature of an
///     entity. See Entity.GetTypeSpecificSignatureProperties() for when this is leveraged.
/// </summary>
/// <remarks>
///     This is intended for use with <see cref="BaseObject" />. It may NOT be used on a <see cref="ValueObject" />.
/// </remarks>
[Serializable]
[AttributeUsage(AttributeTargets.Property)]
public class DomainSignatureAttribute : Attribute
{
}