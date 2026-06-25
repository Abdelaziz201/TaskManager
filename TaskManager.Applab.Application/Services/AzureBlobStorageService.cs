using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Application.Settings;

namespace TaskManager.Applab.Application.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IOptions<FileStorageSettings> settings)
    {
        var config = settings.Value;
        var serviceClient = new BlobServiceClient(config.StorageAccount);
        _containerClient = serviceClient.GetBlobContainerClient(config.BlobContainer);
        _containerClient.CreateIfNotExists();
    }

    public async Task<(string storedFileName, long sizeBytes)> SaveFileAsync(Stream fileStream, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var blobClient = _containerClient.GetBlobClient(storedFileName);
        await blobClient.UploadAsync(fileStream, overwrite: false);

        var properties = await blobClient.GetPropertiesAsync();
        return (storedFileName, properties.Value.ContentLength);
    }

    public async Task DeleteFileAsync(string storedFileName)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public Stream GetFileStream(string storedFileName)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        return blobClient.OpenRead();
    }
}