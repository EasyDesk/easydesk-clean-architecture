using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Http
{
    public delegate Task<HttpResponseMessage> HttpRequest(HttpClient client);

    public abstract class HttpService
    {
        private readonly Func<HttpClient> _httpClientFactory;

        public HttpService(IHttpClientFactory httpClientFactory, string clientName = null, string defaultVersion = null)
        {
            _httpClientFactory = () =>
            {
                var client = clientName is null
                    ? httpClientFactory.CreateClient()
                    : httpClientFactory.CreateClient(clientName);

                if (defaultVersion != null)
                {
                    client.DefaultRequestHeaders.Add("Api-Version", defaultVersion);
                }

                return client;
            };
        }

        private HttpRequestBuilder Builder(HttpRequest request) => new(_httpClientFactory(), request);

        protected HttpRequestBuilder Get(string url) => Builder(client => client.GetAsync(url));

        protected HttpRequestBuilder Post<T>(string url, T body) => Builder(client => client.PostAsJsonAsync(url, body));

        protected HttpRequestBuilder Put<T>(string url, T body) => Builder(client => client.PutAsJsonAsync(url, body));

        protected HttpRequestBuilder Delete(string url) => Builder(client => client.DeleteAsync(url));
    }
}
