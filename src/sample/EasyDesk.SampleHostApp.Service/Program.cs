using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Logging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.Extensions.Configuration;
using Rebus.Config;

var builder = CleanArchitectureApp.CreateHostBuilder("EasyDesk.SampleHostApp", args).ConfigureDefaults();

builder
    .AddLogging(options => options
        .EnableRequestLogging()
        .EnableResultLogging())
    .AddAsyncApi()
    .AddJsonSerialization()
    .AddRebusMessaging(
        inputQueueAddress: "sample-host",
        (t, e) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e.InputQueueAddress),
        options =>
        {
            options.FailuresOptions.AddDispatchAsFailure();
            options.UseOutbox = false;
            options.UseInbox = false;
        });

return await builder.Run();
