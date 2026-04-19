using Xunit;

/// <summary>
/// ProgressFormatter クラスの単体テスト。
/// ProgressFormatter はバイト数のフォーマットと進捗バーの生成を行います。
/// </summary>
public class ProgressFormatterTests
{
    /// <summary>
    /// ProgressFormatterTests クラスの新しいインスタンスを初期化します。
    /// </summary>
    public ProgressFormatterTests()
    {
    }

    /// <summary>
    /// バイト数を適切な単位に変換するテスト（B単位）。
    /// </summary>
    [Fact]
    public void FormatBytes_WithBytes_ReturnsBytes()
    {
        // Arrange
        long bytes = 512;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("512 B", result);
    }

    /// <summary>
    /// バイト数を適切な単位に変換するテスト（KB単位）。
    /// </summary>
    [Fact]
    public void FormatBytes_WithKilobytes_ReturnsKB()
    {
        // Arrange
        long bytes = 1536; // 1.5 KB
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Contains("KB", result);
    }

    /// <summary>
    /// バイト数を適切な単位に変換するテスト（MB単位）。
    /// </summary>
    [Fact]
    public void FormatBytes_WithMegabytes_ReturnsMB()
    {
        // Arrange
        long bytes = 1572864; // 1.5 MB
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Contains("MB", result);
    }

    /// <summary>
    /// バイト数を適切な単位に変換するテスト（GB単位）。
    /// </summary>
    [Fact]
    public void FormatBytes_WithGigabytes_ReturnsGB()
    {
        // Arrange
        long bytes = 1610612736; // 1.5 GB
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Contains("GB", result);
    }

    /// <summary>
    /// バイト数のフォーマットで小数点以下の表示が最大3桁であることを確認するテスト。
    /// </summary>
    [Fact]
    public void FormatBytes_WithFractionalBytes_ReturnsMax3DecimalPlaces()
    {
        // Arrange
        long bytes = 1025; // 1.0009765625 KB
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("1.001 KB", result);
    }

    /// <summary>
    /// バイト数が0の場合のテスト。
    /// </summary>
    [Fact]
    public void FormatBytes_WithZero_ReturnsZeroBytes()
    {
        // Arrange
        long bytes = 0;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("0 B", result);
    }

    /// <summary>
    /// バイト数が負の場合のテスト。
    /// </summary>
    [Fact]
    public void FormatBytes_WithNegativeBytes_ReturnsNegativeBytes()
    {
        // Arrange
        long bytes = -100;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("-100 B", result);
    }

    /// <summary>
    /// バイト数の境界値テスト（1024 B = 1 KB）。
    /// </summary>
    [Fact]
    public void FormatBytes_At1024BytesBoundary_Returns1KB()
    {
        // Arrange
        long bytes = 1024;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("1 KB", result);
    }

    /// <summary>
    /// バイト数の境界値テスト（1048576 B = 1 MB）。
    /// </summary>
    [Fact]
    public void FormatBytes_At1048576BytesBoundary_Returns1MB()
    {
        // Arrange
        long bytes = 1048576;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("1 MB", result);
    }

    /// <summary>
    /// バイト数の境界値テスト（1073741824 B = 1 GB）。
    /// </summary>
    [Fact]
    public void FormatBytes_At1073741824BytesBoundary_Returns1GB()
    {
        // Arrange
        long bytes = 1073741824;
        
        // Act
        string result = ProgressFormatter.FormatBytes(bytes);
        
        // Assert
        Assert.Equal("1 GB", result);
    }

    /// <summary>
    /// 進捗バー生成のテスト（0%）。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_With0Percent_ReturnsEmptyBar()
    {
        // Arrange
        double progress = 0.0;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal("[--------------------]", result);
    }

    /// <summary>
    /// 進捗バー生成のテスト（50%）。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_With50Percent_ReturnsHalfBar()
    {
        // Arrange
        double progress = 0.5;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal("[##########----------]", result);
    }

    /// <summary>
    /// 進捗バー生成のテスト（100%）。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_With100Percent_ReturnsFullBar()
    {
        // Arrange
        double progress = 1.0;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal("[####################]", result);
    }

    /// <summary>
    /// 進捗バー生成のテスト（負の進捗は0%として扱われる）。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_WithNegativeProgress_ReturnsEmptyBar()
    {
        // Arrange
        double progress = -0.5;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal("[--------------------]", result);
    }

    /// <summary>
    /// 進捗バー生成のテスト（100%を超える進捗は100%として扱われる）。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_WithProgressOver100_ReturnsFullBar()
    {
        // Arrange
        double progress = 1.5;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal("[####################]", result);
    }

    /// <summary>
    /// 進捗バーの長さをカスタマイズするテスト。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_WithCustomWidth_ReturnsCustomBar()
    {
        // Arrange
        double progress = 0.5;
        int width = 10;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress, width);
        
        // Assert
        Assert.Equal("[#####-----]", result);
    }

    /// <summary>
    /// 進捗バーの長さが0の場合のテスト。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_WithZeroWidth_ReturnsEmptyBar()
    {
        // Arrange
        double progress = 0.5;
        int width = 0;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress, width);
        
        // Assert
        Assert.Equal("[]", result);
    }

    /// <summary>
    /// 進捗バーの長さが負の場合のテスト。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_WithNegativeWidth_ReturnsEmptyBar()
    {
        // Arrange
        double progress = 0.5;
        int width = -10;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress, width);
        
        // Assert
        Assert.Equal("[]", result);
    }

    /// <summary>
    /// 進捗バーのデフォルト幅のテスト。
    /// </summary>
    [Fact]
    public void GenerateProgressBar_DefaultWidth_Is20()
    {
        // Arrange
        double progress = 0.0;
        
        // Act
        string result = ProgressFormatter.GenerateProgressBar(progress);
        
        // Assert
        Assert.Equal(22, result.Length); // 2 brackets + 20 characters
    }
}