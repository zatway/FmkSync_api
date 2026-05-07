using Application.DTO.Projects;
using Application.Interfaces;
using Application.Projects.Commands.CreateProjectTaskStatusColumn;
using Application.Projects.Commands.DeleteProjectTaskStatusColumn;
using Application.Projects.Commands.ReorderProjectTaskStatusColumns;
using Application.Projects.Commands.UpdateProjectTaskStatusColumn;
using Application.Projects.Commands.UploadProjectAttachment;
using Application.Projects.Commands.UploadProjectCommentAttachments;
using Application.Projects.Queries.GetProjectTaskStatusColumns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects([FromQuery] bool includeArchived = false)
        {
            var result = await _mediator.Send(new GetProjectsQuery(includeArchived));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var result = await _mediator.Send(new GetProjectByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("{id:guid}/task-status-columns")]
        public async Task<IActionResult> GetTaskStatusColumns(Guid id)
        {
            var result = await _mediator.Send(new GetProjectTaskStatusColumnsQuery(id));
            return Ok(result);
        }

        [HttpPut("{id:guid}/task-status-columns")]
        public async Task<IActionResult> CreateTaskStatusColumn(Guid id, [FromBody] CreateTaskStatusColumnBody body)
        {
            var colId = await _mediator.Send(new CreateProjectTaskStatusColumnCommand(
                id,
                body.Name,
                body.ColorHex,
                body.SemanticKind,
                body.IsDoneColumn,
                body.IsBlockedColumn));
            return Ok(colId);
        }

        public record CreateTaskStatusColumnBody(
            string Name,
            string? ColorHex,
            byte? SemanticKind = null,
            bool? IsDoneColumn = null,
            bool? IsBlockedColumn = null);

        [HttpPatch("{id:guid}/task-status-columns/reorder")]
        public async Task<IActionResult> ReorderTaskStatusColumns(Guid id, [FromBody] ReorderTaskStatusColumnsBody body)
        {
            await _mediator.Send(new ReorderProjectTaskStatusColumnsCommand(id, body.OrderedColumnIds));
            return NoContent();
        }

        public record ReorderTaskStatusColumnsBody(IReadOnlyList<Guid> OrderedColumnIds);

        [HttpPatch("{projectId:guid}/task-status-columns/{columnId:guid}")]
        public async Task<IActionResult> UpdateTaskStatusColumn(
            Guid projectId,
            Guid columnId,
            [FromBody] UpdateTaskStatusColumnBody body)
        {
            await _mediator.Send(new UpdateProjectTaskStatusColumnCommand(projectId, columnId, body.Name, body.ColorHex));
            return NoContent();
        }

        public record UpdateTaskStatusColumnBody(string Name, string? ColorHex);

        [HttpDelete("{projectId:guid}/task-status-columns/{columnId:guid}")]
        public async Task<IActionResult> DeleteTaskStatusColumn(
            Guid projectId,
            Guid columnId,
            [FromQuery] Guid? moveTasksToColumnId)
        {
            await _mediator.Send(new DeleteProjectTaskStatusColumnCommand(projectId, columnId, moveTasksToColumnId));
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var projectId = await _mediator.Send(request);
            return Ok(projectId);
        }

        // --- Обновить проект ---
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            var updated = await _mediator.Send(request with { Id = id });
            return updated ? NoContent() : NotFound();
        }

        // --- Удалить проект ---
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var result = await _mediator.Send(new DeleteProjectRequest(id));
            return result ? NoContent() : NotFound();
        }

        // --- История проекта ---
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetProjectHistory(Guid id)
        {
            var result = await _mediator.Send(new GetProjectHistoryQuery(id));
            return Ok(result);
        }

        // --- Комментарии ---
        [HttpPut("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateProjectCommentRequest request)
        {
            var comment = await _mediator.Send(request with { ProjectId = id });
            return Ok(comment);
        }

        [HttpPatch("comments/{id:guid}")]
        public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateProjectCommentRequest request)
        {
            var updated = await _mediator.Send(request with { Id = id });
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("comments/{id:guid}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var deleted = await _mediator.Send(new DeleteProjectCommentRequest(id));
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetComments(Guid id)
        {
            var comments = await _mediator.Send(new GetProjectCommentsQuery(id));
            return Ok(comments);
        }

        [HttpPost("comments/{id:guid}/attachments")]
        public async Task<IActionResult> UploadProjectCommentAttachments([FromRoute] Guid id, [FromForm] List<IFormFile> files)
        {
            var result = await _mediator.Send(new UploadProjectCommentAttachmentsCommand(id, files));
            return Ok(result);
        }

        [HttpGet("comment-attachments/{id:guid}")]
        [HttpGet("/api/v1/ProjectComments/attachments/{id:guid}")]
        public async Task<IActionResult> DownloadProjectCommentAttachment(
            [FromServices] IKomSyncContext context,
            [FromServices] IFileStorage storage,
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var att = await context.ProjectCommentAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

            if (att == null) return NotFound();

            var stream = await storage.OpenReadAsync(att.StoredPath, cancellationToken);
            if (stream == null) return NotFound();

            return File(stream, att.ContentType ?? "application/octet-stream", att.FileName);
        }

        [HttpPost("{id:guid}/attachments")]
        public async Task<IActionResult> UploadProjectAttachments(Guid id, [FromForm] List<IFormFile> files)
        {
            var result = await _mediator.Send(new UploadProjectAttachmentCommand(id, files));
            return Ok(result);
        }

        [HttpGet("{projectId:guid}/attachments/{attachmentId:guid}/download")]
        [HttpGet("/api/v1/ProjectAttachments/attachments/{attachmentId:guid}")]
        public async Task<IActionResult> DownloadProjectAttachment(
            [FromServices] IKomSyncContext context,
            [FromServices] IFileStorage storage,
            [FromServices] ICurrentUserService currentUser,
            [FromRoute] Guid projectId,
            [FromRoute] Guid attachmentId,
            CancellationToken cancellationToken)
        {
            var att = await context.ProjectAttachments
                .AsNoTracking()
                .Include(a => a.Project)
                .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(
                    a => a.Id == attachmentId && (projectId == Guid.Empty || a.ProjectId == projectId),
                    cancellationToken);

            if (att == null) return NotFound();

            var uid = currentUser.UserId ?? Guid.Empty;
            if (!Application.Common.ProjectAccessRules.UserCanViewProject(
                    currentUser.Role, uid, att.Project, currentUser.DepartmentId))
                return Forbid();

            var stream = await storage.OpenReadAsync(att.StoredPath, cancellationToken);
            if (stream == null) return NotFound();

            return File(stream, att.ContentType ?? "application/octet-stream", att.FileName);
        }
    }
}