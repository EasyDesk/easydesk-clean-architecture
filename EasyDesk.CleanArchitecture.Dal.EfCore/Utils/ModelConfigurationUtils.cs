using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils
{
    public static class ModelConfigurationUtils
    {
        private const string _dateTimeType = "datetime";
        private const string _dateType = "date";
        private const string _timeType = "time";

        public static PropertyBuilder<T> HasTypeDateTime<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(_dateTimeType);
        }

        public static PropertyBuilder<T> HasTypeDate<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(_dateType);
        }

        public static PropertyBuilder<T> HasTypeTime<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(_timeType);
        }
    }
}
