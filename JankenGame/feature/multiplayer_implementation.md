# JankenGame - 複数プレイヤー対応実装ガイド

## 実装内容

Jankenゲームを2人対戦から**3人以上でプレイ可能**に拡張しました。

## 変更されたファイル

### 1. `Models/Janken/JankenPlayer.cs`
**プレイヤーモデルの拡張**

- `PlayerType` 列挙型を追加（Human/Computer）
- プレイヤーの一意ID（GUID）を追加
- プレイヤー名フィールドを追加
- コンストラクタでプレイヤー情報を初期化

```csharp
public class JankenPlayer
{
    public string Id { get; }           // 一意なプレイヤーID
    public string Name { get; set; }    // プレイヤー名
    public PlayerType Type { get; set; } // Human or Computer
    public JankenHand? Hand { get; set; } // 出された手
}
```

### 2. `Models/Janken/MultiPlayerGameRecord.cs` (新規作成)
**複数プレイヤーの結果記録**

- `MultiPlayerGameRecord` レコード型
  - 全プレイヤーの手を記録（Dictionary<playerId, hand>）
  - 勝った手を記録
  - 勝者のプレイヤーID一覧を記録
  
- `MultiPlayerGameRecordList` クラス
  - プレイヤーごとの勝敗数を取得（GetWins, GetLosses, GetDraws）

### 3. `Services/Janken/JankenGameService.cs`
**複数プレイヤー対応メソッドを追加**

```csharp
public (JankenHand? winningHand, List<string> winnerIds) 
    DetermineMultiPlayerWinner(List<JankenPlayer> players)
```

既存の `JankenLogicService.GetWinningHands()` を活用して複数プレイヤーの勝者を判定

### 4. `Components/Pages/Janken.razor`
**UIの完全リニューアル**

#### 新機能
- **プレイヤー数選択**: 2〜6人から選択可能
- **複数プレイヤー表示**: 全プレイヤーの手を同時表示
- **スコアボード**: プレイヤーごとの勝敗スコアを表示
- **複数勝者対応**: 同じ手で複数プレイヤーが勝った場合に対応

#### UIコンポーネント
```
1. プレイヤー数選択ドロップダウン
2. グー/パー/チョキボタン（ユーザーのみ操作）
3. 全プレイヤーの手の表示
4. ゲーム結果メッセージ
5. スコアボード（表形式）
6. 履歴表示（最大5件）
```

### 5. `Components/Pages/Janken.razor.css` (新規作成)
**複数プレイヤー対応のスタイル**

- ゲーム設定セクション
- プレイヤーの手表示用スタイル
- スコアボード（テーブル形式）
- 履歴表示のスタイル

## ゲームロジック

### 複数プレイヤーでの勝敗判定

既存の `JankenLogicService.GetWinningHands()` メソッドを活用：

```
1. 全員同じ手 → 引き分け
2. 3種類全部出た → 引き分け
3. 2種類の手 → 勝った手を判定
   - グー < パー < チョキ < グー
```

### 流れ

1. ユーザーが手を選択
2. コンピューターが自動的に各プレイヤーの手を決定（`rnd.Next(0, 3)`）
3. すべてのプレイヤーの手から勝者を判定
4. スコアボードと履歴を更新

## 使用方法

### 基本操作

1. **対戦人数を選択**: ドロップダウンから2〜6人を選択
2. **手を選ぶ**: 「グー」「パー」「チョキ」いずれかのボタンをクリック
3. **結果確認**: 各プレイヤーの手と勝者が表示される
4. **次のラウンド**: 「リセット」ボタンで初期化し、新しいラウンドを開始

### スコアボード

各プレイヤーの累計成績が表示されます：
- **勝**: 当該ラウンドで勝利した回数
- **敗**: 当該ラウンドで敗北した回数
- **引き分け**: 引き分けになったラウンド数

## 技術的な注意点

### 互換性
- 既存の `JankenRecordList` は従来通り2人対戦で使用可能
- 新しい `MultiPlayerGameRecordList` が複数プレイヤーに対応

### スケーラビリティ
- プレイヤー数は理論的に無制限に対応可能
- ドロップダウンで2〜6人に制限（UIの都合）
- `JankenLogicService` は任意数のプレイヤーに対応

### パフォーマンス
- 各ラウンドは O(n) の処理（nはプレイヤー数）
- 現在のプレイヤー数制限（6人）では問題なし

## 将来の拡張案

- [ ] ゲームルール設定（引き分けの扱い）
- [ ] プレイヤー名のカスタマイズ機能
- [ ] 複数ラウンドのトーナメント機能
- [ ] 統計・分析機能
