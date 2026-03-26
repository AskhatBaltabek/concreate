namespace ContentGenerator.Application.Interfaces;

public interface IMediaStorageService
{
    Task<string> SaveMediaAsync(Guid projectId, string fileName, Stream content, CancellationToken cancellationToken);
    Task DeleteMediaAsync(string filePath, CancellationToken cancellationToken);
    string GetMediaUrl(string filePath);
}
