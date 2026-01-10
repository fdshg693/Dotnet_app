# Blazor対応への移行手順書

## 概要

現在のCLI RPGゲームを、コアロジックとUI層を分離して、CLI・Blazorの両方で動作可能なアーキテクチャに移行します。

## 目標アーキテクチャ

```
┌─────────────────────────────────────────────────┐
│         UI Layer (Presentation)                  │
│  ┌──────────────────┐  ┌──────────────────┐    │
│  │   CLI Frontend   │  │  Blazor Frontend │    │
│  │  (Console I/O)   │  │  (Web UI)        │    │
│  └────────┬─────────┘  └────────┬─────────┘    │
└───────────┼──────────────────────┼──────────────┘
            │                      │
            └──────────┬───────────┘
                       ↓
┌─────────────────────────────────────────────────┐
│      Application Service Layer                   │
│  ┌─────────────────────────────────────────┐   │
│  │  IGameService (Interface)               │   │
│  │  - TriggerEvent()                       │   │
│  │  - ExecuteTurn(action)                  │   │
│  │  - GetGameState()                       │   │
│  └─────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────┐   │
│  │  GameService (Implementation)           │   │
│  │  - Returns GameState DTOs               │   │
│  │  - No Console.WriteLine                 │   │
│  └─────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────┐
│         Core Domain Layer                        │
│  (既存のManager, Factory, Interfacesなど)       │
│  - Player, Enemy, Combat, Inventory             │
│  - No UI dependencies                            │
└─────────────────────────────────────────────────┘
```

## 現在の問題点

### UI混在箇所の一覧

| ファイル | 問題 | 影響度 |
|---------|------|--------|
| `GameSystem.cs` | `Console.WriteLine`が直接埋め込まれている | 高 |
| `BattleManager.cs` | `UserInteraction.SelectAttackStrategy()`を直接呼び出し | 高 |
| `ShopSystem.cs` | `Console.ReadKey()`でキー入力を直接処理 | 高 |
| `EventManager.cs` | コンソール出力が散在 | 中 |
| `UserInteraction.cs` | 完全にCLI依存（分離不可） | 低（UI層に移動） |

### 分離が必要な理由

1. **テスタビリティ**: Console I/Oが直接埋め込まれているとユニットテストが困難
2. **再利用性**: Blazor UIから同じロジックを呼び出せない
3. **保守性**: UIとビジネスロジックの混在によりコード変更が困難

---

## 移行手順（フェーズ分け）

### Phase 0: 事前準備（現状把握）

**目的**: 既存コードの依存関係を明確化

#### 0.1 依存関係マップの作成

```bash
# Console.WriteLineの使用箇所を全て洗い出し
grep -r "Console\." GameEngine/ > console-dependencies.txt
```

#### 0.2 UI呼び出し箇所の特定

- `UserInteraction`クラスの呼び出し元
- `Console.ReadLine()`/`Console.ReadKey()`の直接呼び出し
- `Console.WriteLine()`のビジネスロジック内埋め込み

---

### Phase 1: DTOとイベントモデルの導入

**目的**: UIとコアロジックのデータ交換用モデルを定義

#### 1.1 GameStateの定義

```csharp
// GameEngine/Models/GameState.cs
namespace GameEngine.Models
{
    /// <summary>
    /// ゲームの現在状態を表すDTO（Data Transfer Object）
    /// </summary>
    public class GameState
    {
        public PlayerState Player { get; set; } = null!;
        public EnemyState? CurrentEnemy { get; set; }
        public BattleState? CurrentBattle { get; set; }
        public ShopState? CurrentShop { get; set; }
        public List<GameMessage> Messages { get; set; } = new();
        public GamePhase Phase { get; set; }
        public bool IsGameOver { get; set; }
    }

    public class PlayerState
    {
        public string Name { get; set; } = string.Empty;
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }
        public int Potions { get; set; }
        public string? EquippedWeapon { get; set; }
        public bool IsAlive { get; set; }
    }

    public class EnemyState
    {
        public string Name { get; set; } = string.Empty;
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public bool IsAlive { get; set; }
    }

    public class BattleState
    {
        public int TurnNumber { get; set; }
        public List<string> AvailableStrategies { get; set; } = new();
        public string? LastPlayerAction { get; set; }
        public int LastDamageDealt { get; set; }
        public int LastDamageTaken { get; set; }
    }

    public class ShopState
    {
        public List<ShopItem> AvailableItems { get; set; } = new();
        public List<string> AvailableWeapons { get; set; } = new();
    }

    public class ShopItem
    {
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class GameMessage
    {
        public string Text { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Combat,
        System
    }

    public enum GamePhase
    {
        Initialization,
        Exploration,
        Battle,
        Shop,
        Rest,
        GameOver
    }
}
```

