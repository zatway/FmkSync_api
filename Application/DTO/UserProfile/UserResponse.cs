using Domain.Enums;

namespace Application.DTO.UserProfile;

public record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    string DepartmentName,
    string PositionName,
    bool HasAvatar);