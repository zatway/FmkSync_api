using Application.Interfaces;
using Domain.Entities;

namespace Application.Common;

public static class ProjectTagsSync
{
    public static void SyncFromNames(Project project, List<string>? names, IKomSyncContext context)
    {
        if (names == null) return;

        var wanted = names
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var tag in project.Tags.ToList())
        {
            if (!wanted.Any(w => string.Equals(w, tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                context.Tags.Remove(tag);
            }
        }

        foreach (var name in wanted)
        {
            if (!project.Tags.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                var tag = new Tag { ProjectId = project.Id, Name = name };
                project.Tags.Add(tag);
                context.Tags.Add(tag);
            }
        }
    }
}
