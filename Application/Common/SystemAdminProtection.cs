using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common;

public static class SystemAdminProtection
{
    public const string SystemAdminEmail = "admin@komsync.local";
    public const string ProtectedDepartmentName = "Admin";
    public const string ProtectedPositionName = "Admin";

    public static bool IsSystemAdminEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && string.Equals(email.Trim(), SystemAdminEmail, StringComparison.OrdinalIgnoreCase);

    public static bool IsProtectedDepartmentName(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(name.Trim(), ProtectedDepartmentName, StringComparison.OrdinalIgnoreCase);

    public static bool IsProtectedPositionName(string? name) =>
        !string.IsNullOrWhiteSpace(name)
        && string.Equals(name.Trim(), ProtectedPositionName, StringComparison.OrdinalIgnoreCase);

    public static bool IsProtectedDepartment(Department department) => IsProtectedDepartmentName(department.Name);
    public static bool IsProtectedPosition(Position position) => IsProtectedPositionName(position.Name);

    public static async Task<bool> IsProtectedDepartmentIdAsync(
        IKomSyncContext context,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var name = await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == departmentId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return IsProtectedDepartmentName(name);
    }

    public static async Task<bool> IsProtectedPositionIdAsync(
        IKomSyncContext context,
        Guid positionId,
        CancellationToken cancellationToken)
    {
        var name = await context.Positions
            .AsNoTracking()
            .Where(p => p.Id == positionId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
        return IsProtectedPositionName(name);
    }
}

