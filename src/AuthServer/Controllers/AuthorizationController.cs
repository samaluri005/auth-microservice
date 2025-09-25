using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
// ...existing code...
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddict.Server.AspNetCore;

[Route("connect")]
public class AuthorizationController : Controller
{
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IUserStore _users;

    public AuthorizationController(IOpenIddictScopeManager scopeManager, IUserStore users)
    {
        _scopeManager = scopeManager;
        _users = users;
    }

    // Authorization endpoint (interactive). OpenIddict middleware will route here.
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.Items["OpenIddict.Server.AspNetCore.OpenIddictServerRequest"] as OpenIddict.Abstractions.OpenIddictRequest
            ?? throw new InvalidOperationException("No OpenIddict request.");

        // If the user is not authenticated, redirect to login (cookie)
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // Build a return URL so after login we come back to authorize endpoint with original query
            var returnUrl = Url.Action("Authorize", "Authorization", Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString()!));
            return Redirect($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        // User is authenticated - create claims principal for authorization response
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var user = await _users.FindByIdAsync(Guid.Parse(userId));

        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, user.Id.ToString()),
            new Claim(Claims.Name, user.DisplayName ?? user.NormalizedIdentifier),
            new Claim("identifier", user.NormalizedIdentifier)
        };

        var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Set scopes granted by default (customize)
        principal.SetScopes(new[] { Scopes.OpenId, Scopes.Email, Scopes.Profile, Scopes.OfflineAccess });
        principal.SetResources("resource_server");

        // Sign in to produce an authorization code
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // Token endpoint: OpenIddict handles token requests automatically, but token endpoint passthrough is enabled
    // so you can intercept or add custom logic if needed. For this skeleton we let OpenIddict handle it.
    [HttpPost("token")]
    public IActionResult Exchange()
    {
        // The OpenIddict middleware will process the token request.
        // If you enabled EnableTokenEndpointPassthrough(), requests reach here on success/failure as needed.
        return Forbid();
    }
}
