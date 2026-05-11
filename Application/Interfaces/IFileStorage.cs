namespace Application.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken = default);

    /// <summary>Удаляет файл с диска по относительному пути из БД; не бросает исключение, если файла нет.</summary>
    Task TryDeleteAsync(string storedPath, CancellationToken cancellationToken = default);
}

