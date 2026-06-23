using Microsoft.Extensions.Options;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Application.Settings;

namespace TaskManager.Applab.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public FileStorageService(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
        Directory.CreateDirectory(_settings.BasePath); // ensure folder exists
    }

    public async Task<(string storedFileName, long sizeBytes)> SaveFileAsync(Stream fileStream, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_settings.BasePath, storedFileName);

        using var output = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(output);

        return (storedFileName, output.Length);
    }

    public Task DeleteFileAsync(string storedFileName)
    {
        var fullPath = Path.Combine(_settings.BasePath, storedFileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Stream GetFileStream(string storedFileName)
    {
        var fullPath = Path.Combine(_settings.BasePath, storedFileName);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }
}