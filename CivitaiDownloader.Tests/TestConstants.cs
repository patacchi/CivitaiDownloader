/// <summary>
/// テストで使用する定数を定義するクラス。
/// テストURLを一元管理して、変更時に1か所の修正で済むようにします。
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Civitai からモデルをダウンロードするためのテスト URL。
    /// この URL は変更・削除される可能性があるため、定期的に確認してください。
    /// </summary>
    public const string CivitaiDownloadUrl = "https://civitai.com/api/download/models/2857981?type=Archive&format=Other";

    /// <summary>
    /// ダウンロードされる予期されるファイル名。
    /// Content-Disposition ヘッダから抽出されたファイル名と一致することを確認します。
    /// </summary>
    public const string ExpectedFilename = "animaPreviewWorkflow_v40.zip";
}