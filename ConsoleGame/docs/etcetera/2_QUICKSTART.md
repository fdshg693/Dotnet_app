# クイックスタートガイド

GameEngineを最速でセットアップして実行する手順です。

## 1分でスタート

```powershell
# 1. MongoDBを起動
docker-compose up -d

# 2. ゲームを実行
cd GameEngine
dotnet run
```

## 完全なセットアップ（初回のみ）

### 前提条件の確認

```powershell
# Dockerの確認
docker --version

# .NET SDKの確認
dotnet --version
```

### ステップ1: プロジェクトのクローン

```powershell
git clone <repository-url>
cd ConsoleGame
```

### ステップ2: MongoDBの起動

```powershell
# Docker Composeで起動
docker-compose up -d

# 起動確認
docker-compose ps
```

**期待される出力:**
```
NAME                        STATUS
mongodb-gameengine          Up (healthy)
mongo-express-gameengine    Up
```

### ステップ3: ゲームの実行

```powershell
# ビルド
dotnet build

# 実行
cd GameEngine
dotnet run
```

## Web UIでデータ確認

ゲームプレイ中にセーブしたデータは、Web UIで確認できます：

1. ブラウザで http://localhost:8081 にアクセス
2. ログイン情報:
   - ユーザー名: `admin`
   - パスワード: `admin`
3. `GameEngineDB` → `PlayerSaves` でセーブデータを確認

## 日常的な使用

### ゲームを開始

```powershell
# MongoDBが起動していない場合
docker-compose up -d

# ゲーム実行
cd GameEngine
dotnet run
```

### ゲーム終了後

```powershell
# MongoDBを停止（データは保持）
docker-compose stop

# または、起動したまま（推奨）
# 次回起動が速くなります
```

## トラブルシューティング

### MongoDBに接続できない

```powershell
# 1. コンテナが起動しているか確認
docker-compose ps

# 2. 起動していない場合
docker-compose up -d

# 3. ログを確認
docker-compose logs mongodb
```

### データをリセットしたい

```powershell
# 全データ削除
docker-compose down -v

# 再起動
docker-compose up -d
```

### ポート競合エラー

ポート27017または8081が既に使用されている場合：

1. `docker-compose.yml` を編集
2. ポート番号を変更（例: `27018:27017`）
3. `game-config.yml` の接続文字列も変更

## 便利なコマンド

```powershell
# MongoDBシェルに接続
docker exec -it mongodb-gameengine mongosh

# セーブデータを確認
docker exec -it mongodb-gameengine mongosh --eval "use GameEngineDB; db.PlayerSaves.find().pretty()"

# ログをリアルタイム表示
docker-compose logs -f

# リソース使用状況
docker stats mongodb-gameengine
```

## 次のステップ

- [MONGODB_SETUP.md](MONGODB_SETUP.md) - MongoDB詳細セットアップガイド
- [docs/mongo.md](docs/mongo.md) - MongoDB操作完全ガイド
- [README.md](README.md) - プロジェクト概要

## よくある質問

### Q: セーブ機能なしでゲームをプレイできますか？

A: はい。MongoDBが起動していなくても、ゲームは正常にプレイできます。セーブ機能のみが無効になります。

### Q: データはどこに保存されますか？

A: Dockerボリューム `consolegame_mongodb_data` に保存されます。コンテナを削除してもデータは保持されます。

### Q: 複数のセーブスロットを使えますか？

A: 現在は `auto_save` スロットのみですが、コードを拡張することで複数スロットに対応可能です。

### Q: バックアップはどうすればいいですか？

A: 詳細は [docs/mongo.md#バックアップとリストア](docs/mongo.md#バックアップとリストア) を参照してください。

簡易版：
```powershell
docker exec mongodb-gameengine mongodump --db=GameEngineDB --archive=/data/backup.archive
docker cp mongodb-gameengine:/data/backup.archive ./backup.archive
```
