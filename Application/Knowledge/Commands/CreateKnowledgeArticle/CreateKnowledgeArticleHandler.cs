using Application.Common;
using Application.Common.Exceptions;
using Application.DTO.Knowledge;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Knowledge.Commands.CreateKnowledgeArticle;

public class CreateKnowledgeArticleHandler(
    IKomSyncContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateKnowledgeArticleCommand, KnowledgeArticleDetailDto>
{
    public async Task<KnowledgeArticleDetailDto> Handle(
        CreateKnowledgeArticleCommand request,
        CancellationToken cancellationToken)
    {
        var role = currentUser.Role;
        if (role is not UserRole.Admin and not UserRole.Manager)
            throw new ForbiddenException("Создание статей доступно администраторам и руководителям");

        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
        var projectId = await KnowledgeLinkValidation.ValidateProjectScopeAsync(
            context,
            currentUser,
            request.ProjectId,
            cancellationToken);

        await KnowledgeLinkValidation.ValidateParentScopeAsync(
            context,
            request.ParentId,
            projectId,
            cancellationToken);

        var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.FromTitle(request.Title)
            : SlugHelper.FromTitle(request.Slug);
        var slug = baseSlug;
        var n = 0;
        while (await context.KnowledgeArticles.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            n++;
            slug = $"{baseSlug}-{n}";
        }

        var entity = new KnowledgeArticle
        {
            Title = request.Title.Trim(),
            Slug = slug,
            ContentMarkdown = request.ContentMarkdown ?? "",
            ParentId = request.ParentId,
            AuthorId = userId,
            SortOrder = request.SortOrder ?? 0,
            ProjectId = projectId,
            ProjectTaskId = null
        };

        context.KnowledgeArticles.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var saved = await context.KnowledgeArticles
            .AsNoTracking()
            .Include(a => a.Author)
            .Include(a => a.Project)
            .Include(a => a.Attachments)
            .FirstAsync(a => a.Id == entity.Id, cancellationToken);

        return KnowledgeArticleDtoFactory.ToDetailDto(saved);
    }
}


