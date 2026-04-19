# データフロー図

## 全体データフロー

```mermaid
sequenceDiagram
    participant Program as Program.Main
    participant CL as CommandLineArgs
    participant FD as FileDownloader
    participant FE as FilenameExtractor
    participant PF as ProgressFormatter
    participant IFS as IFileSystem
    participant HttpClient as HttpClient

    Program->>CL: Parse(args)
    Note right of CL: URL, OutputDirectory,<br/>Filename, Token 解析<br/>環境変数 CIVITAI_API_KEY から token 取得

    Program->>FD: AddTokenToUrl(url, token)
    Note right of FD: token が null でなければ<br/>URL に追加

    Program->>FD: DownloadFileAsync(url, outputDir, filename, overwrite, progress)
    FD->>HttpClient: GetAsync(url, ResponseHeadersRead)
    
    HttpClient-->>FD: HttpResponseMessage
    
    FD->>FE: ExtractFilenameFromContentDisposition()
    Note right of FE: Content-Disposition から<br/>ファイル名抽出
    FE-->>FD: filename string
    
    alt overwrite=false 且つファイルが存在
        FD->>IFS: FileExists(path)
        IFS-->>FD: true/false
        
        alt ファイルが存在
            FD->>IFS: ReadKey(false)
            IFS-->>FD: ConsoleKey
            
            loop y/n が入力されるまで
                FD->>FD: Check if Y or N
                FD-->>IFS: ReadKey(false) if invalid
            end
            
            FD->>FD: Check if Y
            alt Y でない場合
                FD-->>Program: DownloadResult {FilePath=null, Status=Cancelled}
            end
        end
    end
    
    FD->>HttpClient: ReadAsStreamAsync()
    HttpClient-->>FD: Stream
    
    loop データを読み込む間
        FD->>FD: Read(buffer)
        FD->>FD: Write(fileStream)
        
        alt 1000ms 経過
            FD->>PF: FormatBytes(bytes)
            FD->>PF: GenerateProgressBar(progress)
            FD-->>Program: Progress report
        end
    end
    
    FD-->>Program: DownloadResult {FilePath=successPath, Status=Success}
    Note right of FD: キャンセル: FilePath=null, Status=Cancelled<br/>失敗: FilePath=null, Status=Failed, ErrorMessage=...
```

## クラス間のデータフロー

```mermaid
flowchart TD
    A[Program.Main] -->|"1. args[]"| B[CommandLineArgs.Parse]
    B -->|2. CommandLineArgs<br/>Url, OutputDirectory,<br/>Filename, Token, AutoOverwrite| C[FileDownloader.AddTokenToUrl]
    C -->|3. downloadUrl| D[FileDownloader.DownloadFileAsync]
    D -->|4. HTTP Request| E[HttpClient]
    E -->|5. HttpResponseMessage| D
    D -->|6. Content-Disposition<br/>file name| F[FilenameExtractor.ExtractFilenameFromContentDisposition]
    F -->|7. filename string| D
    D -->|8. customFilename| G{Check custom filename}
    G -->|Yes| I[outputPath]
    G -->|No| H[FilenameExtractor.ExtractFilenameFromUrl]
    H -->|8. filename string| I[outputPath]
    I -->|overwrite=false<br/>file exists?| J{User Confirmation}
    J -->|y| Q[Download File]
    J -->|n| N[Cancel]
    I -->|overwrite=true| Q
    
    style A fill:#e1f5ff
    style B fill:#e1f5ff
    style C fill:#e1f5ff
    style D fill:#e1f5ff
    style I fill:#e1f5ff
    style Q fill:#fff4e1
    style N fill:#ffe1e1
    style F fill:#fff4e1
    style H fill:#fff4e1
```

## メソッド呼び出し順序とデータフロー

```mermaid
flowchart TD
    Start[Program.Main] --> Parse[CommandLineArgs.Parse]
    Parse --> TokenUrl[FileDownloader.AddTokenToUrl]
    TokenUrl --> Download[FileDownloader.DownloadFileAsync]
    
    Download --> HttpClient[HttpClient.GetAsync]
    HttpClient --> Headers[Content-Disposition Headers]
    Headers --> ExtractFilename[FilenameExtractor.ExtractFilenameFromContentDisposition]
    ExtractFilename --> CheckFilename{Filename Found?}
    CheckFilename -->|Yes| SaveFile[Save File]
    CheckFilename -->|No| ExtractUrl[FilenameExtractor.ExtractFilenameFromUrl]
    ExtractUrl --> CheckUrl{Filename Found?}
    CheckUrl -->|Yes| SaveFile
    CheckUrl -->|No| Failed[Status=Failed]
    
    SaveFile --> CheckOverwrite{overwrite=false<br/>and file exists?}
    CheckOverwrite -->|Yes| UserConfirm[User Confirmation]
    CheckOverwrite -->|overwrite=true| DownloadData[Download Data]
    UserConfirm --> CheckKey{Key == Y?}
    CheckKey -->|Yes| DownloadData
    CheckKey -->|No| Cancel[Status=Cancelled]
    
    DownloadData --> Progress[Report Progress]
    Progress --> CheckProgress{1000ms passed?}
    CheckProgress -->|Yes| FormatBytes[ProgressFormatter.FormatBytes]
    CheckProgress -->|Yes| GenerateBar[ProgressFormatter.GenerateProgressBar]
    FormatBytes --> GenerateBar
    GenerateBar --> Report[Progress Report]
    CheckProgress -->|No| DownloadData
    
    DownloadData --> Success[Return outputPath, Status=Success]
    
    style Start fill:#e1f5ff
    style Parse fill:#e1f5ff
    style TokenUrl fill:#e1f5ff
    style Download fill:#e1f5ff
    style HttpClient fill:#fff4e1
    style Headers fill:#fff4e1
    style ExtractFilename fill:#fff4e1
    style ExtractUrl fill:#fff4e1
    style SaveFile fill:#fff4e1
    style UserConfirm fill:#fff4e1
    style DownloadData fill:#fff4e1
    style Progress fill:#fff4e1
    style FormatBytes fill:#fff4e1
    style GenerateBar fill:#fff4e1
    style Report fill:#fff4e1
    style Success fill:#e1f5ff
    style Cancel fill:#ffe1e1
    style Failed fill:#ffe1e1
```

## 進捗報告フロー

```mermaid
flowchart TD
    Start[Download Start] --> ReadBuffer[Read Buffer]
    ReadBuffer --> WriteStream[Write to FileStream]
    WriteStream --> UpdateRead[Update totalRead]
    UpdateRead --> CheckTime{Time >= 1000ms<br/>since last report?}
    
    CheckTime -->|Yes| Report[Report Progress<br/>progress, downloaded, total]
    CheckTime -->|No| NextRead[Next Buffer Read]
    
    Report --> NextRead
    NextRead --> CheckRead{Bytes Read > 0?}
    
    CheckRead -->|Yes| ReadBuffer
    CheckRead -->|No| FinalReport[Final Progress Report<br/>100%]
    
    FinalReport --> End[Download Complete]
    
    style Start fill:#e1f5ff
    style ReadBuffer fill:#fff4e1
    style WriteStream fill:#fff4e1
    style UpdateRead fill:#fff4e1
    style Report fill:#fff4e1
    style FinalReport fill:#fff4e1
    style End fill:#e1f5ff