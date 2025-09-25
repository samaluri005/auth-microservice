using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("connect")]
public class UserInfoController : Controller
{
    [Authorize]
    [HttpGet("userinfo")]
    public IActionResult UserInfo()
    {
        var sub = User.FindFirst(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject)?.Value;
        var name = User.FindFirst(OpenIddict.Abstractions.OpenIddictConstants.Claims.Name)?.Value;
        var email = User.FindFirst(OpenIddict.Abstractions.OpenIddictConstants.Claims.Email)?.Value;

        return Ok(new
        {
            sub,
            name,
            email
        });
    }
}
