# KoClip Release Build Script
# このスクリプトはKoClipアプリケーションのリリースパッケージを作成します
# 2つのビルドを作成: フレームワーク依存版（軽量）と自己完結型版（単一EXE）

param(
    [string]$Version = "",
    [string]$Configuration = "Release"
)

# 変数定義
$ProjectPath = "KoClipCS"
$ProjectFile = "$ProjectPath\KoClipCS.csproj"

# バージョンが指定されていない場合、プロジェクトファイルから取得
if ([string]::IsNullOrEmpty($Version)) {
    Write-Host "バージョンが指定されていません。プロジェクトファイルから取得します..." -ForegroundColor Yellow
    
    if (Test-Path $ProjectFile) {
        [xml]$projectXml = Get-Content $ProjectFile
        $Version = $projectXml.Project.PropertyGroup.Version
        
        if ([string]::IsNullOrEmpty($Version)) {
            Write-Error "プロジェクトファイルからバージョンを取得できませんでした"
            exit 1
        }
        
        Write-Host "プロジェクトファイルからバージョンを取得: $Version" -ForegroundColor Green
    } else {
        Write-Error "プロジェクトファイルが見つかりません: $ProjectFile"
        exit 1
    }
}

# スクリプトの実行ポリシーを確認
Write-Host ""
Write-Host "=== KoClip Dual Release Build Script ===" -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""
$TestProjectPath = "KoClipCS.Tests"
$TestProjectFile = "$TestProjectPath\KoClipCS.Tests.csproj"
$DistDir = "dist"

# フレームワーク依存ビルド用
$TempFrameworkDir = "$DistDir\temp_framework"
$FrameworkZipFile = "$DistDir\KoClip-v$Version-framework-dependent-release.zip"

# 自己完結型ビルド用
$TempStandaloneDir = "$DistDir\temp_standalone"
$StandaloneZipFile = "$DistDir\KoClip-v$Version-standalone-release.zip"

# ビルド開始時刻を記録
$BuildStartTime = Get-Date

# 必要なファイルの存在確認
Write-Host "1. プロジェクトファイルの確認..." -ForegroundColor Cyan
if (-not (Test-Path $ProjectFile)) {
    Write-Error "プロジェクトファイルが見つかりません: $ProjectFile"
    exit 1
}
Write-Host "   ✓ プロジェクトファイル確認完了" -ForegroundColor Green

# 既存のビルド成果物をクリーンアップ
Write-Host ""
Write-Host "2. クリーンアップ..." -ForegroundColor Cyan
try {
    # distディレクトリが存在する場合は中身を完全に削除
    if (Test-Path $DistDir) {
        Write-Host "   既存のdistフォルダの中身をクリア中..." -ForegroundColor Gray
        Remove-Item -Path "$DistDir\*" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   ✓ distフォルダの中身をクリア完了" -ForegroundColor Green
    } else {
        # distディレクトリを作成
        New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
        Write-Host "   ✓ distディレクトリを作成: $DistDir" -ForegroundColor Green
    }
} catch {
    Write-Error "クリーンアップ中にエラーが発生しました: $($_.Exception.Message)"
    exit 1
}

# dotnet clean
Write-Host ""
Write-Host "3. dotnet clean..." -ForegroundColor Cyan
try {
    dotnet clean $ProjectFile --configuration $Configuration --verbosity minimal
    Write-Host "   ✓ dotnet clean 完了" -ForegroundColor Green
} catch {
    Write-Error "dotnet clean でエラーが発生しました: $($_.Exception.Message)"
    exit 1
}

# テストの実行（オプション）
Write-Host ""
Write-Host "4. テストの実行..." -ForegroundColor Cyan
if (Test-Path $TestProjectFile) {
    try {
        dotnet test $TestProjectFile --configuration $Configuration --verbosity minimal --no-restore
        Write-Host "   ✓ テスト実行完了" -ForegroundColor Green
    } catch {
        Write-Warning "テストでエラーが発生しましたが、ビルドを継続します: $($_.Exception.Message)"
    }
} else {
    Write-Host "   ⚠ テストプロジェクトが見つかりません。スキップします。" -ForegroundColor Yellow
}

# ========================================
# フレームワーク依存ビルド（.NET非同梱版・軽量版）
# ========================================
Write-Host ""
Write-Host "5. .NET非同梱版ビルド（軽量版）..." -ForegroundColor Cyan
$frameworkBuildSuccess = $false
try {
    Write-Host "   .NET Runtimeを含めない軽量版を作成中..." -ForegroundColor Gray
    dotnet publish $ProjectFile `
        -c $Configuration `
        -r win-x64 `
        --self-contained false `
        -p:PublishSingleFile=false `
        -o $TempFrameworkDir `
        --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ .NET非同梱版ビルド完了" -ForegroundColor Green
        
        # README.mdをコピー
        if (Test-Path "README.md") {
            Copy-Item -Path "README.md" -Destination (Join-Path $TempFrameworkDir "README.md") -Force
        }
        
        # ZIPファイルの作成
        Compress-Archive -Path "$TempFrameworkDir\*" -DestinationPath $FrameworkZipFile -Force
        Write-Host "   ✓ ZIPファイル作成完了: $FrameworkZipFile" -ForegroundColor Green
        $frameworkBuildSuccess = $true
    } else {
        throw "dotnet publishが失敗しました"
    }
} catch {
    Write-Host "   ✗ .NET非同梱版ビルドが失敗しました: $($_.Exception.Message)" -ForegroundColor Red
}

