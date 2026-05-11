using Application.Interfaces;

namespace WebApi.Services;

public class LocalFileStorage(IWebHostEnvironment env) : IFileStorage
{
    private readonly string _root = Path.Combine(env.ContentRootPath, "uploads");

    public async Task<string> SaveAsync(Stream content, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_root);

        var safeName = SanitizeFileName(fileName);
        var storedFileName = $"{Guid.NewGuid():N}_{safeName}";
        var storedPath = Path.Combine(_root, storedFileName);

        await using var fs = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await content.CopyToAsync(fs, cancellationToken);

        // relative path persisted in DB
        return Path.Combine("uploads", storedFileName).Replace('\\', '/');
    }

    public Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        var relative = storedPath.Replace('\\', '/');
        if (relative.StartsWith("/")) relative = relative[1..];

        var fullPath = Path.Combine(env.ContentRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task TryDeleteAsync(string storedPath, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var relative = storedPath.Replace('\\', '/');
        if (relative.StartsWith("/")) relative = relative[1..];

        var fullPath = Path.Combine(env.ContentRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // логирование на уровне вызывающего кода при необходимости
            }
        }

        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }
}

