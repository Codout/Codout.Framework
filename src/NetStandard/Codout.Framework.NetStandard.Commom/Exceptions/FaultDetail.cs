using System.Runtime.Serialization;

namespace Codout.Framework.NetStandard.Commom.Exceptions
{
    ///<summary>
    /// Detalhes do erro
    ///</summary>
    [DataContract]
    public class FaultDetail
    {
        ///<summary>
        /// Construtor da Classe ErrorDetail
        ///</summary>
        ///<param name="code"></param>
        ///<param name="errorMessage"></param>
        public FaultDetail(string code, string errorMessage)
        {
            Code = code;
            ErrorMessage = errorMessage;
        }

        ///<summary>
        /// Código do erro
        ///</summary>
        [DataMember]
        public string Code { get; private set; }

        ///<summary>
        /// Descrição do erro
        ///</summary>
        [DataMember]
        public string ErrorMessage { get; private set; }
    }
}
