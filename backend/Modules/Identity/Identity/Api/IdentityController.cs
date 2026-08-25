using Identity.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api;

public sealed record RegisterRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record AccessTokenResponse(string AccessToken);

[ApiController]
[Route("api/identity")]
public sealed class IdentityController(
    UserManager<ApplicationUser> userManager,
    JwtTokenFactory tokenFactory) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokenResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        await userManager.AddToRoleAsync(user, "explorer");

        return new AccessTokenResponse(tokenFactory.CreateToken(user, await userManager.GetRolesAsync(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        return new AccessTokenResponse(tokenFactory.CreateToken(user, await userManager.GetRolesAsync(user)));
    }
}
