using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string url = null;
        string outputDirectory = Environment.CurrentDirectory;
        string filename = null;

        // コマンドライン引数の解析
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-url":
                    if (i + 1 < args.Length)
                    {
                        url = args[++i];
                    }
                    break;
                case "-output":
                    if (i + 1 < args.Length)
                    {
                        outputDirectory = args[++i];
                    }
                    break;
                case "-filename":
                    if (i + 1 < args.Length)
                    {
                        filename = args[++i];
                    }
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return;
            }
        }

        // URLの検証
        if (string.IsNullOrWhiteSpace(url))
        {
            Console.Error.WriteLine("エラー: -url 引数が指定されていません。");
            PrintUsage();
            return;
        }

        // 出力ディレクトリの確認と作成
        try
        {
            if (!System.IO.Directory.Exists(outputDirectory))
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"エラー: 出力ディレクトリ '{outputDirectory}' の作成に失敗しました: {ex.Message}");
            return;
        }

        // ダウンロードの実行
        string downloadedFilePath = await DownloadFileWithFilename(url, outputDirectory, filename);

        if (!string.IsNullOrEmpty(downloadedFilePath))
        {
            Console.WriteLine($"ダウンロードが完了しました: {downloadedFilePath}");
        }
        else
        {
            Console.Error.WriteLine("ダウンロードに失敗しました。");
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Civitai Downloader - Civitai からモデルをダウンロードするツール");
        Console.WriteLine();
        Console.WriteLine("使用方法:");
        Console.WriteLine("  CivitaiDownloader.exe -url <URL> [オプション]");
        Console.WriteLine();
        Console.WriteLine("引数:");
        Console.WriteLine("  -url <URL>              ダウンロードするURL（必須）");
        Console.WriteLine("  -output <ディレクトリ>  出力ディレクトリ（オプション、デフォルト: カレントディレクトリ）");
        Console.WriteLine("  -filename <ファイル名>  ファイル名（オプション、指定なしはサーバーから取得）");
        Console.WriteLine("  -h, --help              使用方法を表示");
        Console.WriteLine();
        Console.WriteLine("例:");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\"");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\" -output \"./downloads\"");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\" -filename \"custom.zip\"");
    }

    static async Task<string> DownloadFileWithFilename(string url, string outputDirectory, string customFilename = null)
    {
        using var httpClient = new HttpClient();
        
        // リダイレクトを追跡して最終的なレスポンスを取得
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

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
            Console.Error.WriteLine("エラー: ファイル名を特定できませんでした。-filename オプションで指定してください。");
            return null;
        }

        // 出力ファイルパス
        string outputPath = System.IO.Path.Combine(outputDirectory, downloadedFilename);

        // ファイルをダウンロード
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var fileStream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        {
            await stream.CopyToAsync(fileStream);
        }

        return outputPath;
    }

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
}