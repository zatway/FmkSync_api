namespace Application.Common;

public static class FileIdCodec
{
    public static string TaskAttachment(Guid id) => $"ta:{id:D}";
    public static string ProjectAttachment(Guid id) => $"pa:{id:D}";
    public static string TaskCommentAttachment(Guid id) => $"tc:{id:D}";
    public static string ProjectCommentAttachment(Guid id) => $"pc:{id:D}";
    public static string KnowledgeArticleAttachment(Guid id) => $"ka:{id:D}";
    public static string UserAvatar(Guid userId) => $"av:{userId:D}";

    public static bool TryParse(string fileId, out string prefix, out Guid id)
    {
        prefix = string.Empty;
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(fileId)) return false;
        var parts = fileId.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return false;
        if (!Guid.TryParse(parts[1], out id)) return false;
        prefix = parts[0].ToLowerInvariant();
        return true;
    }
}

