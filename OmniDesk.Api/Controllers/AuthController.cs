using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniDesk.Application.Identity;
using OmniDesk.Application.Identity.Models;
using OmniDesk.Infrastructure.Identity;

namespace OmniDesk.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IRegistrationService _registrationService;

    public AuthController(
        IAuthenticationService authenticationService,
        IRegistrationService registrationService)
    {
        _authenticationService = authenticationService;
        _registrationService = registrationService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(
            request,
            cancellationToken);

        return Ok(result);
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registrationService.RegisterAsync(
            request,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}