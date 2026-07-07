using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MyIndustry.Container.Extensions;
using MyIndustry.Gateway.Handlers;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureMyIndustrySerilog("MyIndustry.Gateway");

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (builder.Environment.IsDevelopment())
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Configuration.AddJsonFile($"ocelot.{environmentName}.json", optional: false);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddOcelot().AddDelegatingHandler<AuthDelegatingHandler>()
;
builder.Services.AddMyIndustryCors(builder.Configuration, builder.Environment);

var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
var authenticationProviderKey = "Bearer";

var jwtSigningKey = JwtExtensions.ResolveSigningKey(builder.Configuration, builder.Environment);
var jwtIssuer = JwtExtensions.ResolveIssuer(builder.Configuration);

var tokenValidationParameters = JwtExtensions.CreateTokenValidationParameters(jwtSigningKey, jwtIssuer);
builder.Services.AddAuthentication()
    .AddJwtBearer(authenticationProviderKey, x =>
    {
        x.Authority = identityUrl;
        x.RequireHttpsMetadata = !builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing");
        x.Audience = JwtExtensions.DefaultAudience;
        x.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddSingleton(tokenValidationParameters);

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

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseCorrelationId();
app.UseSecurityHeaders();
app.UseMyIndustryRequestLogging();
app.MapHealthChecks("/health");

try
{
    await app.UseOcelot();
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
