# GameEngine - Refactoring Summary

## 改善内容

このリファクタリングでは、以下の問題点を修正しました：

### 1. エラーハンドリングの強化

#### EnemyFactory
- **改善前**: 基本的なtry-catchのみ
- **改善後**:
  - ファイル存在チェック
  - YAML解析エラーの詳細なハンドリング
  - 敵仕様の妥当性検証（HP, AP, DP, 戦略名など）
  - 詳細なエラーメッセージ
  - 利用可能な敵キー一覧の提供

#### GameConfigLoader
- フォールバック機能（設定ファイルが見つからない場合はデフォルト値を使用）
- 設定値の妥当性検証
- シングルトンパターンによる安全な設定管理

### 2. ユーザー入力検証の強化

#### UserInteraction クラス
新しいメソッドを追加：
- `ReadPositiveInteger`: 
  - 範囲指定機能（minValue, maxValue）
  - 試行回数制限
  - より詳細なエラーメッセージ
- `ReadConfirmation`: Yes/No形式の確認入力
- `ReadChoice`: 複数選択肢からの選択機能

### 3. 責任の分離

#### GameSystem の分離
- **BattleManager**: 戦闘ロジックを完全に分離
  - ターン管理
  - 戦闘結果の構造化（BattleResult）
  - 勝敗の処理
- **EventManager**: イベント管理を分離
  - 重み付けによるイベント選択
  - ショップイベント処理
  - 戦闘イベント処理

#### Player クラスの責任分離
新しいManagerを追加：
- **CombatManager**: 戦闘関連の機能
  - 攻撃実行
  - 戦略変更
- **RewardManager**: 報酬獲得の処理
  - 敵撃破時の報酬
  - レベルアップ処理

### 4. 設定の外部化

#### game-config.yml
ハードコードされていた値をYAMLファイルに移行：
- プレイヤー初期ステータス
- レベルアップボーナス
- アイテム設定
- イベント確率（重み付け方式）
- ショップ設定
- 敵のゴールド計算設定
- 武器ステータス

#### GameConstants の更新
- 設定ファイルから動的に値を読み込む
- 後方互換性を維持
- デフォルト値のフォールバック

## アーキテクチャの改善

### 新しいクラス構造

```
Configuration/
  ├── GameConfigLoader.cs       # 設定管理
  └── (game-config.yml)          # 設定ファイル

Systems/
  ├── GameSystem.cs              # ゲーム全体の進行管理
  ├── EventManager.cs            # イベント管理
  ├── UserInteraction.cs         # ユーザー入力（強化版）
  └── BattleSystem/
      └── BattleManager.cs       # 戦闘管理

Manager/
  ├── HealthManager.cs           # 既存
  ├── InventoryManager.cs        # 既存
  ├── ExperienceManager.cs       # 既存
  ├── CombatManager.cs           # 新規：戦闘機能
  └── RewardManager.cs           # 新規：報酬処理

Factory/
  └── EnemyFactory.cs            # エラーハンドリング強化
```

### 設計パターンの適用

1. **シングルトンパターン**: GameConfigLoader
2. **マネージャーパターン**: 各種Manager（強化）
3. **ファクトリーパターン**: EnemyFactory（改善）
4. **ストラテジーパターン**: AttackStrategy（既存）
5. **結果オブジェクトパターン**: BattleResult

## 拡張性の向上

### 設定ファイルによる調整
ゲームバランスの調整が容易になりました：
- コードの再コンパイル不要
- 設定ファイルを編集するだけ
- 複数の設定プリセットを作成可能

### 新しいイベントタイプの追加
EventManager に新しいイベントを追加しやすくなりました：
```csharp
public enum GameEventType
{
    Shop,
    Battle,
    Treasure,  // 将来の拡張用
    Rest       // 将来の拡張用
}
```

### エラー耐性
- ファイル読み込みエラーへの対応
- 無効な設定値の検証
- フォールバック機能

## テスト容易性の向上

1. **依存性注入**: GameSystem, EventManager, BattleManager
2. **設定の再読み込み**: GameConfigLoader.ReloadConfig()
3. **結果オブジェクト**: BattleResult で戦闘結果を構造化
4. **検証メソッド**: 各種Validate メソッド

## 使用方法

### ゲーム設定のカスタマイズ
`game-config.yml` を編集：
```yaml
Player:
  InitialHP: 150  # プレイヤーの初期HPを変更
  BaseAP: 15      # 初期攻撃力を変更

Events:
  ShopEventWeight: 2  # ショップの出現率を上げる
  BattleEventWeight: 1
```

### 新しいゲームシステムの使用
```csharp
var player = CreatePlayer("Hero");
var gameSystem = new GameSystem(player);
gameSystem.RunGameLoop();  // 自動的にゲームループを実行
```

## 今後の改善案

1. **ログシステムの追加**: 戦闘ログ、イベントログの保存
2. **セーブ/ロード機能**: ゲームの進行状態を保存
3. **マルチ言語対応**: 設定ファイルでメッセージを管理
4. **難易度設定**: 設定ファイルで難易度プリセット
5. **ユニットテストの追加**: 各Managerのテストケース
