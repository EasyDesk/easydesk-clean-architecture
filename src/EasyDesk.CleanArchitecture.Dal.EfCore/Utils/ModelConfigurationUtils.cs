using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils
{
    public static class ModelConfigurationUtils
    {
        private const string DateTimeType = "datetime";
        private const string DateType = "date";
        private const string TimeType = "time";

        public static PropertyBuilder<T> HasTypeDateTime<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(DateTimeType);
        }

        public static PropertyBuilder<T> HasTypeDate<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(DateType);
        }

        public static PropertyBuilder<T> HasTypeTime<T>(this PropertyBuilder<T> property)
        {
            return property
                .HasColumnType(TimeType);
        }
    }
}