**実装タスク**:
- [ ] `GameEngine/Models/GameState.cs`を作成
- [ ] 既存の`IPlayer`/`IEnemy`からDTOへのマッピング拡張メソッドを作成

---

#### 1.2 アクションモデルの定義

```csharp
// GameEngine/Models/PlayerAction.cs
namespace GameEngine.Models
{
    /// <summary>
    /// プレイヤーの行動を表す抽象クラス
    /// </summary>
    public abstract class PlayerAction
    {
        public ActionType Type { get; protected set; }
    }

    public enum ActionType
    {
        Attack,
        UseItem,
        Shop,
        Continue,
        Quit,
        Save
    }

    // 戦闘アクション
    public class AttackAction : PlayerAction
    {
        public string StrategyName { get; set; } = string.Empty;
        
        public AttackAction()
        {
            Type = ActionType.Attack;
        }
    }

    // アイテム使用
    public class UseItemAction : PlayerAction
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public UseItemAction()
        {
            Type = ActionType.UseItem;
        }
    }

    // ショップアクション
    public class ShopAction : PlayerAction
    {
        public ShopActionType ShopType { get; set; }
        public string? ItemName { get; set; }
        public int Quantity { get; set; }

        public ShopAction()
        {
            Type = ActionType.Shop;
        }
    }

    public enum ShopActionType
    {
        BuyPotion,
        BuyWeapon,
        Exit
    }

    // 継続/終了アクション
    public class GameControlAction : PlayerAction
    {
        public GameControlAction(ActionType type)
        {
            if (type != ActionType.Continue && 
                type != ActionType.Quit && 
                type != ActionType.Save)
            {
                throw new ArgumentException("Invalid action type for GameControlAction");
            }
            Type = type;
        }
    }
}
```

**実装タスク**:
- [ ] `GameEngine/Models/PlayerAction.cs`を作成
- [ ] アクションバリデーションロジックを追加

---

### Phase 2: サービス層の作成

**目的**: UIに依存しないビジネスロジック層を構築

#### 2.1 IGameServiceインターフェースの定義

```csharp
// GameEngine/Services/IGameService.cs
namespace GameEngine.Services
{
    /// <summary>
    /// ゲームのコアロジックを提供するサービスインターフェース
    /// UI層から呼び出され、GameStateを返す
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// ゲームを初期化
        /// </summary>
        GameState InitializeGame(string playerName);

        /// <summary>
        /// 現在のゲーム状態を取得
        /// </summary>
        GameState GetCurrentState();

        /// <summary>
        /// ランダムイベントをトリガー
        /// </summary>
        GameState TriggerRandomEvent();

        /// <summary>
        /// プレイヤーアクションを実行
        /// </summary>
        GameState ExecuteAction(PlayerAction action);

        /// <summary>
        /// 戦闘を開始
        /// </summary>
        GameState StartBattle();

        /// <summary>
        /// 戦闘ターンを実行
        /// </summary>
        GameState ExecuteBattleTurn(AttackAction action);

        /// <summary>
        /// ショップに入る
        /// </summary>
        GameState EnterShop();

        /// <summary>
        /// ショップアクションを実行
        /// </summary>
        GameState ExecuteShopAction(ShopAction action);

        /// <summary>
        /// ゲームを保存
        /// </summary>
        Task<GameState> SaveGameAsync();

        /// <summary>
        /// ゲームをロード
        /// </summary>
        Task<GameState> LoadGameAsync(string playerName);
    }
}
```

**実装タスク**:
- [ ] `GameEngine/Services/IGameService.cs`を作成
- [ ] インターフェースのXMLドキュメントコメントを充実させる

---

#### 2.2 GameService実装の作成

