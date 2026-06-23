namespace TaskManager.Applab.Application.Settings;

public class FileStorageSettings
{
    public string BasePath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}