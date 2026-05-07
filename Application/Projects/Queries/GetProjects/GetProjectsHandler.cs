using Application.Common;
using Application.DTO.Projects;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Queries.GetProjects;

public class GetProjectsHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetProjectsQuery, List<ProjectBriefDto>>
{
    public async Task<List<ProjectBriefDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        IQueryable<Domain.Entities.Project> query = context.Projects;

        if (!request.IncludeArchived)
            query = query.Where(p => !p.IsArchived);

        query = query.WhereUserCanSeeProject(role, userId, currentUser.DepartmentId);

        var projects = await query
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks).ThenInclude(t => t.StatusColumn)
            .ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectBriefDto(
            p.Id,
            p.Key,
            p.Name,
            p.Description,
            p.OwnerId,
            p.Owner.FullName,
            p.Owner != null && p.Owner.Avatar != null,
            p.Members.Count,
            p.Tasks.Count,
            p.Tasks.Count(t => t.StatusColumn != null && !TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
            p.Tasks.Count(t => t.StatusColumn != null && TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
            ProjectProgressCalculator.Compute(p.Tasks.ToList()),
            p.DueDate,
            p.UpdatedAt,
            p.CreatedAt,
            p.UpdatedAt,
            p.Color,
            p.Icon?.ToString(),
            p.IsArchived
        )).ToList();
    }
}
