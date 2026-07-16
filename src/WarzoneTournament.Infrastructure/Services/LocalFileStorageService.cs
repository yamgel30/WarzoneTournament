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

        if (!await HasValidImageMagicBytesAsync(fileStream, ct))
            throw new InvalidOperationException("El contenido del archivo no corresponde a una imagen válida.");

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

    // Reads the first 12 bytes to verify the file is actually an image (magic bytes check).
    // Resets stream position to 0 afterward so the caller can still read the full content.
    private static async Task<bool> HasValidImageMagicBytesAsync(Stream stream, CancellationToken ct)
    {
        if (!stream.CanSeek) return true; // non-seekable streams (e.g. network) skip check

        var header = new byte[12];
        var read = await stream.ReadAsync(header, 0, header.Length, ct);
        stream.Seek(0, SeekOrigin.Begin);

        if (read < 4) return false;

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
        // GIF: 47 49 46 38 (GIF8)
        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38) return true;
        // WebP: RIFF at 0 + WEBP at 8
        if (read >= 12 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50) return true;

        return false;
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
