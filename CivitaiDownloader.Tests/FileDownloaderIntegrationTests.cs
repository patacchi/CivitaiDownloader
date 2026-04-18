using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// FileDownloader クラスの統合テスト。
/// 実際のネットワークアクセスで Civitai からファイルをダウンロードします。
/// このテストはネットワークに依存するため、手動実行を推奨します。
/// </summary>
public class FileDownloaderIntegrationTests : IDisposable
{
    /// <summary>
    /// テスト用の一時ディレクトリ。
    /// </summary>
    private readonly string _tempDirectory;

    /// <summary>
    /// FileDownloaderIntegrationTests クラスの新しいインスタンスを初期化します。
    /// 一時ディレクトリを作成します。
    /// </summary>
    public FileDownloaderIntegrationTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"Civitai_Integration_{Path.GetRandomFileName()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// テスト終了時に一時ディレクトリとその中のすべてのファイルを削除します。
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // 削除に失敗した場合は無視
            }
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Civitai から実際にファイルをダウンロードする統合テスト。
    /// </summary>
    /// <remarks>
    /// このテストは実際のネットワークアクセスを行います。
    /// テスト実行にはインターネット接続が必要です。
    /// テストURLは TestConstants.CivitaiDownloadUrl で定義されています。
    /// </remarks>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    public async Task DownloadFileFromCivitaiAsync()
    {
        // Arrange - テスト用の URL（TestConstants から取得）
        string testUrl = TestConstants.CivitaiDownloadUrl;
        string expectedFilename = TestConstants.ExpectedFilename;

        // Act - FileDownloader を使用してファイルをダウンロード
        var downloader = new FileDownloader();
        string downloadedFilePath = await downloader.DownloadFileAsync(testUrl, _tempDirectory);

        // Assert - ダウンロードされたファイルが存在することを確認
        Assert.NotNull(downloadedFilePath);
        Assert.True(File.Exists(downloadedFilePath));
        Assert.Equal(expectedFilename, Path.GetFileName(downloadedFilePath));
    }

    /// <summary>
    /// 無効な URL でのテスト（エラーハンドリング）。
    /// </summary>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    public async Task DownloadFileFromInvalidUrlAsync()
    {
        // Arrange
        string invalidUrl = "https://invalid-url-that-does-not-exist.com/file.zip";
        var downloader = new FileDownloader();

        // Act
        string result = await downloader.DownloadFileAsync(invalidUrl, _tempDirectory);

        // Assert - 無効な URL の場合は null を返す
        Assert.Null(result);
    }
}