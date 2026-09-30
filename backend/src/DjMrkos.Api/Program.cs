using DjMrkos.Api.Endpoints;
using DjMrkos.Api.Middleware;
using DjMrkos.Api.Security;
using DjMrkos.Application;
using DjMrkos.Infrastructure;
using DjMrkos.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "Frontend";

builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyOptions.SectionName));
builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection(FrontendOptions.SectionName));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

// RequireAuthorization(schemeName) below looks up a *policy* by that name — this is the policy,
// not just the scheme registration above, and it's what actually enforces "must present a valid key".
builder.Services.AddAuthorization(options =>
    options.AddPolicy(ApiKeyAuthenticationHandler.SchemeName, policy =>
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName).RequireAuthenticatedUser()));

var allowedOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
    policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DJ MrKos API", Version = "v1" });
    options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key del panel admin.",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiKeyAuthenticationHandler.SchemeName } },
            []
        },
    });
});

var app = builder.Build();

// In production the API sits behind Caddy (TLS termination + reverse proxy) in the same
// Docker network — Caddy is never on loopback, so the default KnownNetworks/KnownProxies
// restriction would silently ignore its X-Forwarded-* headers. Without this, the app always
// sees plain-HTTP requests and UseHttpsRedirection below would redirect every single API call.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapMenuEndpoints();
app.MapModulesEndpoints();
app.MapCategoriesEndpoints();
app.MapEventsEndpoints();
app.MapSongRequestsEndpoints();
app.MapTestimonialsEndpoints();
app.MapLeadsEndpoints();
app.MapPromotionsEndpoints();

app.MapHub<SongRequestHub>("/hubs/song-requests");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestampUtc = DateTimeOffset.UtcNow }))
    .WithTags("Health")
    .AllowAnonymous();

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
