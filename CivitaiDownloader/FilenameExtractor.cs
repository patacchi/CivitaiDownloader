using System;
using System.Net;
using System.Text.RegularExpressions;

/// <summary>
/// ファイル名を抽出するためのクラス。
/// Content-Disposition ヘッダや URL からファイル名を抽出します。
/// </summary>
public static class FilenameExtractor
{
    /// <summary>
    /// Content-Disposition ヘッダの値からファイル名を抽出します。
    /// </summary>
    /// <param name="contentDisposition">Content-Disposition ヘッダの値。例: "attachment; filename=\"filename.zip\""</param>
    /// <returns>抽出されたファイル名。ファイル名が見つからない場合は null。</returns>
    public static string ExtractFilenameFromContentDisposition(string contentDisposition)
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
    public static string ExtractFilenameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var fileName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                // URLエンコードされた文字列をデコード
                return WebUtility.UrlDecode(fileName);
            }
        }
        catch
        {
            // URL解析に失敗した場合は null を返す
        }

        return null;
    }
}
