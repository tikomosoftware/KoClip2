# KoClip ビルドスクリプト使用方法

このドキュメントでは、KoClipアプリケーションのリリースパッケージを作成するためのビルドスクリプトの使用方法を説明します。

## 📁 ファイル構成

- `build-release.ps1` - 基本的なリリースビルドスクリプト
- `build-release.bat` - PowerShellスクリプトを実行するバッチファイル
- `build-release-advanced.ps1` - 開発者向け詳細ビルドスクリプト
- `BUILD-README.md` - このファイル（使用方法説明）

## 🚀 基本的な使用方法

### 方法1: バッチファイルを使用（最も簡単）

```cmd
build-release.bat
```

ダブルクリックまたはコマンドプロンプトから実行してください。

### 方法2: PowerShellスクリプトを直接実行

```powershell
# 基本的なビルド
.\build-release.ps1

# バージョンを指定してビルド
.\build-release.ps1 -Version "2.1"
```

### 方法3: 詳細ビルドスクリプトを使用

```powershell
# 基本的な詳細ビルド
.\build-release-advanced.ps1

# オプション付きビルド
.\build-release-advanced.ps1 -Version "2.1" -SkipTests -OpenFolder
```

## ⚙️ スクリプトオプション

### build-release.ps1 のオプション

| パラメータ | デフォルト値 | 説明 |
|-----------|-------------|------|
| `-Version` | "2.0" | リリースバージョン番号 |
| `-Configuration` | "Release" | ビルド構成（Release/Debug） |

### build-release-advanced.ps1 のオプション

| パラメータ | デフォルト値 | 説明 |
|-----------|-------------|------|
| `-Version` | "2.0" | リリースバージョン番号 |
| `-Configuration` | "Release" | ビルド構成（Release/Debug） |
| `-SkipTests` | false | テストをスキップする |
| `-SkipClean` | false | クリーンアップをスキップする |
| `-OpenFolder` | false | 完了後にリリースフォルダを開く |
| `-OutputPath` | "" | カスタム出力パス |

## 📋 ビルドプロセス

スクリプトは以下の手順でリリースパッケージを作成します：

1. **環境チェック** - .NET SDK とプロジェクトファイルの確認
2. **クリーンアップ** - 既存のビルド成果物を削除
3. **依存関係の復元** - NuGetパッケージの復元
4. **テスト実行** - 単体テストの実行（オプション）
5. **ビルド** - リリース構成でのビルド
6. **発行** - アプリケーションの発行
7. **パッケージ作成** - 必要なファイルをリリースフォルダにコピー
8. **ZIP作成** - リリースパッケージのZIPファイル作成
9. **検証** - パッケージの整合性確認

## 📦 出力ファイル

ビルド完了後、以下のファイルが作成されます：

- `KoClip-v{Version}-Release/` - リリースファイル一式
- `KoClip-v{Version}-Release.zip` - 配布用ZIPファイル

### リリースパッケージ内容

```
KoClip-v2.0-Release/
├── KoClipCS.exe              # メイン実行ファイル
├── KoClipCS.dll              # アプリケーションライブラリ
├── KoClipCS.deps.json        # 依存関係情報
├── KoClipCS.runtimeconfig.json # ランタイム設定
├── SixLabors.ImageSharp.dll  # WebP対応ライブラリ
├── README.md                 # ユーザー向け説明書
└── VERSION.txt               # バージョン情報（詳細ビルドのみ）
```

## 🔧 トラブルシューティング

### PowerShell実行ポリシーエラー

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### .NET SDK が見つからない

1. [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) をインストール
2. コマンドプロンプトで `dotnet --version` を実行して確認

### ビルドエラー

1. Visual Studio または VS Code でプロジェクトを開く
2. エラーメッセージを確認
3. 必要に応じて依存関係を更新

### テストエラー

```powershell
# テストをスキップしてビルド
.\build-release-advanced.ps1 -SkipTests
```

## 🎯 使用例

### 開発版ビルド

```powershell
# デバッグ版でテストをスキップ
.\build-release-advanced.ps1 -Configuration "Debug" -SkipTests -Version "2.1-dev"
```

### 本番リリース

```powershell
# 完全なテスト付きリリースビルド
.\build-release-advanced.ps1 -Version "2.1" -OpenFolder
```

### CI/CD環境

```powershell
# 自動化環境向け（対話なし）
.\build-release.ps1 -Version $env:BUILD_VERSION -Configuration "Release"
```

## 📝 注意事項

1. **管理者権限は不要** - 通常のユーザー権限で実行可能
2. **インターネット接続** - 初回実行時はNuGetパッケージのダウンロードが必要
3. **ディスク容量** - 約100MB程度の空き容量が必要
4. **ウイルス対策ソフト** - 実行ファイル作成時に誤検知される場合があります

## 🔄 継続的インテグレーション

GitHub ActionsなどのCI/CDパイプラインで使用する場合：

```yaml
- name: Build Release Package
  run: |
    .\build-release.ps1 -Version "${{ github.ref_name }}"
  shell: pwsh
```

## 📞 サポート

ビルドスクリプトに関する問題や改善提案がある場合は、GitHubのIssuesページでお知らせください。

---

© 2026 tikomo software

