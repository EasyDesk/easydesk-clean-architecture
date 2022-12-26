namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class HttpRequestMessageExtensions
{
    public static async Task<HttpRequestMessage> Clone(this HttpRequestMessage httpRequestMessage)
    {
        HttpRequestMessage httpRequestMessageClone = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);

        if (httpRequestMessage.Content != null)
        {
            var ms = new MemoryStream();
            await httpRequestMessage.Content.CopyToAsync(ms);
            ms.Position = 0;
            httpRequestMessageClone.Content = new StreamContent(ms);

            httpRequestMessage.Content.Headers?.ToList().ForEach(header => httpRequestMessageClone.Content.Headers.Add(header.Key, header.Value));
        }

        httpRequestMessageClone.Version = httpRequestMessage.Version;

        httpRequestMessage.Options.ToList().ForEach(props => httpRequestMessageClone.Options.TryAdd(props.Key, props.Value));
        httpRequestMessage.Headers.ToList().ForEach(header => httpRequestMessageClone.Headers.TryAddWithoutValidation(header.Key, header.Value));

        return httpRequestMessageClone;
    }
}
