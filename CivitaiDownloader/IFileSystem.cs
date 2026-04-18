using System;
using System.IO;

/// <summary>
/// ファイルシステム操作のインターフェース。
/// テスト容易性を高めるため、ファイル操作をこのインターフェースに抽象化します。
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// ファイルが存在するか確認します。
    /// </summary>
    /// <param name="path">確認するファイルのパス。</param>
    /// <returns>ファイルが存在する場合は true。</returns>
    bool FileExists(string path);

    /// <summary>
    /// ファイルを削除します。
    /// </summary>
    /// <param name="path">削除するファイルのパス。</param>
    void DeleteFile(string path);

    /// <summary>
    /// ユーザーの入力を1文字取得します。
    /// </summary>
    /// <param name="intercept">true の場合、入力された文字は表示されません。</param>
    /// <returns>入力された文字。</returns>
    ConsoleKey ReadKey(bool intercept);
}

/// <summary>
/// 実際のファイルシステム操作を実行するデフォルト実装。
/// </summary>
public class DefaultFileSystem : IFileSystem
{
    /// <summary>
    /// ファイルが存在するか確認します。
    /// </summary>
    /// <param name="path">確認するファイルのパス。</param>
    /// <returns>ファイルが存在する場合は true。</returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// ファイルを削除します。
    /// </summary>
    /// <param name="path">削除するファイルのパス。</param>
    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    /// <summary>
    /// ユーザーの入力を1文字取得します。
    /// </summary>
    /// <param name="intercept">true の場合、入力された文字は表示されません。</param>
    /// <returns>入力された文字。</returns>
    public ConsoleKey ReadKey(bool intercept)
    {
        return Console.ReadKey(intercept).Key;
    }
}