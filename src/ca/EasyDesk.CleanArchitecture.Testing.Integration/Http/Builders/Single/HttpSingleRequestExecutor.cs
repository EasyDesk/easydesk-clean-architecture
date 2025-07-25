﻿using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.Commons.Tasks;
using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;

public class HttpSingleRequestExecutor<T>
    : HttpRequestExecutor<HttpResponseWrapper<T, Nothing>, ImmutableHttpResponseMessage, HttpSingleRequestExecutor<T>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HttpSingleRequestExecutor(
        HttpRequestBuilder httpRequestBuilder,
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions)
        : base(httpRequestBuilder)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public HttpResponseWrapper<T, Nothing> PollUntil(Func<T, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollUntil(async wrapped => predicate(await wrapped.AsData()), interval, timeout);

    public HttpResponseWrapper<T, Nothing> PollWhile(Func<T, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWhile(async wrapped => predicate(await wrapped.AsData()), interval, timeout);

    protected override async Task<ImmutableHttpResponseMessage> MakeRequest(CancellationToken timeoutToken)
    {
        using var req = HttpRequestBuilder.CreateRequest().ToHttpRequestMessage();
        using var res = await _httpClient.SendAsync(req, timeoutToken);
        return await ImmutableHttpResponseMessage.From(res);
    }

    protected override HttpResponseWrapper<T, Nothing> Wrap(AsyncFunc<ImmutableHttpResponseMessage> request) =>
        new(request, _jsonSerializerOptions);
}
