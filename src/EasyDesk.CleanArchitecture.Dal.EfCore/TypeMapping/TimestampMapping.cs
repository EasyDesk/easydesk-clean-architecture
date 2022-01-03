using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;

public class TimestampMapping : BaseTypeMapping<Timestamp, DateTime>
{
    public const string TimestampType = "datetime";

    private static readonly ValueConverter<Timestamp, DateTime> _converter = new(
        timestamp => timestamp.AsDateTime,
        dateTime => Timestamp.FromUtcDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));

    private TimestampMapping(RelationalTypeMappingParameters parameters) : base(parameters)
    {
    }

    public TimestampMapping() : base(_converter, TimestampType)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new TimestampMapping(parameters);
}
