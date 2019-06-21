using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Codout.Framework.Common.Annotations
{

    /// <summary>
    /// Valida uma propriedade comparando com outra.
    /// </summary>
    public class CompareAttribute : ValidationAttribute
    {
        #region CompareAttribute
        /// <summary>
        /// Compara uma propriedade com outra.
        /// </summary>
        /// <param name="otherProperty"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CompareAttribute(string otherProperty)
        {
            OtherProperty = otherProperty ?? throw new ArgumentNullException("otherProperty");
        }
        #endregion

        #region Propriedades
        /// <summary>
        /// Outra propriedade.
        /// </summary>
        public string OtherProperty { get; private set; }
        #endregion

        #region IsValid
        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class. 
        /// </returns>
        /// <param name="value">The value to validate.</param><param name="validationContext">The context information about the validation operation.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult($"Não foi possível encontrar um propriedade com o nome {OtherProperty}");
            }

            var otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (!Equals(value, otherPropertyValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }
        #endregion

        #region FormatPropertyForClientValidation
        /// <summary>
        /// Formata uma propriedade para validação no cliente.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string FormatPropertyForClientValidation(string property)
        {
            if (property == null)
            {
                throw new ArgumentException("A propriedade não pode ser nula.", "property");
            }
            return "*." + property;
        }
        #endregion
    }
}
