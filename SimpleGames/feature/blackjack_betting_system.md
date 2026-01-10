# ブラックジャック チップベッティングシステム 実装方針

## 概要

ブラックジャックにポーカー風のチップベッティングシステムを追加する。プレイヤーはチップを持ち、入場料を支払い、ラウンドごとにベット・レイズ・フォールド（降りる）の選択ができるようにする。

## 要件定義

### 基本ルール

1. **チップ管理**
   - 各プレイヤーは初期チップ（例: 1000チップ）を持つ
   - チップがマイナスになるベットは不可
   - チップが0になったプレイヤーは参加不可

2. **ゲームフロー**
   1. **アンティ（入場料）**: ゲーム開始時に全員が一定額（例: 10チップ）を支払う
   2. **初期配牌**: 全員に2枚ずつカードを配る
   3. **ベッティングラウンド**: 各プレイヤーが順番に行動
      - **フォールド（降りる）**: 既に払ったチップを捨ててラウンドから降りる
      - **コール（揃える）**: 現在の最高ベット額に揃える
      - **レイズ（上乗せ）**: 現在の最高ベット額より多く賭ける
   4. **追加カード（ヒット）**: コール/レイズしたプレイヤーのみカードを引ける
   5. **レイズ後の再ベッティング**: 誰かがレイズした場合、他の参加中プレイヤーも再度ベット
   6. **ディーラーターン**: 全プレイヤーの行動完了後、ディーラーが自動プレイ
   7. **精算**: 勝者に pot（賭け金総額）を分配

### ベットルール詳細

- **最小レイズ額**: 10チップ
- **最大レイズ額**: 所持チップ全額（オールイン）
- **レイズ後の対応**: 既にコールしていたプレイヤーも再度コール/レイズ/フォールドを選択
- **ヒットのタイミング**: ベットアクション完了後のみ

## アーキテクチャ設計

### 新規モデル

#### 1. `BettingAction` (enum)
```csharp
public enum BettingAction
{
    Fold,      // 降りる
    Call,      // 揃える
    Raise,     // 上乗せ
    Check,     // 追加ベットなし（現在のベット額が0の場合のみ）
    AllIn      // 全額賭け
}
```

#### 2. `PlayerBettingState` (class)
```csharp
public class PlayerBettingState
{
    public BlackJackPlayer Player { get; set; }
    public int TotalChips { get; set; }          // 所持チップ
    public int CurrentBet { get; set; }          // このラウンドで賭けた額
    public bool HasFolded { get; set; }          // 降りたか
    public bool IsAllIn { get; set; }            // オールインか
    public bool HasActedThisRound { get; set; }  // この周で行動したか
}
```

#### 3. `BettingRound` (class)
```csharp
public class BettingRound
{
    public int CurrentBet { get; set; }          // 現在の最高ベット額
    public int Pot { get; set; }                 // ポット（賭け金総額）
    public List<PlayerBettingState> PlayerStates { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public bool RoundComplete { get; set; }      // 全員の行動完了
}
```

#### 4. `BlackJackGameState` (enum) - 拡張
```csharp
public enum BlackJackGameState
{
    Waiting,           // ゲーム開始前
    Ante,              // アンティ（入場料）支払い
    InitialDeal,       // 初期配牌
    BettingRound,      // ベッティング中
    PlayersTurn,       // プレイヤーのカードアクション（ヒット/スタンド）
    DealerTurn,        // ディーラーターン
    Showdown,          // 結果表示と精算
    GameOver           // ゲーム終了
}
```

### 既存モデルの拡張

#### `BlackJackPlayer` クラス
```csharp
public class BlackJackPlayer : BlackJackParticipant
{
    public int Chips { get; set; } = 1000;       // 所持チップ（初期値1000）
    public int CurrentBet { get; set; } = 0;     // 現在のベット額
    public bool HasFolded { get; set; } = false; // フォールドしたか
    
    // 既存プロパティ
    // Name, Cards, Score, IsBust, IsBlackjack, Wins, Losses, Draws...
}
```

### 新規サービス

#### `BlackJackBettingService` (新規)
ベッティングロジックを管理

**主要メソッド:**
- `void StartAnte(int anteAmount)` - アンティ徴収
- `bool CanPlayerBet(BlackJackPlayer player, int amount)` - ベット可能か判定
- `void PlaceBet(BlackJackPlayer player, int amount, BettingAction action)` - ベット処理
- `bool IsBettingRoundComplete()` - 全員の行動完了判定
- `int GetCurrentBet()` - 現在の最高ベット額取得
- `int GetPot()` - ポット総額取得
- `void DistributePot(List<BlackJackPlayer> winners)` - 精算処理
- `List<BlackJackPlayer> GetActivePlayers()` - フォールドしていないプレイヤー取得

### 既存サービスの変更

#### `BlackJackGameService` (拡張)
ベッティングシステムを統合

**追加プロパティ:**
- `BlackJackBettingService BettingService { get; }` - ベッティングサービス

**変更メソッド:**
- `void StartGame()` - アンティ徴収と初期配牌
- `void Hit()` - ベット完了後のみヒット可能に変更
- `void Stand()` - 次のベッティングラウンドへ移行

**追加メソッド:**
- `void PlaceBet(int amount, BettingAction action)` - プレイヤーのベット
- `bool CanCurrentPlayerAct()` - 現在のプレイヤーが行動可能か
- `int GetMinimumBet()` - 現在の最小ベット額（コール額）
- `void ProcessShowdown()` - 精算処理

#### `BlackJackGameStateManager` (拡張)
状態遷移にベッティング状態を追加

