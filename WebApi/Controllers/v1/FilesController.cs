using Application.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/files")]
[Authorize]
public class FilesController(
    IKomSyncContext context,
    IFileStorage storage,
    ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{fileId}")]
    public async Task<IActionResult> DownloadById(string fileId, CancellationToken cancellationToken)
    {
        var uid = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var role = currentUser.Role ?? throw new UnauthorizedAccessException();

        if (!FileIdCodec.TryParse(fileId, out var prefix, out var id))
            return NotFound();

        return prefix switch
        {
            "ta" => await DownloadTaskAttachment(id, uid, role, cancellationToken),
            "pa" => await DownloadProjectAttachment(id, uid, role, cancellationToken),
            "tc" => await DownloadTaskCommentAttachment(id, uid, role, cancellationToken),
            "pc" => await DownloadProjectCommentAttachment(id, uid, role, cancellationToken),
            "ka" => await DownloadKnowledgeArticleAttachment(id, cancellationToken),
            "av" => await DownloadUserAvatar(id, cancellationToken),
            _ => NotFound()
        };
    }

    private async Task<IActionResult> DownloadTaskAttachment(Guid id, Guid uid, Domain.Enums.UserRole role, CancellationToken cancellationToken)
    {
        var att = await context.TaskAttachments
            .AsNoTracking()
            .Include(a => a.ProjectTask)
            .ThenInclude(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();
        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.ProjectTask.Project, currentUser.DepartmentId))
            return Forbid();
        return await FileFromStorage(att.StoredPath, att.ContentType, att.FileName, cancellationToken);
    }

    private async Task<IActionResult> DownloadProjectAttachment(Guid id, Guid uid, Domain.Enums.UserRole role, CancellationToken cancellationToken)
    {
        var att = await context.ProjectAttachments
            .AsNoTracking()
            .Include(a => a.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();
        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.Project, currentUser.DepartmentId))
            return Forbid();
        return await FileFromStorage(att.StoredPath, att.ContentType, att.FileName, cancellationToken);
    }

    private async Task<IActionResult> DownloadTaskCommentAttachment(Guid id, Guid uid, Domain.Enums.UserRole role, CancellationToken cancellationToken)
    {
        var att = await context.TaskCommentAttachments
            .AsNoTracking()
            .Include(a => a.TaskComment)
            .ThenInclude(c => c.Task)
            .ThenInclude(t => t.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();
        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.TaskComment.Task.Project, currentUser.DepartmentId))
            return Forbid();
        return await FileFromStorage(att.StoredPath, att.ContentType, att.FileName, cancellationToken);
    }

    private async Task<IActionResult> DownloadProjectCommentAttachment(Guid id, Guid uid, Domain.Enums.UserRole role, CancellationToken cancellationToken)
    {
        var att = await context.ProjectCommentAttachments
            .AsNoTracking()
            .Include(a => a.ProjectComment)
            .ThenInclude(c => c.Project)
            .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();
        if (!ProjectAccessRules.UserCanViewProject(role, uid, att.ProjectComment.Project, currentUser.DepartmentId))
            return Forbid();
        return await FileFromStorage(att.StoredPath, att.ContentType, att.FileName, cancellationToken);
    }

    private async Task<IActionResult> DownloadKnowledgeArticleAttachment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var att = await context.KnowledgeArticleAttachments
            .AsNoTracking()
            .Include(a => a.Article)
            .ThenInclude(ar => ar.Project)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();
        await KnowledgeLinkValidation.EnsureArticleVisibleAsync(context, currentUser, att.Article, cancellationToken);
        return await FileFromStorage(att.StoredPath, att.ContentType, att.FileName, cancellationToken);
    }

    private async Task<IActionResult> DownloadUserAvatar(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Avatar })
            .FirstOrDefaultAsync(cancellationToken);
        if (user?.Avatar == null || user.Avatar.Length == 0)
            return NotFound();
        return File(user.Avatar, "image/jpeg");
    }

    private async Task<IActionResult> FileFromStorage(
        string storedPath,
        string? contentType,
        string fileName,
        CancellationToken cancellationToken)
    {
        var stream = await storage.OpenReadAsync(storedPath, cancellationToken);
        if (stream == null) return NotFound();
        return File(stream, contentType ?? "application/octet-stream", fileName);
    }
}

