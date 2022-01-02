using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping
{
    public class MappingPlugin : IRelationalTypeMappingSourcePlugin
    {
        private readonly IDictionary<Type, Func<RelationalTypeMapping>> _mappingsByType;

        public MappingPlugin(IDictionary<Type, Func<RelationalTypeMapping>> mappingsByType)
        {
            _mappingsByType = mappingsByType;
        }

        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            return _mappingsByType
                .GetOption(mappingInfo.ClrType)
                .Map(f => f())
                .OrElseNull();
        }
    }

    public class MappingPluginOptionsExtension : IDbContextOptionsExtension
    {
        private readonly IDictionary<Type, Func<RelationalTypeMapping>> _mappingsByType;

        public MappingPluginOptionsExtension(IDictionary<Type, Func<RelationalTypeMapping>> mappingsByType)
        {
            _mappingsByType = mappingsByType;
        }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddSingleton<IRelationalTypeMappingSourcePlugin>(_ => new MappingPlugin(_mappingsByType));
        }

        public void Validate(IDbContextOptions options)
        {
        }

        private class MappingInfo : DbContextOptionsExtensionInfo
        {
            public MappingInfo(IDbContextOptionsExtension extension) : base(extension)
            {
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
            }

            public override bool IsDatabaseProvider => false;

            public override string LogFragment => "MappingPlugin=ON";

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is MappingInfo;

            public override int GetServiceProviderHashCode() => 0;
        }

        public DbContextOptionsExtensionInfo Info => new MappingInfo(this);
    }
}
