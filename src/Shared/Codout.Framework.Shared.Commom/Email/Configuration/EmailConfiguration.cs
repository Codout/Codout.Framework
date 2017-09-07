using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Codout.Framework.Shared.Commom.Email.Configuration
{
    /// <summary>
    /// Obtem as configurações do envio de Email
    /// </summary>
    public class EmailConfiguration : ConfigurationSection
    {

        /// <summary>
        /// Define se o formato do Email é HTML ou Texto puro
        /// </summary>
        [ConfigurationProperty("isBodyHtml", IsRequired = true)]
        public bool IsBodyHtml
        {
            get
            {
                return (bool)this["isBodyHtml"];
            }
            set
            {
                this["isBodyHtml"] = value;
            }
        }

        /// <summary>
        /// Login para autenticação da conta de Email
        /// </summary>
        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get
            {
                return (string)this["userName"];
            }
            set
            {
                this["userName"] = value;
            }
        }

        /// <summary>
        /// Senha para autenticação da conta de Email
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }

        /// <summary>
        /// Obtém ou define o nome ou o endereço IP do host usado para transações de SMTP.
        /// </summary>
        [ConfigurationProperty("smtp", IsRequired = true)]
        public string Smtp
        {
            get
            {
                return (string)this["smtp"];
            }
            set
            {
                this["smtp"] = value;
            }
        }

        /// <summary>
        /// btém ou define a porta usada para transações de SMTP.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        /// <summary>
        /// Especifique se o SmtpClient usa Secure Sockets Layer (SSL) para criptografar a conexão.
        /// </summary>
        [ConfigurationProperty("enableSsl", IsRequired = true)]
        public bool EnableSsl
        {
            get
            {
                return (bool)this["enableSsl"];
            }
            set
            {
                this["enableSsl"] = value;
            }
        }

        /// <summary>
        /// Especifica como e-mails enviados serão tratados.
        /// </summary>
        [ConfigurationProperty("defaultCredentials", IsRequired = true)]
        public bool DefaultCredentials
        {
            get
            {
                return (bool)this["defaultCredentials"];
            }
            set
            {
                this["defaultCredentials"] = value;
            }
        }

        /// <summary>
        /// Email de origem
        /// </summary>
        [ConfigurationProperty("emailFrom", IsRequired = true)]
        public string EmailFrom
        {
            get
            {
                return (string)this["emailFrom"];
            }
            set
            {
                this["emailFrom"] = value;
            }
        }

        /// <summary>
        /// Nome do Remetente do Email
        /// </summary>
        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get
            {
                return (string)this["displayName"];
            }
            set
            {
                this["displayName"] = value;
            }
        }
    }
}
