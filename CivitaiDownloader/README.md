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
| `-url <URL>` | ダウンロードする URL（必須） |
| `-output <ディレクトリ>` | 出力ディレクトリ（デフォルト: カレントディレクトリ） |
| `-filename <ファイル名>` | ファイル名（指定なしはサーバーから取得） |
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