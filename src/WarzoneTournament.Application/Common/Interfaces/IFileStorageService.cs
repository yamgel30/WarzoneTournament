namespace WarzoneTournament.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
    Task<Stream> GetFileAsync(string fileUrl, CancellationToken ct = default);
    bool IsValidImageFile(string fileName, string contentType);
    long MaxFileSizeBytes { get; }
}
