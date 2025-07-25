using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace DocstreamAISearch.Web.Services;

public class BrowserFileWrapper : IFormFile
{
    private readonly IBrowserFile _browserFile;

    public BrowserFileWrapper(IBrowserFile browserFile)
    {
        _browserFile = browserFile;
    }

    public string ContentType => _browserFile.ContentType;
    public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_browserFile.Name}\"";
    public IHeaderDictionary Headers => new HeaderDictionary();
    public long Length => _browserFile.Size;
    public string Name => "file";
    public string FileName => _browserFile.Name;

    public void CopyTo(Stream target)
    {
        using var stream = _browserFile.OpenReadStream();
        stream.CopyTo(target);
    }

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        using var stream = _browserFile.OpenReadStream();
        await stream.CopyToAsync(target, cancellationToken);
    }

    public Stream OpenReadStream()
    {
        return _browserFile.OpenReadStream();
    }
}
