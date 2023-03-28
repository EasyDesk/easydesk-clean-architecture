using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.Testing;

public static class Args
{
    public static IEnumerable<T> Are<T>(IEnumerable<T> matchingArgument) =>
        Arg.Is<IEnumerable<T>>(arg => arg.SequenceEqual(matchingArgument));
}
