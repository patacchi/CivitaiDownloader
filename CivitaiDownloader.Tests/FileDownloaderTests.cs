using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// FileDownloader クラスの単体テスト。
/// FileDownloader は Civitai からファイルをダウンロードし、Content-Disposition ヘッダからファイル名を抽出します。
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
    /// Content-Disposition からファイル名を抽出するテスト（クォート付き）。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithQuotedFilename_ReturnsFilename()
    {
        // Arrange
        string contentDisposition = "attachment; filename=\"animaPreviewWorkflow_v40.zip\"";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        Assert.Equal("animaPreviewWorkflow_v40.zip", result);
    }

    /// <summary>
    /// Content-Disposition からファイル名を抽出するテスト（クォートなし）。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithoutQuotedFilename_ReturnsFilename()
    {
        // Arrange
        string contentDisposition = "attachment; filename=test_file.zip";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        Assert.Equal("test_file.zip", result);
    }

    /// <summary>
    /// Content-Disposition が null の場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithNullInput_ReturnsNull()
    {
        // Arrange
        string contentDisposition = null;
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Content-Disposition が空文字の場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithEmptyInput_ReturnsNull()
    {
        // Arrange
        string contentDisposition = "";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// URL からファイル名を抽出するテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromUrl_WithValidUrl_ReturnsFilename()
    {
        // Arrange
        string url = "https://civitai.com/api/download/models/123/file.zip";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Equal("file.zip", result);
    }

    /// <summary>
    /// URL の末尾がスラッシュの場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromUrl_WithTrailingSlash_ReturnsNull()
    {
        // Arrange
        string url = "https://civitai.com/api/download/models/123/";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// URL が無効な場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromUrl_WithInvalidUrl_ReturnsNull()
    {
        // Arrange
        string url = "not-a-valid-url";
        
        // Act
        string result = FileDownloaderTests.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Null(result);
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
        string existingFilename = "test_file.zip";
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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient);

            // Act
            string result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: false);

            // Assert
            Assert.Null(result);
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
        string existingFilename = "test_file.zip";
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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var downloader = new FileDownloader(httpClient);

            // Act
            string result = await downloader.DownloadFileAsync(testUrl, testOutputDirectory, overwrite: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingFilePath, result);
            Assert.Equal("new content", File.ReadAllText(result));
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
    /// Content-Disposition からファイル名を抽出します。
    /// </summary>
    /// <param name="contentDisposition">Content-Disposition ヘッダの値。</param>
    /// <returns>抽出されたファイル名。ファイル名が見つからない場合は null。</returns>
    static string ExtractFilenameFromContentDisposition(string contentDisposition)
    {
        if (string.IsNullOrEmpty(contentDisposition))
        {
            return null;
        }

        // filename="value" の形式からファイル名を抽出
        var match = System.Text.RegularExpressions.Regex.Match(contentDisposition, @"filename=""([^""]+)""");
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        // filename=value の形式からファイル名を抽出
        match = System.Text.RegularExpressions.Regex.Match(contentDisposition, @"filename=([^\s;]+)");
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim().Trim('"');
        }

        return null;
    }

    /// <summary>
    /// URL からファイル名を抽出します。
    /// </summary>
    /// <param name="url">ファイルを含む URL。</param>
    /// <returns>抽出されたファイル名。抽出できない場合は null。</returns>
    static string ExtractFilenameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var fileName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
        }
        catch
        {
            // URL解析に失敗した場合は null を返す
        }

        return null;
    }
}