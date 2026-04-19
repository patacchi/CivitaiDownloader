using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using CivitaiDownloader.Tests.Mocks;

/// <summary>
/// FileDownloader クラスの単体テスト。
/// FileDownloader は Civitai からファイルをダウンロードし、Content-Disposition ヘッダから正しいファイル名を抽出します。
/// ファイル名抽出と進捗フォーマットのテストは FilenameExtractorTests と ProgressFormatterTests に移行しました。
/// </summary>
public class FileDownloaderTests
{
    /// <summary>
    /// FileDownloaderTests クラスの新しいインスタンスを初期化します。
    /// </summary>
    public FileDownloaderTests()
    {
    }

    /// <summary>
    /// 同名ファイルが存在し、overwrite=false の場合、ユーザー確認プロンプトを表示してキャンセルすることをテストします。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_WithExistingFileAndNoOverwrite_CancelsDownload()
    {
        // Arrange
        string testUrl = "https://example.com/file.zip";
        string testOutputDirectory = Path.GetTempPath();
        string existingFilename = $"test_file_{Guid.NewGuid()}.zip";
        string existingFilePath = Path.Combine(testOutputDirectory, existingFilename);

        // 既存ファイルを作成
        File.WriteAllText(existingFilePath, "existing content");

        try
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("downloaded content")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = existingFilename } }
                    }
                });

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadKey(false)).Returns(ConsoleKey.N);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient, mockFileSystem.Object);

            // Act
            var result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: false);

            // Assert
            Assert.Null(result.FilePath);
            Assert.Equal(FileDownloader.DownloadStatus.Cancelled, result.Status);
        }
        finally
        {
            // 既存ファイルを削除
            if (File.Exists(existingFilePath))
            {
                File.Delete(existingFilePath);
            }
        }
    }

    /// <summary>
    /// 同名ファイルが存在し、overwrite=true の場合、自動的に上書きすることをテストします。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_WithExistingFileAndOverwrite_True_OverwritesFile()
    {
        // Arrange
        string testUrl = "https://example.com/file.zip";
        string testOutputDirectory = Path.GetTempPath();
        string existingFilename = $"test_file_{Guid.NewGuid()}.zip";
        string existingFilePath = Path.Combine(testOutputDirectory, existingFilename);

        // 既存ファイルを作成
        File.WriteAllText(existingFilePath, "existing content");

        try
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("new content")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = existingFilename } }
                    }
                });

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient, mockFileSystem.Object);

            // Act
            var result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: true);

            // Assert
            Assert.NotNull(result.FilePath);
            Assert.Equal(existingFilePath, result.FilePath);
            Assert.Equal("new content", File.ReadAllText(result.FilePath));
            Assert.Equal(FileDownloader.DownloadStatus.Success, result.Status);
        }
        finally
        {
            // 既存ファイルを削除
            if (File.Exists(existingFilePath))
            {
                File.Delete(existingFilePath);
            }
        }
    }

    /// <summary>
    /// 同名ファイルが存在し、overwrite=false でユーザーが 'y' を入力した場合、ダウンロードが実行されることをテストします。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_WithExistingFileAndUserConfirms_DownloadsFile()
    {
        // Arrange
        string testUrl = "https://example.com/file.zip";
        string testOutputDirectory = Path.GetTempPath();
        string existingFilename = $"test_file_{Guid.NewGuid()}.zip";
        string existingFilePath = Path.Combine(testOutputDirectory, existingFilename);

        // 既存ファイルを作成
        File.WriteAllText(existingFilePath, "existing content");

        try
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("new content")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = existingFilename } }
                    }
                });

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadKey(false)).Returns(ConsoleKey.Y);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient, mockFileSystem.Object);

            // Act
            var result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: false);

            // Assert
            Assert.NotNull(result.FilePath);
            Assert.Equal(existingFilePath, result.FilePath);
            Assert.Equal("new content", File.ReadAllText(result.FilePath));
            Assert.Equal(FileDownloader.DownloadStatus.Success, result.Status);
        }
        finally
        {
            // 既存ファイルを削除
            if (File.Exists(existingFilePath))
            {
                File.Delete(existingFilePath);
            }
        }
    }

    /// <summary>
    /// カスタムストリームを使用して、100%報告後に途中経過が報告されないことを確認するテスト。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_WithCustomDelayedStream_ShouldNotReportFurtherReportsAfter100Percent()
    {
        // Arrange
        var reportedProgress = new System.Collections.Generic.List<(double progress, long downloaded, long total)>();

        // カスタムストリームを作成（100ms遅延、81920バイトごとに返す）
        // Content-Length を大きく設定して、複数回の報告が発生するようにする
        int contentSize = 100000; // 100KB
        byte[] contentBytes = new byte[contentSize];
        new System.Random().NextBytes(contentBytes);

        var delayedStream = new DelayedStream(contentBytes, 100, 81920);

        // カスタム HttpContent を作成
        var customContent = new DelayedHttpContent(delayedStream);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = customContent
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var downloader = new FileDownloader(httpClient, new DefaultFileSystem());

        // Act
        string tempDir = Path.GetTempPath();
        string tempFileName = $"test_{Guid.NewGuid()}.zip";
        var result = await downloader.DownloadFileAsync(
            "https://example.com/test.zip",
            tempDir,
            customFilename: tempFileName,
            progress: new Progress<(double progress, long downloaded, long total)>(report => reportedProgress.Add(report))
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(reportedProgress.Count > 0, $"進捗が1回も報告されていません (報告回数: {reportedProgress.Count})");

        // 最後の報告が100%であることを確認
        var finalReport = reportedProgress[reportedProgress.Count - 1];
        Assert.Equal(1.0, finalReport.progress);

        // テスト用に：100%報告後に途中経過が報告されていないか確認
        bool found100Percent = false;
        foreach (var report in reportedProgress)
        {
            if (report.progress >= 1.0)
            {
                found100Percent = true;
            }
            else if (found100Percent)
            {
                // 100%報告後に途中経過が報告された場合、テスト失敗
                Assert.Fail("100%報告後に途中経過が報告されました");
            }
        }

        // 後処理：一時ファイルを削除
        string tempFilePath = Path.Combine(tempDir, tempFileName);
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }

    /// <summary>
    /// DownloadFileAsync が DownloadResult を返すことを確認するテスト。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_ReturnsDownloadResult()
    {
        // Arrange
        var delayedStream = new DelayedStream(new byte[1024], 0);
        var httpContent = new DelayedHttpContent(delayedStream);

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = httpContent
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockFileSystem = new Mock<IFileSystem>();
        mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);
        var downloader = new FileDownloader(httpClient, mockFileSystem.Object);

        string tempDir = Path.GetTempPath();
        string tempFileName = $"test_{Guid.NewGuid()}.zip";

        try
        {
            // Act
            var result = await downloader.DownloadFileAsync("https://example.com/test.zip", tempDir, customFilename: tempFileName, overwrite: false);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.FilePath);
            Assert.Equal(FileDownloader.DownloadStatus.Success, result.Status);
            Assert.Null(result.ErrorMessage);
        }
        finally
        {
            // テストファイルを削除
            string tempFilePath = Path.Combine(tempDir, tempFileName);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    /// <summary>
    /// DownloadFileAsync が Cancelled 状態を返すことを確認するテスト。
    /// </summary>
    [Fact]
    public async Task DownloadFileAsync_WithCancelledStatus_ReturnsCancelled()
    {
        // Arrange
        string testUrl = "https://example.com/file.zip";
        string testOutputDirectory = Path.GetTempPath();
        string existingFilename = $"test_file_{Guid.NewGuid()}.zip";
        string existingFilePath = Path.Combine(testOutputDirectory, existingFilename);

        // 既存ファイルを作成
        File.WriteAllText(existingFilePath, "existing content");

        try
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("downloaded content")
                    {
                        Headers = { ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = existingFilename } }
                    }
                });

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadKey(false)).Returns(ConsoleKey.N);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient, mockFileSystem.Object);

            // Act
            var result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: false);

            // Assert
            Assert.Null(result.FilePath);
            Assert.Equal(FileDownloader.DownloadStatus.Cancelled, result.Status);
            Assert.Null(result.ErrorMessage);
        }
        finally
        {
            // 既存ファイルを削除
            if (File.Exists(existingFilePath))
            {
                File.Delete(existingFilePath);
            }
        }
    }

    /// <summary>
    /// AddTokenToUrl のテスト：Token が指定されている場合、URL に token が追加されます。
    /// </summary>
    [Fact]
    public void AddTokenToUrl_WithToken_AddsTokenToUrl()
    {
        // Arrange
        string url = "https://example.com/api/download";
        string token = "test_token_123";

        // Act
        string result = FileDownloader.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download?token=test_token_123", result);
    }

    /// <summary>
    /// AddTokenToUrl のテスト：Token が null の場合、URL は変更されません。
    /// </summary>
    [Fact]
    public void AddTokenToUrl_WithNullToken_DoesNotModifyUrl()
    {
        // Arrange
        string url = "https://example.com/api/download";
        string token = null;

        // Act
        string result = FileDownloader.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download", result);
    }

    /// <summary>
    /// AddTokenToUrl のテスト：Token が空文字の場合、URL は変更されません。
    /// </summary>
    [Fact]
    public void AddTokenToUrl_WithEmptyToken_DoesNotModifyUrl()
    {
        // Arrange
        string url = "https://example.com/api/download";
        string token = "";

        // Act
        string result = FileDownloader.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download", result);
    }

    /// <summary>
    /// AddTokenToUrl のテスト：URL に既にクエリパラメータがある場合、& で token が追加されます。
    /// </summary>
    [Fact]
    public void AddTokenToUrl_WithExistingQuery_AddsTokenWithAmpersand()
    {
        // Arrange
        string url = "https://example.com/api/download?format=zip";
        string token = "test_token_123";

        // Act
        string result = FileDownloader.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download?format=zip&token=test_token_123", result);
    }

    /// <summary>
    /// AddTokenToUrl のテスト：URL に既にクエリパラメータがあり、かつ末尾が & の場合、token が追加されます。
    /// </summary>
    [Fact]
    public void AddTokenToUrl_WithQueryEndingInAmpersand_AddsTokenAfterAmpersand()
    {
        // Arrange
        string url = "https://example.com/api/download?format=zip&";
        string token = "test_token_123";

        // Act
        string result = FileDownloader.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download?format=zip&&token=test_token_123", result);
    }
}