using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.AspNetCore.Http;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time
{
    public class ClientSentDateTime : ITimestampProvider
    {
        private const string DateTimeHeaderName = "x-custom-date";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITimestampProvider _defaultDateTimeProvider;

        public ClientSentDateTime(IHttpContextAccessor httpContextAccessor, ITimestampProvider defaultDateTimeProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _defaultDateTimeProvider = defaultDateTimeProvider;
        }

        public Timestamp Now => _httpContextAccessor
            .HttpContext
            .Request
            .Headers
            .GetOption(DateTimeHeaderName)
            .Map(s => Timestamp.FromUtcDateTime(DateTime.Parse(s.ToString())))
            .OrElseGet(() => _defaultDateTimeProvider.Now);
    }
}
