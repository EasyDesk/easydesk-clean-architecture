using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;

public class SaveChangesExtensionsRunner
{
    private readonly IEnumerable<ISaveChangesExtension> _extensions;

    public SaveChangesExtensionsRunner(IEnumerable<ISaveChangesExtension> extensions)
    {
        _extensions = extensions;
    }

    public int Run(DbContext dbContext)
    {
        var saveChanges = () => dbContext.SaveChanges();
        return _extensions.FoldRight(saveChanges, (ext, curr) => () => ext.Run(dbContext, curr))();
    }

    public async Task<int> RunAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        AsyncFunc<CancellationToken, int> saveChangesAsync = dbContext.SaveChangesAsync;
        return await _extensions.FoldRight(saveChangesAsync, (ext, curr) => t => ext.RunAsync(dbContext, t, curr))(cancellationToken);
    }
}
