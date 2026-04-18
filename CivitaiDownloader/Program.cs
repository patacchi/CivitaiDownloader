using System;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// Civitai からモデルをダウンロードするための CLI アプリケーションのエントリーポイント。
/// </summary>
class Program
{
    /// <summary>
    /// アプリケーションのエントリーポイント。
    /// コマンドライン引数を解析し、指定された URL からファイルをダウンロードします。
    /// </summary>
    /// <param name="args">コマンドライン引数（-url, -output, -filename）</param>
    static async Task Main(string[] args)
    {
        // コマンドライン引数の解析
        var commandLineArgs = CommandLineArgs.Parse(args);

        // ヘルプ表示の場合は終了
        if (commandLineArgs.ShowHelp)
        {
            return;
        }

        // URLの検証
        if (string.IsNullOrWhiteSpace(commandLineArgs.Url))
        {
            Console.Error.WriteLine("エラー: -url 引数が指定されていません。");
            PrintUsage();
            return;
        }

        // 出力ディレクトリの確認と作成
        try
        {
            if (!System.IO.Directory.Exists(commandLineArgs.OutputDirectory))
            {
                System.IO.Directory.CreateDirectory(commandLineArgs.OutputDirectory);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"エラー: 出力ディレクトリ '{commandLineArgs.OutputDirectory}' の作成に失敗しました: {ex.Message}");
            return;
        }

        string downloadUrl = FileDownloader.AddTokenToUrl(commandLineArgs.Url, commandLineArgs.Token);

        // 進捗表示用のハンドラ
        var progress = new Progress<(double progress, long downloaded, long total)>(report =>
        {
            // 進捗バーとバイト数を表示
            string progressBar = FileDownloader.GenerateProgressBar(report.progress);
            string downloadedStr = FileDownloader.FormatBytes(report.downloaded);
            string totalStr = FileDownloader.FormatBytes(report.total);
            
            // 1行上にカーソルを移動して上書き
            Console.Write($"\rダウンロード中: {progressBar} ({downloadedStr} / {totalStr})");
        });

        // ダウンロードの実行
        using var downloader = new FileDownloader();
        string downloadedFilePath = await downloader.DownloadFileAsync(
            downloadUrl,
            commandLineArgs.OutputDirectory,
            commandLineArgs.Filename,
            commandLineArgs.AutoOverwrite,
            progress);

        Console.WriteLine(); // 進捗表示の行を改行
        if (!string.IsNullOrEmpty(downloadedFilePath))
        {
            Console.WriteLine($"ダウンロードが完了しました: {downloadedFilePath}");
        }
        else
        {
            Console.Error.WriteLine("ダウンロードに失敗しました。");
        }
    }

    /// <summary>
    /// 使用方法をコンソールに出力します。
    /// </summary>
    static void PrintUsage()
    {
        Console.WriteLine("Civitai Downloader - Civitai からモデルをダウンロードするツール");
        Console.WriteLine();
        Console.WriteLine("使用方法:");
        Console.WriteLine("  CivitaiDownloader.exe <URL> [オプション]");
        Console.WriteLine("  CivitaiDownloader.exe -url <URL> [オプション]");
        Console.WriteLine();
        Console.WriteLine("引数:");
        Console.WriteLine("  <URL>                   ダウンロードするURL（位置パラメータ、または -url オプション）");
        Console.WriteLine("  -url <URL>              ダウンロードするURL（オプション）");
        Console.WriteLine("  -output <ディレクトリ>  出力ディレクトリ（オプション、デフォルト: カレントディレクトリ）");
        Console.WriteLine("  -filename <ファイル名>  ファイル名（オプション、指定なしはサーバーから取得）");
        Console.WriteLine("  -token <トークン>       アクセストークン（オプション、または環境変数 CIVITAI_API_KEY）");
        Console.WriteLine("  -y                      既存ファイルを自動的に上書き（確認なし）");
        Console.WriteLine("  -h, --help              使用方法を表示");
        Console.WriteLine();
        Console.WriteLine("例:");
        Console.WriteLine("  CivitaiDownloader.exe \"https://civitai.com/api/download/models/123\"");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\"");
        Console.WriteLine("  CivitaiDownloader.exe \"https://civitai.com/api/download/models/123\" -output \"./downloads\"");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\" -output \"./downloads\"");
        Console.WriteLine("  CivitaiDownloader.exe -url \"https://civitai.com/api/download/models/123\" -filename \"custom.zip\"");
    }
}