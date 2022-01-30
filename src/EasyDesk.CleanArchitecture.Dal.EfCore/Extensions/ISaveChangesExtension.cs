using EasyDesk.Tools;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;

public interface ISaveChangesExtension
{
    int Run(DbContext context, Func<int> next);

    Task<int> RunAsync(DbContext context, AsyncFunc<int> next, CancellationToken cancellationToken);
}