```csharp
// GameEngine/Services/GameService.cs
namespace GameEngine.Services
{
    /// <summary>
    /// IGameServiceの実装クラス
    /// Console I/Oを完全に排除し、GameStateのみを返す
    /// </summary>
    public class GameService : IGameService
    {
        private readonly IPlayer _player;
        private readonly EventManager _eventManager;
        private readonly SaveDataManager? _saveDataManager;
        private readonly List<GameMessage> _messageBuffer;
        private GamePhase _currentPhase;
        private IEnemy? _currentEnemy;
        private int _battleTurnCount;

        public GameService(string playerName)
        {
            // プレイヤー初期化（既存のCreatePlayer相当）
            _player = CreatePlayer(playerName);
            _eventManager = new EventManager(_player);
            _messageBuffer = new List<GameMessage>();
            _currentPhase = GamePhase.Initialization;

            // SaveDataManagerの初期化
            try
            {
                var config = GameConfigLoader.Instance;
                _saveDataManager = new SaveDataManager(
                    config.MongoDB.ConnectionString,
                    config.MongoDB.DatabaseName,
                    config.MongoDB.CollectionName
                );
            }
            catch
            {
                _saveDataManager = null;
                AddMessage("セーブ機能は利用できません", MessageType.Warning);
            }
        }

        public GameState InitializeGame(string playerName)
        {
            _currentPhase = GamePhase.Exploration;
            AddMessage($"ゲーム開始: {playerName}", MessageType.System);
            return GetCurrentState();
        }

        public GameState GetCurrentState()
        {
            var state = new GameState
            {
                Player = MapPlayerState(_player),
                CurrentEnemy = _currentEnemy != null ? MapEnemyState(_currentEnemy) : null,
                CurrentBattle = _currentPhase == GamePhase.Battle ? CreateBattleState() : null,
                CurrentShop = _currentPhase == GamePhase.Shop ? CreateShopState() : null,
                Messages = new List<GameMessage>(_messageBuffer),
                Phase = _currentPhase,
                IsGameOver = !_player.IsAlive
            };

            // メッセージバッファをクリア（読み取り後）
            _messageBuffer.Clear();

            return state;
        }

        public GameState TriggerRandomEvent()
        {
            // EventManagerの結果を受け取り、GameStateに変換
            // Console.WriteLineを使わない実装に変更
            var random = new Random();
            if (random.Next(3) == 0)
            {
                return EnterShop();
            }
            else
            {
                return StartBattle();
            }
        }

        public GameState StartBattle()
        {
            _currentPhase = GamePhase.Battle;
            _currentEnemy = EnemyFactory.CreateRandomEnemy();
            _battleTurnCount = 0;
            
            AddMessage($"{_currentEnemy.Name}が現れた！", MessageType.Combat);
            return GetCurrentState();
        }

        public GameState ExecuteBattleTurn(AttackAction action)
        {
            if (_currentPhase != GamePhase.Battle || _currentEnemy == null)
            {
                AddMessage("戦闘中ではありません", MessageType.Error);
                return GetCurrentState();
            }

            _battleTurnCount++;

            // プレイヤーターン
            _player.ChangeAttackStrategy(action.StrategyName);
            int enemyHPBefore = _currentEnemy.HP;
            _player.Attack(_currentEnemy);
            int damageDealt = enemyHPBefore - _currentEnemy.HP;

            AddMessage($"{_player.Name}は{action.StrategyName}で攻撃！", MessageType.Combat);
            AddMessage($"{damageDealt}ダメージを与えた！", MessageType.Combat);

            // 敵撃破チェック
            if (!_currentEnemy.IsAlive)
            {
                int expGained = _currentEnemy.Experience;
                _player.AddExperience(expGained);
                AddMessage($"{_currentEnemy.Name}を倒した！", MessageType.Success);
                AddMessage($"{expGained}経験値を獲得！", MessageType.Info);

                _currentPhase = GamePhase.Exploration;
                _currentEnemy = null;
                return GetCurrentState();
            }

            // 敵ターン
            int playerHPBefore = _player.HP;
            _currentEnemy.Attack(_player);
            int damageTaken = playerHPBefore - _player.HP;

            AddMessage($"{_currentEnemy.Name}の攻撃！", MessageType.Combat);
            AddMessage($"{damageTaken}ダメージを受けた！", MessageType.Warning);

            // プレイヤー敗北チェック
            if (!_player.IsAlive)
            {
                AddMessage("あなたは倒れた...", MessageType.Error);
                _currentPhase = GamePhase.GameOver;
            }

            return GetCurrentState();
        }

        public GameState EnterShop()
        {
            _currentPhase = GamePhase.Shop;
            AddMessage("ショップへようこそ！", MessageType.Info);
            return GetCurrentState();
        }

        public GameState ExecuteShopAction(ShopAction action)
        {
            if (_currentPhase != GamePhase.Shop)
            {
                AddMessage("ショップにいません", MessageType.Error);
                return GetCurrentState();
            }

            switch (action.ShopType)
            {
                case ShopActionType.BuyPotion:
                    _player.BuyPotion(action.Quantity);
                    AddMessage($"ポーションを{action.Quantity}個購入しました", MessageType.Success);
                    break;

                case ShopActionType.BuyWeapon:
                    if (!string.IsNullOrEmpty(action.ItemName))
                    {
                        var weapon = WeaponFactory.CreateWeapon(action.ItemName);
                        _player.EquipWeapon(weapon);
                        AddMessage($"{action.ItemName}を装備しました", MessageType.Success);
                    }
                    break;

                case ShopActionType.Exit:
                    _currentPhase = GamePhase.Exploration;
                    AddMessage("ショップを出ました", MessageType.Info);
                    break;
            }

            return GetCurrentState();
        }

        public GameState ExecuteAction(PlayerAction action)
        {
            return action switch
            {
                AttackAction attackAction => ExecuteBattleTurn(attackAction),
                ShopAction shopAction => ExecuteShopAction(shopAction),
                GameControlAction controlAction => HandleControlAction(controlAction),
                _ => GetCurrentState()
            };
        }

        public async Task<GameState> SaveGameAsync()
        {
            if (_saveDataManager == null)
            {
                AddMessage("セーブ機能が利用できません", MessageType.Error);
                return GetCurrentState();
            }

            try
            {
                await _saveDataManager.SavePlayerDataAsync(_player);
                AddMessage("ゲームをセーブしました", MessageType.Success);
            }
            catch (Exception ex)
            {
                AddMessage($"セーブに失敗: {ex.Message}", MessageType.Error);
            }

            return GetCurrentState();
        }

        public async Task<GameState> LoadGameAsync(string playerName)
        {
            // 実装省略（LoadPlayerDataAsync相当）
            throw new NotImplementedException();
        }

        // Private helper methods
        private void AddMessage(string text, MessageType type)
        {
            _messageBuffer.Add(new GameMessage
            {
                Text = text,
                Type = type,
                Timestamp = DateTime.UtcNow
            });
        }

        private PlayerState MapPlayerState(IPlayer player)
        {
            return new PlayerState
            {
                Name = player.Name,
                HP = player.HP,
                MaxHP = player.MaxHP,
                Level = player.Level,
                Experience = player.Experience,
                Gold = player.ReturnTotalGold(),
                Potions = player.ReturnTotalPotions(),
                EquippedWeapon = player.EquippedWeapon?.Name,
                IsAlive = player.IsAlive
            };
        }

        private EnemyState MapEnemyState(IEnemy enemy)
        {
            return new EnemyState
            {
                Name = enemy.Name,
                HP = enemy.HP,
                MaxHP = enemy.HP, // 初期HP保存が必要
                IsAlive = enemy.IsAlive
            };
        }

        private BattleState CreateBattleState()
        {
            return new BattleState
            {
                TurnNumber = _battleTurnCount,
                AvailableStrategies = new List<string> { "Default", "Melee", "Magic" }
            };
        }

        private ShopState CreateShopState()
        {
            return new ShopState
            {
                AvailableItems = new List<ShopItem>
                {
                    new ShopItem { Name = "Potion", Price = 50, Type = "Consumable" }
                },
                AvailableWeapons = new List<string> { "SWORD", "AXE", "BOW" }
            };
        }

        private GameState HandleControlAction(GameControlAction action)
        {
            // Continue/Quit/Save処理
            return GetCurrentState();
        }

        private IPlayer CreatePlayer(string name)
        {
            // 既存のProgram.csのCreatePlayer相当
            var healthManager = new HealthManager(
                GameConstants.InitialHP,
                GameConstants.InitialDP,
                null
            );

            var inventoryManager = new InventoryManager();
            var experienceManager = new ExperienceManager();

            return new Player(
                name,
                healthManager,
                inventoryManager,
                experienceManager
            );
        }
    }
}
```

