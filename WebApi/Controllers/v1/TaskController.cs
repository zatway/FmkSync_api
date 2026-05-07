using Application.DTO.Tasks;
using Application.Interfaces;
using Application.Tasks.Commands.UploadTaskAttachment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/task")]
[Authorize]
public class TaskController(IMediator mediator) : ControllerBase
{
    // POST api/v1/Task
    [HttpPut]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest command)
    {
        var taskId = await mediator.Send(command);
        return Ok(taskId);
    }

    // PATCH api/v1/Task/{id}
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");

        var result = await mediator.Send(command);
        return result ? NoContent() : NotFound();
    }

    // POST api/v1/Task/assign
    [HttpPost("assign")] // Уникальный путь для действия
    public async Task<IActionResult> AssignUser([FromBody] AssignUserRequest command)
    {
        var result = await mediator.Send(command);
        return result ? NoContent() : NotFound();
    }

    // POST api/v1/Task/status
    [HttpPost("status")] // Уникальный путь для действия
    public async Task<IActionResult> ChangeTaskStatus([FromBody] ChangeTaskStatusCommand command)
    {
        var result = await mediator.Send(command);
        return result ? NoContent() : NotFound();
    }

    // DELETE api/v1/Task/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await mediator.Send(new DeleteTaskRequest(id));
        return result ? NoContent() : NotFound();
    }

    // GET api/v1/Task/project/{projectId}
    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<TaskShortDto>>> GetTasksList([FromRoute] Guid projectId)
    {
        return Ok(await mediator.Send(new GetTasksListQuery(projectId)));
    }

    // GET api/v1/Task/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetailedDto>> GetTaskById([FromRoute] Guid id)
    {
        var result = await mediator.Send(new GetTaskByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("{taskId:guid}/attachments")]
    public async Task<IActionResult> UploadTaskAttachments(Guid taskId, [FromForm] List<IFormFile> files)
    {
        var result = await mediator.Send(new UploadTaskAttachmentCommand(taskId, files));
        return Ok(result);
    }

    [HttpGet("{taskId:guid}/attachments/{attachmentId:guid}/download")]
    [HttpGet("/api/v1/TaskAttachments/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadTaskAttachment(
        [FromServices] IKomSyncContext context,
        [FromServices] IFileStorage storage,
        [FromServices] ICurrentUserService currentUser,
        [FromRoute] Guid taskId,
        [FromRoute] Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var att = await context.TaskAttachments
            .AsNoTracking()
            .Include(a => a.ProjectTask)
            .ThenInclude(t => t!.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(
                a => a.Id == attachmentId && (taskId == Guid.Empty || a.ProjectTaskId == taskId),
                cancellationToken);

        if (att == null) return NotFound();

        var uid = currentUser.UserId ?? Guid.Empty;
        if (att.ProjectTask == null ||
            !Application.Common.ProjectAccessRules.UserCanViewProject(
                currentUser.Role, uid, att.ProjectTask.Project, currentUser.DepartmentId))
            return Forbid();

        var stream = await storage.OpenReadAsync(att.StoredPath, cancellationToken);
        if (stream == null) return NotFound();

        return File(stream, att.ContentType ?? "application/octet-stream", att.FileName);
    }
}