namespace Columnist.Services.Dms;

public sealed class DmsOptions
{
    public required string RootPath { get; init; }          // physical folder
    public string BaseUrl { get; init; } = "/dms";
    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024;
    public int MaxFilesPerUpload { get; init; } = 10;
    public string[] AllowedExtensions { get; init; } = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];
}
