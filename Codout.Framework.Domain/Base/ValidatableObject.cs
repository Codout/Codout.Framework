using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Domain.Base;

/// <summary>
///     Serves as the base class for objects that are validatable.
/// </summary>
[Serializable]
public abstract class ValidatableObject : BaseObject
{
    /// <summary>
    ///     Determines whether this instance is valid.
    /// </summary>
    /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
    public virtual bool IsValid()
    {
        return ValidationResults().Count == 0;
    }

    /// <summary>
    ///     Validates all properties of the object and returns the validation results if any of
    ///     them were deemed invalid.
    /// </summary>
    /// <returns>A collection of validation results.</returns>
    public virtual ICollection<ValidationResult> ValidationResults()
    {
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this, null, null), validationResults, true);
        return validationResults;
    }
}