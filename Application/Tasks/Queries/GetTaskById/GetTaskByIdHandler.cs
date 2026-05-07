using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.TaskComment;
using Application.DTO.TaskHistory;
using Application.DTO.Tasks;
using Application.DTO.Attachments;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tasks.Queries.GetTaskById;

public class GetTaskByIdHandler(IKomSyncContext context, IMapper mapper, ICurrentUserService currentUser)
    : IRequestHandler<GetTaskByIdQuery, TaskDetailedDto?>
{
    public async Task<TaskDetailedDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await context.Tasks
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Project)
            .ThenInclude(p => p.Members)
            .Include(t => t.StatusColumn)
            .Include(t => t.Assignee)
            .Include(t => t.Responsible)
            .Include(t => t.Creator)
            .Include(t => t.Attachments)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Comments).ThenInclude(c => c.Attachments)
            .Include(t => t.History).ThenInclude(h => h.ChangedBy)
            .Include(t => t.Watchers).ThenInclude(w => w.User)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
            return null;

        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        if (!ProjectAccessRules.UserCanViewProject(currentUser.Role, uid, task.Project, currentUser.DepartmentId))
            throw new ForbiddenException("Нет доступа к этой задаче");

        var dto = mapper.Map<TaskDetailedDto>(task);
        dto.Key = $"{task.Project.Key}-{task.TaskNumber}";
        var commentDtos = mapper.Map<TaskCommentDto[]>(task.Comments.OrderBy(c => c.CreatedAt).ToArray());
        foreach (var c in commentDtos)
        {
            var entity = task.Comments.First(e => e.Id == c.Id);
            c.Attachments = entity.Attachments
                .OrderBy(a => a.CreatedAt)
                .Select(a => new CommentAttachmentDto(
                    FileIdCodec.TaskCommentAttachment(a.Id),
                    a.FileName,
                    a.ContentType,
                    a.SizeBytes,
                    a.CreatedAt))
                .ToArray();
        }

        var commentList = commentDtos.ToList();
        var byId = commentList.ToDictionary(c => c.Id);
        foreach (var c in byId.Values)
            c.Replies = new List<TaskCommentDto>();

        foreach (var c in commentList)
        {
            if (c.ParentCommentId.HasValue && byId.TryGetValue(c.ParentCommentId.Value, out var parent))
                parent.Replies.Add(c);
        }

        foreach (var c in byId.Values)
            c.Replies = c.Replies.OrderBy(r => r.CreatedAt).ToList();

        dto.Comments = commentList.Where(c => !c.ParentCommentId.HasValue).OrderBy(c => c.CreatedAt).ToArray();
        dto.History = mapper.Map<TaskHistoryDto[]>(task.History.OrderByDescending(h => h.ChangedAt).ToArray());
        dto.Watchers = task.Watchers
            .Where(w => w.User != null)
            .Select(w => new TaskAssigneeDto(w.User!.Id, w.User.FullName, null, w.User.Avatar != null))
            .ToList();

        dto.FileAttachments = task.Attachments
            .OrderBy(a => a.CreatedAt)
            .Select(a => new FileAttachmentDto(
                FileIdCodec.TaskAttachment(a.Id),
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.CreatedAt))
            .ToList();

        return dto;
    }
}