**実装タスク**:
- [ ] `GameEngine/Services/GameService.cs`を作成
- [ ] 既存の`GameSystem`/`BattleManager`から処理を移植
- [ ] **全ての`Console.WriteLine`を`AddMessage`に置き換え**
- [ ] ユニットテストを作成

---

### Phase 3: CLI UIの再構築

**目的**: サービス層を呼び出す薄いUI層として再実装

#### 3.1 CLIゲームコントローラーの作成

```csharp
// GameEngine/UI/CLI/CliGameController.cs
namespace GameEngine.UI.CLI
{
    /// <summary>
    /// CLI用のゲームコントローラー
    /// IGameServiceを呼び出し、結果をコンソールに表示
    /// </summary>
    public class CliGameController
    {
        private readonly IGameService _gameService;

        public CliGameController(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        public void Run()
        {
            Console.WriteLine("=== CLI RPG Game ===");
            
            // プレイヤー名入力
            Console.Write("Enter your name: ");
            string playerName = Console.ReadLine()?.Trim() ?? "Hero";

            // ゲーム初期化
            var state = _gameService.InitializeGame(playerName);
            DisplayGameState(state);

            // メインループ
            while (!state.IsGameOver)
            {
                state = _gameService.TriggerRandomEvent();
                DisplayGameState(state);

                if (state.Phase == GamePhase.Battle)
                {
                    state = HandleBattlePhase(state);
                }
                else if (state.Phase == GamePhase.Shop)
                {
                    state = HandleShopPhase(state);
                }

                if (!AskContinue())
                {
                    break;
                }
            }

            Console.WriteLine("\n=== Game Over ===");
            Console.WriteLine("Thank you for playing!");
        }

        private GameState HandleBattlePhase(GameState state)
        {
            while (state.Phase == GamePhase.Battle && !state.IsGameOver)
            {
                // 戦略選択
                string strategy = PromptStrategySelection(state.CurrentBattle!.AvailableStrategies);
                
                var action = new AttackAction { StrategyName = strategy };
                state = _gameService.ExecuteBattleTurn(action);
                
                DisplayGameState(state);
            }

            return state;
        }

        private GameState HandleShopPhase(GameState state)
        {
            while (state.Phase == GamePhase.Shop)
            {
                Console.WriteLine("\n--- Shop Menu ---");
                Console.WriteLine("1. Buy Potion");
                Console.WriteLine("2. Buy Weapon");
                Console.WriteLine("3. Exit");
                Console.Write("Choose: ");

                var key = Console.ReadKey(intercept: true);
                Console.WriteLine();

                ShopAction? action = null;

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        Console.Write("Quantity: ");
                        if (int.TryParse(Console.ReadLine(), out int qty))
                        {
                            action = new ShopAction 
                            { 
                                ShopType = ShopActionType.BuyPotion, 
                                Quantity = qty 
                            };
                        }
                        break;

                    case ConsoleKey.D2:
                        Console.WriteLine("1. SWORD  2. AXE  3. BOW");
                        var weaponKey = Console.ReadKey(intercept: true);
                        string? weaponName = weaponKey.Key switch
                        {
                            ConsoleKey.D1 => "SWORD",
                            ConsoleKey.D2 => "AXE",
                            ConsoleKey.D3 => "BOW",
                            _ => null
                        };

                        if (weaponName != null)
                        {
                            action = new ShopAction
                            {
                                ShopType = ShopActionType.BuyWeapon,
                                ItemName = weaponName,
                                Quantity = 1
                            };
                        }
                        break;

                    case ConsoleKey.D3:
                        action = new ShopAction { ShopType = ShopActionType.Exit };
                        break;
                }

                if (action != null)
                {
                    state = _gameService.ExecuteShopAction(action);
                    DisplayGameState(state);
                }
            }

            return state;
        }

        private void DisplayGameState(GameState state)
        {
            // メッセージ表示
            foreach (var message in state.Messages)
            {
                DisplayMessage(message);
            }

            // プレイヤー情報
            var player = state.Player;
            Console.WriteLine($"\n[{player.Name}] HP: {player.HP}/{player.MaxHP} | Lv.{player.Level} | Gold: {player.Gold} | Potions: {player.Potions}");

            // 敵情報（戦闘中の場合）
            if (state.CurrentEnemy != null && state.CurrentEnemy.IsAlive)
            {
                var enemy = state.CurrentEnemy;
                Console.WriteLine($"[{enemy.Name}] HP: {enemy.HP}");
            }
        }

        private void DisplayMessage(GameMessage message)
        {
            var color = message.Type switch
            {
                MessageType.Success => ConsoleColor.Green,
                MessageType.Warning => ConsoleColor.Yellow,
                MessageType.Error => ConsoleColor.Red,
                MessageType.Combat => ConsoleColor.Cyan,
                MessageType.System => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            Console.WriteLine(message.Text);
            Console.ResetColor();
        }

        private string PromptStrategySelection(List<string> strategies)
        {
            Console.WriteLine("\n--- Select Strategy ---");
            for (int i = 0; i < strategies.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {strategies[i]}");
            }

            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                int index = key.Key switch
                {
                    ConsoleKey.D1 => 0,
                    ConsoleKey.D2 => 1,
                    ConsoleKey.D3 => 2,
                    _ => -1
                };

                if (index >= 0 && index < strategies.Count)
                {
                    return strategies[index];
                }
            }
        }

        private bool AskContinue()
        {
            Console.Write("\nContinue? (Y/N): ");
            var key = Console.ReadKey(intercept: true);
            Console.WriteLine();
            return key.Key != ConsoleKey.N;
        }
    }
}
```

