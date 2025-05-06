// See https://aka.ms/new-console-template for more information

using CoreApiCommunicator;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyIndustry.Queue;

var builder = Host.CreateDefaultBuilder(args);

// Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");


builder.ConfigureAppConfiguration((context, config) =>
{
    var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    var d = Environment.GetEnvironmentVariables();
    // appsettings.json dosyasını yükleyin
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
});
builder.ConfigureServices((hostContext, services) =>
{
    var configuration = hostContext.Configuration;

    services.AddHttpClient("core-api", client =>
    {
        client.BaseAddress = new Uri(configuration["CoreApiUrl"]);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    services.AddTransient(typeof(ICoreApiCommunicator<,>), typeof(CoreApiCommunicator<,>));

    var rabbitMqSettings = configuration.GetSection("RabbitMq");
    services.AddMassTransit(x =>
    {
        x.AddConsumer<CreateSellerConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings["Host"], ushort.Parse(rabbitMqSettings["Port"]), "/", h =>
            {
                h.Username(rabbitMqSettings["UserName"]);
                h.Password(rabbitMqSettings["Password"]);
            });

            cfg.ReceiveEndpoint("create_seller_queue", e => { e.ConfigureConsumer<CreateSellerConsumer>(context); });
        });
    });
});

var app = builder.Build();
await app.RunAsync();