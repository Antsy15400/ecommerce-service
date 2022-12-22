using System.Security.Claims;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Models;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Authorize]
public class TokenController : ApiControllerBase 
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenController(
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    [HttpPost("refresh",Name = "refresh")]
    public async Task<ActionResult> RefreshToken([FromBody] ApiToken apiToken)
    {
        if (apiToken is null) return BadRequest("Invalid apiToken");

        ClaimsPrincipal userPrincipal = _tokenService.GetPrincipalsFromExpireToken(apiToken.AccessToken);

        string userId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        ApplicationUser user = await _userManager.FindByIdAsync(userId);

        if (user is null || user.RefreshToken != apiToken.RefreshToken || user.RefreshTokenExpireTime <= DateTime.Now)
            return BadRequest("User null or refreshtoken diferent or token is expire");

        string newAccessToken = await _tokenService.CreateToken(new UserBase{Email = user.Email});
        string newRefreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpireTime = DateTime.Now.AddHours(3);

        await _userManager.UpdateAsync(user);

        return Ok(new AuthenticateResponse(RefreshToken: newRefreshToken, AccessToken: newAccessToken));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        ClaimsPrincipal userPrincipal = HttpContext.User;
        string userEmail = userPrincipal.FindFirst(ClaimTypes.Email)!.Value;

        ApplicationUser currentUser = await _userManager.FindByEmailAsync(userEmail);

        if (currentUser is null) return BadRequest();

        currentUser.RefreshToken = null;

        await _userManager.UpdateAsync(currentUser);

        return NoContent();
    }
}