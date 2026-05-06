using MediatR;

namespace Application.DTO.Auth;

public record ForgotPasswordRequest(string Email, string? FrontendBaseUrl = null) : IRequest<bool>;