# ========================================
# 自己完結型ビルド（.NET同梱版・単一EXE版）
# ========================================
Write-Host ""
Write-Host "6. .NET同梱版ビルド（単一EXE版）..." -ForegroundColor Cyan
$standaloneBuildSuccess = $false
try {
    Write-Host "   .NET Runtimeを含む単一EXEを作成中..." -ForegroundColor Gray
    dotnet publish $ProjectFile `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $TempStandaloneDir `
        --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ .NET同梱版ビルド完了" -ForegroundColor Green
        
        # README.mdをコピー
        if (Test-Path "README.md") {
            Copy-Item -Path "README.md" -Destination (Join-Path $TempStandaloneDir "README.md") -Force
        }
        
        # ZIPファイルの作成
        Compress-Archive -Path "$TempStandaloneDir\*" -DestinationPath $StandaloneZipFile -Force
        Write-Host "   ✓ ZIPファイル作成完了: $StandaloneZipFile" -ForegroundColor Green
        $standaloneBuildSuccess = $true
    } else {
        throw "dotnet publishが失敗しました"
    }
} catch {
    Write-Host "   ✗ .NET同梱版ビルドが失敗しました: $($_.Exception.Message)" -ForegroundColor Red
}

# 両方のビルドが失敗した場合はエラー終了
if (-not $frameworkBuildSuccess -and -not $standaloneBuildSuccess) {
    Write-Error "両方のビルドが失敗しました"
    exit 1
}

# 一時ディレクトリをクリーンアップ
Write-Host ""
Write-Host "7. 一時ファイルのクリーンアップ..." -ForegroundColor Cyan
try {
    if (Test-Path $TempFrameworkDir) {
        Remove-Item -Path $TempFrameworkDir -Recurse -Force
        Write-Host "   ✓ .NET非同梱版の一時ファイルを削除" -ForegroundColor Green
    }
    
    if (Test-Path $TempStandaloneDir) {
        Remove-Item -Path $TempStandaloneDir -Recurse -Force
        Write-Host "   ✓ .NET同梱版の一時ファイルを削除" -ForegroundColor Green
    }
} catch {
    Write-Warning "一時ファイルの削除中にエラーが発生しました: $($_.Exception.Message)"
}

# ビルド結果のサマリー表示
Write-Host ""
Write-Host "8. ビルドサマリー..." -ForegroundColor Cyan

# ビルド時間を計算
$BuildEndTime = Get-Date
$BuildDuration = $BuildEndTime - $BuildStartTime
$BuildTimeSeconds = [math]::Round($BuildDuration.TotalSeconds, 1)

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ビルド完了！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# フレームワーク依存ビルドの情報
if ($frameworkBuildSuccess -and (Test-Path $FrameworkZipFile)) {
    $frameworkZipInfo = Get-Item $FrameworkZipFile
    $frameworkZipHash = Get-FileHash $FrameworkZipFile -Algorithm SHA256
    
    Write-Host "📦 .NET非同梱版（軽量版）:" -ForegroundColor Cyan
    Write-Host "   ファイル名: $($frameworkZipInfo.Name)" -ForegroundColor White
    Write-Host "   サイズ: $([math]::Round($frameworkZipInfo.Length / 1MB, 2)) MB" -ForegroundColor White
    Write-Host "   SHA256: $($frameworkZipHash.Hash)" -ForegroundColor Gray
    Write-Host "   ⚠ 実行には .NET 9.0 Desktop Runtime が必要です" -ForegroundColor Yellow
    Write-Host ""
}

# 自己完結型ビルドの情報
if ($standaloneBuildSuccess -and (Test-Path $StandaloneZipFile)) {
    $standaloneZipInfo = Get-Item $StandaloneZipFile
    $standaloneZipHash = Get-FileHash $StandaloneZipFile -Algorithm SHA256
    
    Write-Host "📦 .NET同梱版（単一EXE版）:" -ForegroundColor Cyan
    Write-Host "   ファイル名: $($standaloneZipInfo.Name)" -ForegroundColor White
    Write-Host "   サイズ: $([math]::Round($standaloneZipInfo.Length / 1MB, 2)) MB" -ForegroundColor White
    Write-Host "   SHA256: $($standaloneZipHash.Hash)" -ForegroundColor Gray
    Write-Host "   ✓ .NET Runtimeのインストール不要で実行可能" -ForegroundColor Green
    Write-Host ""
}

Write-Host "⏱ 合計ビルド時間: $BuildTimeSeconds 秒" -ForegroundColor White
Write-Host ""
Write-Host "次のステップ:" -ForegroundColor Cyan
Write-Host "1. 各ZIPファイルをテスト環境で動作確認" -ForegroundColor White
Write-Host "2. GitHubのReleasesページにアップロード" -ForegroundColor White
Write-Host "3. リリースノートの作成" -ForegroundColor White
Write-Host ""