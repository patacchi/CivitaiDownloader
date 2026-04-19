using System;
using System.IO;
using System.Threading.Tasks;

namespace CivitaiDownloader.Tests.Mocks;

/// <summary>
/// ReadAsync に遅延を追加するカスタムストリーム。
/// ネットワークを想定して、一定時間には一定のデータしか返さない。
/// </summary>
public class DelayedStream : MemoryStream
{
    private readonly int _delayMs;
    private readonly int _bytesPerRead;
    private long _position = 0;

    public DelayedStream(byte[] data, int delayMs = 100, int bytesPerRead = 8192)
        : base(data)
    {
        _delayMs = delayMs;
        _bytesPerRead = bytesPerRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        // 一定時間待機（ネットワーク遅延のシミュレーション）
        await Task.Delay(_delayMs, cancellationToken);
        
        // 読み込むバイト数を制限
        int bytesRead = Math.Min(count, _bytesPerRead);
        
        // 残りのデータを読み込む
        long remaining = Length - Position;
        if (remaining <= 0)
        {
            return 0; // EOF
        }
        
        // 指定されたバイト数 only (残りが少なければ残り全部)
        bytesRead = Math.Min(bytesRead, (int)remaining);
        
        // ストリームからデータを読み込む
        int read = await base.ReadAsync(buffer, offset, bytesRead, cancellationToken);
        _position += read;
        return read;
    }
}