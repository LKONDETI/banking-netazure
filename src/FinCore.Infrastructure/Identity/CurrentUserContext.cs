using FinCore.Application.Services;
using Microsoft.AspNetCore.Http;

namespace FinCore.Infrastructure.Identity;

/// <summary>
/// Reads identity claims from the current HTTP context.
/// Claims are populated by cookie authentication middleware.
/// All properties are synchronous — no GetAwaiter().GetResult() anywhere.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Entity => GetClaim("entity");
    public string AccessId => GetClaim("accessid");
    public string TaxId => GetClaim("taxid");

    private string GetClaim(string claimType)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return string.Empty;

        return user.FindFirst(claimType)?.Value ?? string.Empty;
    }
}
