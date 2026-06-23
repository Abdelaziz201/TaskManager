namespace TaskManager.Applab.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string storedFileName, long sizeBytes)> SaveFileAsync(Stream fileStream, string originalFileName);
    Task DeleteFileAsync(string storedFileName);
    Stream GetFileStream(String storedFileName);
}