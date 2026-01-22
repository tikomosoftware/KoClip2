# リリース手順

このガイドでは、KoClip2の新しいバージョンをリリースする方法を説明します。

## 自動リリースの仕組み

GitHub Actionsを使用して、タグをプッシュすると自動的に以下が実行されます：

1. ビルドスクリプトを実行
2. 2つのZIPファイルを作成
   - フレームワーク依存版（軽量）
   - 自己完結型版（単一EXE）
3. GitHub Releaseを作成
4. ZIPファイルをReleaseに添付

## リリース手順

### 1. バージョン番号を決定

セマンティックバージョニングに従います：
- **メジャー**: 互換性のない変更（例: 2.0.0 → 3.0.0）
- **マイナー**: 後方互換性のある機能追加（例: 2.0.0 → 2.1.0）
- **パッチ**: バグ修正（例: 2.0.0 → 2.0.1）

### 2. プロジェクトファイルのバージョンを更新

`KoClipCS/KoClipCS.csproj`を編集：

```xml
<PropertyGroup>
  <AssemblyVersion>2.0.3.0</AssemblyVersion>
  <FileVersion>2.0.3.0</FileVersion>
  <Version>2.0.3</Version>
</PropertyGroup>
```

### 3. 変更をコミット

```bash
git add KoClipCS/KoClipCS.csproj
git commit -m "Bump version to 2.0.3"
```

### 4. タグを作成してプッシュ

```bash
# タグを作成（vプレフィックス付き）
git tag v2.0.3

# タグをプッシュ（これでGitHub Actionsが起動）
git push origin v2.0.3
```

### 5. GitHub Actionsの実行を確認

1. GitHubリポジトリの「Actions」タブを開く
2. 「Release Build」ワークフローが実行中であることを確認
3. 完了まで待つ（約5-10分）

### 6. Releaseを確認・編集

1. GitHubリポジトリの「Releases」タブを開く
2. 新しいReleaseが作成されていることを確認
3. リリースノートを編集（変更内容を追加）
4. 必要に応じてスクリーンショットを追加

## ローカルでテストビルド

リリース前にローカルでビルドをテストできます：

```powershell
# デフォルトバージョン（2.0）でビルド
.\build-release.ps1

# 特定のバージョンでビルド
.\build-release.ps1 -Version "2.0.3"
```

ビルド成果物は`dist`フォルダに作成されます。

## トラブルシューティング

### ビルドが失敗する

1. GitHub Actionsのログを確認
2. ローカルで同じバージョンでビルドを試す
3. テストが失敗していないか確認

### タグを間違えた

```bash
# ローカルのタグを削除
git tag -d v2.0.3

# リモートのタグを削除
git push origin :refs/tags/v2.0.3

# 正しいタグを作成し直す
git tag v2.0.3
git push origin v2.0.3
```

### Releaseを削除したい

1. GitHubの「Releases」ページで該当のReleaseを開く
2. 「Delete」ボタンをクリック
3. タグも削除する場合は上記の手順でタグを削除

## 注意事項

- タグは一度プッシュすると自動的にビルドが開始されます
- タグ名は必ず`v`で始める必要があります（例: `v2.0.3`）
- バージョン番号はプロジェクトファイルと一致させることを推奨
- Releaseは自動的に公開されます（draft: false）
