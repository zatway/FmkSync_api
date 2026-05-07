using Application.Common.Exceptions;
using Application.Common;
using Application.Interfaces;
using Application.DTO.Auth;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.Register;

public class RegisterHandler(
    IKomSyncContext context,
    IPasswordHasher passwordHasher,
    IMapper mapper) : IRequestHandler<RegisterRequest>
{
    public async Task Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!await context.Departments.AnyAsync(u => u.Id.ToString() == request.DepartmentId, cancellationToken))
            throw new BadRequestException("Подразделение не существует");

        var departmentId = Guid.Parse(request.DepartmentId);
        var positionId = Guid.Parse(request.PositionId);

        if (!await context.Departments.AnyAsync(u => u.Id == departmentId, cancellationToken))
            throw new BadRequestException("Подразделение не существует");

        if (!await context.Positions.AnyAsync(u => u.Id == positionId, cancellationToken))
            throw new BadRequestException("Должность не существует");
        if (await SystemAdminProtection.IsProtectedDepartmentIdAsync(context, departmentId, cancellationToken))
            throw new ForbiddenException("Регистрация в системное подразделение запрещена.");
        if (await SystemAdminProtection.IsProtectedPositionIdAsync(context, positionId, cancellationToken))
            throw new ForbiddenException("Регистрация на системную должность запрещена.");

        var user = mapper.Map<User>(request);
        user.PasswordHash = passwordHasher.Hash(request.Password);

        await context.Users.AddAsync(user, cancellationToken);

        await context.ApplicationForRegistrations.AddAsync(new ApplicationForRegistration
        {
            UserId = user.Id,
            RequestedRole = request.Role,
            Status = RegistrationApplicationStatus.Pending
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
