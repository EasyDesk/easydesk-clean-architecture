namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public delegate Task<HttpResponseMessage> HttpRequestDelegate(HttpClient httpClient);
