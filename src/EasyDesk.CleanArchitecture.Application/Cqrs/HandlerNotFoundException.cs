﻿namespace EasyDesk.CleanArchitecture.Application.Cqrs;

public class HandlerNotFoundException : Exception
{
    public HandlerNotFoundException(Type requestType)
        : base($"Unable to find an handler for request of type '{requestType.Name}'")
    {
        RequestType = requestType;
    }

    public Type RequestType { get; }
}