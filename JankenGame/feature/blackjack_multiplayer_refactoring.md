# ブラックジャック 複数プレイヤー対応リファクタリング

## 概要

ブラックジャックゲームを複数プレイヤー対応に拡張するため、ゲームロジックと状態管理をサービス層に移動しました。

## 実装内容

### 1. 新規追加ファイル

#### `Models/BlackJack/BlackJackGameState.cs`
ゲームの進行状態を管理するenum:
- `Waiting`: ゲーム開始前
- `PlayersTurn`: プレイヤーターン
- `DealerTurn`: ディーラーターン
- `GameOver`: ゲーム終了

### 2. 拡張したクラス

#### `BlackJackGameService`
以下の機能を追加:

**プレイヤー管理:**
- `Players`: プレイヤーリスト
- `Dealer`: ディーラー
- `CurrentPlayerIndex`: 現在のプレイヤーインデックス
- `CurrentPlayer`: 現在のプレイヤー取得プロパティ
- `AddPlayer()`: プレイヤー追加

**ゲーム進行管理:**
- `GameState`: 現在のゲーム状態
- `StartGame()`: ゲーム開始と初期配牌
- `Hit()`: 現在のプレイヤーがヒット
- `Stand()`: 現在のプレイヤーがスタンド
- `ResetGame()`: ゲームリセット

**内部ロジック:**
- `MoveToNextPlayer()`: 次のプレイヤーに移動
- `CheckCurrentPlayerBlackjackOrBust()`: 自動進行判定
- `StartDealerTurn()`: ディーラーターン開始
- `EndGame()`: ゲーム終了と勝敗判定
- `RecordResult()`: 結果記録
- `GetResultMessage()`: プレイヤーごとの結果メッセージ取得

### 3. リファクタリングしたコンポーネント

#### `Components/Pages/Blackjack.razor`

**変更前の問題点:**
- Hit/Standロジックがコンポーネントに直書き
- ゲーム状態管理がローカル変数
- ディーラーのロジックも直書き
- 拡張性が低い

**変更後の改善点:**
- すべてのゲームロジックを`BlackJackGameService`に委譲
- コンポーネントはUIイベントとデータバインディングのみ
- 状態管理はサービスで一元管理
- 複数プレイヤー追加が容易

**主要な変更:**
```csharp
// Before: ローカル変数での状態管理
bool GameStarted, GameOver;
string ResultMessage = "";

// After: サービスから状態を取得
bool GameStarted => gameService.GameState != BlackJackGameState.Waiting;
bool GameOver => gameService.GameState == BlackJackGameState.GameOver;
bool IsPlayerTurn => gameService.GameState == BlackJackGameState.PlayersTurn 
                     && gameService.CurrentPlayer == Player;
```

```csharp
// Before: コンポーネント内でロジック実行
void Hit()
{
    Player.Cards.Add(gameService.DrawCard());
    if (Player.IsBust)
        EndGame();
}

// After: サービスに委譲
void Hit()
{
    if (IsPlayerTurn)
    {
        gameService.Hit();
    }
}
```

## 複数プレイヤー対応の実装例

現在のコードは1プレイヤーですが、以下のように簡単に拡張できます:

```razor
@code {
    List<BlackJackPlayer> Players = new();
    BlackJackGameService gameService = null!;

    protected override void OnInitialized()
    {
        gameService = new BlackJackGameService();
        
        // 複数プレイヤーを追加
        var player1 = new BlackJackPlayer { Name = "プレイヤー1" };
        var player2 = new BlackJackPlayer { Name = "プレイヤー2" };
        var player3 = new BlackJackPlayer { Name = "プレイヤー3" };
        
        gameService.AddPlayer(player1);
        gameService.AddPlayer(player2);
        gameService.AddPlayer(player3);
        
        Players = gameService.Players;
    }

    void StartGame()
    {
        gameService.StartGame();
    }

    void Hit()
    {
        gameService.Hit();
    }

    void Stand()
    {
        gameService.Stand();
    }
}
```

UIも以下のように更新:
```razor
<div class="game-board">
    @foreach (var player in Players)
    {
        <section class="@(gameService.CurrentPlayer == player ? "active-player" : "")">
            <h4>@player.Name</h4>
            @foreach (var card in player.Cards)
            {
                <div class="card">@card.Name</div>
            }
            <p>スコア: @player.Score</p>
            @if (gameService.GameState == BlackJackGameState.GameOver)
            {
                <p class="result">@gameService.GetResultMessage(player)</p>
            }
        </section>
    }
</div>

<div class="controls">
    @if (gameService.CurrentPlayer != null && gameService.GameState == BlackJackGameState.PlayersTurn)
    {
        <p>現在のプレイヤー: @gameService.CurrentPlayer.Name</p>
        <button @onclick="Hit">ヒット</button>
        <button @onclick="Stand">スタンド</button>
    }
    else if (gameService.GameState == BlackJackGameState.DealerTurn)
    {
        <p>ディーラーのターン...</p>
    }
    else if (gameService.GameState == BlackJackGameState.GameOver)
    {
        <button @onclick="Reset">リセット</button>
    }
}
```

## ゲームフロー

```
[Waiting] 
    ↓ StartGame()
[PlayersTurn] (Player 1)
    ↓ Hit() / Stand() / Auto(Blackjack/Bust)
[PlayersTurn] (Player 2)
    ↓ Hit() / Stand() / Auto(Blackjack/Bust)
[PlayersTurn] (Player 3)
    ↓ Stand()
[DealerTurn]
    ↓ 自動でディーラーが17以上まで引く
[GameOver]
    ↓ ResetGame()
[Waiting]
```

## メリット

1. **関心の分離**: UIロジックとゲームロジックが明確に分離
2. **テスト容易性**: サービス層のロジックを独立してテスト可能
3. **拡張性**: 複数プレイヤー対応が容易
4. **保守性**: ロジックの変更がコンポーネントに影響しない
5. **再利用性**: 他のUIからも同じサービスを利用可能

## 今後の拡張案

- プレイヤー数の動的設定
- ベット機能の追加
- AIプレイヤーの実装
- スプリット・ダブルダウンなどのルール追加
- ゲーム履歴の保存・表示
