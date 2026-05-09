using Application.Common;
using Application.Interfaces;
using Application.Organization.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Organization.Queries.GetPositionUsers;

public class GetPositionUsersHandler(IKomSyncContext context)
    : IRequestHandler<GetPositionUsersQuery, IReadOnlyList<OrgMemberDto>>
{
    public async Task<IReadOnlyList<OrgMemberDto>> Handle(
        GetPositionUsersQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Positions.AnyAsync(p => p.Id == request.PositionId, cancellationToken);
        if (!exists)
            return Array.Empty<OrgMemberDto>();

        var list = await context.Users
            .AsNoTracking()
            .Where(u => u.PositionId == request.PositionId)
            .OrderBy(u => u.FullName)
            .Select(u => new OrgMemberDto(u.Id, u.FullName, u.Email))
            .ToListAsync(cancellationToken);
        return list.Where(u => !SystemUserDisplayName.IsSeededSystemAdmin(u.FullName)).ToList();
    }
}
