using System;
using System.IO;
using Xunit;

/// <summary>
/// CommandLineArgs クラスの単体テスト。
/// CommandLineArgs はコマンドライン引数を解析して保持します。
/// </summary>
public class CommandLineArgsTests
{
    /// <summary>
    /// CommandLineArgsTests クラスの新しいインスタンスを初期化します。
    /// </summary>
    public CommandLineArgsTests()
    {
    }

    /// <summary>
    /// --url のみ指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithUrlOnly_ReturnsCorrectUrl()
    {
        // Arrange
        string[] args = { "--url", "https://example.com/file.zip" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --output のみ指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithOutputOnly_ReturnsCorrectOutputDirectory()
    {
        // Arrange
        string[] args = { "--output", "./downloads" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --filename のみ指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithFilenameOnly_ReturnsCorrectFilename()
    {
        // Arrange
        string[] args = { "--filename", "custom.zip" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Equal("custom.zip", result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// すべての引数を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithAllArgs_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { 
            "--url", "https://example.com/file.zip",
            "--output", "./downloads",
            "--filename", "custom.zip" 
        };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.Equal("custom.zip", result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// -h フラグを指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithHelpFlag_h_ReturnsShowHelpTrue()
    {
        // Arrange
        string[] args = { "-h" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.True(result.ShowHelp);
    }

    /// <summary>
    /// --help フラグを指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithHelpFlag_help_ReturnsShowHelpTrue()
    {
        // Arrange
        string[] args = { "--help" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.True(result.ShowHelp);
    }

    /// <summary>
    /// 引数なしの場合のデフォルト値テスト。
    /// </summary>
    [Fact]
    public void Parse_WithNoArgs_ReturnsDefaultValues()
    {
        // Arrange
        string[] args = { };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --url と --output の組み合わせテスト。
    /// </summary>
    [Fact]
    public void Parse_WithUrlAndOutput_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { 
            "--url", "https://example.com/file.zip",
            "--output", "./downloads" 
        };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --url, --output, --filename のすべてを指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithUrlOutputAndFilename_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { 
            "--url", "https://example.com/file.zip",
            "--output", "./downloads",
            "--filename", "custom.zip" 
        };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.Equal("custom.zip", result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// 大文字小文字の区別なしテスト（--URL）。
    /// </summary>
    [Fact]
    public void Parse_WithUppercaseFlag_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { "--URL", "https://example.com/file.zip" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
    }

    /// <summary>
    /// 空の引数配列テスト。
    /// </summary>
    [Fact]
    public void Parse_WithEmptyArray_ReturnsDefaultValues()
    {
        // Arrange
        string[] args = Array.Empty<string>();
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// 位置パラメータで URL を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithUrlAsPositionalArg_ReturnsCorrectUrl()
    {
        // Arrange
        string[] args = { "https://example.com/file.zip" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal("https://example.com/file.zip", result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// -y オプション指定時のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithYFlag_ReturnsAutoOverwriteTrue()
    {
        // Arrange
        string[] args = { "-y" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal(Environment.CurrentDirectory, result.OutputDirectory);
        Assert.Null(result.Filename);
        Assert.False(result.ShowHelp);
        Assert.True(result.AutoOverwrite);
    }

    /// <summary>
    /// --token オプションと --output の組み合わせテスト。
    /// </summary>
    [Fact]
    public void Parse_WithTokenAndOutput_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { "--token", "test_token", "--output", "./downloads" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal("test_token", result.Token);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --token オプションと --filename の組み合わせテスト。
    /// </summary>
    [Fact]
    public void Parse_WithTokenAndFilename_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { "--token", "test_token", "--filename", "custom.zip" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal("test_token", result.Token);
        Assert.Equal("custom.zip", result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// --token, --output, --filename のすべてを指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithTokenOutputAndFilename_ReturnsCorrectValues()
    {
        // Arrange
        string[] args = { 
            "--token", "test_token",
            "--output", "./downloads",
            "--filename", "custom.zip" 
        };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Null(result.Url);
        Assert.Equal("test_token", result.Token);
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./downloads")), result.OutputDirectory);
        Assert.Equal("custom.zip", result.Filename);
        Assert.False(result.ShowHelp);
    }

    /// <summary>
    /// 環境変数 CIVITAI_API_KEY が設定されている場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithCivitaiApiKeyEnvironmentVariable_ReturnsTokenFromEnv()
    {
        // Arrange
        string[] args = { "--url", "https://example.com/file.zip" };
        string originalEnv = Environment.GetEnvironmentVariable("CIVITAI_API_KEY");
        
        try
        {
            Environment.SetEnvironmentVariable("CIVITAI_API_KEY", "env_token_456");
            var result = CommandLineArgs.Parse(args);
            
            // Assert
            Assert.Equal("env_token_456", result.Token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CIVITAI_API_KEY", originalEnv);
        }
    }

    /// <summary>
    /// --token オプションと環境変数の両方が指定された場合のテスト（--token が優先）。
    /// </summary>
    [Fact]
    public void Parse_WithTokenAndCivitaiApiKey_ReturnsTokenFromArg()
    {
        // Arrange
        string[] args = { "--url", "https://example.com/file.zip", "--token", "arg_token_789" };
        string originalEnv = Environment.GetEnvironmentVariable("CIVITAI_API_KEY");
        
        try
        {
            Environment.SetEnvironmentVariable("CIVITAI_API_KEY", "env_token_456");
            var result = CommandLineArgs.Parse(args);
            
            // Assert
            Assert.Equal("arg_token_789", result.Token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CIVITAI_API_KEY", originalEnv);
        }
    }

    /// <summary>
    /// --output に相対パス（./target）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithRelativeOutputDirectoryDotPrefix_ReturnsResolvedPath()
    {
        // Arrange
        string[] args = { "--output", "./target" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        string expectedPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./target"));
        Assert.Equal(expectedPath, result.OutputDirectory);
    }

    /// <summary>
    /// --output にネストされた相対パス（directtarget/nest1）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithNestedRelativeOutputDirectory_ReturnsResolvedPath()
    {
        // Arrange
        string[] args = { "--output", "directtarget/nest1" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        string expectedPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "directtarget/nest1"));
        Assert.Equal(expectedPath, result.OutputDirectory);
    }

    /// <summary>
    /// --output に複数ネストされた相対パス（./nest1/nest2/target）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithDeeplyNestedRelativeOutputDirectory_ReturnsResolvedPath()
    {
        // Arrange
        string[] args = { "--output", "./nest1/nest2/target" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        string expectedPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "./nest1/nest2/target"));
        Assert.Equal(expectedPath, result.OutputDirectory);
    }

    /// <summary>
    /// --output に親ディレクトリを指す相対パス（../somedir）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithParentRelativeOutputDirectory_ReturnsResolvedPath()
    {
        // Arrange
        string[] args = { "--output", "../somedir" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        string expectedPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../somedir"));
        Assert.Equal(expectedPath, result.OutputDirectory);
    }

    /// <summary>
    /// --output に絶対パス（/home/user/target）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithAbsolutePath_ReturnsUnchangedPath()
    {
        // Arrange
        string[] args = { "--output", "/home/user/target" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        // Unix では /home/user/target、Windows では C:\home\user\target になる可能性あり
        // Path.IsPathRooted で判定するため、入力されたパスがそのまま返る
        Assert.Equal("/home/user/target", result.OutputDirectory);
    }

    /// <summary>
    /// Windows の絶対パス（C:\temp\target）を指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithWindowsAbsolutePath_ReturnsUnchangedPath()
    {
        // Arrange
        string[] args = { "--output", @"C:\temp\target" };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal(@"C:\temp\target", result.OutputDirectory);
    }

    /// <summary>
    /// --output に存在しないネストされたディレクトリパスを指定した場合のテスト。
    /// </summary>
    [Fact]
    public void Parse_WithNonExistentNestedOutputDirectory_ReturnsResolvedPath()
    {
        // Arrange
        string nonExistentPath = Path.Combine(Path.GetTempPath(), $"test_nested_{Guid.NewGuid()}", "subdir");
        string[] args = { "--output", nonExistentPath };
        
        // Act
        var result = CommandLineArgs.Parse(args);
        
        // Assert
        Assert.Equal(nonExistentPath, result.OutputDirectory);
    }
}