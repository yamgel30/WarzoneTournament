using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;

namespace WarzoneTournament.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly ILogger<LocalFileStorageService> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public long MaxFileSizeBytes { get; }

    public LocalFileStorageService(IConfiguration config, ILogger<LocalFileStorageService> logger)
    {
        _uploadPath = config["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        MaxFileSizeBytes = long.TryParse(config["FileStorage:MaxFileSizeBytes"], out long max) ? max : 10 * 1024 * 1024;
        _logger = logger;

        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (!IsValidImageFile(fileName, contentType))
            throw new InvalidOperationException("Invalid file type.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var subFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var folderPath = Path.Combine(_uploadPath, subFolder);
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, uniqueFileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream, ct);

        var relativePath = Path.Combine("uploads", subFolder, uniqueFileName).Replace("\\", "/");
        _logger.LogInformation("File uploaded: {Path}", relativePath);
        return $"/{relativePath}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            var filePath = Path.Combine("wwwroot", fileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file: {Url}", fileUrl);
        }
        return Task.CompletedTask;
    }

    public Task<Stream> GetFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var filePath = Path.Combine("wwwroot", fileUrl.TrimStart('/'));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public bool IsValidImageFile(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension) &&
               AllowedMimeTypes.Contains(contentType.ToLowerInvariant());
    }
}
