using Microsoft.EntityFrameworkCore;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;

public interface IModelExtension
{
    void Run(ModelBuilder modelBuilder, Action next);
}
