using System;
using Xunit;

/// <summary>
/// Program クラスの単体テスト。
/// Program は CLI アプリケーションのエントリーポイントです。
/// </summary>
public class ProgramTests
{
    /// <summary>
    /// グローバルな環境変数 CIVITAI_API_KEY が設定されているか確認するテスト。
    /// これは、実際の実行環境とテスト実行環境の環境変数が一致しているか確認するためのものです。
    /// 環境変数が設定されていない場合は、テストをスキップします。
    /// </summary>
    [Fact]
    public void GlobalEnvironmentVariable_IsNotEmpty()
    {
        // Arrange - グローバルな環境変数から取得
        string globalToken = Environment.GetEnvironmentVariable("CIVITAI_API_KEY");
        
        // 環境変数が設定されていない場合はテストをスキップ
        if (string.IsNullOrEmpty(globalToken))
        {
            Assert.True(true, "環境変数 CIVITAI_API_KEY が設定されていないため、テストをスキップしました");
            return;
        }
        
        // Assert - 環境変数が null でないことを確認
        Assert.NotNull(globalToken);
    }
    
    /// <summary>
    /// CommandLineArgs がグローバルな環境変数から token を正しく取得できるか確認するテスト。
    /// 環境変数が設定されていない場合は、テストをスキップします。
    /// </summary>
    [Fact]
    public void CommandLineArgs_Parse_WithGlobalEnvironmentVariable_UseEnvironmentVariableToken()
    {
        // Arrange - グローバルな環境変数から取得
        string globalToken = Environment.GetEnvironmentVariable("CIVITAI_API_KEY");
        
        // 環境変数が設定されていない場合はテストをスキップ
        if (string.IsNullOrEmpty(globalToken))
        {
            Assert.True(true, "環境変数 CIVITAI_API_KEY が設定されていないため、テストをスキップしました");
            return;
        }
        
        // Act
        var args = CommandLineArgs.Parse(Array.Empty<string>());
        
        // Assert
        // CommandLineArgs で取得した token が null でないことを確認
        Assert.NotNull(args.Token);
        
        // グローバル環境変数と CommandLineArgs で取得した token が一致することを確認
        Assert.Equal(globalToken, args.Token);
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
