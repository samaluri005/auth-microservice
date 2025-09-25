using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// Very small interactive login used by the authorization code flow (dev/demo)
// In production: replace with a polished React UI hosted separately that POSTs to this endpoint (or implement cookie-less flows)
[Route("account")]
public class AccountController : Controller
{
    private readonly IUserStore _users;

    public AccountController(IUserStore users) => _users = users;

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl)
    {
        // Return a simple HTML form for demonstration (in production use React)
        var html = $@"
            <html><body>
            <h2>Login (dev)</h2>
            <form method='post' action='/account/login'>
                <input name='identifier' placeholder='phone or email' />
                <input type='hidden' name='returnUrl' value='{returnUrl}' />
                <button type='submit'>Sign in</button>
            </form>
            </body></html>";
        return Content(html, "text/html");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginPost([FromForm] string identifier, [FromForm] string? returnUrl)
    {
        // normalize identifier (simple lowercase demo; integrate libphonenumber in real app)
        var normalized = identifier?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(identifier));
        var user = await _users.FindByNormalizedIdentifierAsync(normalized)
                 ?? await _users.CreateAsync(normalized);

        // Create authentication cookie principal
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("identifier", normalized)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        // If returnUrl is empty, redirect to a simple page
        if (string.IsNullOrEmpty(returnUrl)) return Redirect("/");

        return Redirect(returnUrl);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
