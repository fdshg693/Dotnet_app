using GameEngine.Interfaces;

namespace GameEngine.Models
{
    /// <summary>
    /// Player/EnemyからDTOへの変換拡張メソッド
    /// </summary>
    public static class GameStateMapper
    {
        /// <summary>
        /// PlayerからPlayerStateへの変換
        /// </summary>
        public static PlayerState ToPlayerState(this Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            // PlayerSaveDataから情報を取得して変換
            var saveData = player.GetSaveData();

            return new PlayerState
            {
                Name = player.Name,
                HP = player.HP,
                MaxHP = player.MaxHP,
                Level = saveData.Level,
                Experience = saveData.TotalExperience,
                Gold = player.ReturnTotalGold(),
                Potions = player.ReturnTotalPotions(),
                EquippedWeapon = saveData.EquippedWeapon?.Name,
                IsAlive = player.IsAlive,
                AttackPower = player.AP,
                DefensePower = player.DP
            };
        }

        /// <summary>
        /// IPlayerからPlayerStateへの変換
        /// </summary>
        public static PlayerState ToPlayerState(this IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var saveData = player.GetSaveData();
            var equippedWeapon = saveData.EquippedWeapon;
            int attackPower = saveData.BaseAP + (equippedWeapon?.AP ?? 0);
            int defensePower = saveData.BaseDP + (equippedWeapon?.DP ?? 0);
            int maxHp = saveData.MaxHP;

            return new PlayerState
            {
                Name = player.Name,
                HP = player.HP,
                MaxHP = maxHp,
                Level = saveData.Level,
                Experience = saveData.TotalExperience,
                Gold = player.ReturnTotalGold(),
                Potions = player.ReturnTotalPotions(),
                EquippedWeapon = equippedWeapon?.Name,
                IsAlive = player.IsAlive,
                AttackPower = attackPower,
                DefensePower = defensePower
            };
        }

        /// <summary>
        /// IEnemyからEnemyStateへの変換
        /// </summary>
        public static EnemyState ToEnemyState(this IEnemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            return new EnemyState
            {
                Name = enemy.Name,
                HP = enemy.HP,
                MaxHP = enemy.MaxHP,
                IsAlive = enemy.IsAlive,
                AttackStrategy = enemy.AttackStrategy?.GetAttackStrategyName() ?? "Unknown"
            };
        }

        /// <summary>
        /// IWeaponからWeaponInfoへの変換
        /// </summary>
        public static WeaponInfo ToWeaponInfo(this IWeapon weapon, int price = 0)
        {
            if (weapon == null)
                throw new ArgumentNullException(nameof(weapon));

            return new WeaponInfo
            {
                Name = weapon.Name,
                AttackPower = weapon.AP,
                DefensePower = weapon.DP,
                Price = price // 価格は外部から指定（IWeaponにはPriceプロパティがない）
            };
        }

        /// <summary>
        /// 戦闘開始時のBattleState作成
        /// </summary>
        public static BattleState CreateInitialBattleState()
        {
            return new BattleState
            {
                TurnNumber = 0,
                AvailableStrategies = new List<string> { "Default", "Melee", "Magic" },
                LastPlayerAction = null,
                LastDamageDealt = 0,
                LastDamageTaken = 0,
                PlayerWon = false,
                BattleEnded = false
            };
        }

        /// <summary>
        /// ショップ開始時のShopState作成
        /// </summary>
        public static ShopState CreateInitialShopState(int potionPrice = 50)
        {
            return new ShopState
            {
                AvailableItems = new List<ShopItem>
                {
                    new ShopItem 
                    { 
                        Name = "Potion", 
                        Price = potionPrice, 
                        Type = "Consumable",
                        Description = "Restores 50 HP"
                    }
                },
                AvailableWeapons = new List<WeaponInfo>
                {
                    // 武器情報は動的に追加する想定
                },
                PotionPrice = potionPrice
            };
        }

        /// <summary>
        /// 空のGameStateを作成
        /// </summary>
        public static GameState CreateEmptyGameState()
        {
            return new GameState
            {
                Player = new PlayerState(),
                CurrentEnemy = null,
                CurrentBattle = null,
                CurrentShop = null,
                Messages = new List<GameMessage>(),
                Phase = GamePhase.Initialization,
                IsGameOver = false
            };
        }

        /// <summary>
        /// GameMessageを作成するヘルパーメソッド
        /// </summary>
        public static GameMessage CreateMessage(string text, MessageType type)
        {
            return new GameMessage
            {
                Text = text,
                Type = type,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 複数のメッセージをまとめて作成
        /// </summary>
        public static List<GameMessage> CreateMessages(params (string text, MessageType type)[] messages)
        {
            var result = new List<GameMessage>();
            foreach (var (text, type) in messages)
            {
                result.Add(CreateMessage(text, type));
            }
            return result;
        }
    }
}
