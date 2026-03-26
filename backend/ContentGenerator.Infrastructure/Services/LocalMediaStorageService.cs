using ContentGenerator.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ContentGenerator.Infrastructure.Services;

public class LocalMediaStorageService : IMediaStorageService
{
    private readonly string _storagePath;
    
    public LocalMediaStorageService(IConfiguration configuration)
    {
        var basePath = configuration["MediaStorage:BasePath"] ?? "MediaStorage";
        _storagePath = Path.Combine(AppContext.BaseDirectory, basePath);
        
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveMediaAsync(Guid projectId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var projectDir = Path.Combine(_storagePath, projectId.ToString());
        if (!Directory.Exists(projectDir))
            Directory.CreateDirectory(projectDir);
            
        var filePath = Path.Combine(projectDir, fileName);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);
        
        return Path.Combine(projectId.ToString(), fileName).Replace("\\", "/");
    }

    public Task DeleteMediaAsync(string filePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_storagePath, filePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
            
        return Task.CompletedTask;
    }

    public string GetMediaUrl(string filePath)
    {
        return $"/media/{filePath}";
    }
}
