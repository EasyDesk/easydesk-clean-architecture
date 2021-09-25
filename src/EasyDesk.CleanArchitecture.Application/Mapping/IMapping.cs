using AutoMapper;
using System;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Application.Mapping
{
    public interface IMapping
    {
        void ConfigureProfile(Profile profile);
    }

    public abstract class AbstractMapping<T> : IMapping
    {
        private Action<T> _mapAction;

        public void ConfigureProfile(Profile profile)
        {
            var expr = CreateMap(profile);
            _mapAction?.Invoke(expr);
        }

        protected abstract T CreateMap(Profile profile);

        protected void Configure(Action<T> action)
        {
            _mapAction += action;
        }
    }

    public abstract class SimpleMapping<TSource, TDest> : AbstractMapping<IMappingExpression<TSource, TDest>>
    {
        protected sealed override IMappingExpression<TSource, TDest> CreateMap(Profile profile) =>
            profile.CreateMap<TSource, TDest>();

        protected void Add<TFrom, TTarget>(Expression<Func<TDest, TTarget>> to, Expression<Func<TSource, TFrom>> from) =>
            Configure(map => map.ForMember(to, o => o.MapFrom(from)));

        protected void Set<T>(Expression<Func<TDest, T>> property, Func<T> value) =>
            Add(property, x => value());

        protected void AddCtorParam<T>(string paramName, Expression<Func<TSource, T>> member) =>
            Configure(map => map.ForCtorParam(paramName, o => o.MapFrom(member)));
    }

    public abstract class DirectMapping<TSource, TDest> : SimpleMapping<TSource, TDest>
    {
        public DirectMapping(Expression<Func<TSource, TDest>> mapper)
        {
            Configure(map => map.ConvertUsing(mapper));
        }
    }

    public abstract class OpenMapping : AbstractMapping<IMappingExpression>
    {
        private readonly Type _source;
        private readonly Type _destination;

        public OpenMapping(Type source, Type destination)
        {
            _source = source;
            _destination = destination;
        }

        protected sealed override IMappingExpression CreateMap(Profile profile) =>
            profile.CreateMap(_source, _destination);

        protected void Add(string dstMember, string srcMember) =>
            Configure(map => map.ForMember(dstMember, o => o.MapFrom(srcMember)));

        protected void Set(string dstMember, Func<object> value) =>
            Configure(map => map.ForMember(dstMember, o => o.MapFrom(src => value())));
    }
}
