using EasyDesk.Tools.Options;
using System;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Tenants
{
    public class TenantService : ITenantProvider, ITenantInitializer
    {
        private bool _wasInitialized = false;
        private Option<string> _tenantId = None;

        public Option<string> TenantId => _wasInitialized
            ? _tenantId
            : throw new InvalidOperationException("Tenant has not been initialized yet");

        public void InitializeTenant(Option<string> tenantId)
        {
            if (_wasInitialized)
            {
                throw new InvalidOperationException("Tenant was already initialized");
            }
            _wasInitialized = true;
            _tenantId = tenantId;
        }
    }
}