**実装タスク**:
- [ ] `GameEngine/UI/CLI/CliGameController.cs`を作成
- [ ] `Program.cs`を新しいコントローラーを使うように変更
- [ ] 既存の`UserInteraction`クラスを`UI/CLI`フォルダに移動

---

#### 3.2 Program.csの修正

```csharp
// GameEngine/Program.cs (修正版)
using GameEngine.Services;
using GameEngine.UI.CLI;

namespace CliRpgGame
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== CLI RPG Game ===");
                Console.WriteLine("Loading game configuration...");
                
                // プレイヤー名入力
                Console.Write("Enter your name: ");
                string playerName = Console.ReadLine()?.Trim() ?? "Hero";

                // サービス初期化
                IGameService gameService = new GameService(playerName);

                // CLIコントローラー起動
                var controller = new CliGameController(gameService);
                controller.Run();

                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFatal Error: {ex.Message}");
                Console.WriteLine("The game cannot continue. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}
```

**実装タスク**:
- [ ] `Program.cs`を修正
- [ ] 既存の`GameSystem`クラスの削除（または非推奨化）

---

### Phase 4: Blazor Serverプロジェクトの作成

**目的**: 同じ`GameService`をBlazor UIから利用

#### 4.1 プロジェクト構成

```
ConsoleGame/
├── GameEngine/              (既存: コアロジック + CLIエントリポイント)
├── GameEngine.Core/         (新規: 共有ライブラリ)
│   ├── Interfaces/
│   ├── Models/
│   ├── Manager/
│   ├── Factory/
│   ├── Services/           (GameService, IGameService)
│   └── Constants/
└── GameEngine.BlazorServer/ (新規: Blazor Server UI)
    ├── Pages/
    │   ├── Index.razor
    │   ├── Game.razor
    │   └── Battle.razor
    ├── Components/
    │   ├── PlayerStatus.razor
    │   ├── EnemyStatus.razor
    │   └── MessageLog.razor
    └── Program.cs
```

