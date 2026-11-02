using Microsoft.AspNetCore.Http;

namespace Columnist.Services.Dms;

public sealed class DmsService : IDmsService
{
    private readonly DmsOptions _options;

    public DmsService(DmsOptions options) => _options = options;

    internal DmsOptions Options => _options;

    public IDmsUploadBatch BeginUpload(DmsFolder folder) => new DmsUploadBatch(this, folder);

    public void Delete(DmsFolder folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            return;   // nothing to delete, or not a plain file name

        var path = Path.Combine(FolderPath(folder), fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    public void Delete(DmsFolder folder, IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
            Delete(folder, fileName);
    }

    public string? GetUrl(DmsFolder folder, string? fileName) =>
        string.IsNullOrWhiteSpace(fileName) ? null : $"{_options.BaseUrl}/{folder}/{fileName}";

    internal string? Validate(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            return $"Allowed file types: {string.Join(", ", _options.AllowedExtensions)}.";
        if (file.Length <= 0)
            return "The file is empty.";
        if (file.Length > _options.MaxFileSizeBytes)
            return $"Each file can't be larger than {_options.MaxFileSizeBytes / (1024 * 1024)} MB.";
        return null;
    }

    internal static string NewFileName(IFormFile file) =>
        $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";

    internal async Task WriteAsync(DmsFolder folder, string fileName, IFormFile file, CancellationToken ct)
    {
        var directory = FolderPath(folder);
        Directory.CreateDirectory(directory);

        await using var target = new FileStream(Path.Combine(directory, fileName), FileMode.CreateNew);
        await file.CopyToAsync(target, ct);
    }

    private string FolderPath(DmsFolder folder) => Path.Combine(_options.RootPath, folder.ToString());
}
