using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Tasks;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tasks.Queries.GetTasksList;

public class GetTasksListHandler(IKomSyncContext context, IMapper mapper, ICurrentUserService currentUser)
    : IRequestHandler<GetTasksListQuery, List<TaskShortDto>>
{
    public async Task<List<TaskShortDto>> Handle(GetTasksListQuery request, CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            throw new NotFoundException("Проект не найден");

        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, uid, project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к проекту");

        var tasks = await context.Tasks
            .AsNoTracking()
            .Where(x => x.ProjectId == request.ProjectId)
            .Include(t => t.Project)
            .Include(t => t.StatusColumn)
            .Include(t => t.Assignee)
            .Include(t => t.Responsible)
            .Include(t => t.Tags)
            .OrderBy(t => t.StatusColumn.SortOrder)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.TaskNumber)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<TaskShortDto>>(tasks);
    }
}
