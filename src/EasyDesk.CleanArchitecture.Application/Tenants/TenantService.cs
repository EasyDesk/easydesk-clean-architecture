using EasyDesk.Tools.Options;
using System;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Tenants
{
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
}
