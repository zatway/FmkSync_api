using Application.Common;
using Application.Interfaces;
using Application.Organization.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organization.Queries.GetDepartmentUsers;

public class GetDepartmentUsersHandler(IKomSyncContext context)
    : IRequestHandler<GetDepartmentUsersQuery, IReadOnlyList<OrgMemberDto>>
{
    public async Task<IReadOnlyList<OrgMemberDto>> Handle(
        GetDepartmentUsersQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Departments.AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
        if (!exists)
            return Array.Empty<OrgMemberDto>();

        var list = await context.Users
            .AsNoTracking()
            .Where(u => u.DepartmentId == request.DepartmentId)
            .OrderBy(u => u.FullName)
            .Select(u => new OrgMemberDto(u.Id, u.FullName, u.Email))
            .ToListAsync(cancellationToken);
        return list.Where(u => !SystemUserDisplayName.IsSeededSystemAdmin(u.FullName)).ToList();
    }
}
