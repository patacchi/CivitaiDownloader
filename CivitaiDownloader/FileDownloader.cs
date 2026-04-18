using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Civitai からファイルをダウンロードするためのクラス。
/// Content-Disposition ヘッダから正しいファイル名を抽出します。
/// </summary>
public class FileDownloader : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    /// <summary>
    /// HttpClient を使用して FileDownloader の新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="httpClient">HTTP リクエストに使用する HttpClient。</param>
    public FileDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// デフォルトの HttpClient を使用して FileDownloader の新しいインスタンスを初期化します。
    /// </summary>
    public FileDownloader() : this(new HttpClient())
    {
    }

    /// <summary>
    /// 指定された URL からファイルをダウンロードし、Content-Disposition ヘッダから抽出したファイル名で保存します。
    /// </summary>
    /// <param name="url">ダウンロードするファイルの URL。</param>
    /// <param name="outputDirectory">ファイルを保存するディレクトリのパス。</param>
    /// <param name="customFilename">オプション。使用するファイル名を明示的に指定します。指定しない場合はサーバーから取得します。</param>
    /// <param name="overwrite">既存ファイルを上書きする場合は true。省略時は false。</param>
    /// <returns>ダウンロードされたファイルの絶対パス。失敗した場合は null。</returns>
    public async Task<string> DownloadFileAsync(string url, string outputDirectory, string customFilename = null, bool overwrite = false)
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
                return null;
            }

            // 出力ファイルパス
            string outputPath = System.IO.Path.Combine(outputDirectory, downloadedFilename);

            // ファイルが既に存在する場合、ユーザーに確認
            if (System.IO.File.Exists(outputPath) && !overwrite)
            {
                Console.Write($"ファイル '{downloadedFilename}' は既に存在します。上書きしますか？(y/n): ");
                var keyInfo = Console.ReadKey(false);
                Console.WriteLine(); // 改行
                
                if (keyInfo.Key != ConsoleKey.Y)
                {
                    Console.WriteLine("ダウンロードをキャンセルしました。");
                    return null;
                }
            }

            // ファイルをダウンロード
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            return outputPath;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"エラー: ファイルのダウンロードに失敗しました。URL: {url}");
            Console.Error.WriteLine($"詳細: {ex.Message}");
            return null;
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
}