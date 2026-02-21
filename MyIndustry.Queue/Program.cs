// See https://aka.ms/new-console-template for more information

using CoreApiCommunicator;
using CoreApiCommunicator.Email;
using CoreApiCommunicator.Sms;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyIndustry.Queue;

var builder = Host.CreateDefaultBuilder(args);

// Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");


builder.ConfigureAppConfiguration((context, config) =>
{
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                          ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                          ?? "Production";
    
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables(); // Environment variables override appsettings
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
    services.AddScoped<IEmailSender, EmailSender>();
    services.AddScoped<ISmsSender, SmsSender>();
    
    var rabbitMqSettings = configuration.GetSection("RabbitMq");
    services.AddMassTransit(x =>
    {
        // Seller artık SellerSetup sayfasından oluşturuluyor
        // Purchaser entity kaldırıldı - User bilgileri Identity'den alınıyor
        x.AddConsumer<IncreaseServiceViewCountConsumer>();
        x.AddConsumer<SendForgotPasswordEmailConsumer>();
        x.AddConsumer<SendConfirmationEmailConsumer>();
        x.AddConsumer<SendPhoneVerificationConsumer>();
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings["Host"], ushort.Parse(rabbitMqSettings["Port"]), "/", h =>
            {
                h.Username(rabbitMqSettings["UserName"]);
                h.Password(rabbitMqSettings["Password"]);
            });

            cfg.ReceiveEndpoint("increase_service_view_count_queue", e => { e.ConfigureConsumer<IncreaseServiceViewCountConsumer>(context); });
            cfg.ReceiveEndpoint("send_forgot_password_email_queue", e => { e.ConfigureConsumer<SendForgotPasswordEmailConsumer>(context); });
            cfg.ReceiveEndpoint("send_confirmation_email_queue", e => { e.ConfigureConsumer<SendConfirmationEmailConsumer>(context); });
            cfg.ReceiveEndpoint("send_phone_verification_queue", e => { e.ConfigureConsumer<SendPhoneVerificationConsumer>(context); });
        });
    });
});

var app = builder.Build();
await app.RunAsync();