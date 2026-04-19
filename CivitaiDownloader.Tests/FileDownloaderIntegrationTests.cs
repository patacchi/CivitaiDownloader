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
    /// テストで作成したディレクトリのリスト（テスト後に削除用）。
    /// </summary>
    private readonly System.Collections.Generic.List<string> _directoriesToCleanUp;

    /// <summary>
    /// FileDownloaderIntegrationTests クラスの新しいインスタンスを初期化します。
    /// 一時ディレクトリを作成します。
    /// </summary>
    public FileDownloaderIntegrationTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"Civitai_Integration_{Path.GetRandomFileName()}");
        Directory.CreateDirectory(_tempDirectory);
        _directoriesToCleanUp = new System.Collections.Generic.List<string>();
    }

    /// <summary>
    /// テスト終了時に一時ディレクトリとその中のすべてのファイルを削除します。
    /// また、テストで作成したディレクトリも削除します。
    /// </summary>
    public void Dispose()
    {
        // テストで作成したディレクトリを削除
        foreach (string dir in _directoriesToCleanUp)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    // 削除に失敗した場合は無視
                }
            }
        }

        // 一時ディレクトリを削除
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
    /// テスト用にユニークなディレクトリ名を生成します。
    /// 既に存在する場合は再試行します。
    /// </summary>
    /// <param name="name">ベース名。</param>
    /// <returns>ユニークなディレクトリパス。</returns>
    private string GenerateUniqueDirectory(string name)
    {
        string basePath = Path.Combine(_tempDirectory, name);
        string uniquePath = basePath;
        int counter = 1;

        while (Directory.Exists(uniquePath))
        {
            uniquePath = $"{basePath}_{counter}";
            counter++;
        }

        return uniquePath;
    }

    /// <summary>
    /// テストで作成したディレクトリをクリーンアップ用リストに追加します。
    /// </summary>
    /// <param name="directoryPath">ディレクトリパス。</param>
    private void AddDirectoryToCleanup(string directoryPath)
    {
        // 既に存在するディレクトリはクリーンアップ対象外
        if (!Directory.Exists(directoryPath))
        {
            _directoriesToCleanUp.Add(directoryPath);
        }
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
    // [Fact]
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    public async Task DownloadFileFromCivitaiAsync()
    {
        // Arrange - テスト用の URL（TestConstants から取得）
        string testUrl = TestConstants.CivitaiDownloadUrl;
        string expectedFilename = TestConstants.ExpectedFilename;

        // Token を取得して URL に追加
        var commandLineArgs = CommandLineArgs.Parse(Array.Empty<string>());
        string downloadUrl = FileDownloader.AddTokenToUrl(testUrl, commandLineArgs.Token);

        // Act - FileDownloader を使用してファイルをダウンロード
        var downloader = new FileDownloader();
        string downloadedFilePath = await downloader.DownloadFileAsync(downloadUrl, _tempDirectory);

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
    
    [Fact]
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
    // [Fact]
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    public async Task DownloadFileFromCivitai_WithProgress_ReportsAt1000msIntervals()
    {
        // Arrange
        string testUrl = TestConstants.CivitaiDownloadUrl;
        var reportedProgress = new System.Collections.Generic.List<(double progress, long downloaded, long total, DateTime timestamp)>();

        // Token を取得して URL に追加
        var commandLineArgs = CommandLineArgs.Parse(Array.Empty<string>());
        string downloadUrl = FileDownloader.AddTokenToUrl(testUrl, commandLineArgs.Token);

        // Act
        var downloader = new FileDownloader();
        var progress = new Progress<(double progress, long downloaded, long total)>(report =>
        {
            reportedProgress.Add((report.progress, report.downloaded, report.total, DateTime.Now));
        });

        string downloadedFilePath = await downloader.DownloadFileAsync(downloadUrl, _tempDirectory, progress: progress);

        // Assert
        Assert.NotNull(downloadedFilePath);
        Assert.True(File.Exists(downloadedFilePath));

        // 進捗報告が行われたことを確認
        Assert.True(reportedProgress.Count > 0, "進捗が報告されませんでした");

        // 進捗報告が100%で終了することを確認
        var finalReport = reportedProgress[reportedProgress.Count - 1];
        Assert.Equal(1.0, finalReport.progress, 2);

        // 進捗報告が1回のみの場合、テスト終了
        if (reportedProgress.Count == 1)
        {
            return;
        }

        // 最終報告（100%）はダウンロード完了直後なので除外して検証
        // 最初の報告から最終報告の1つ前までを検証
        if (reportedProgress.Count > 2)
        {
            for (int i = 1; i < reportedProgress.Count - 1; i++)
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
    // [Fact]
    [Fact(Skip = "ネットワークアクセスが必要なため、手動実行のみ。TestConstants.cs を修正してテストを有効にしてください。")]
    public async Task DownloadFileFromCivitai_WithProgress_FinishesAt100Percent()
    {
        // Arrange
        string testUrl = TestConstants.CivitaiDownloadUrl;
        var reportedProgress = new System.Collections.Generic.List<(double progress, long downloaded, long total)>();

        // Token を取得して URL に追加
        var commandLineArgs = CommandLineArgs.Parse(Array.Empty<string>());
        string downloadUrl = FileDownloader.AddTokenToUrl(testUrl, commandLineArgs.Token);

        // Act
        var downloader = new FileDownloader();
        var progress = new Progress<(double progress, long downloaded, long total)>(report =>
        {
            reportedProgress.Add(report);
        });

        string downloadedFilePath = await downloader.DownloadFileAsync(downloadUrl, _tempDirectory, progress: progress);

        // Assert
        Assert.NotNull(downloadedFilePath);
        Assert.True(File.Exists(downloadedFilePath));

        // 最終報告が100%であることを確認
        var finalReport = reportedProgress[reportedProgress.Count - 1];
        Assert.Equal(1.0, finalReport.progress, 2);
    }

    /// <summary>
    /// Directory.CreateDirectory にネストされたパスを指定した場合、すべてのディレクトリが作成されることをテストします。
    /// 例: nest1/nest2/target
    /// </summary>
    [Fact]
    public void Directory_CreateDirectory_WithNestedPath_CreatesAllDirectories()
    {
        // Arrange
        string uniqueName = $"Nested_{Guid.NewGuid():N}";
        string nestedPath = Path.Combine(_tempDirectory, uniqueName, "nest1", "nest2", "target");

        // Act
        Directory.CreateDirectory(nestedPath);

        // Assert
        Assert.True(Directory.Exists(nestedPath));
        Assert.True(Directory.Exists(Path.Combine(_tempDirectory, uniqueName, "nest1")));
        Assert.True(Directory.Exists(Path.Combine(_tempDirectory, uniqueName, "nest1", "nest2")));
        Assert.True(Directory.Exists(Path.Combine(_tempDirectory, uniqueName, "nest1", "nest2", "target")));
    }

    /// <summary>
    /// Directory.CreateDirectory に . で始まる相対パスを指定した場合、ディレクトリが作成されることをテストします。
    /// 例: ./target
    /// </summary>
    [Fact]
    public void Directory_CreateDirectory_WithDotPrefixPath_CreatesDirectory()
    {
        // Arrange
        string uniqueName = $"DotPrefix_{Guid.NewGuid():N}";
        string dotPrefixPath = Path.Combine(_tempDirectory, "./" + uniqueName);

        // Act
        Directory.CreateDirectory(dotPrefixPath);

        // Assert
        Assert.True(Directory.Exists(dotPrefixPath));
    }

    /// <summary>
    /// Directory.CreateDirectory に . で始まらない相対パスを指定した場合、ディレクトリが作成されることをテストします。
    /// 例: directtarget/nest1
    /// </summary>
    [Fact]
    public void Directory_CreateDirectory_WithNoPrefixPath_CreatesDirectory()
    {
        // Arrange
        string uniqueName = $"NoPrefix_{Guid.NewGuid():N}";
        string noPrefixPath = Path.Combine(_tempDirectory, uniqueName, "nest1");

        // Act
        Directory.CreateDirectory(noPrefixPath);

        // Assert
        Assert.True(Directory.Exists(noPrefixPath));
    }

    /// <summary>
    /// Directory.CreateDirectory に / で始まる絶対パスを指定した場合、ディレクトリが作成されることをテストします。
    /// 例: /home/user/target (Unix) または C:\temp\target (Windows)
    /// </summary>
    [Fact]
    public void Directory_CreateDirectory_WithAbsolutePath_CreatesDirectory()
    {
        // Arrange
        // Unix 形式の絶対パス（/home/user/target）
        string uniqueName = $"AbsolutePath_{Guid.NewGuid():N}";
        string absolutePath = Path.Combine("/", "home", "user", uniqueName);

        // Act
        Directory.CreateDirectory(absolutePath);

        // Assert
        // Path.IsPathRooted でルート判定されるため、絶対パスとして処理される
        Assert.True(Directory.Exists(absolutePath));
    }

    /// <summary>
    /// 既存のディレクトリに同じ名前でテストを実行した場合、既存のディレクトリが残ることをテストします。
    /// </summary>
    [Fact]
    public void Directory_CreateDirectory_WithExistingDirectory_PreservesExisting()
    {
        // Arrange
        string existingDirName = $"Existing_{Guid.NewGuid():N}";
        string existingDirPath = Path.Combine(_tempDirectory, existingDirName);
        
        // 既存ディレクトリを作成
        Directory.CreateDirectory(existingDirPath);
        string existingFilePath = Path.Combine(existingDirPath, "existing_file.txt");
        File.WriteAllText(existingFilePath, "This is an existing file.");

        // Act
        Directory.CreateDirectory(existingDirPath);

        // Assert
        // 既存のディレクトリとファイルが残っていることを確認
        Assert.True(Directory.Exists(existingDirPath));
        Assert.True(File.Exists(existingFilePath));
    }
}