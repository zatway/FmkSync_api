using System.Security.Cryptography;
using System.Text;
using Application.DTO.Auth;
using Application.Interfaces;
using Application.Options;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler(
    IKomSyncContext context,
    IEmailSender emailSender,
    IOptions<PasswordResetOptions> options)
    : IRequestHandler<ForgotPasswordRequest, bool>
{
    public async Task<bool> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);

        if (user == null)
            return true;

        var old = context.PasswordResetTokens.Where(t => t.UserId == user.Id && t.UsedAtUtc == null);
        context.PasswordResetTokens.RemoveRange(old);

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var hash = HashToken(rawToken);

        var entity = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(options.Value.TokenLifetimeHours)
        };

        context.PasswordResetTokens.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var baseUrl = NormalizeBaseUrl(request.FrontendBaseUrl)
            ?? NormalizeBaseUrl(options.Value.FrontendBaseUrl)
            ?? "http://localhost";
        var url = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await emailSender.SendAsync(
            user.Email,
            "Восстановление пароля KomSync",
            $"<p>Ссылка для сброса пароля (действует {options.Value.TokenLifetimeHours} ч.):</p><p><a href=\"{url}\">{url}</a></p>",
            cancellationToken);

        return true;
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? NormalizeBaseUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
            return null;

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }
}
