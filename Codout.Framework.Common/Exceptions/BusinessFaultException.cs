using System.Runtime.Serialization;

namespace Codout.Framework.Common.Exceptions
{
    ///<summary>
    /// Detalhes de exceção do tipo BusinessFault
    ///</summary>
    [DataContract]
    public class BusinessFaultException
    {
        ///<summary>
        /// Construtor BusinessFault
        ///</summary>
        ///<param name="errorDetail">Detalhes do erro</param>
        public BusinessFaultException(FaultDetail errorDetail)
        {
            Code = errorDetail.Code;
            ErrorMessage = errorDetail.ErrorMessage;
        }

        /// <summary>
        /// Construtor BusinessFault
        /// </summary>
        public BusinessFaultException() { }

        ///<summary>
        /// Código do Erro
        ///</summary>
        [DataMember]
        public string Code { get; set; }
        ///<summary>
        /// Descrição do Erro
        ///</summary>
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
