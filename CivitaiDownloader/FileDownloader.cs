using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// Civitai からファイルをダウンロードするためのクラス。
/// Content-Disposition ヘッダから正しいファイル名を抽出します。
/// </summary>
public class FileDownloader : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IFileSystem _fileSystem;
    private bool _disposed = false;

    /// <summary>
    /// HttpClient と IFileSystem を使用して FileDownloader の新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="httpClient">HTTP リクエストに使用する HttpClient。</param>
    /// <param name="fileSystem">ファイルシステム操作に使用する IFileSystem。</param>
    public FileDownloader(HttpClient httpClient, IFileSystem fileSystem)
    {
        _httpClient = httpClient;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// HttpClient を使用して FileDownloader の新しいインスタンスを初期化します。
    /// デフォルトの IFileSystem（DefaultFileSystem）を使用します。
    /// </summary>
    /// <param name="httpClient">HTTP リクエストに使用する HttpClient。</param>
    public FileDownloader(HttpClient httpClient) : this(httpClient, new DefaultFileSystem())
    {
    }

    /// <summary>
    /// デフォルトの HttpClient を使用して FileDownloader の新しいインスタンスを初期化します。
    /// デフォルトの IFileSystem（DefaultFileSystem）を使用します。
    /// </summary>
    public FileDownloader() : this(new HttpClient(), new DefaultFileSystem())
    {
    }

    /// <summary>
    /// ダウンロードの結果を表すクラス。
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// ダウンロードに成功したファイルの絶対パス。失敗した場合は null。
        /// </summary>
        public string FilePath { get; init; }
        
        /// <summary>
        /// 終了原因。
        /// </summary>
        public DownloadStatus Status { get; init; }
        
        /// <summary>
        /// エラーメッセージ。失敗した場合は null 以外。
        /// </summary>
        public string ErrorMessage { get; init; }
    }
    
    /// <summary>
    /// ダウンロードの終了状態を表す列挙型。
    /// </summary>
    public enum DownloadStatus
    {
        /// <summary>
        /// ダウンロードに成功しました。
        /// </summary>
        Success,
        
        /// <summary>
        /// ユーザーによるキャンセル。
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// エラーで失敗しました。
        /// </summary>
        Failed
    }

    /// <summary>
    /// 指定された URL からファイルをダウンロードし、Content-Disposition ヘッダから抽出したファイル名で保存します。
    /// </summary>
    /// <param name="url">ダウンロードするファイルの URL。</param>
    /// <param name="outputDirectory">ファイルを保存するディレクトリのパス。</param>
    /// <param name="customFilename">オプション。使用するファイル名を明示的に指定します。指定しない場合はサーバーから取得します。</param>
    /// <param name="overwrite">既存ファイルを上書きする場合は true。省略時は false。</param>
    /// <param name="progress">進捗を報告する IProgress オブジェクト（省略時は非同期で進捗を報告しない）。</param>
    /// <returns>ダウンロード結果を含む DownloadResult オブジェクト。失敗した場合は FilePath が null、Status が Failed。</returns>
    public async Task<DownloadResult> DownloadFileAsync(string url, string outputDirectory, string customFilename = null, bool overwrite = false, IProgress<(double progress, long downloaded, long total)> progress = null)
    {
        try
        {
            // リダイレクトを追跡して最終的なレスポンスを取得
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            // Content-Disposition ヘッダからファイル名を抽出
            string downloadedFilename = null;

            if (response.Content.Headers.ContentDisposition != null)
            {
                downloadedFilename = ExtractFilenameFromContentDisposition(response.Content.Headers.ContentDisposition.ToString());
            }

            // カスタムファイル名が指定されている場合はそれを使用
            if (!string.IsNullOrEmpty(customFilename))
            {
                downloadedFilename = customFilename;
            }
            // サーバーから取得したファイル名がなければ、URLから推測
            else if (string.IsNullOrEmpty(downloadedFilename))
            {
                downloadedFilename = ExtractFilenameFromUrl(url);
            }

            // ファイル名がまだ取得できない場合はエラー
            if (string.IsNullOrEmpty(downloadedFilename))
            {
                return new DownloadResult { FilePath = null, Status = DownloadStatus.Failed, ErrorMessage = "ファイル名を取得できません。" };
            }

            // 出力ファイルパス
            string outputPath = System.IO.Path.Combine(outputDirectory, downloadedFilename);

            // ファイルが既に存在する場合、ユーザーに確認
            if (_fileSystem.FileExists(outputPath) && !overwrite)
            {
                ConsoleKey keyInfo;
                do
                {
                    Console.Write($"ファイル '{downloadedFilename}' は既に存在します。上書きしますか？(y/n): ");
                    keyInfo = _fileSystem.ReadKey(false);
                    Console.WriteLine(); // 改行
                    
                    if (keyInfo != ConsoleKey.Y && keyInfo != ConsoleKey.N)
                    {
                        Console.WriteLine("y または n を入力してください。");
                    }
                }
                while (keyInfo != ConsoleKey.Y && keyInfo != ConsoleKey.N);
                
                if (keyInfo != ConsoleKey.Y)
                {
                    return new DownloadResult { FilePath = null, Status = DownloadStatus.Cancelled, ErrorMessage = null };
                }
            }

            // Content-Length から総バイト数を取得
            long totalBytes = response.Content.Headers.ContentLength ?? 0;

            // ファイルをダウンロード
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                // 進捗を報告するため、ストリームをバッファを使ってコピー
                byte[] buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                // 進捗報告の間隔（ミリ秒）
                const int ProgressUpdateIntervalMs = 1000;
                DateTime lastProgressReport = DateTime.MinValue;
                bool progressSessionCompleted = false;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    // 進捗を報告（1000ms 経過時のみ、かつセッション完了していない場合）
                    DateTime now = DateTime.Now;
                    if (!progressSessionCompleted && (now - lastProgressReport).TotalMilliseconds >= ProgressUpdateIntervalMs)
                    {
                        double progressPercent = totalBytes > 0 ? (double)totalRead / totalBytes : 0;
                        progress?.Report((progressPercent, totalRead, totalBytes));
                        lastProgressReport = now;

                        // 100%の場合はセッション完了フラグを立てる
                        if (progressPercent >= 1.0)
                        {
                            progressSessionCompleted = true;
                        }
                    }
                }

                // ダウンロード完了時に100%の進捗を報告（セッション完了していない場合のみ）
                if (!progressSessionCompleted)
                {
                    double finalProgressPercent = totalBytes > 0 ? (double)totalRead / totalBytes : 1.0;
                    progress?.Report((finalProgressPercent, totalRead, totalBytes));
                    progressSessionCompleted = true;
                }
            }

            return new DownloadResult { FilePath = outputPath, Status = DownloadStatus.Success, ErrorMessage = null };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"エラー: ファイルのダウンロードに失敗しました。URL: {url}");
            Console.Error.WriteLine($"詳細: {ex.Message}");
            return new DownloadResult { FilePath = null, Status = DownloadStatus.Failed, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Content-Disposition ヘッダの値からファイル名を抽出します。
    /// </summary>
    /// <param name="contentDisposition">Content-Disposition ヘッダの値。例: "attachment; filename=\"filename.zip\""</param>
    /// <returns>抽出されたファイル名。ファイル名が見つからない場合は null。</returns>
    static string ExtractFilenameFromContentDisposition(string contentDisposition)
    {
        if (string.IsNullOrEmpty(contentDisposition))
        {
            return null;
        }

        // filename="value" の形式（旧形式）からファイル名を抽出
        var match = Regex.Match(contentDisposition, @"filename=""([^""]+)""");
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        // filename=value の形式（クォートなし）からファイル名を抽出
        match = Regex.Match(contentDisposition, @"filename=([^\s;]+)");
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
            var fileName = System.IO.Path.GetFileName(path);
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

    /// <summary>
    /// FileDownloader インスタンスを破棄します。
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 指定されたdisposedフラグに従ってリソースを破棄します。
    /// </summary>
    /// <param name="disposing">マネージリソースを破棄する場合は true。</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // マネージリソースを破棄
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// FileDownloader インスタンスのデストラクタ。
    /// </summary>
    ~FileDownloader()
    {
        Dispose(false);
    }

    /// <summary>
    /// バイト数を適切な単位に変換します。
    /// </summary>
    /// <param name="bytes">バイト数。</param>
    /// <returns>変換されたバイト数（単位付き）。</returns>
    internal static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }
        else if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.###} KB";
        }
        else if (bytes < 1024 * 1024 * 1024)
        {
            return $"{bytes / (1024.0 * 1024.0):0.###} MB";
        }
        else
        {
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):0.###} GB";
        }
    }

    /// <summary>
    /// 進捗バーを生成します。
    /// </summary>
    /// <param name="progress">進捗率（0.0 から 1.0 の範囲）。</param>
    /// <param name="width">バーの長さ。</param>
    /// <returns>生成された進捗バー文字列。</returns>
    internal static string GenerateProgressBar(double progress, int width = 20)
    {
        if (progress < 0) progress = 0;
        if (progress > 1) progress = 1;

        int filled = (int)(progress * width);
        int empty = width - filled;

        return "[" + new string('#', filled) + new string('-', empty) + "]";
    }

    /// <summary>
    /// URL に token を追加します（Token が指定されている場合のみ）。
    /// </summary>
    /// <param name="url">元の URL。</param>
    /// <param name="token">アクセストークン。</param>
    /// <returns>token が null の場合は元の URL、それ以外は token を追加した URL。</returns>
    internal static string AddTokenToUrl(string url, string token)
    {
        if (string.IsNullOrEmpty(token))
            return url;

        string separator = url.Contains("?") ? "&" : "?";
        return $"{url}{separator}token={token}";
    }
}
