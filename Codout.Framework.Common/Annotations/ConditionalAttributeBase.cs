using System;
using System.ComponentModel.DataAnnotations;

namespace Codout.Framework.Common.Annotations
{
    /// <summary>
    /// Classe base para validações condicionais.
    /// </summary>
    public class ConditionalAttributeBase : ValidationAttribute
    {
        #region Variáveis
        private const string DefaultErrorMessage = "O campo {0} está inválido.";
        #endregion

        #region Construtores
        /// <summary>
        /// Construtor padrão.
        /// </summary>
        public ConditionalAttributeBase()
            : this(DefaultErrorMessage)
        {
        }

        /// <summary>
        /// Construtor com opção de mensagem de erro.
        /// </summary>
        /// <param name="errorMessage"></param>
        public ConditionalAttributeBase(string errorMessage)
            : base(errorMessage)
        {
        }
        #endregion

        #region ShouldRunValidation
        /// <summary>
        /// Verifica se a validação deve ser executada.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dependentProperty"></param>
        /// <param name="targetValue"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected bool ShouldRunValidation(
            object value,
            string dependentProperty,
            object targetValue,
            ValidationContext validationContext)
        {
            var dependentvalue = GetDependentFieldValue(dependentProperty, validationContext);

            // compare the value against the target value
            return ((dependentvalue == null && targetValue == null) ||
                    (dependentvalue != null && dependentvalue.Equals(targetValue)));
        }
        #endregion

        #region GetDependentFieldValue
        /// <summary>
        /// Busca o valor do campo dependente.
        /// </summary>
        /// <param name="dependentProperty"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected object GetDependentFieldValue(string dependentProperty, ValidationContext validationContext)
        {
            // get a reference to the property this validation depends upon
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(dependentProperty);

            if (field == null)
                throw new MissingMemberException(containerType.Name, dependentProperty);

            // get the value of the dependent property
            var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);
            return dependentvalue;
        }
        #endregion
    }
}
