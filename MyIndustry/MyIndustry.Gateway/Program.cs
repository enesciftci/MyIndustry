using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Gateway.Handlers;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Configuration.AddJsonFile($"ocelot.{environmentName}.json", optional: false);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOcelot().AddDelegatingHandler<AuthDelegatingHandler>()
;
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        corsBuilder =>
        {
            corsBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
var authenticationProviderKey = "Bearer";

builder.Services.AddAuthentication()
    .AddJwtBearer(authenticationProviderKey, x =>
    {
        x.Authority = identityUrl;
        x.RequireHttpsMetadata = false;
        x.Authority = identityUrl;
        x.Audience = "myindustry";
        x.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey("O'<wl]8K:1m!4g+h24R7X,HSDlv0W[b7z.`3'A$b~cPb[f('Oox|~vNz_g]&<:u"u8.ToArray()),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };
        
    });
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
await app.UseOcelot();
app.UseHttpsRedirection();

app.Run();