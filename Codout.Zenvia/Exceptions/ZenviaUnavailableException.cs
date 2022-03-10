using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão específica quando uma solicitação HTTP não é atendida porque o serviço está indisponível (503).
    /// </summary>
    public class ZenviaUnavailableException : ZenviaException
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaUnavailableException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaUnavailableException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaUnavailableException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
