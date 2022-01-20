using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;

public class ModelExtensionsRunner
{
    private readonly IEnumerable<IModelExtension> _extensions;

    public ModelExtensionsRunner(IEnumerable<IModelExtension> extensions)
    {
        _extensions = extensions;
    }

    public void Run(ModelBuilder modelBuilder, Action<ModelBuilder> onModelCreating)
    {
        _extensions.FoldRight(onModelCreating, (ext, curr) => modelBuilder => ext.Run(modelBuilder, curr))(modelBuilder);
    }
}
