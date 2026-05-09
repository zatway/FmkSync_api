using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Attachments;
using Application.DTO.Projects;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects.Queries.GetProjectById;

public class GetProjectByIdHandler(IKomSyncContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetProjectByIdQuery, ProjectDetailedDto>
{
    public async Task<ProjectDetailedDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .AsSplitQuery()
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks).ThenInclude(t => t.StatusColumn)
            .Include(p => p.Tasks).ThenInclude(t => t.Assignee)
            .Include(p => p.Tasks).ThenInclude(t => t.Responsible)
            .Include(p => p.Tasks).ThenInclude(t => t.Creator)
            .Include(p => p.Tasks).ThenInclude(t => t.Watchers).ThenInclude(w => w.User)
            .Include(p => p.Tags)
            .Include(p => p.Categories)
            .Include(p => p.Department)
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
            throw new NotFoundException("Проект не найден");

        var currentUserId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, currentUserId, project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к этому проекту");

        var role = currentUser.Role;
        var isAdminOrManager = ProjectAccessRules.UserCanManageProjectsAndColumns(role);
        var canCreateTasks = TaskAccessRules.UserCanCreateTasks(role);

        var tasks = project.Tasks.ToList();
        var progress = ProjectProgressCalculator.Compute(tasks);

        // Участники формально + все, кто фигурирует в задачах (исполнитель, ответственный, автор, наблюдатели) — для форм задач и блока «Команда».
        var teamMembers = new List<MemberDto>();
        var teamMemberIds = new HashSet<Guid>();
        foreach (var m in project.Members)
        {
            if (SystemUserDisplayName.IsSeededSystemAdmin(m.FullName)) continue;
            if (teamMemberIds.Add(m.Id))
                teamMembers.Add(new MemberDto(m.Id, m.FullName, m.Email, m.Role, m.Avatar != null));
        }

        void addTaskParticipant(User? u)
        {
            if (u == null || u.Id == project.OwnerId) return;
            if (SystemUserDisplayName.IsSeededSystemAdmin(u.FullName)) return;
            if (!teamMemberIds.Add(u.Id)) return;
            teamMembers.Add(new MemberDto(u.Id, u.FullName, u.Email, u.Role, u.Avatar != null));
        }

        foreach (var t in tasks)
        {
            addTaskParticipant(t.Assignee);
            addTaskParticipant(t.Responsible);
            addTaskParticipant(t.Creator);
            foreach (var w in t.Watchers)
                addTaskParticipant(w.User);
        }

        var attachments = project.Attachments
            .OrderBy(a => a.CreatedAt)
            .Select(a => new FileAttachmentDto(
                FileIdCodec.ProjectAttachment(a.Id),
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.CreatedAt))
            .ToList();

        return new ProjectDetailedDto(
            project.Id,
            project.Key,
            project.Name,
            project.Description,
            project.StartDate,
            project.DueDate,
            project.CompletedAt,
            project.CreatedAt,
            project.UpdatedAt,
            project.Color,
            project.Icon?.ToString(),
            new OwnerDto(project.OwnerId, project.Owner.FullName, project.Owner.Email, project.Owner.Role, project.Owner.Avatar != null),
            teamMembers,
            new TaskStatsDto(
                tasks.Count,
                tasks.Count(t => t.StatusColumn.SemanticKind == 0 && !TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
                tasks.Count(t => t.StatusColumn.SemanticKind == 1 && !TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
                tasks.Count(t => t.StatusColumn.SemanticKind == 2 && !TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
                tasks.Count(t => TaskStatusColumnRules.IsDoneLike(t.StatusColumn)),
                tasks.Count(t => TaskStatusColumnRules.IsBlockedLike(t.StatusColumn)),
                0
            ),
            progress,
            project.Tags.OrderBy(t => t.Name).Select(t => new ProjectTagDto(t.Id, t.Name)).ToList(),
            project.Categories.FirstOrDefault()?.Name,
            project.Department?.Name,
            project.DepartmentId,
            project.IsArchived,
            false,
            new PermissionsDto(
                CanEdit: isAdminOrManager,
                CanDelete: isAdminOrManager,
                CanManageMembers: isAdminOrManager,
                CanViewHistory: true,
                CanManageTaskColumns: isAdminOrManager,
                CanCreateTasks: canCreateTasks
            ),
            attachments,
            null
        );
    }
}