#### 4.2 GameEngine.Coreプロジェクトの作成

```bash
# 新規クラスライブラリプロジェクト作成
dotnet new classlib -n GameEngine.Core -o GameEngine.Core

# ソリューションに追加
dotnet sln add GameEngine.Core/GameEngine.Core.csproj

# 依存関係追加
cd GameEngine.Core
dotnet add package YamlDotNet --version 16.3.0
dotnet add package MongoDB.Driver --version 2.29.0
```

**移動するファイル**:
- `Interfaces/` → `GameEngine.Core/Interfaces/`
- `Models/` → `GameEngine.Core/Models/`
- `Manager/` → `GameEngine.Core/Manager/`
- `Factory/` → `GameEngine.Core/Factory/`
- `Services/` → `GameEngine.Core/Services/`
- `Constants/` → `GameEngine.Core/Constants/`
- `Configuration/` → `GameEngine.Core/Configuration/`

**実装タスク**:
- [ ] `GameEngine.Core`プロジェクト作成
- [ ] ファイルを移動（namespaceを`GameEngine.Core`に変更）
- [ ] `GameEngine.csproj`に`ProjectReference`追加

---

#### 4.3 Blazor Serverプロジェクトの作成

```bash
# Blazor Serverプロジェクト作成
dotnet new blazorserver -n GameEngine.BlazorServer -o GameEngine.BlazorServer

# ソリューションに追加
dotnet sln add GameEngine.BlazorServer/GameEngine.BlazorServer.csproj

# GameEngine.Coreへの参照追加
cd GameEngine.BlazorServer
dotnet add reference ../GameEngine.Core/GameEngine.Core.csproj
```

#### 4.4 Blazor UIコンポーネントの実装例

```razor
@* GameEngine.BlazorServer/Pages/Game.razor *@
@page "/game"
@inject IGameService GameService
@implements IDisposable

<PageTitle>RPG Game</PageTitle>

<div class="game-container">
    <div class="game-header">
        <h1>Console RPG - Blazor Edition</h1>
    </div>

    @if (gameState != null)
    {
        <div class="game-content">
            <!-- プレイヤー情報 -->
            <PlayerStatus Player="@gameState.Player" />

            <!-- メッセージログ -->
            <MessageLog Messages="@gameState.Messages" />

            <!-- フェーズ別UI -->
            @switch (gameState.Phase)
            {
                case GamePhase.Battle:
                    <BattleView 
                        BattleState="@gameState.CurrentBattle" 
                        Enemy="@gameState.CurrentEnemy"
                        OnAttack="HandleAttack" />
                    break;

                case GamePhase.Shop:
                    <ShopView 
                        ShopState="@gameState.CurrentShop"
                        PlayerGold="@gameState.Player.Gold"
                        OnPurchase="HandleShopAction" />
                    break;

                case GamePhase.Exploration:
                    <ExplorationView OnTriggerEvent="TriggerEvent" />
                    break;

                case GamePhase.GameOver:
                    <GameOverView PlayerState="@gameState.Player" />
                    break;
            }
        </div>
    }
    else
    {
        <p>Loading game...</p>
    }
</div>

@code {
    private GameState? gameState;
    private string playerName = "Hero";

    protected override void OnInitialized()
    {
        gameState = GameService.InitializeGame(playerName);
    }

    private void TriggerEvent()
    {
        gameState = GameService.TriggerRandomEvent();
        StateHasChanged();
    }

    private void HandleAttack(string strategyName)
    {
        var action = new AttackAction { StrategyName = strategyName };
        gameState = GameService.ExecuteBattleTurn(action);
        StateHasChanged();
    }

    private void HandleShopAction(ShopAction action)
    {
        gameState = GameService.ExecuteShopAction(action);
        StateHasChanged();
    }

    public void Dispose()
    {
        // クリーンアップ処理
    }
}
```

