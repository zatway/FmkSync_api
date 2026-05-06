using Application.DTO.Auth;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator, ICurrentUserService _currentUser) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest command)
    {
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest command)
    {
        var response = await mediator.Send(command); 
        return Ok(response);
    } 

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshToken)
    {
        var response = await mediator.Send(refreshToken); 
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await mediator.Send(new RevokeTokenRequest(_currentUser.UserId));
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var frontendBaseUrl = ResolveFrontendBaseUrl();
        await mediator.Send(request with { FrontendBaseUrl = frontendBaseUrl });
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await mediator.Send(request);
        return NoContent();
    }

    private string? ResolveFrontendBaseUrl()
    {
        var origin = Request.Headers.Origin.ToString();
        if (Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            return originUri.GetLeftPart(UriPartial.Authority);

        var referer = Request.Headers.Referer.ToString();
        if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return refererUri.GetLeftPart(UriPartial.Authority);

        return null;
    }
}