using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lukascoding.TelegramBotApiClient.Exceptions;
using lukascoding.TelegramBotApiClient.Types;
using Newtonsoft.Json;

namespace lukascoding.TelegramBotApiClient
{
    /// <summary>
    /// ApiClient to communicate with the telegram Api
    /// </summary>
    public class WebApiClient : HttpClient
    {
        private readonly string _baseAddress;
        private readonly CancellationToken _cancellationToken;
        private readonly IWebProxy _proxy;

        /// <summary>
        /// new webapiclient
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="cancellationToken"></param>
        public WebApiClient(string baseAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            _baseAddress = baseAddress;
            _cancellationToken = cancellationToken;
        }
        /// <summary>
        /// new webapiclient
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="proxy"></param>
        /// <param name="cancellationToken"></param>
        public WebApiClient(string baseAddress, IWebProxy proxy, CancellationToken cancellationToken = default(CancellationToken))
        {
            _baseAddress = baseAddress;
            _proxy = proxy;
            _cancellationToken = cancellationToken;
        }

        public async Task<TResponse> GetAsync<TResponse>(string path)
        {
            return await GetAsync<TResponse>(path, null);
        }

        public async Task<TResponse> GetAsync<TResponse>(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Get, pathWithQuery, null))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                return await ParseResponseMessage<TResponse>(responseMessage);
            }
        }

        public async Task PutAsync(string path, object content)
        {
            await PutAsync(path, null, content);
        }

        public async Task PutAsync(string path, IDictionary<string, object> urlParameters, object content)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Put, pathWithQuery, content))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public async Task<TResponse> PostAsync<TResponse>(string path, object content)
        {
            using (var requestMessage = CreateRequestMessage(HttpMethod.Post, path, content))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
                return await ParseResponseMessage<TResponse>(responseMessage);
            }
        }

        public async Task PostAsync(string path, object content)
        {
            await PostAsync(path, null, content);
        }

        public async Task PostAsync(string path, IDictionary<string, object> urlParameters, object content)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Post, pathWithQuery, content))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public async Task DeleteAsync(string path, IDictionary<string, object> urlParameters)
        {
            var pathWithQuery = BuildPathWithQuery(path, urlParameters);
            using (var requestMessage = CreateRequestMessage(HttpMethod.Delete, pathWithQuery, null))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        public async Task DeleteAsync(string path, object content)
        {
            using (var requestMessage = CreateRequestMessage(HttpMethod.Delete, path, content))
            using (var responseMessage = await SendRequestMessage(requestMessage))
            {
                CheckResponseMessage(responseMessage);
            }
        }

        private static string BuildPathWithQuery(string path, IDictionary<string, object> urlParameters)
        {
            if (urlParameters != null || urlParameters.Count == 0)
            {
                var query = string.Join("&", urlParameters.Select(p => p.Value != null ? $"{p.Key}={Uri.EscapeUriString(p.Value.ToString())}" : p.Key));
                if (!string.IsNullOrEmpty(query))
                {
                    path += "?" + query;
                }
            }
            return path;
        }

        private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string path, object content)
        {
            var message = new HttpRequestMessage(method, path);
            if (content != null)
            {
                var json = JsonConvert.SerializeObject(content);
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return message;
        }

        private async Task<HttpResponseMessage> SendRequestMessage(HttpRequestMessage message)
        {

            using (var client = CreateHttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_baseAddress);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.SendAsync(message).ConfigureAwait(false);

                    //response.EnsureSuccessStatusCode();

                    return response;
                }
                catch (HttpRequestException e) when (e.Message.Contains("401"))
                {
                    throw new ApiRequestException("Invalid token", 401, e);
                }
                catch (TaskCanceledException e)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        throw;

                    throw new ApiRequestException("Request timed out", 408, e);
                }
                //catch (HttpRequestException e) when (e.Message.Contains("400") || e.Message.Contains("403") ||
                //                                     e.Message.Contains("409"))
                //{
                    
                //}
            }
        }

        protected virtual HttpClient CreateHttpClient()
        {
            if (_proxy != null)
            {
                return new HttpClient(new HttpClientHandler()
                {
                    Proxy = _proxy,
                    UseProxy = true
                });
            }
            return new HttpClient(new HttpClientHandler());
        }

        private static void CheckResponseMessage(HttpResponseMessage response)
        {

        }

        private static async Task<T> ParseResponseMessage<T>(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<ApiResponse<T>>(responseString);

            if (responseObject == null)
                responseObject = new ApiResponse<T> { Ok = false, Message = "No response received" };

            //if (!responseObject.Ok)
            //    throw ApiRequestException.FromApiResponse(responseObject);

            return responseObject.ResultObject;
        }
    }
}
