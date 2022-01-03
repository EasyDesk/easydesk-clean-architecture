using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;

public class TimeOfDayMapping : BaseTypeMapping<TimeOfDay, TimeSpan>
{
    public const string TimeOfDayType = "time";

    private static readonly ValueConverter<TimeOfDay, TimeSpan> _converter = new(
        timeOfDay => timeOfDay.AsTimeSpan,
        timeSpan => TimeOfDay.FromTimeSpan(timeSpan));

    private TimeOfDayMapping(RelationalTypeMappingParameters parameters) : base(parameters)
    {
    }

    public TimeOfDayMapping() : base(_converter, TimeOfDayType)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new TimeOfDayMapping(parameters);
}
