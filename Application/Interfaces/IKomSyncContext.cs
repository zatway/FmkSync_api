using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces;

public interface IKomSyncContext
{
    DatabaseFacade Database { get; }
    DbSet<User> Users { get; }
    DbSet<ApplicationForRegistration> ApplicationForRegistrations { get; }
    DbSet<Category> Categories { get; }
    DbSet<Position> Positions { get; }
    DbSet<Department> Departments { get; }
    DbSet<ProjectComment> ProjectComments { get; }
    DbSet<ProjectHistory> ProjectHistories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectTask> Tasks { get; }
    DbSet<ProjectTaskStatusColumn> ProjectTaskStatusColumns { get; }
    DbSet<ProjectTaskWatcher> ProjectTaskWatchers { get; }
    DbSet<TaskCommentAttachment> TaskCommentAttachments { get; }
    DbSet<ProjectCommentAttachment> ProjectCommentAttachments { get; }
    DbSet<TaskHistory> TaskHistories { get; }
    DbSet<TaskComment> TaskComments { get; }
    DbSet<KnowledgeArticle> KnowledgeArticles { get; }
    DbSet<KnowledgeArticleAttachment> KnowledgeArticleAttachments { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<TaskAttachment> TaskAttachments { get; }
    DbSet<ProjectAttachment> ProjectAttachments { get; }
    DbSet<DeadlineReminderLog> DeadlineReminderLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}