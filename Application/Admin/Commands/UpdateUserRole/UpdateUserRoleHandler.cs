using Application.Interfaces;
using Application.Common;
using Application.Common.Exceptions;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(Guid UserId, UserRole Role) : IRequest<bool>;

public class UpdateUserRoleHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserRoleCommand, bool>
{
    public async Task<bool> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        _ = currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            return false;
        if (SystemAdminProtection.IsSystemAdminEmail(user.Email))
            throw new ForbiddenException("Нельзя изменять роль системного администратора.");

        user.Role = request.Role;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