**実装タスク**:
- [ ] `Game.razor`メインページ作成
- [ ] `PlayerStatus.razor`コンポーネント作成
- [ ] `BattleView.razor`コンポーネント作成
- [ ] `ShopView.razor`コンポーネント作成
- [ ] `MessageLog.razor`コンポーネント作成
- [ ] DI設定（`Program.cs`で`IGameService`をシングルトン登録）

---

#### 4.5 Blazor Program.csの設定

```csharp
// GameEngine.BlazorServer/Program.cs
using GameEngine.Core.Services;
using GameEngine.BlazorServer.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ゲームサービスの登録（Scoped: セッションごとに独立したゲーム状態）
builder.Services.AddScoped<IGameService>(sp => 
{
    // ユーザー名は後でセッションから取得する想定
    return new GameService("Player");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

**実装タスク**:
- [ ] DI設定を追加
- [ ] セッション管理機能の実装（オプション）

---

### Phase 5: テストとリファクタリング

#### 5.1 ユニットテストの作成

```bash
# テストプロジェクト作成
dotnet new xunit -n GameEngine.Core.Tests -o GameEngine.Core.Tests
dotnet sln add GameEngine.Core.Tests/GameEngine.Core.Tests.csproj
cd GameEngine.Core.Tests
dotnet add reference ../GameEngine.Core/GameEngine.Core.csproj
dotnet add package Moq
```

**テスト例**:

```csharp
// GameEngine.Core.Tests/Services/GameServiceTests.cs
using Xunit;
using GameEngine.Core.Services;
using GameEngine.Core.Models;

namespace GameEngine.Core.Tests.Services
{
    public class GameServiceTests
    {
        [Fact]
        public void InitializeGame_ShouldReturnValidState()
        {
            // Arrange
            var service = new GameService("TestPlayer");

            // Act
            var state = service.InitializeGame("TestPlayer");

            // Assert
            Assert.NotNull(state);
            Assert.Equal("TestPlayer", state.Player.Name);
            Assert.Equal(GamePhase.Exploration, state.Phase);
            Assert.True(state.Player.IsAlive);
        }

        [Fact]
        public void StartBattle_ShouldCreateEnemy()
        {
            // Arrange
            var service = new GameService("TestPlayer");
            service.InitializeGame("TestPlayer");

            // Act
            var state = service.StartBattle();

            // Assert
            Assert.Equal(GamePhase.Battle, state.Phase);
            Assert.NotNull(state.CurrentEnemy);
            Assert.True(state.CurrentEnemy.IsAlive);
        }

