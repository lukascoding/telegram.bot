using System.Collections.Generic;
using System.Threading.Tasks;

namespace lukascoding.TelegramBotApiClient
{
    /// <summary>
    /// a client interface to use the api
    /// </summary>
    internal interface IWebApiClient
    {
        Task<TResponse> GetAsync<TResponse>(string path);
        Task<TResponse> GetAsync<TResponse>(string path, IDictionary<string, object> urlParameters);
        Task Put(string path, object content);
        Task Put(string path, IDictionary<string, object> urlParameters);
        Task Put(string path, IDictionary<string, object> urlParameters, object content);
        Task<TResponse> Post<TResponse>(string path, object content);
        Task Post(string path, object content);
        Task Post(string path, IDictionary<string, object> urlParameters, object content);
    }
}
