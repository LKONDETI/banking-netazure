using FinCore.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Controllers
// -----------------------------------------------------------------------
builder.Services.AddControllers();

// -----------------------------------------------------------------------
// Authentication — cookie-based; claims: entity, accessid, taxid
// -----------------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FinCore.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// -----------------------------------------------------------------------
// Swagger / OpenAPI
// -----------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinCore Banking API",
        Version = "v1",
        Description = "Banking API for FinCore — GET /api/accounts endpoint"
    });
});

// -----------------------------------------------------------------------
// Application + Infrastructure services (MediatR, FluentValidation, DB2)
// -----------------------------------------------------------------------
builder.Services.AddFinCoreInfrastructure();

// -----------------------------------------------------------------------
// Build
// -----------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }
