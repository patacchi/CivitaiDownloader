# メモリーバンク

## プロジェクト概要

| 項目 | 内容 |
|------|------|
| プロジェクト名 | Civitai Downloader |
| 言語 | C# (.NET 10.0) |
| フレームワーク | .NET 10.0 |
| テストフレームワーク | xunit |
| モックフレームワーク | Moq |
| プロジェクトディレクトリ | `q:\MyDocuments\Visual Studio 2022\Projects\CivitaiDownloader` |

## アーキテクチャ概要

### エントリーポイント
- `Program.Main` - CLI アプリケーションのエントリーポイント

### 主要なクラス（概要）
- **Program** - エントリーポイント、CLI UI
- **CommandLineArgs** - コマンドライン引数の解析と保持
- **FileDownloader** - ファイルダウンロードの主要ロジック（メイン制御、他のクラスに責務を委譲）
- **FilenameExtractor** - ファイル名抽出（Content-Disposition/URL）の静的クラス
- **ProgressFormatter** - 進捗表示フォーマット（バイト数/バー）の静的クラス
- **IFileSystem** - ファイルシステム操作の抽象化
- **DefaultFileSystem** - IFileSystem の実装

### 詳細な仕様
- **データフロー**: [DATA_FLOW.md](./DATA_FLOW.md)
- **クラスの責務**: [ARCHITECTURE.md](./ARCHITECTURE.md)

### テスト用クラス
- **DelayedStream** - ネットワーク遅延をシミュレートするカスタムストリーム
- **DelayedHttpContent** - DelayedStream を返すカスタム HttpContent

### テストクラス一覧
- **FileDownloaderTests** - FileDownloader の単体テスト（DownloadFileAsync, AddTokenToUrl）
- **FilenameExtractorTests** - FilenameExtractor の単体テスト（ファイル名抽出）
- **ProgressFormatterTests** - ProgressFormatter の単体テスト（バイト数フォーマット、進捗バー）
- **CommandLineArgsTests** - CommandLineArgs の単体テスト
- **ProgramTests** - Program の単体テスト
- **FileDownloaderIntegrationTests** - 統合テスト（Civitai API連携）

## テスト戦略

- **単体テスト**: FileDownloaderTests, CommandLineArgsTests, ProgramTests
- **統合テスト**: FileDownloaderIntegrationTests
- **モック**: Moq を使用して HttpClient と IFileSystem をモック

## ファイル構造

```
CivitaiDownloader/
├── CivitaiDownloader/
│   ├── Program.cs
│   ├── CommandLineArgs.cs
│   ├── FileDownloader.cs
│   ├── FilenameExtractor.cs       # 新規
│   ├── ProgressFormatter.cs       # 新規
│   ├── IFileSystem.cs
│   └── AssemblyInfo.cs
├── CivitaiDownloader.Tests/
│   ├── ProgramTests.cs
│   ├── CommandLineArgsTests.cs
│   ├── FileDownloaderTests.cs
│   ├── FileDownloaderIntegrationTests.cs
│   └── TestConstants.cs
├── docs/
│   ├── DATA_FLOW.md               - データフロー図
│   ├── ARCHITECTURE.md            - アーキテクチャとクラス責務
│   ├── MEMORY_BANK.md             - プロジェクト概要（このファイル）
│   └── PROJECT_PROGRESS.md        - 進捗管理
├── CivitaiDownloader.sln
└── README.md
```

## リンク

- **データフロー詳細**: [DATA_FLOW.md](./DATA_FLOW.md)
- **アーキテクチャ詳細**: [ARCHITECTURE.md](./ARCHITECTURE.md)
- **進捗管理**: [PROJECT_PROGRESS.md](./PROJECT_PROGRESS.md)