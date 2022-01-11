using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Time;

public static class DependencyInjection
{
    public static IServiceCollection AddTimestampProvider(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton(_ => CreateDefaultDateTimeService(configuration));
    }

    private static ITimestampProvider CreateDefaultDateTimeService(IConfiguration configuration)
    {
        if (configuration.GetValueAsOption<bool>("DateTimeTestingUtils:UseFixedDateTime").OrElse(false))
        {
            var utcNow = configuration.RequireValue<DateTime>("DateTimeTestingUtils:FixedDateTime");
            utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            return new FixedDateTime(Timestamp.FromUtcDateTime(utcNow));
        }
        else
        {
            return new MachineDateTime();
        }
    }
}
