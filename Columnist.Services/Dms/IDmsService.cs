namespace Columnist.Services.Dms;

public interface IDmsService
{
    IDmsUploadBatch BeginUpload(DmsFolder folder);
    void Delete(DmsFolder folder, string? fileName);
    void Delete(DmsFolder folder, IEnumerable<string> fileNames);
    string? GetUrl(DmsFolder folder, string? fileName);
}
