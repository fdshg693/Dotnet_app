using System;
using System.Collections.Generic;

namespace GameEngine.Models
{
    /// <summary>
    /// ゲームの現在状態を表すDTO（Data Transfer Object）
    /// UI層とコアロジック層のデータ交換に使用
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

    /// <summary>
    /// プレイヤーの状態情報
    /// </summary>
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
        public int AttackPower { get; set; }
        public int DefensePower { get; set; }
    }

    /// <summary>
    /// 敵の状態情報
    /// </summary>
    public class EnemyState
    {
        public string Name { get; set; } = string.Empty;
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public bool IsAlive { get; set; }
        public string AttackStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// 戦闘の状態情報
    /// </summary>
    public class BattleState
    {
        public int TurnNumber { get; set; }
        public List<string> AvailableStrategies { get; set; } = new();
        public string? LastPlayerAction { get; set; }
        public int LastDamageDealt { get; set; }
        public int LastDamageTaken { get; set; }
        public bool PlayerWon { get; set; }
        public bool BattleEnded { get; set; }
    }

    /// <summary>
    /// ショップの状態情報
    /// </summary>
    public class ShopState
    {
        public List<ShopItem> AvailableItems { get; set; } = new();
        public List<WeaponInfo> AvailableWeapons { get; set; } = new();
        public int PotionPrice { get; set; }
    }

    /// <summary>
    /// ショップのアイテム情報
    /// </summary>
    public class ShopItem
    {
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 武器情報
    /// </summary>
    public class WeaponInfo
    {
        public string Name { get; set; } = string.Empty;
        public int AttackPower { get; set; }
        public int DefensePower { get; set; }
        public int Price { get; set; }
    }

    /// <summary>
    /// ゲームメッセージ
    /// UI層で表示するログメッセージ
    /// </summary>
    public class GameMessage
    {
        public string Text { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// メッセージの種類
    /// </summary>
    public enum MessageType
    {
        Info,       // 一般情報
        Success,    // 成功メッセージ
        Warning,    // 警告
        Error,      // エラー
        Combat,     // 戦闘ログ
        System,     // システムメッセージ
        Experience, // 経験値・レベルアップ
        Gold        // ゴールド関連
    }

    /// <summary>
    /// ゲームのフェーズ（状態）
    /// </summary>
    public enum GamePhase
    {
        Initialization, // 初期化中
        Exploration,    // 探索中（待機状態）
        Battle,         // 戦闘中
        Shop,           // ショップ
        Rest,           // 休憩
        GameOver        // ゲームオーバー
    }
}
