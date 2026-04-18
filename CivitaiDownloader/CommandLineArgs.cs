using System;

/// <summary>
/// コマンドライン引数を解析して保持するクラス。
/// </summary>
class CommandLineArgs
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
    /// コマンドライン引数を解析して CommandLineArgs インスタンスを初期化します。
    /// </summary>
    /// <param name="args">コマンドライン引数配列。</param>
    /// <returns>解析されたコマンドライン引数を含む CommandLineArgs インスタンス。</returns>
    public static CommandLineArgs Parse(string[] args)
    {
        string url = null;
        string outputDirectory = Environment.CurrentDirectory;
        string filename = null;
        bool showHelp = false;

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
                    showHelp = true;
                    break;
            }
        }

        return new CommandLineArgs(url, outputDirectory, filename, showHelp);
    }

    /// <summary>
    /// CommandLineArgs クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="url">ダウンロードする URL。</param>
    /// <param name="outputDirectory">出力ディレクトリのパス。</param>
    /// <param name="filename">使用するファイル名。</param>
    /// <param name="showHelp">ヘルプ表示が必要な場合は true。</param>
    private CommandLineArgs(string url, string outputDirectory, string filename, bool showHelp)
    {
        Url = url;
        OutputDirectory = outputDirectory;
        Filename = filename;
        ShowHelp = showHelp;
    }
}