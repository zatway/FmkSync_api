using Application.Common;

using Application.Common.Exceptions;

using Application.DTO.Knowledge;

using Application.Interfaces;

using Domain.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;



namespace Application.Knowledge.Commands.UpdateKnowledgeArticle;



public class UpdateKnowledgeArticleHandler(

    IKomSyncContext context,

    ICurrentUserService currentUser)

    : IRequestHandler<UpdateKnowledgeArticleCommand, KnowledgeArticleDetailDto?>

{

    public async Task<KnowledgeArticleDetailDto?> Handle(

        UpdateKnowledgeArticleCommand request,

        CancellationToken cancellationToken)

    {

        var role = currentUser.Role;

        if (role is not UserRole.Admin and not UserRole.Manager)

            throw new ForbiddenException("Редактирование статей доступно администраторам и руководителям");



        var article = await context.KnowledgeArticles

            .Include(a => a.Author)

            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);



        if (article == null) return null;



        Guid? newPid = article.ProjectId;

        if (request.ScopeChanged)
        {
            newPid = await KnowledgeLinkValidation.ValidateProjectScopeAsync(
                context, currentUser, request.ProjectId, cancellationToken);
        }



        var finalParentId = request.ParentChanged ? request.ParentId : article.ParentId;



        if (request.ParentChanged && request.ParentId.HasValue && request.ParentId.Value == article.Id)

            throw new BadRequestException("Статья не может быть родителем самой себя");



        await KnowledgeLinkValidation.ValidateParentScopeAsync(
            context, finalParentId, newPid, cancellationToken);



        if (!string.IsNullOrWhiteSpace(request.Title))

            article.Title = request.Title.Trim();



        if (request.Slug != null)

        {

            var baseSlug = SlugHelper.FromTitle(request.Slug);

            var slug = baseSlug;

            var n = 0;

            while (await context.KnowledgeArticles.AnyAsync(

                       x => x.Slug == slug && x.Id != article.Id,

                       cancellationToken))

            {

                n++;

                slug = $"{baseSlug}-{n}";

            }



            article.Slug = slug;

        }



        if (request.ContentMarkdown != null)

            article.ContentMarkdown = request.ContentMarkdown;



        if (request.ParentChanged)

            article.ParentId = request.ParentId;



        if (request.SortOrder.HasValue)

            article.SortOrder = request.SortOrder.Value;



        if (request.ScopeChanged)
        {
            article.ProjectId = newPid;
            article.ProjectTaskId = null;
        }



        article.UpdateTimestamp();

        await context.SaveChangesAsync(cancellationToken);



        var saved = await context.KnowledgeArticles

            .AsNoTracking()

            .Include(a => a.Author)

            .Include(a => a.Project)

            .FirstAsync(a => a.Id == article.Id, cancellationToken);



        return KnowledgeArticleDtoFactory.ToDetailDto(saved);

    }

}


