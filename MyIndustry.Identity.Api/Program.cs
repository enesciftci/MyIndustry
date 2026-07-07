using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Container.Extensions;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Aggregate.ValueObjects;
using MyIndustry.Identity.Api.Services;
using MyIndustry.Identity.Domain.Service;
using MyIndustry.Identity.Repository;
using RabbitMqCommunicator;
using RedisCommunicator;
using Serilog;
using StackExchange.Redis;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureMyIndustrySerilog("MyIndustry.Identity.Api");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
if (builder.Environment.IsDevelopment())
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("type", "99")); // UserType.Admin
});

builder.Services.AddMyIndustryCors(builder.Configuration, builder.Environment);
builder.Services.AddMyIndustryRateLimiting(builder.Configuration);

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICustomMessagePublisher, CustomMessageMessagePublisher>();
builder.Services.AddHttpClient<IMainApiLegalDocumentAcceptanceClient, MainApiLegalDocumentAcceptanceClient>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddMassTransit(x =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq");

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqSettings["Host"], ushort.Parse(rabbitMqSettings["Port"]), "/", h =>
            {
                h.Username(rabbitMqSettings["UserName"]);
                h.Password(rabbitMqSettings["Password"]);
            });

            cfg.ConfigureEndpoints(context);
        });
    });
}

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(p =>
    {
        p.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MyIndustryIdentityDbContext>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(15); // Email verification code lifespan
});

var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
var jwtSigningKey = JwtExtensions.ResolveSigningKey(builder.Configuration, builder.Environment);
var jwtIssuer = JwtExtensions.ResolveIssuer(builder.Configuration);
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:SigningKey"] = jwtSigningKey });

var tokenValidationParameters = JwtExtensions.CreateTokenValidationParameters(jwtSigningKey, jwtIssuer);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    
}).AddJwtBearer(options =>
{
    options.Authority = identityUrl;
    options.Audience = JwtExtensions.DefaultAudience;
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing");
    options.TokenValidationParameters = tokenValidationParameters;
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) return;
            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            if (authService != null && await authService.IsTokenBlacklistedAsync(jti))
                context.Fail("Token has been revoked (blacklisted).");
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddDbContextPool<MyIndustryIdentityDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("MyIndustryIdentityDb"), npgsqlDbContextOptionsBuilder =>
                npgsqlDbContextOptionsBuilder.EnableRetryOnFailure(2, TimeSpan.FromSeconds(10), null)
                    .CommandTimeout(60))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        
        if (builder.Environment.IsDevelopment())
            options.EnableSensitiveDataLogging();
    });

if (!builder.Environment.IsEnvironment("Testing"))
{
    var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
    var redis = ConnectionMultiplexer.Connect(redisConfiguration);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddSingleton<IRedisCommunicator, RedisCommunicator.RedisCommunicator>();

    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
        .SetApplicationName("MyIndustry-Identity");
}
else
{
    builder.Services.AddDataProtection()
        .SetApplicationName("MyIndustry-Identity");
    // IRedisCommunicator is registered by WebApplicationFactory in tests.
}


AllowedHostsExtensions.ValidateProductionAllowedHosts(builder.Configuration, builder.Environment);

var app = builder.Build();

var forwardedProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
foreach (var ip in forwardedProxies)
{
    if (System.Net.IPAddress.TryParse(ip, out var addr))
        forwardedOptions.KnownProxies.Add(addr);
}
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

// Auto-migrate database (skipped in test environment)
if (!app.Environment.IsEnvironment("Testing"))
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyIndustryIdentityDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Check if we should reset the database (via environment variable)
    var resetDb = Environment.GetEnvironmentVariable("RESET_IDENTITY_DATABASE") == "true";
    
    if (resetDb && !app.Environment.IsDevelopment())
    {
        logger.LogWarning("RESET_IDENTITY_DATABASE is set but ignored in non-Development environment.");
        resetDb = false;
    }
    
    if (resetDb)
    {
        logger.LogWarning("RESET_IDENTITY_DATABASE is true - Dropping and recreating database...");
        await db.Database.EnsureDeletedAsync();
        logger.LogInformation("Identity database deleted successfully.");
    }
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations. Attempting fresh database creation...");
        // If migration fails (e.g., database was created with EnsureCreated), reset and try again
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        logger.LogInformation("Fresh database created and migrations applied.");
    }
    
    // Seed admin user to database
    await SeedAdminUser(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
app.UseCorrelationId();
app.UseSecurityHeaders();
app.UseMyIndustryRequestLogging();
app.UseMyIndustryExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMyIndustryRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapIdentityApi<ApplicationUser>();
app.MapHealthChecks("/health");

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// Admin kullanıcıyı veritabanına ekle (şifre ortam değişkeni veya config'ten; asla koda sabit yazmayın)
static async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    const string defaultAdminEmail = "admin@admin.com";
    var adminEmail = configuration["SeedAdmin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? defaultAdminEmail;
    var adminPassword = configuration["SeedAdmin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    
    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        logger.LogWarning("SeedAdmin:Password or ADMIN_PASSWORD not set - skipping admin user seed. Set in production.");
        return;
    }
    
    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            FirstName = "Admin",
            LastName = "Administrator",
            Type = UserType.Admin,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        
        if (result.Succeeded)
        {
            logger.LogInformation("Admin user seeded to database successfully");
        }
        else
        {
            logger.LogError("Failed to seed admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}

namespace MyIndustry.Identity.Api
{
    public partial class Program;
}