        [Fact]
        public void ExecuteBattleTurn_ShouldDealDamage()
        {
            // Arrange
            var service = new GameService("TestPlayer");
            service.InitializeGame("TestPlayer");
            var battleState = service.StartBattle();
            int initialEnemyHP = battleState.CurrentEnemy!.HP;

            // Act
            var action = new AttackAction { StrategyName = "Default" };
            var resultState = service.ExecuteBattleTurn(action);

            // Assert
            Assert.True(resultState.CurrentEnemy!.HP < initialEnemyHP);
        }
    }
}
```

**実装タスク**:
- [ ] `GameServiceTests.cs`を作成
- [ ] 各メソッドのユニットテスト実装
- [ ] モック使用によるDBアクセスのテスト分離

---

#### 5.2 既存コードの段階的削除

**削除対象**:
- [ ] `GameSystem.cs`（`GameService`に置き換え）
- [ ] `BattleManager.cs`の旧実装
- [ ] `ShopSystem.cs`の旧実装
- [ ] `EventManager.cs`（必要に応じて保持）

**保持するもの**:
- [ ] `UserInteraction.cs`（CLI専用として`UI/CLI`フォルダに移動）
- [ ] `Manager/`（コアロジックなのでそのまま）
- [ ] `Factory/`（コアロジックなのでそのまま）

---

## Phase別完了チェックリスト

### Phase 1: DTOとイベントモデル
- [ ] `GameState.cs`実装
- [ ] `PlayerAction.cs`実装
- [ ] 既存モデルからDTOへのマッパー作成
- [ ] DTOのユニットテスト作成

### Phase 2: サービス層
- [ ] `IGameService.cs`定義
- [ ] `GameService.cs`実装
- [ ] `Console.WriteLine`完全排除
- [ ] サービスのユニットテスト作成

### Phase 3: CLI UI再構築
- [ ] `CliGameController.cs`実装
- [ ] `Program.cs`修正
- [ ] CLI動作確認テスト

### Phase 4: Blazor UI
- [ ] `GameEngine.Core`プロジェクト分離
- [ ] `GameEngine.BlazorServer`プロジェクト作成
- [ ] Blazorコンポーネント実装
- [ ] DI設定
- [ ] Blazor動作確認

### Phase 5: テストとクリーンアップ
- [ ] 統合テスト実装
- [ ] 旧コードの削除
- [ ] ドキュメント更新

---

## 技術的な注意事項

### 1. 状態管理

**CLI**: ステートレス（ゲームループ内で状態保持）
**Blazor**: ステートフル（`IGameService`をScoped登録してセッション管理）

### 2. 非同期処理

- `SaveGameAsync`/`LoadGameAsync`は`async/await`を使用
- Blazorでは`await`が必須（UIスレッドブロックを避ける）
- CLIでは`.Wait()`も許容（ただし推奨は`await`）

### 3. エラーハンドリング

- `GameService`内で例外キャッチし、`GameMessage`に変換
- UI層ではエラーメッセージをユーザーフレンドリーに表示

### 4. YAML設定ファイルの配置

```xml
<!-- GameEngine.Core.csproj -->
<ItemGroup>
  <Content Include="enemy-specs.yml">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
  <Content Include="game-config.yml">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

## 移行後のアーキテクチャ利点

### 1. テスタビリティの向上
- `IGameService`のモック化が容易
- Console I/O依存の排除によりユニットテストが可能に

### 2. UI非依存
- 同じコアロジックを複数のフロントエンドから利用可能
- CLI、Blazor、将来的にはAPIエンドポイント化も可能

### 3. 保守性の向上
- ビジネスロジックとプレゼンテーション層の明確な分離
- 変更の影響範囲が限定的

### 4. 拡張性
- 新しいUI追加が容易（WPF、MAUI、Unityなど）
- AIプレイヤーの実装が容易（`PlayerAction`を自動生成）

---

## トラブルシューティング

### 問題: Blazorでゲーム状態が保持されない

**原因**: `IGameService`がTransientで登録されている

**解決策**: 
```csharp
builder.Services.AddScoped<IGameService, GameService>();
```

### 問題: YAMLファイルが見つからない

**原因**: `enemy-specs.yml`が出力ディレクトリにコピーされていない

**解決策**:
```xml
<Content Include="*.yml">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</Content>
```

### 問題: CLIとBlazorで動作が異なる

**原因**: `GameService`内に意図しない状態依存がある

**解決策**: ステートフル性を確認し、`GetCurrentState()`で常に最新状態を返すようにする

---

## 参考資料

- [Clean Architecture in ASP.NET Core](https://learn.microsoft.com/ja-jp/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Blazor Server vs WebAssembly](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/hosting-models)
- [Dependency Injection in .NET](https://learn.microsoft.com/ja-jp/dotnet/core/extensions/dependency-injection)

---

## 最終目標構成

```
ConsoleGame/
├── GameEngine.Core/           # UI非依存のコアロジック
│   ├── Services/
│   │   ├── IGameService.cs
│   │   └── GameService.cs
│   ├── Models/               # DTOs
│   │   ├── GameState.cs
│   │   └── PlayerAction.cs
│   ├── Interfaces/           # ドメインインターフェース
│   ├── Manager/              # ビジネスロジック
│   └── Factory/
├── GameEngine/               # CLIエントリポイント
│   ├── Program.cs
│   └── UI/
│       └── CLI/
│           ├── CliGameController.cs
│           └── UserInteraction.cs
└── GameEngine.BlazorServer/  # Blazor UIエントリポイント
    ├── Program.cs
    ├── Pages/
    │   └── Game.razor
    └── Components/
        ├── PlayerStatus.razor
        └── BattleView.razor
```

この構成により、**同一のゲームロジックを異なるUI技術で利用可能**になります。
