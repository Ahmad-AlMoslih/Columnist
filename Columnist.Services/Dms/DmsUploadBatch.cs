using Microsoft.AspNetCore.Http;

namespace Columnist.Services.Dms;

internal sealed class DmsUploadBatch : IDmsUploadBatch
{
    private readonly DmsService _dms;
    private readonly DmsFolder _folder;
    private readonly List<string> _fileNames = [];
    private bool _committed;

    public DmsUploadBatch(DmsService dms, DmsFolder folder)
    {
        _dms = dms;
        _folder = folder;
    }

    public IReadOnlyList<string> FileNames => _fileNames;

    public async Task<DmsUploadResult> AddAsync(IFormFile file, CancellationToken ct = default)
    {
        if (_fileNames.Count >= _dms.Options.MaxFilesPerUpload)
            return DmsUploadResult.Fail($"You can upload up to {_dms.Options.MaxFilesPerUpload} images at a time.");

        var error = _dms.Validate(file);
        if (error is not null)
            return DmsUploadResult.Fail(error);

        var fileName = DmsService.NewFileName(file);
        _fileNames.Add(fileName);   // tracked BEFORE writing, so a half-written file is cleaned up too
        await _dms.WriteAsync(_folder, fileName, file, ct);

        return DmsUploadResult.Ok(fileName);
    }

    public void Commit() => _committed = true;

    public void Dispose()
    {
        if (!_committed)
            _dms.Delete(_folder, _fileNames);
    }
}
