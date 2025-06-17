using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public interface IAggregateRootModel;

public interface IAggregateRootModel<TDomain, TPersistence> : IEntityPersistence<TDomain, TPersistence>, IAggregateRootModel
    where TPersistence : IAggregateRootModel<TDomain, TPersistence>;
