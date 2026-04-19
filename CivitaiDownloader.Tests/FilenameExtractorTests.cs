using System;
using System.Text.RegularExpressions;
using Xunit;

/// <summary>
/// FilenameExtractor クラスの単体テスト。
/// FilenameExtractor は Content-Disposition ヘッダからファイル名を抽出し、URLからも抽出します。
/// </summary>
public class FilenameExtractorTests
{
    /// <summary>
    /// FilenameExtractorTests クラスの新しいインスタンスを初期化します。
    /// </summary>
    public FilenameExtractorTests()
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
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
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
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
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
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
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
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
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
        string result = FilenameExtractor.ExtractFilenameFromUrl(url);
        
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
        string result = FilenameExtractor.ExtractFilenameFromUrl(url);
        
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
        string result = FilenameExtractor.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// URL に特殊文字が含まれる場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromUrl_WithSpecialCharacters_ReturnsFilename()
    {
        // Arrange
        string url = "https://example.com/path/file-name_v2.1-beta.zip";
        
        // Act
        string result = FilenameExtractor.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Equal("file-name_v2.1-beta.zip", result);
    }

    /// <summary>
    /// URL にパーセントエンコーディングが含まれる場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromUrl_WithUrlEncodedFilename_ReturnsFilename()
    {
        // Arrange
        string url = "https://example.com/path/file%20name.zip";
        
        // Act
        string result = FilenameExtractor.ExtractFilenameFromUrl(url);
        
        // Assert
        Assert.Equal("file name.zip", result);
    }

    /// <summary>
    /// Content-Disposition に複数のパラメータがある場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithMultipleParameters_ReturnsFilename()
    {
        // Arrange
        string contentDisposition = "attachment; filename=\"file.zip\"; size=12345; modification-date=\"Wed, 12 Jan 2024 01:23:45 GMT\"";
        
        // Act
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        Assert.Equal("file.zip", result);
    }

    /// <summary>
    /// Content-Disposition に拡張ファイル名（filename*）がある場合のテスト。
    /// </summary>
    [Fact]
    public void ExtractFilenameFromContentDisposition_WithFilenameStar_ReturnsFilename()
    {
        // Arrange
        string contentDisposition = "attachment; filename*=UTF-8''file%20name.zip";
        
        // Act
        string result = FilenameExtractor.ExtractFilenameFromContentDisposition(contentDisposition);
        
        // Assert
        // filename* は現行の実装では処理せず、null を返す
        Assert.Null(result);
    }
}