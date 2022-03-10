using System;

namespace Codout.Zenvia.Exceptions
{
    /// <summary>
    /// Classe de excessão específica quando uma solicitação HTTP não é concluída por não ter sido encontrado o ponto de destino(Não encontrado - 404).
    /// </summary>
    public class ZenviaNotFoundException : ZenviaException
    {
        /// <summary>
        /// Método construtor.
        /// </summary>
        public ZenviaNotFoundException() : base() { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        public ZenviaNotFoundException(string message) : base(message) { }

        /// <summary>
        /// Método construtor.
        /// </summary>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaNotFoundException(Exception innerException) : base(string.Empty, innerException) { }

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="message">Mensagem de erro.</param>
        /// <param name="innerException">Causa da falha.</param>
        public ZenviaNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
