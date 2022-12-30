using Ecommerce.Core.Enums;
using Ecommerce.Infrastructure.Jwt;
using Ecommerce.Infrastructure.Payment;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Api.Dtos.Authentication;
using Ecommerce.Infrastructure.EmailSender;
using Ecommerce.Infrastructure.EmailSender.Models;
using Ecommerce.Infrastructure.EmailSender.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Api.Controllers;

public class AuthenticateController : ApiControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailService;
    public AuthenticateController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IStripeService stripeService,
        IEmailSender emailService,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _stripeService = stripeService;
        _emailService = emailService;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        ApplicationUser userExist = await _userManager.FindByEmailAsync(registerRequest.Email);

        if (userExist != null) return BadRequest("User already exist");

        ApplicationUser user = new()
        {
            Email = registerRequest.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerRequest.UserName,
            PhoneNumber = registerRequest.PhoneNumber,
        };

        var customer = await _stripeService.CreateCustomerToken(user);
        user.CustomerId = customer.Id;

        IdentityResult result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded) return BadRequest("User creation failed! Please try again");

        await _userManager.AddToRoleAsync(user, UserRoles.User);

        await SendMailToConfirmEmail(user);

        return Ok("Check you mail message and confirm your email");
    }

    [HttpGet("confirm-email", Name = "ConfirmEmail")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ConfirmEmail(string token, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null) return NotFound("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded) return BadRequest("Could not confirm the email");

        return Ok("Email confirm successfully");
    }

    [HttpPost("register-admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest model)
    {
        ApplicationUser userExist = await _userManager.FindByEmailAsync(model.Email);

        if (userExist != null) return BadRequest("User already exist");

        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.UserName
        };

        var customer = await _stripeService.CreateCustomerToken(user);

        user.CustomerId = customer.Id;

        IdentityResult result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) return BadRequest("Error occurred creating the user");

        await _userManager.AddToRoleAsync(user, UserRoles.Admin);

        await SendMailToConfirmEmail(user);

        return Ok("User created successfully");
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null) return Unauthorized();

        bool isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

        if (!isEmailConfirmed)
        {
            await SendMailToConfirmEmail(user);

            return BadRequest("You need to confirm your email. Check your mail to confirm");
        }

        var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

        if (!signInResult.Succeeded) return BadRequest("Incorrect password");

        string accessToken = await _tokenService.CreateToken(user);

        string RefreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken = RefreshToken;

        user.RefreshTokenExpireTime = DateTime.Now.AddHours(3);

        await _userManager.UpdateAsync(user);

        return Ok(new AuthenticateResponse(AccessToken: accessToken, RefreshToken: RefreshToken));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();

        return Ok("Closed session");
    }

    private async Task SendMailToConfirmEmail(ApplicationUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user); 

        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authenticate", new { token, email = user.Email}, Request.Scheme);

        var mailRequest = await CreateMailRequest(user, confirmationLink!);

        await _emailService.SendAsync(mailRequest);
    }

    private async Task<MailRequest> CreateMailRequest(ApplicationUser user, string confirmationLink)
    {
        var emailConfirmationMailModel = new EmailConfirmationMailModel(User: user, ConfirmationLink: confirmationLink);

        string template = await _emailService.GetTemplate(((int)MailTemplates.EmailConfirmation));

        string compiledTemplate = await _emailService.GetCompiledTemplateAsync(template, emailConfirmationMailModel);

        return new MailRequest(
            Body: compiledTemplate,
            Subject: "Email confirmation",
            Email: user.Email
        );
    }
}