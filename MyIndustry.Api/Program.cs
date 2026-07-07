using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;
using MyIndustry.Api.Data;
using MyIndustry.Api.Services;
using MyIndustry.Container.Extensions;
using MyIndustry.Container.Services;
using RedisCommunicator;
using Serilog;
using StackExchange.Redis;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureMyIndustrySerilog("MyIndustry.Api");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

if (builder.Environment.IsDevelopment())
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Services
    .AddDbContextPool<MyIndustryDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("MyIndustry"), npgsqlDbContextOptionsBuilder =>
                npgsqlDbContextOptionsBuilder.EnableRetryOnFailure(2, TimeSpan.FromSeconds(10), null)
                    .CommandTimeout(60))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        
        if (builder.Environment.IsDevelopment())
            options.EnableSensitiveDataLogging();
    });

// Add Identity DbContext for accessing user information
builder.Services
    .AddDbContextPool<MyIndustry.Identity.Repository.MyIndustryIdentityDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("MyIndustryIdentityDb"), npgsqlDbContextOptionsBuilder =>
                npgsqlDbContextOptionsBuilder.EnableRetryOnFailure(2, TimeSpan.FromSeconds(10), null)
                    .CommandTimeout(60))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        
        if (builder.Environment.IsDevelopment())
            options.EnableSensitiveDataLogging();
    });

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<ISecurityProvider, SecurityProvider>();

// Image storage: R2 (S3) veya local fallback
var r2Options = new R2Options();
builder.Configuration.GetSection(R2Options.SectionName).Bind(r2Options);
var useR2 = !string.IsNullOrWhiteSpace(r2Options.AccountId)
    && !string.IsNullOrWhiteSpace(r2Options.AccessKeyId)
    && !string.IsNullOrWhiteSpace(r2Options.SecretAccessKey)
    && !string.IsNullOrWhiteSpace(r2Options.PublicBaseUrl);
if (useR2)
{
    builder.Services.AddSingleton(r2Options);
    builder.Services.AddSingleton<IImageStorageService, R2ImageStorageService>();
}
else
{
    builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>();
}
builder.Services.AddSingleton<IImageUploadValidator, ImageUploadValidator>();
builder.Services.AddHttpClient<IRecaptchaVerificationService, RecaptchaVerificationService>();
builder.Services.AddHttpContextAccessor();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing")
    && string.IsNullOrWhiteSpace(redisConnectionString))
    throw new InvalidOperationException("ConnectionStrings:Redis must be set in Production for JWT blacklist support.");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddSingleton<IRedisCommunicator, RedisCommunicator.RedisCommunicator>();
}

builder.Services.AddMyIndustryCors(builder.Configuration, builder.Environment);
builder.Services.AddMyIndustryRateLimiting(builder.Configuration);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddApiVersioning(config =>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader();
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
    config.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyIndustry API - V1", Version = "v1" });
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath, true);
    c.OperationFilter<RemoveVersionFromParameter>();
    c.DocumentFilter<ReplaceVersionWithExactValueInPath>();
    c.SchemaFilter<EnumSchemaFilter>();
});

var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
var jwtSigningKey = JwtExtensions.ResolveSigningKey(builder.Configuration, builder.Environment);
var jwtIssuer = JwtExtensions.ResolveIssuer(builder.Configuration);
var tokenValidationParameters = JwtExtensions.CreateTokenValidationParameters(jwtSigningKey, jwtIssuer);
tokenValidationParameters.RoleClaimType = ClaimTypes.Role;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.Authority = identityUrl;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing");
    options.Audience = JwtExtensions.DefaultAudience;
    options.TokenValidationParameters = tokenValidationParameters;
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) return;
            var redis = context.HttpContext.RequestServices.GetService<IRedisCommunicator>();
            if (redis != null)
            {
                var blacklisted = await redis.GetCacheValueAsync<string>("jwt_blacklist:" + jti);
                if (blacklisted != null)
                    context.Fail("Token has been revoked (blacklisted).");
            }
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("type", "99")); // UserType.Admin
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMyIndustryMediatRLogging(builder.Configuration);
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(MyIndustry.ApplicationService.Handler.Seller.CreateSellerCommand.CreateSellerCommandHandler).Assembly);
    configuration.AddMyIndustryLoggingBehavior();
});

// Süresi dolan ilanları pasif yapan arka plan servisi (paketteki ilan süresi / PostDurationInDays)
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddHostedService<MyIndustry.Api.BackgroundServices.ExpiredListingsDeactivationService>();

AllowedHostsExtensions.ValidateProductionAllowedHosts(builder.Configuration, builder.Environment);

var app = builder.Build();

// Reverse proxy (Nginx, Dokploy vb.) arkasında X-Forwarded-Proto / X-Forwarded-For kullanılıyorsa
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

// Production'da HTTPS yönlendirmesi (proxy HTTPS'i sonlandırıyorsa UseForwardedHeaders ile scheme doğru gelir)
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

// Auto-create database tables and run migrations (skipped in test environment)
if (!app.Environment.IsEnvironment("Testing"))
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyIndustryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Check if we should reset the database (via environment variable)
    var resetDb = Environment.GetEnvironmentVariable("RESET_DATABASE") == "true";
    
    if (resetDb && !app.Environment.IsDevelopment())
    {
        logger.LogWarning("RESET_DATABASE is set but ignored in non-Development environment.");
        resetDb = false;
    }
    
    if (resetDb)
    {
        logger.LogWarning("RESET_DATABASE is true - Dropping and recreating all tables...");
        await db.Database.EnsureDeletedAsync();
        logger.LogInformation("Database deleted successfully.");
    }
    
    // Run pending migrations (this will also create the database if it doesn't exist)
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed. Attempting fresh database creation...");
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        logger.LogInformation("Fresh database created and migrations applied.");
    }
    
    // Seed dummy data
    await DataSeeder.SeedAsync(db);
}

app.UseCorrelationId();
app.UseSecurityHeaders();
app.UseMyIndustryRequestLogging();
app.UseMyIndustryExceptionHandling();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    }
});

// Also serve files from /tmp/uploads as a fallback for production environments
var tmpUploadsPath = "/tmp/uploads";
if (Directory.Exists(tmpUploadsPath) || !app.Environment.IsDevelopment())
{
    Directory.CreateDirectory(tmpUploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(tmpUploadsPath),
        RequestPath = "/uploads",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        }
    });
}

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
app.MapHealthChecks("/health");

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

namespace MyIndustry.Api
{
    public partial class Program;
}
