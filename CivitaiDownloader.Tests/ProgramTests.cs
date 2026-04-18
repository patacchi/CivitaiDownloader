using System;
using Xunit;

/// <summary>
/// Program クラスの単体テスト。
/// Program は CLI アプリケーションのエントリーポイントです。
/// </summary>
public class ProgramTests
{
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
        string result = Program.AddTokenToUrl(url, token);

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
        string result = Program.AddTokenToUrl(url, token);

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
        string result = Program.AddTokenToUrl(url, token);

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
        string result = Program.AddTokenToUrl(url, token);

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
        string result = Program.AddTokenToUrl(url, token);

        // Assert
        Assert.Equal("https://example.com/api/download?format=zip&&token=test_token_123", result);
    }
}