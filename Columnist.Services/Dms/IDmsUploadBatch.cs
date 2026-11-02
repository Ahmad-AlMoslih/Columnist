using Microsoft.AspNetCore.Http;

namespace Columnist.Services.Dms;

/// Files uploaded through a batch are deleted automatically when it is disposed,
/// unless Commit() was called. Use it with `using`.
public interface IDmsUploadBatch : IDisposable
{
    IReadOnlyList<string> FileNames { get; }
    Task<DmsUploadResult> AddAsync(IFormFile file, CancellationToken ct = default);
    void Commit();
}
