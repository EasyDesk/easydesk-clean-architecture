﻿using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public delegate Option<string> HttpRequestTenantReader(HttpContext httpContext, Option<Agent> agent);

public delegate Option<string> AsyncMessageTenantReader(IMessageContext messageContext);

public static class CommonTenantReaders
{
    public const string TenantIdHttpHeader = "x-tenant-id";
    public const string TenantIdHttpQueryParam = "tenantId";

    public static Option<string> ReadFromHttpContext(HttpContext httpContext) =>
        ReadTenantIdFromHeaders(httpContext.Request) | ReadTenantIdFromQuery(httpContext.Request);

    private static Option<string> ReadTenantIdFromHeaders(HttpRequest request) =>
        request.Headers[TenantIdHttpHeader].WhereNotNull().FirstOption();

    private static Option<string> ReadTenantIdFromQuery(HttpRequest request) =>
        request.Query[TenantIdHttpQueryParam].SelectMany(h => h.AsOption()).FirstOption();

    public static Option<string> ReadFromMessageContext(IMessageContext messageContext) =>
        messageContext.Headers.GetOption(MultitenantMessagingUtils.TenantIdHeader);
}
