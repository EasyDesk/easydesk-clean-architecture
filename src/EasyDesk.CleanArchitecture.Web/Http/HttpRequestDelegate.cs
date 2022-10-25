namespace EasyDesk.CleanArchitecture.Web.Http;

public delegate Task<HttpResponseMessage> HttpRequestDelegate(HttpClient httpClient);
