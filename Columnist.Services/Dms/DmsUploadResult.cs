namespace Columnist.Services.Dms;

public sealed record DmsUploadResult(bool Success, string? FileName, string? Error)
{
    public static DmsUploadResult Ok(string fileName) => new(true, fileName, null);
    public static DmsUploadResult Fail(string error) => new(false, null, error);
}
