using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Aggregate.ValueObjects;
using MyIndustry.Identity.Domain.Service;
using MyIndustry.Identity.Repository;
using RabbitMqCommunicator;
using RedisCommunicator;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
if (builder.Environment.IsDevelopment())
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICustomMessagePublisher, CustomMessageMessagePublisher>();

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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    
}).AddJwtBearer(options =>
{
    options.Authority = identityUrl;
    options.Audience = "myindustry";
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey("O'<wl]8K:1m!4g+h24R7X,HSDlv0W[b7z.`3'A$b~cPb[f('Oox|~vNz_g]&<:u"u8.ToArray()),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
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

var redisConfiguration = builder.Configuration.GetConnectionString("Redis");
var redis = ConnectionMultiplexer.Connect(redisConfiguration);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisCommunicator, RedisCommunicator.RedisCommunicator>();


var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyIndustryIdentityDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations");
    }
    
    // Seed admin user to database
    await SeedAdminUser(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
// app.UseHttpsRedirection(); // Disabled for HTTP support

app.MapControllers();
app.MapIdentityApi<ApplicationUser>();

app.Run();

// Admin kullanıcıyı veritabanına ekle
static async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    const string adminEmail = "admin@admin.com";
    const string adminPassword = "anadolu11Aa.*!";
    
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