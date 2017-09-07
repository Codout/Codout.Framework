using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Codout.Framework.NetStandard.Commom.Exceptions
{
    ///<summary>
    /// Detalhes de exceção do tipo DataContractFault
    ///</summary>
    [DataContract]
    public class DataContractFaultException
    {
        ///<summary>
        /// Código do Erro
        ///</summary>
        [DataMember]
        public string Entity { get; set; }
        ///<summary>
        /// Descrição do Erro
        ///</summary>
        [DataMember]
        public IList<string> ErrorsMessages { get; set; }
    }
}