**追加メソッド:**
- `void StartAnte()` - アンティ状態へ
- `void StartBettingRound()` - ベッティング開始
- `void StartShowdown()` - 精算処理開始

## UI設計

### Blackjack.razor の変更

#### 表示要素の追加

1. **プレイヤー情報拡張**
   - 所持チップ表示
   - 現在のベット額表示
   - フォールド状態表示

2. **ポット情報**
   - 現在のポット総額
   - 現在の最高ベット額

3. **ベットコントロール**
   - フォールドボタン
   - コールボタン（現在のベット額表示）
   - レイズボタン + 金額入力
   - クイックレイズボタン（+10, +50, +100）
   - オールインボタン

#### ゲームフロー UI

```
[ゲーム開始前]
- プレイヤー人数選択
- 各プレイヤーの初期チップ設定（オプション）

[アンティ支払い]
- 「アンティ: 10チップを支払います」表示
- 自動で次へ

[ベッティングラウンド]
- 現在のプレイヤー強調表示
- 利用可能なアクション表示
  - フォールド | コール (XX チップ) | レイズ | オールイン
- レイズ選択時: 金額入力フィールド表示

[カードアクション]
- ヒット | スタンド
- （ベット確定後のみ利用可能）

[精算]
- 勝者表示
- 獲得チップ表示
- 次のゲームへボタン
```

### CSS 追加要素

```css
.chips-display {
    /* チップ表示スタイル */
}

.betting-controls {
    /* ベットボタングループ */
}

.pot-display {
    /* ポット情報表示 */
}

.folded-player {
    /* フォールドしたプレイヤーのスタイル（半透明など） */
}

.raise-input {
    /* レイズ金額入力 */
}
```

## 実装ステップ

### Phase 1: モデル層の実装
1. ✅ `BettingAction` enum 作成
2. ✅ `PlayerBettingState` class 作成
3. ✅ `BettingRound` class 作成
4. ✅ `BlackJackGameState` enum 拡張
5. ✅ `BlackJackPlayer` にチップ関連プロパティ追加

### Phase 2: サービス層の実装
1. ✅ `BlackJackBettingService` 作成
   - アンティ処理
   - ベット処理（フォールド、コール、レイズ）
   - ベッティングラウンド完了判定
   - 精算処理
2. ✅ `BlackJackGameService` 拡張
   - BettingService 統合
   - ゲームフロー変更
3. ✅ `BlackJackGameStateManager` 拡張
   - 新しい状態遷移追加

### Phase 3: UI層の実装
1. ✅ `Blackjack.razor` 拡張
   - チップ表示追加
   - ベットコントロール追加
   - ポット表示追加
2. ✅ `Blackjack.razor.css` 拡張
   - ベット関連スタイル追加

### Phase 4: テストと調整
1. ✅ 基本的なベットフロー確認
2. ✅ エッジケース対応
   - オールイン処理
   - チップ不足時の処理
   - 全員フォールド時の処理
3. ✅ UI/UX 改善

## 設計上の考慮事項

### 1. レイズ後の処理
- レイズが発生した場合、既にコールしていたプレイヤーも再度行動する必要がある
- `HasActedThisRound` フラグで管理
- 全員が同じベット額になるまでラウンドは続く

### 2. オールイン処理
- プレイヤーが全額ベットした場合、それ以上のアクションは不要
- ただし、他のプレイヤーはそれ以上にレイズ可能
- サイドポット（複数の異なる賭け金レベル）は Phase 1 では未実装

### 3. フォールド後の処理
- フォールドしたプレイヤーはカードを見せない
- 統計（Losses）にはカウントしない、または別カテゴリにする
- 賭けたチップは戻らない

### 4. ディーラーの扱い
- ディーラーはベットに参加しない
- ポットは勝利プレイヤー間で分配
- ディーラーはあくまで判定役

### 5. 複数勝者の処理
- 同点の場合、ポットを均等分配
- 端数は最初の勝者に配分

### 6. 初期チップ設定
- デフォルト: 1000チップ
- UI で変更可能にする（オプション）

## 将来の拡張案

### Phase 2 以降の機能
- **サイドポット**: 異なる金額でオールインした場合の複数ポット処理
- **ブラインド制**: アンティの代わりにスモールブラインド・ビッグブラインドを導入
- **チップ補充**: チップが尽きた場合のリバイ（再購入）システム
- **ベット履歴**: 各ラウンドのベット履歴表示
- **AI強化**: ベット額に応じたAIの判断ロジック
- **トーナメントモード**: 複数ゲーム連続プレイでランキング

## 参考資料

- ポーカーのベッティングルール
- 既存の `multiplayer_implementation.md` - マルチプレイヤー実装パターン
- 既存の `blackjack_multiplayer_refactoring.md` - ブラックジャック構造

## 実装時の注意点

1. **サービスインスタンス化**: サービスは `Program.cs` に登録されていないため、コンポーネント内で直接インスタンス化
2. **状態管理**: Blazor Server の `StateHasChanged()` を適切に呼び出す
3. **日本語UI**: すべてのメッセージは日本語で実装
4. **既存機能の保持**: 統計情報（勝率など）は既存通り維持
5. **CSS スコープ**: `.razor.css` ファイルでスコープドスタイルを使用

## まとめ

このチップベッティングシステムにより、ブラックジャックがより戦略的で緊張感のあるゲームになります。ポーカー風のベット/レイズ/フォールドメカニクスを導入することで、単純なヒット/スタンドの判断だけでなく、リスク管理とブラフ要素が加わります。

実装は段階的に行い、まずは基本的なベットフローを確立してから、オールインやサイドポットなどの高度な機能を追加していきます。
