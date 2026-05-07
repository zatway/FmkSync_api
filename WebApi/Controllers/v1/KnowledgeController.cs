using Application.DTO.Knowledge;
using Application.Knowledge.Commands.CreateKnowledgeArticle;
using Application.Knowledge.Commands.DeleteKnowledgeArticle;
using Application.Knowledge.Commands.UpdateKnowledgeArticle;
using Application.Knowledge.Queries.GetKnowledgeArticleBySlug;
using Application.Knowledge.Queries.GetKnowledgeArticles;
using Application.Knowledge.Commands.UploadKnowledgeArticleAttachments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/knowledge")]
[Authorize]
public class KnowledgeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeArticleListItemDto>>> List(
        [FromQuery] Guid? projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetKnowledgeArticlesQuery(projectId), cancellationToken));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<KnowledgeArticleDetailDto>> BySlug(string slug, CancellationToken cancellationToken)
    {
        var article = await mediator.Send(new GetKnowledgeArticleBySlugQuery(slug), cancellationToken);
        return article == null ? NotFound() : Ok(article);
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeArticleDetailDto>> Create(
        [FromBody] CreateKnowledgeArticleCommand command,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        return Ok(created);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<KnowledgeArticleDetailDto>> Update(
        Guid id,
        [FromBody] UpdateKnowledgeArticleCommand command,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(command with { Id = id }, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> UploadAttachments(
        Guid id,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UploadKnowledgeArticleAttachmentsCommand(id, files), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await mediator.Send(new DeleteKnowledgeArticleCommand(id), cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
