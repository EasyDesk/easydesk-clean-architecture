using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.CleanArchitecture.Infrastructure.DependencyInjection;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDateTimeLocale(this IServiceCollection services, IConfiguration config)
        {
            services.AddConfigAsSingleton<DateTimeLocaleSettings>(config);
            services.AddSingleton<ILocale, DateTimeLocale>();

            return services;
        }

        public static IServiceCollection AddDateTimeProvider(this IServiceCollection services, IConfiguration config)
        {
            return services.AddSingleton(_ => CreateDefaultDateTimeService(config));
        }

        private static ITimestampProvider CreateDefaultDateTimeService(IConfiguration config)
        {
            if (config.GetValue("DateTimeTestingUtils:UseFixedDateTime", false))
            {
                var utcNow = config.GetValue("DateTimeTestingUtils:FixedDateTime", DateTime.UtcNow);
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
                return new FixedDateTime(Timestamp.FromUtcDateTime(utcNow));
            }
            else
            {
                return new MachineDateTime();
            }
        }
    }
}
