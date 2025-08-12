using Autofac;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Data.Common;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public sealed class EfCoreDataAccessOptions<T, TBuilder, TExtension>
    where T : AbstractDbContext
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private const string MigrationsTableSuffix = "EFMigrationsHistory";
    private const string CleanArchitectureDbContextPrefix = "__CA";

    private readonly IEfCoreProvider<TBuilder, TExtension> _provider;
    private readonly ISet<Type> _registeredDbContextTypes = new HashSet<Type>();
    private Action<DbContextOptionsBuilder>? _configureDbContextOptions;
    private Action<TBuilder>? _configureProviderOptions;
    private Action<ServiceRegistry>? _configureRegistry;

    public EfCoreDataAccessOptions(IEfCoreProvider<TBuilder, TExtension> provider)
    {
        _provider = provider;
    }

    internal Assembly InternalMigrationsAssembly => _provider.GetType().Assembly;

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> ConfigureDbContextOptions(Action<DbContextOptionsBuilder> configure)
    {
        _configureDbContextOptions += configure;
        return this;
    }

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> ConfigureProviderOptions(Action<TBuilder> configure)
    {
        _configureProviderOptions += configure;
        return this;
    }

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> WithService<S>()
        where S : class
    {
        _configureRegistry += registry => registry.ConfigureServices(services => services.AddScoped(p => p.GetRequiredService<T>() as S
            ?? throw new InvalidOperationException(
                $"Cannot use {typeof(S).Name} as a service type for DbContext {typeof(T).Name}.")));
        return this;
    }

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> WithCustomDbContext<C>()
        where C : DbContext
    {
        _configureRegistry += registry => RegisterDbContext<C>(registry, isCleanArchitectureContext: false);
        return this;
    }

    internal void RegisterUtilityServices(ServiceRegistry registry)
    {
        registry.ConfigureContainer(builder =>
        {
            builder.Register(_ => _provider.NewConnection())
                .InstancePerUseCase();

            builder.RegisterType<TransactionEnlistingOnCommandInterceptor>()
                .InstancePerUseCase();

            builder.RegisterType<DbContextEnlistingOnSaveChangesInterceptor>()
                .InstancePerUseCase();

            builder
                .Register(c => new MigrationsService(
                    _registeredDbContextTypes.Select(t => c.Resolve(t)).Cast<DbContext>().ToList(),
                    c.Resolve<ILogger<MigrationsService>>()))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfCoreSaveChangesHandler>()
                .AsSelf()
                .As<ISaveChangesHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<EfCoreUnitOfWorkManager>()
                .AsSelf()
                .As<IUnitOfWorkManager>()
                .InstancePerUseCase();

            builder.Register(c => MigrationCommand(c.Resolve<IComponentContext>()));
        });

        _configureRegistry?.Invoke(registry);
    }

    private Command MigrationCommand(IComponentContext context)
    {
        var command = new Command("migrate", "Apply migrations to the database");
        var syncOption = new Option<bool>(aliases: ["--sync", "--synchronous",], getDefaultValue: () => false, description: "Apply migrations synchronously");
        command.AddOption(syncOption);
        command.SetHandler(sync => context.Resolve<MigrationsService>().Migrate(sync), syncOption);
        return command;
    }

    internal void RegisterDbContext<C>(ServiceRegistry registry, bool isCleanArchitectureContext = true)
        where C : DbContext
    {
        if (_registeredDbContextTypes.Contains(typeof(C)))
        {
            return;
        }

        registry.ConfigureServices(services => services.AddDbContext<C>((provider, options) =>
        {
            var connection = provider.GetRequiredService<DbConnection>();
            _provider.ConfigureDbProvider(options, connection, relationalOptions =>
            {
                if (isCleanArchitectureContext)
                {
                    relationalOptions.MigrationsAssembly(InternalMigrationsAssembly.GetName().Name);
                }

                var migrationsTableName = isCleanArchitectureContext ? $"{CleanArchitectureDbContextPrefix}_{typeof(C).Name}_{MigrationsTableSuffix}" : $"{typeof(C).Name}_{MigrationsTableSuffix}";
                relationalOptions.MigrationsHistoryTable(migrationsTableName, EfCoreConstants.MigrationsSchema);

                _configureProviderOptions?.Invoke(relationalOptions);
            });

            options.AddInterceptors(provider.GetRequiredService<TransactionEnlistingOnCommandInterceptor>());
            options.AddInterceptors(provider.GetRequiredService<DbContextEnlistingOnSaveChangesInterceptor>());
            _configureDbContextOptions?.Invoke(options);
        }));

        if (!isCleanArchitectureContext)
        {
            registry.ConfigureContainer(builder =>
            {
                builder.RegisterDecorator<C>((c, _, inner) =>
                {
                    c.Resolve<EfCoreSaveChangesHandler>().AddDbContext(inner);
                    return inner;
                });
            });
        }

        _registeredDbContextTypes.Add(typeof(C));
    }
}
