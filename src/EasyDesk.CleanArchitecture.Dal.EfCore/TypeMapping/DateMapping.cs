using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;

public class DateMapping : BaseTypeMapping<Date, DateTime>
{
    public const string DateType = "date";

    private static readonly ValueConverter<Date, DateTime> _converter = new(
        date => date.AsDateTime,
        dateTime => Date.FromDateTime(dateTime));

    private DateMapping(RelationalTypeMappingParameters parameters) : base(parameters)
    {
    }

    public DateMapping() : base(_converter, DateType)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new DateMapping(parameters);
}
