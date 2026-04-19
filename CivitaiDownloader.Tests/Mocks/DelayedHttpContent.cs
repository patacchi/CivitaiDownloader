using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CivitaiDownloader.Tests.Mocks;

/// <summary>
/// 遅延ストリームを返すカスタム HttpContent。
/// </summary>
public class DelayedHttpContent : HttpContent
{
    private readonly DelayedStream _delayedStream;

    public DelayedHttpContent(DelayedStream delayedStream)
    {
        _delayedStream = delayedStream;
        
        // Content-Length を設定
        Headers.ContentLength = delayedStream.Length;
        
        // Content-Disposition を設定
        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = "test.zip"
        };
        Headers.ContentDisposition = contentDisposition;
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _delayedStream.Length;
        return true;
    }

    protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
    {
        return Task.CompletedTask;
    }

    protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream>(_delayedStream);
    }
}