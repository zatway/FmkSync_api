using Application.Interfaces;
using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebApi.Services;

public record SeedAdminSettings
{
    public bool Enabled { get; init; } = true;
    public string Email { get; init; } = SystemAdminProtection.SystemAdminEmail;
    public string Password { get; init; } = "Admin123!";
    public string FullName { get; init; } = "System Admin";
    public string DepartmentName { get; init; } = SystemAdminProtection.ProtectedDepartmentName;
    public string PositionName { get; init; } = SystemAdminProtection.ProtectedPositionName;
}

public class SeedAdminHostedService(
    IServiceProvider sp,
    IOptions<SeedAdminSettings> options
) : IHostedService
{
    private readonly SeedAdminSettings _settings = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return;

        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IKomSyncContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var normalized = _settings.Email.Trim().ToUpperInvariant();
        var dep = await context.Departments.FirstOrDefaultAsync(
            d => d.Name.ToLower() == _settings.DepartmentName.Trim().ToLower(),
            cancellationToken);
        if (dep == null)
        {
            dep = new Department { Name = _settings.DepartmentName };
            (context as DbContext)!.Add(dep);
        }

        var pos = await context.Positions.FirstOrDefaultAsync(
            p => p.DepartmentId == dep.Id && p.Name.ToLower() == _settings.PositionName.Trim().ToLower(),
            cancellationToken);
        if (pos == null)
        {
            pos = new Position { Name = _settings.PositionName, DepartmentId = dep.Id };
            (context as DbContext)!.Add(pos);
        }

        var existing = await context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, cancellationToken);
        if (existing != null)
        {
            existing.Role = UserRole.Admin;
            existing.IsApproved = true;
            existing.DepartmentId = dep.Id;
            existing.PositionId = pos.Id;
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        var user = new User
        {
            FullName = _settings.FullName,
            Email = _settings.Email.Trim(),
            NormalizedEmail = normalized,
            PasswordHash = hasher.Hash(_settings.Password),
            Role = UserRole.Admin,
            IsApproved = true,
            DepartmentId = dep.Id,
            PositionId = pos.Id
        };

        (context as DbContext)!.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

