using Autofac;
using Autofac.Core;
using EasyDesk.Commons.Options;

namespace EasyDesk.Extensions.DependencyInjection;

public static class ComponentContextExtensions
{
    public static Option<T> ResolveOption<T>(this IComponentContext context) where T : class =>
        context.ResolveOptional<T>().AsOption();

    public static Option<T> ResolveOption<T>(this IComponentContext context, params IEnumerable<Parameter> parameters) where T : class =>
        context.ResolveOptional<T>(parameters).AsOption();
}

public static class ComponentContextValueExtensions
{
    public static Option<T> ResolveOption<T>(this IComponentContext context) where T : struct =>
        context.ResolveOptional<T>().AsOption();

    public static Option<T> ResolveOption<T>(this IComponentContext context, params IEnumerable<Parameter> parameters) where T : struct =>
        context.ResolveOptional<T>(parameters).AsOption();
}
