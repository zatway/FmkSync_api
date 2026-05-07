using Application.DTO.TaskComments;
using Application.TaskComments.Commands.UploadTaskCommentAttachments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/taskComments")]
[Authorize]
public class TaskCommentsController(IMediator mediator) : ControllerBase
{
    // POST api/v1/Task
    [HttpPut]
    public async Task<IActionResult> Add([FromBody] AddTaskCommentRequest command)
    {
        var commentId = await mediator.Send(command);
        return Ok(commentId);
    }

    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateTaskCommentRequest command)
    {
        var result = await mediator.Send(command);
        return result ? NoContent() : NotFound();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTaskCommentRequest(id));
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{commentId:guid}/attachments")]
    public async Task<IActionResult> UploadAttachments([FromRoute] Guid commentId, [FromForm] List<IFormFile> files)
    {
        var result = await mediator.Send(new UploadTaskCommentAttachmentsCommand(commentId, files));
        return Ok(result);
    }
}