using System;

/// <summary>
/// 進捗フォーマットを処理するためのクラス。
/// バイト数のフォーマットと進捗バーの生成を行います。
/// </summary>
public static class ProgressFormatter
{
    /// <summary>
    /// バイト数を適切な単位に変換します。
    /// </summary>
    /// <param name="bytes">バイト数。</param>
    /// <returns>変換されたバイト数（単位付き）。</returns>
    public static string FormatBytes(long bytes)
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
    /// <param name="width">バーの長さ（デフォルト: 20）。</param>
    /// <returns>生成された進捗バー文字列。</returns>
    public static string GenerateProgressBar(double progress, int width = 20)
    {
        if (progress < 0) progress = 0;
        if (progress > 1) progress = 1;

        // width が負の場合は0として扱う
        if (width < 0) width = 0;

        int filled = (int)(progress * width);
        int empty = width - filled;

        return "[" + new string('#', filled) + new string('-', empty) + "]";
    }
}