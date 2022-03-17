using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Codout.Zenvia.Exceptions;
using Codout.Zenvia.Models;
using Codout.Zenvia.Models.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Codout.Zenvia
{
    /// <summary>
    /// Classe principal que faz a comunicação com a API do Zenvia, enviando as mensagens e recebendo suas respectivas respostas.
    /// </summary>
    public class ZenviaApi
    {
        /// <summary>
        /// Atributo que representa a URL básica do serviço da API Zenvia.
        /// </summary>
        private string BaseUrl;

        /// <summary>
        /// Atributo que representa a URL de envio de mensagens da API Zenvia.
        /// </summary>
        private string SendSmsUrl;

        /// <summary>
        /// Token de API
        /// </summary>
        public string ApiToken { private get; set; }


        /// <summary>
        /// Método Construtor.
        /// </summary>
        /// <param name="baseUrl">URL do serviço de API da Zenvia.</param>
        public ZenviaApi(string apiToken, string baseUrl = "https://api.zenvia.com/v2")
        {
            this.ApiToken = apiToken;
            this.BaseUrl = baseUrl;
            this.SendSmsUrl = this.BaseUrl + "/channels/sms/messages";
        }

        /// <summary>
        /// Método que verifica se o nome de usuário e/ou a senha estão em branco.
        /// </summary>
        private void UsernameOrPasswordEmpty()
        {
            if (string.IsNullOrEmpty(this.ApiToken))
            {
                throw new ZenviaAuthenticationException("ZenviaApi: Token de API é requerido!");
            }
        }

        /// <summary>
        /// Método que envia uma mensagem SMS à API Zenvia para ser enviada ao destinatário.
        /// </summary>
        /// <param name="messageSms">Objeto do tipo <see cref="SingleMessageSms"/> que contém os dados necessários para o envio.</param>
        /// <returns>Objeto do tipo <see cref="SendSmsResponse"/> contendo a resposta da requisição feita.Pode conter uma mensagem de sucesso, 
        /// confirmação de entrega ou uma mensagem informando uma falha no envio.</returns>
        public async Task<Models.Responses.MessageSms> SendSms(MessageSms messageSms)
        {
            UsernameOrPasswordEmpty();
            try
            {
                var result = await CallPostAsync<Models.Responses.MessageSms, MessageSms>(SendSmsUrl, messageSms);
                return result;
            }
            catch (ZenviaException ex)
            {
                throw new ZenviaException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Método que faz uma requisição do tipo Get à API Zenvia.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto esperado como retorno.</typeparam>
        /// <param name="url">URL de destino da requisição.</param>
        /// <param name="obj">Objeto de tipo definido resultante da requisição.</param>
        /// <returns>Objeto de tipo definido resultante da requisição.</returns>
        private async Task<T> CallGetAsync<T>(string url, Dictionary<string, string> queryParams)
        {
            if (queryParams.Any())
            {
                var sb = new StringBuilder(url);
                if (!url.EndsWith("?"))
                {
                    sb.Append("?");
                }
                var paramString = ParamsFormat(queryParams);
                sb.Append(paramString);

                url = sb.ToString();
            }

            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };
            setHeaders(httpClient);
            using var response = await httpClient.GetAsync(url);
            return await ProcessHttpResponse<T>(response);
        }

        /// <summary>
        /// Método que faz uma requisição do tipo Post à API Zenvia.
        /// </summary>
        /// <typeparam name="TResponse">Tipo de objeto esperado como retorno.</typeparam>
        /// <typeparam name="TRequest">Tipo de objeto a ser enviado.</typeparam>
        /// <param name="url">URL de destino da requisição.</param>
        /// <param name="obj">Objeto à ser serializado e enviado à API.</param>
        /// <returns>Objeto de tipo definido resultante da requisição.</returns>
        private async Task<TResponse> CallPostAsync<TResponse, TRequest>(string url, TRequest obj)
            where TRequest : BaseObject
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(url);
                setHeaders(httpClient);
                var content = new StringContent(obj.ToJson(), Encoding.Default, "application/json");
                using var response = await httpClient.PostAsync(url, content);
                return await ProcessHttpResponse<TResponse>(response);
            }
            catch (ArgumentNullException ex)
            {
                throw new ZenviaException("Falha ao processar requisição: " + ex.Message, ex);
            }
            catch (HttpRequestException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Falha ao processar requisição:\n");
                sb.Append("Mensagem: " + ex.Message);
                if (ex.InnerException != null)
                {
                    sb.Append("\nDetalhe: " + ex.InnerException.Message);
                }
                throw new ZenviaException(sb.ToString(), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<T> ProcessHttpResponse<T>(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                // Código 200 - Solicitação OK.
                case HttpStatusCode.OK:
                    return Desserialize<T>(await response.Content.ReadAsStringAsync());
                // Código 201 - Criada.
                case HttpStatusCode.Created:
                    return Desserialize<T>(await response.Content.ReadAsStringAsync());
                // Código 400 - Bad Request.
                case HttpStatusCode.BadRequest:
                    throw new ZenviaUnauthorizedException("O servidor da API rejeitou a solicitação: BadRequest (400)!");
                // Código 401 - Não Autorizado.
                case HttpStatusCode.Unauthorized:
                    throw new ZenviaUnauthorizedException("O servidor da API rejeitou a solicitação: Unauthorized (401)!");
                // Código 403 - Proibido.
                case HttpStatusCode.Forbidden:
                    throw new ZenviaForbiddenException("O servidor da API rejeitou a solicitação: Forbidden (403)!");
                // Código 404 - Não encontrado.
                case HttpStatusCode.NotFound:
                    throw new ZenviaNotFoundException("O servidor da API não foi encontrado: Not Found (404)!");
                // Código 408 - Tempo esgotado.
                case HttpStatusCode.RequestTimeout:
                    throw new ZenviaRequestTimeoutException("O servidor da API não respondeu a solicitação em tempo habil: Request Timeout (408)!");
                // Código 503 - Serviço indisponível.
                case HttpStatusCode.ServiceUnavailable:
                    throw new ZenviaUnavailableException("O serviço solicitado não está disponível: Service Unavailable (503)!");
                // Qualquer outro caso diferente dos demais.
                default:
                    throw new ZenviaException(string.Format("A requisição HTTP não pôde ser atendida! StatusCode: {1}", response.StatusCode));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        private string ParamsFormat(Dictionary<string, string> queryParams)
        {
            bool haveMtId = false;
            StringBuilder sb = new StringBuilder();
            if (queryParams.ContainsKey("mtId"))
            {
                sb.Append("mtId=").Append(queryParams.Where(x => x.Key == "mtId").First().Value);
                haveMtId = true;
            }
            if (queryParams.ContainsKey("mobile"))
            {
                if (haveMtId)
                {
                    sb.Append("&");
                }
                sb.Append("mobile=").Append(queryParams.Where(x => x.Key == "mobile").First().Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Método que define o cabeçalho da requisição HTTP para o envio à API Zenvia.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP que processará a requisição.</param>
        private void setHeaders(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-API-TOKEN", ApiToken);
        }

        /// <summary>
        /// Método que serializa um objeto para ser enviado à API Zenvia. O resultado é a represenação json do objeto.
        /// </summary>
        /// <param name="obj">Objeto que será serializado.</param>
        /// <returns>Representação json referente ao objeto informado.</returns>
        private string Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Método que desserializa uma representação json em objeto do tipo definido nos parâmetros.
        /// </summary>
        /// <typeparam name="T">Tipo de objeto que se espera da representação json.</typeparam>
        /// <param name="json">Representação json do objeto.</param>
        /// <returns>Objeto de tipo definido nos parâmetros resultante da representação json informada.</returns>
        private T Desserialize<T>(String json)
        {
            // Contorno de um bug do Zenvia que envia uma aspa '”' ao invés de uma aspa dupla '"' quando o comando para
            // Cancelar SMS agendado é chamado.
            json = json.Replace('”', '"');
            var jo = JObject.Parse(json);
            return JsonConvert.DeserializeObject<T>(jo.ToString());
        }
    }
}
