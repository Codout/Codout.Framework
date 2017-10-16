using System.Web;

namespace Codout.Framework.Commom.Helpers
{
    public static class Http
    {
        /// <summary>
        /// Obtem o endereço IP de uma requisção Web
        /// </summary>
        /// <returns>Retorna o endereço IP</returns>
        public static string GetClientIp()
        {
            if (HttpContext.Current == null)
                return string.Empty;

            string result;
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                string[] ipRange = ip.Split(',');
                int le = ipRange.Length - 1;
                result = ipRange[0];
            }
            else
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return result;
        }
    }
}
