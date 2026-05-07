using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Service.TaskHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

/// <summary>
/// Основной DbContext для приложения KomSync.
/// </summary>
public class KomSyncDbContext(DbContextOptions<KomSyncDbContext> options, ICurrentUserService _currentUserService)
    : DbContext(options), IKomSyncContext
{
    DatabaseFacade IKomSyncContext.Database => Database;
    public DbSet<User> Users => Set<User>();
    public DbSet<ApplicationForRegistration> ApplicationForRegistrations => Set<ApplicationForRegistration>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ProjectComment> ProjectComments => Set<ProjectComment>();
    public DbSet<ProjectHistory> ProjectHistories => Set<ProjectHistory>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();
    public DbSet<ProjectTaskStatusColumn> ProjectTaskStatusColumns => Set<ProjectTaskStatusColumn>();
    public DbSet<ProjectTaskWatcher> ProjectTaskWatchers => Set<ProjectTaskWatcher>();
    public DbSet<TaskCommentAttachment> TaskCommentAttachments => Set<TaskCommentAttachment>();
    public DbSet<ProjectCommentAttachment> ProjectCommentAttachments => Set<ProjectCommentAttachment>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();
    public DbSet<KnowledgeArticleAttachment> KnowledgeArticleAttachments => Set<KnowledgeArticleAttachment>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<ProjectAttachment> ProjectAttachments => Set<ProjectAttachment>();
    public DbSet<DeadlineReminderLog> DeadlineReminderLogs => Set<DeadlineReminderLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KomSyncDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ignoredProperties = new[] { "Id", "CreatedAt", "UpdatedAt" };

        var projectHistories = new List<ProjectHistory>();
        var taskHistories = new List<TaskHistory>();

        foreach (var entry in ChangeTracker.Entries<Project>().Where(e => e.State == EntityState.Modified))
        {
            var projectId = entry.Entity.Id;

            foreach (var property in entry.Properties)
            {
                if (ignoredProperties.Contains(property.Metadata.Name) || !property.IsModified)
                    continue;

                var oldValue = property.OriginalValue?.ToString() ?? string.Empty;
                var newValue = property.CurrentValue?.ToString() ?? string.Empty;

                if (oldValue == newValue) continue;

                projectHistories.Add(new ProjectHistory
                {
                    ProjectId = projectId,
                    Field = property.Metadata.Name,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ChangedById = _currentUserService.UserId
                });
            }
        }

        foreach (var entry in ChangeTracker.Entries<ProjectTask>().Where(e => e.State == EntityState.Modified))
        {
            var taskId = entry.Entity.Id;

            foreach (var property in entry.Properties)
            {
                if (ignoredProperties.Contains(property.Metadata.Name) || !property.IsModified)
                    continue;

                var oldValue = property.OriginalValue?.ToString() ?? string.Empty;
                var newValue = property.CurrentValue?.ToString() ?? string.Empty;

                if (oldValue == newValue) continue;

                taskHistories.Add(new TaskHistory
                {
                    TaskId = taskId,
                    PropertyName = property.Metadata.Name,
                    OldValue = oldValue,
                    NewValue = newValue,
                    ChangedById = _currentUserService.UserId,
                    Action = TaskHistoryService.DetermineTaskHistoryAction(property.Metadata.Name)
                });
            }
        }

        string? actorDisplayName = null;
        if (_currentUserService.UserId is { } aid)
        {
            actorDisplayName = await Users.AsNoTracking()
                .Where(u => u.Id == aid)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        foreach (var h in projectHistories)
        {
            h.ChangedByDisplayName = actorDisplayName;
        }

        foreach (var h in taskHistories)
        {
            h.ChangedByDisplayName = actorDisplayName;
        }

        if (projectHistories.Count > 0)
            await ProjectHistories.AddRangeAsync(projectHistories, cancellationToken);

        if (taskHistories.Count > 0)
            await TaskHistories.AddRangeAsync(taskHistories, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }
}