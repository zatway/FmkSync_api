namespace Application.Common;

/// <summary>
/// Служебная учётка из сида (SeedAdmin__FullName, по умолчанию «System Admin»).
/// Не показываем в списках участников, исполнителей и подборе пользователей.
/// </summary>
public static class SystemUserDisplayName
{
    public static bool IsSeededSystemAdmin(string? fullName) =>
        !string.IsNullOrWhiteSpace(fullName)
        && string.Equals(fullName.Trim(), "System Admin", StringComparison.OrdinalIgnoreCase);
}
