using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Analytics;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Analytics.Queries.GetAnalyticsDashboard;

public class GetAnalyticsDashboardHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardDto>
{
    public async Task<AnalyticsDashboardDto> Handle(
        GetAnalyticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role;

        IQueryable<Domain.Entities.Project> visible = context.Projects.Where(p => !p.IsArchived);
        visible = visible.WhereUserCanSeeProject(role, uid, currentUser.DepartmentId);

        var visibleIds = await visible.Select(p => p.Id).ToListAsync(cancellationToken);

        if (request.ProjectId.HasValue)
        {
            if (!visibleIds.Contains(request.ProjectId.Value))
                throw new ForbiddenException("Нет доступа к этому проекту или проект не найден.");
            visibleIds = new List<Guid> { request.ProjectId.Value };
        }

        if (visibleIds.Count == 0)
        {
            return new AnalyticsDashboardDto(0, 0, Array.Empty<StatusCountDto>(), Array.Empty<UserLoadDto>());
        }

        var today = DateTime.UtcNow.Date;

        var openTasks = await context.Tasks
            .AsNoTracking()
            .Where(t => visibleIds.Contains(t.ProjectId) && !t.StatusColumn.IsDoneColumn)
            .Include(t => t.StatusColumn)
            .Include(t => t.Assignee)
            .ToListAsync(cancellationToken);

        var openCount = openTasks.Count;
        var overdueCount = openTasks.Count(t => t.Deadline != null && t.Deadline.Value.Date < today);

        var byStatus = openTasks
            .GroupBy(t => t.StatusColumn.Name)
            .Select(g => new StatusCountDto(g.Key, g.Count()))
            .ToList();

        var topAssignees = openTasks
            .Where(t => t.AssigneeId != null && t.Assignee != null)
            .GroupBy(t => new { t.AssigneeId, t.Assignee!.FullName })
            .Select(g => new UserLoadDto(g.Key.AssigneeId!.Value, g.Key.FullName, g.Count()))
            .Where(u => !SystemUserDisplayName.IsSeededSystemAdmin(u.FullName))
            .OrderByDescending(x => x.ActiveTaskCount)
            .Take(10)
            .ToList();

        return new AnalyticsDashboardDto(openCount, overdueCount, byStatus, topAssignees);
    }
}
