using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.Testing.MatrixExpansion;

public static class Matrix
{
    public static MatrixBuilder<T> Axis<T>(IEnumerable<T> axis) => new(axis);

    public static MatrixBuilder<T> Axis<T>(params T[] axis) => Axis(axis.AsEnumerable());

    public static MatrixBuilder<T> Fixed<T>(T value) => new(new T[] { value });
}
