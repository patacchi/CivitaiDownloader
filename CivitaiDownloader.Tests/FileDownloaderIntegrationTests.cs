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
    /// テスト実行する場合は、[Fact] 行のコメントアウトを解除、[Fact(Skip)...]行をコメントした上で使用して下さい。
    /// </remarks>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    // [Fact]
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
    /// <remarks>
    /// テスト実行する場合は、[Fact] 行のコメントアウトを解除、[Fact(Skip)...]行をコメントした上で使用して下さい。
    /// </remarks>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    // [Fact]
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

    /// <summary>
    /// Civitai から実際にファイルをダウンロードし、進捗報告が1000ms間隔で行われることをテストします。
    /// </summary>
    /// <remarks>
    /// このテストは実際のネットワークアクセスを行います。
    /// テスト実行にはインターネット接続が必要です。
    /// テストURLは TestConstants.CivitaiDownloadUrl で定義されています。
    /// テスト実行する場合は、[Fact] 行のコメントアウトを解除、[Fact(Skip)...]行をコメントした上で使用して下さい。
    /// </remarks>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    // [Fact]
    public async Task DownloadFileFromCivitai_WithProgress_ReportsAt1000msIntervals()
    {
        // Arrange
        string testUrl = TestConstants.CivitaiDownloadUrl;
        var reportedProgress = new System.Collections.Generic.List<(double progress, long downloaded, long total, DateTime timestamp)>();

        // Act
        var downloader = new FileDownloader();
        var progress = new Progress<(double progress, long downloaded, long total)>(report =>
        {
            reportedProgress.Add((report.progress, report.downloaded, report.total, DateTime.Now));
        });

        string downloadedFilePath = await downloader.DownloadFileAsync(testUrl, _tempDirectory, progress: progress);

        // Assert
        Assert.NotNull(downloadedFilePath);
        Assert.True(File.Exists(downloadedFilePath));

        // 進捗報告が行われたことを確認
        Assert.True(reportedProgress.Count > 0, "進捗が報告されませんでした");

        // 進捗報告が1000ms間隔で行われたことを確認
        if (reportedProgress.Count > 1)
        {
            for (int i = 1; i < reportedProgress.Count; i++)
            {
                var timeDiff = (reportedProgress[i].timestamp - reportedProgress[i - 1].timestamp).TotalMilliseconds;
                Assert.True(timeDiff >= 900, $"進捗報告間隔が1000ms未満です: {timeDiff}ms (index: {i})");
            }
        }
    }

    /// <summary>
    /// Civitai から実際にファイルをダウンロードし、進捗報告が100%で終了することをテストします。
    /// </summary>
    /// <remarks>
    /// このテストは実際のネットワークアクセスを行います。
    /// テスト実行にはインターネット接続が必要です。
    /// テストURLは TestConstants.CivitaiDownloadUrl で定義されています。
    /// テスト実行する場合は、[Fact] 行のコメントアウトを解除、[Fact(Skip)...]行をコメントした上で使用して下さい。
    /// </remarks>
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    // [Fact]
    public async Task DownloadFileFromCivitai_WithProgress_FinishesAt100Percent()
    {
        // Arrange
        string testUrl = TestConstants.CivitaiDownloadUrl;
        var reportedProgress = new System.Collections.Generic.List<(double progress, long downloaded, long total)>();

        // Act
        var downloader = new FileDownloader();
        var progress = new Progress<(double progress, long downloaded, long total)>(report =>
        {
            reportedProgress.Add(report);
        });

        string downloadedFilePath = await downloader.DownloadFileAsync(testUrl, _tempDirectory, progress: progress);

        // Assert
        Assert.NotNull(downloadedFilePath);
        Assert.True(File.Exists(downloadedFilePath));

        // 最終報告が100%であることを確認
        var finalReport = reportedProgress[reportedProgress.Count - 1];
        Assert.Equal(1.0, finalReport.progress, 2);
    }
}
