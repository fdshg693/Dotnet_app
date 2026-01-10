# MongoDB Save System Setup Guide

このガイドでは、ゲームのセーブデータをMongoDBに保存するための環境構築手順を説明します。

## 前提条件

- Docker Desktop がインストールされていること
- .NET 8.0 SDK がインストールされていること

## MongoDB（Docker Compose）のセットアップ

### 推奨方法: Docker Composeを使用

プロジェクトルートに `docker-compose.yml` が用意されているため、簡単に環境を構築できます。

#### 1. 環境変数の設定（オプション）

デフォルト設定で問題なければスキップ可能です。カスタマイズする場合：

```powershell
# .env.exampleをコピー
Copy-Item .env.example .env

# 必要に応じて編集
notepad .env
```

#### 2. MongoDBとMongo Expressを起動

```powershell
# すべてのサービスを起動
docker-compose up -d

# 起動確認
docker-compose ps
```

これで以下のサービスが起動します：
- **MongoDB**: ポート 27017（ゲームから接続）
- **Mongo Express**: ポート 8081（Web UI）

#### 3. 動作確認

**コマンドラインで確認：**
```powershell
docker exec -it mongodb-gameengine mongosh --eval "db.adminCommand('ping')"
```

**Web UIで確認：**
1. ブラウザで http://localhost:8081 にアクセス
2. ユーザー名: `admin`, パスワード: `admin`
3. GameEngineDBデータベースを確認

3. GameEngineDBデータベースを確認

### 代替方法: docker runコマンドを使用

Docker Composeを使用しない場合は、以下のコマンドでも起動できます：

```powershell
docker run -d `
  --name mongodb-gameengine `
  -p 27017:27017 `
  -v mongodb_data:/data/db `
  mongo:latest
```

### 3. MongoDB接続設定（確認）

デフォルトでは `localhost:27017` に接続します。`game-config.yml` で確認：

```yaml
MongoDB:
  ConnectionString: "mongodb://localhost:27017"
  DatabaseName: "GameEngineDB"
  CollectionName: "PlayerSaves"
```

## セーブ機能の使用方法

### ゲーム中のセーブ

各バトルやイベント後に、以下の選択肢が表示されます：

- **Continue**: ゲームを続ける（セーブなし）
- **Save & Continue**: セーブしてゲームを続ける
- **Save & Quit**: セーブしてゲームを終了
- **Quit**: セーブせずにゲームを終了

矢印キー（↑↓）で選択し、Enterキーで決定します。

### セーブデータの内容

以下のデータがMongoDBに保存されます：

- プレイヤー名
- HP（現在値・最大値）
- AP・DP（基礎値）
- 所持金・ポーション数
- レベル・経験値
- 装備武器情報
- 攻撃戦略
- 保存日時

## MongoDB管理

### Docker Composeコマンド

```powershell
# サービス起動
docker-compose up -d

# サービス停止（データは保持）
docker-compose stop

# サービス再起動
docker-compose restart

# ログ確認
docker-compose logs -f mongodb

# サービス削除（データは保持）
docker-compose down

# サービス削除＋データ削除
docker-compose down -v

# ステータス確認
docker-compose ps
```

### 個別のコンテナ操作

```powershell
# コンテナの停止
docker stop mongodb-gameengine

# コンテナの再起動
docker start mongodb-gameengine

# コンテナの削除（データも削除されます）
docker stop mongodb-gameengine
docker rm mongodb-gameengine
```

### MongoDBに直接アクセス

```powershell
# MongoDBシェルに接続
docker exec -it mongodb-gameengine mongosh

# GameEngineDBに切り替え
use GameEngineDB

# セーブデータを確認
db.PlayerSaves.find().pretty()
```

### Mongo Express（Web UI）

1. ブラウザで http://localhost:8081 にアクセス
2. ユーザー名: `admin`, パスワード: `admin`
3. GUIでデータベースを管理

## データの永続化

Docker Composeを使用している場合、データは自動的に永続化されます：

- `mongodb_data` ボリュームにデータベースファイルを保存
- `mongodb_config` ボリュームに設定ファイルを保存
- コンテナを削除してもデータは保持されます

### バックアップ

```powershell
# バックアップディレクトリ作成
mkdir backup

# データベースをバックアップ
docker exec mongodb-gameengine mongodump `
  --db=GameEngineDB `
  --archive=/data/backup/backup.archive

# ホストにコピー
docker cp mongodb-gameengine:/data/backup/backup.archive ./backup/
```

### リストア

```powershell
# バックアップをコンテナにコピー
docker cp ./backup/backup.archive mongodb-gameengine:/data/restore/

# リストア実行
docker exec mongodb-gameengine mongorestore `
  --db=GameEngineDB `
  --archive=/data/restore/backup.archive
```

## トラブルシューティング

### MongoDB接続エラーが発生する

1. Dockerコンテナが起動しているか確認：
   ```powershell
   docker-compose ps
   # または
   docker ps | Select-String "mongodb-gameengine"
   ```

2. ポート27017が使用可能か確認：
   ```powershell
   netstat -an | Select-String "27017"
   ```

3. ログを確認：
   ```powershell
   docker-compose logs mongodb
   ```

4. ヘルスチェック：
   ```powershell
   docker inspect mongodb-gameengine | Select-String "Health"
   ```

5. `game-config.yml` の接続文字列を確認

### サービスが起動しない

```powershell
# ログで原因を確認
docker-compose logs mongodb

# 強制再作成
docker-compose up -d --force-recreate mongodb

# 完全にクリーンアップして再起動
docker-compose down -v
docker-compose up -d
```

### セーブ機能が利用できない

ゲーム起動時に以下のメッセージが表示される場合、MongoDB接続に失敗しています：

```
Warning: セーブ機能を初期化できませんでした
ゲームはセーブ機能なしで続行されます。
```

上記のトラブルシューティング手順を実行してください。

## 開発者向け情報

### プロジェクト構成

```
ConsoleGame/
├── docker-compose.yml          # Docker Compose設定
├── Dockerfile.mongo            # MongoDBカスタムDockerfile
├── .env.example                # 環境変数テンプレート
├── mongo-init/
│   └── init-db.js             # DB初期化スクリプト
└── GameEngine/
    ├── game-config.yml         # ゲーム設定（MongoDB接続情報含む）
    ├── Manager/
    │   └── SaveDataManager.cs # MongoDB操作クラス
    └── Models/
        └── PlayerSaveData.cs   # セーブデータモデル
```

### 実装されているクラス

- `SaveDataManager`: MongoDB操作を管理
- `PlayerSaveData`: セーブデータモデル
- `GameSystem`: セーブ機能統合
- `UserInteraction.SelectGameAction()`: UI選択

### セーブデータの拡張

`PlayerSaveData.cs` と `Player.GetSaveData()` を編集することで、保存するデータを追加できます。

### より詳細な情報

MongoDB操作の詳細については、[docs/mongo.md](docs/mongo.md) を参照してください：
- MongoDBの基本操作
- データ操作（CRUD）
- バックアップ・リストア
- パフォーマンスチューニング
- トラブルシューティング
