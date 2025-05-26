using System;
using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Common.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value != null && (bool)value;
    }
}