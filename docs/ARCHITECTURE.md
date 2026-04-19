# アーキテクチャと責務マップ

## 全体アーキテクチャ

```mermaid
graph TB
    subgraph "Presentation Layer"
        A[Program] -->|コマンドライン引数解析| B[CommandLineArgs]
        A -->|進捗表示| C[Progress Handler]
    end
    
    subgraph "Application Layer"
        B -->|設定データ| D[FileDownloader]
        C -->|進捗報告| D
    end
    
    subgraph "Domain Layer"
        D -->|ファイル名抽出| E[FileName Extractors]
        D -->|HTTP通信| F[HttpClient]
        D -->|ユーザー確認| G[IFileSystem]
        D -->|ファイル操作| G
    end
    
    subgraph "Infrastructure Layer"
        E -->|正規表現| H[Regex]
        F -->|HTTP通信| I[Civitai API]
        G -->|実装| J[DefaultFileSystem]
    end
    
    style A fill:#e1f5ff
    style B fill:#e1f5ff
    style C fill:#e1f5ff
    style D fill:#fff4e1
    style E fill:#fff4e1
    style F fill:#fff4e1
    style G fill:#fff4e1
    style H fill:#e1ffe1
    style I fill:#ffe1e1
    style J fill:#e1ffe1
```

## クラスの責務マップ

| クラス名 | 責務 | 主要メソッド/プロパティ | 依存関係 |
|---------|------|------------------------|----------|
| **Program** | エントリーポイント、CLI UI、主要な制御フロー | `Main()`, `PrintUsage()` | CommandLineArgs, FileDownloader |
| **CommandLineArgs** | コマンドライン引数の解析と保持 | `Parse()`, `Url`, `OutputDirectory`, `Filename`, `Token`, `AutoOverwrite`, `ShowHelp` | なし |
| **FileDownloader** | ファイルダウンロードの主要ロジック、Content-Disposition からファイル名抽出 | `DownloadFileAsync()`, `ExtractFilenameFromContentDisposition()`, `ExtractFilenameFromUrl()`, `AddTokenToUrl()`, `FormatBytes()`, `GenerateProgressBar()` | HttpClient, IFileSystem |
| **IFileSystem** | ファイルシステム操作の抽象化（テスト容易性向上） | `FileExists()`, `DeleteFile()`, `ReadKey()` | なし |
| **DefaultFileSystem** | IFileSystem の実装（実際のファイルシステム操作） | `FileExists()`, `DeleteFile()`, `ReadKey()` | System.IO, System |
| **TestConstants** | テストで使用する定数（短時間/長時間リンク） | `CivitaiDownloadUrl_Short`, `ExpectedFilename_Short`, `CivitaiDownloadUrl_Long`, `ExpectedFilename_Long` | なし |

## モジュール間の依存関係

```mermaid
graph TD
    A[Program] --> B[CommandLineArgs]
    A --> C[FileDownloader]
    A --> D[Progress Handler]
    
    B --> C
    
    C --> E[HttpClient]
    C --> F[IFileSystem]
    C --> G[DefaultFileSystem]
    
    F --> G
    
    style A fill:#e1f5ff
    style B fill:#e1f5ff
    style C fill:#fff4e1
    style D fill:#e1ffe1
    style E fill:#e1ffe1
    style F fill:#fff4e1
    style G fill:#e1ffe1
```

## 各クラスの詳細説明

### Program
- **役割**: アプリケーションのエントリーポイント
- **主な処理**:
  1. コマンドライン引数の解析
  2. 出力ディレクトリの確認と作成
  3. FileDownloader を使用してファイルをダウンロード
  4. 進捗を表示
  5. 結果をコンソールに出力
  6. エラー発生時の処理

### CommandLineArgs
- **役割**: コマンドライン引数の解析と保持
- **主な処理**:
   1. `--url`, `--output`, `--filename`, `-y`, `--token`, `-h`, `--help` を解析
  2. 環境変数 `CIVITAI_API_KEY` から token を取得
  3. 位置パラメータ（URL）を解析

### FileDownloader
- **役割**: ファイルダウンロードの主要ロジックを実行
- **主な処理**:
  1. HTTP リクエストを送信
  2. Content-Disposition ヘッダからファイル名を抽出
  3. URL からファイル名を抽出
  4. ユーザー確認プロンプトを表示
  5. ファイルをストリームでダウンロード
  6. 進捗を報告（1000ms 間隔）
  7. IDisposable を実装して HttpClient を適切に破棄

### IFileSystem
- **役割**: ファイルシステム操作の抽象化（テスト容易性向上）
- **主な処理**:
  1. `FileExists`: ファイルの存在を確認
  2. `DeleteFile`: ファイルを削除
  3. `ReadKey`: キー入力を取得

### DefaultFileSystem
- **役割**: IFileSystem の実装（実際のファイルシステム操作）
- **主な処理**:
  1. `FileExists`: `File.Exists` を使用
  2. `DeleteFile`: `File.Delete` を使用
  3. `ReadKey`: `Console.ReadKey` を使用

## デザインパターン

- **Strategy パターン**: IFileSystem と DefaultFileSystem
  - テスト時に IFileSystem をモックして実際のファイル操作を回避可能
  
- **Dependency Injection**: FileDownloader に HttpClient と IFileSystem を渡す
  - 依存性を下げるため、コンストラクタで依存関係を注入
  
- **Progress Pattern**: IProgress<(double progress, long downloaded, long total)> を使用
  - 進捗報告を非同期で処理

## テスト戦略

- **単体テスト**: FileDownloaderTests, CommandLineArgsTests, ProgramTests
- **統合テスト**: FileDownloaderIntegrationTests
- **モック**: Moq を使用して HttpClient と IFileSystem をモック