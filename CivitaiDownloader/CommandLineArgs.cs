using System;
using System.IO;

/// <summary>
/// コマンドライン引数を解析して保持するクラス。
/// </summary>
public class CommandLineArgs
{
    /// <summary>
    /// ダウンロードする URL。
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// 出力ディレクトリのパス。指定がない場合はカレントディレクトリ。
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// 使用するファイル名。指定がない場合はサーバーから取得。
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// ヘルプ表示が必要な場合に true。
    /// </summary>
    public bool ShowHelp { get; }

    /// <summary>
    /// 既存ファイルを自動的に上書きする場合は true。
    /// </summary>
    public bool AutoOverwrite { get; }

    /// <summary>
    /// アクセストークン。
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// コマンドライン引数を解析して CommandLineArgs インスタンスを初期化します。
    /// </summary>
    /// <param name="args">コマンドライン引数配列。</param>
    /// <returns>解析されたコマンドライン引数を含む CommandLineArgs インスタンス。</returns>
    public static CommandLineArgs Parse(string[] args)
    {
        string url = null;
        string outputDirectory = Environment.CurrentDirectory;
        string filename = null;
        string token = null;
        bool showHelp = false;
        bool autoOverwrite = false;
        bool urlFromPositionalArg = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--url":
                    if (i + 1 < args.Length)
                    {
                        url = args[++i];
                    }
                    break;
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        outputDirectory = args[++i];
                    }
                    break;
                case "--filename":
                    if (i + 1 < args.Length)
                    {
                        filename = args[++i];
                    }
                    break;
                case "-y":
                    autoOverwrite = true;
                    break;
                case "--token":
                    if (i + 1 < args.Length)
                    {
                        token = args[++i];
                    }
                    break;
                case "-h":
                case "--help":
                    showHelp = true;
                    break;
            }
        }

        // --url オプションが指定されていない場合、最初の位置パラメータを URL として使用
        if (string.IsNullOrEmpty(url))
        {
            // URL 形式の位置パラメータを検索（http:// または https:// で始まる）
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (!string.IsNullOrEmpty(arg) && (arg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    url = arg;
                    break;
                }
            }
        }

        // Token が指定されていない場合、環境変数 CIVITAI_API_KEY を使用
        if (string.IsNullOrEmpty(token))
        {
            token = Environment.GetEnvironmentVariable("CIVITAI_API_KEY");
        }

        return new CommandLineArgs(url, outputDirectory, filename, token, showHelp, autoOverwrite);
    }

    /// <summary>
    /// CommandLineArgs クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="url">ダウンロードする URL。</param>
    /// <param name="outputDirectory">出力ディレクトリのパス。</param>
    /// <param name="filename">使用するファイル名。</param>
    /// <param name="token">アクセストークン。</param>
    /// <param name="showHelp">ヘルプ表示が必要な場合は true。</param>
    /// <param name="autoOverwrite">既存ファイルを自動的に上書きする場合は true。</param>
    private CommandLineArgs(string url, string outputDirectory, string filename, string token, bool showHelp, bool autoOverwrite = false)
    {
        Url = url;
        OutputDirectory = ResolveOutputDirectory(outputDirectory);
        Filename = filename;
        Token = token;
        ShowHelp = showHelp;
        AutoOverwrite = autoOverwrite;
    }

    /// <summary>
    /// 出力ディレクトリパスを解決します。
    /// 相対パスの場合はカレントディレクトリからの絶対パスに変換します。
    /// ただし、ルートから始まる絶対パスはそのまま使用します。
    /// </summary>
    /// <param name="outputDirectory">出力ディレクトリのパス。</param>
    /// <returns>解決された出力ディレクトリのパス。</returns>
    private static string ResolveOutputDirectory(string outputDirectory)
    {
        // パスがルートから始まるか確認（Unix: /, Windows: C:\ など）
        if (Path.IsPathRooted(outputDirectory))
        {
            return outputDirectory;
        }

        // 相対パスの場合はカレントディレクトリからの絶対パスに変換
        return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputDirectory));
    }
}