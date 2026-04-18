using System;
using System.IO;
using System.Threading.Tasks;
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