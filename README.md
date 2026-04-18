# Civitai Downloader

Civitai から AI モデルをダウンロードするための CLI ツールです。

## 特徴

- Civitai API からファイルをダウンロード
- `Content-Disposition` ヘッダから正しいファイル名を自動抽出
- .NET 10.0 でクロスプラットフォーム対応（Windows, Linux, macOS）

## 必要条件

- .NET 10.0 Runtime または SDK

## 使用方法

### 基本的な使用方法

```bash
dotnet CivitaiDownloader.dll -url "<ダウンロードURL>"
```

### オプション

| オプション | 説明 |
|-----------|------|
| `<URL>` | ダウンロードする URL（位置パラメータ、または `-url` オプション） |
| `-url <URL>` | ダウンロードする URL（オプション） |
| `-output <ディレクトリ>` | 出力ディレクトリ（デフォルト: カレントディレクトリ） |
| `-filename <ファイル名>` | ファイル名（指定なしはサーバーから取得） |
| `-token <トークン>` | アクセストークン（環境変数 `CIVITAI_API_KEY` も使用可能） |
| `-y` | 既存ファイルを自動的に上書き（確認なし） |
| `-h, --help` | 使用方法を表示 |

### 使用例

```bash
# 基本的なダウンロード
dotnet CivitaiDownloader.dll -url "https://civitai.com/api/download/models/123"

# 出力ディレクトリを指定
dotnet CivitaiDownloader.dll -url "https://civitai.com/api/download/models/123" -output "./downloads"

# ファイル名を指定
dotnet CivitaiDownloader.dll -url "https://civitai.com/api/download/models/123" -filename "custom.zip"
```

## ビルド方法

### Windows でのビルド

```bash
cd "Q:\MyDocuments\Visual Studio 2022\Projects\CivitaiDownloader"
dotnet build CivitaiDownloader.sln
```

### Ubuntu でのビルド

Ubuntu に .NET 10.0 SDK をインストールした後：

```bash
dotnet build
```

### Ubuntu での実行

```bash
# .NET 10.0 Runtime をインストール
sudo apt update && sudo apt install -y dotnet-runtime-10.0

# 実行
dotnet CivitaiDownloader.dll -url "https://civitai.com/api/download/models/123"
```

## テスト

```bash
dotnet test CivitaiDownloader.sln
```

## ドキュメント

詳細なドキュメントは `docs/` ディレクトリに格納しています。

| ドキュメント | 説明 |
|-------------|------|
| [DATA_FLOW.md](docs/DATA_FLOW.md) | データフロー図（Mermaid で記述） |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | アーキテクチャとクラスの責務マップ |
| [MEMORY_BANK.md](docs/MEMORY_BANK.md) | メモリーバンク（プロジェクト要約） |
| [PROJECT_PROGRESS.md](docs/PROJECT_PROGRESS.md) | プロジェクト進捗管理 |

