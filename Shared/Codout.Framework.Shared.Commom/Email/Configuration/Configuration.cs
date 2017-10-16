using System.Configuration;

namespace Codout.Framework.Commom.Email.Configuration
{
    /// <summary>
    /// Acessa as configurações de envio de Emails
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// Obtem as configurações de envio de Email
        /// </summary>
        /// <returns></returns>
        public static EmailConfiguration EmailConfiguration
        {
            get { return (EmailConfiguration)ConfigurationManager.GetSection("softprime/email"); }
        }
    }
}
