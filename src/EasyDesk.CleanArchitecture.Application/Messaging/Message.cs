using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public record Message(
    Guid Id,
    Timestamp Timestamp,
    string Type,
    Option<string> TenantId,
    string Content);
