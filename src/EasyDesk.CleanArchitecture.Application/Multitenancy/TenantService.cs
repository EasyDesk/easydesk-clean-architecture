﻿namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantService : ITenantProvider, ITenantInitializer
{
    private bool _wasInitialized = false;

    public Option<string> TenantId { get; private set; } = None;

    public void InitializeTenant(Option<string> tenantId)
    {
        if (_wasInitialized)
        {
            throw new InvalidOperationException("Tenant was already initialized");
        }
        _wasInitialized = true;
        TenantId = tenantId;
    }
}