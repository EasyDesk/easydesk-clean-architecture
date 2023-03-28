namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public delegate IQueryable<T> QueryWrapper<T>(IQueryable<T> query);
