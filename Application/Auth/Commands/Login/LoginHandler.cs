using Application.Interfaces;
using Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.DTO.Auth;

namespace Application.Auth.Commands.Login;

public class LoginHandler(
    IKomSyncContext context,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider
) : IRequestHandler<LoginRequest, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpper();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный логин или пароль");

        if (!user.IsApproved)
            throw new UnauthorizedAccessException("Регистрация ещё не подтверждена администратором");

        var accessToken = jwtProvider.CreateAccessToken(user);

        var refreshTokenData = jwtProvider.CreateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenData.RefreshToken,
            ExpiresAt = refreshTokenData.ExpiredTime,
            UserId = user.Id
        };

        var oldTokens = context.RefreshTokens
            .Where(t => t.UserId == user.Id && t.ExpiresAt < DateTimeOffset.UtcNow);

        context.RefreshTokens.RemoveRange(oldTokens);

        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new TokenResponse(
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAt,
            refreshToken.ExpiresAt
        );
    }
}