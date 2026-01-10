using GameEngine.Configuration;
using GameEngine.Interfaces;
using GameEngine.Systems.BattleSystem;

namespace GameEngine.Systems
{
    /// <summary>
    /// ゲームイベントの管理を行うクラス
    /// </summary>
    public class EventManager
    {
        private readonly IPlayer _player;
        private readonly BattleManager _battleManager;
        private readonly Random _random;
        private readonly GameConfig _config;

        public EventManager(IPlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _battleManager = new BattleManager(_player);
            _random = new Random();
            _config = GameConfigLoader.Instance;
        }

        /// <summary>
        /// ランダムイベントを発生させる
        /// </summary>
        /// <returns>プレイヤーが生存している場合はtrue</returns>
        public bool TriggerRandomEvent()
        {
            var eventType = DetermineEventType();

            switch (eventType)
            {
                case GameEventType.Shop:
                    return HandleShopEvent();

                case GameEventType.Battle:
                    return HandleBattleEvent();

                default:
                    Console.WriteLine("Unknown event occurred.");
                    return _player.IsAlive;
            }
        }

        /// <summary>
        /// 発生するイベントタイプを決定する
        /// </summary>
        private GameEventType DetermineEventType()
        {
            int totalWeight = _config.Events.TotalWeight;
            int roll = _random.Next(0, totalWeight);

            if (roll < _config.Events.ShopEventWeight)
            {
                return GameEventType.Shop;
            }
            else
            {
                return GameEventType.Battle;
            }
        }

        /// <summary>
        /// ショップイベントを処理する
        /// </summary>
        private bool HandleShopEvent()
        {
            Console.WriteLine("\n=== You found a shop! ===\n");
            
            // ゴールド報酬を付与
            int goldReward = _random.Next(
                _config.Shop.GoldRewardMin, 
                _config.Shop.GoldRewardMax + 1);
            
            _player.GainGold(goldReward);
            Console.WriteLine($"You received {goldReward} gold as a discovery bonus!");

            // ショップを開く
            ShopSystem.Shop(_player);

            // 状態表示
            Console.WriteLine($"\nStatus - {_player.Name}: {_player.HP} HP");

            // 回復アイテム使用の機会を提供
            RestSystem.UsePotion(_player);

            return _player.IsAlive;
        }

        /// <summary>
        /// 戦闘イベントを処理する
        /// </summary>
        private bool HandleBattleEvent()
        {
            Console.WriteLine("\n=== You encounter a wild enemy! ===\n");

            var battleResult = _battleManager.StartBattle();

            // 戦闘後の回復アイテム使用
            if (_player.IsAlive)
            {
                RestSystem.UsePotion(_player);
            }

            return _player.IsAlive;
        }
    }

    /// <summary>
    /// ゲームイベントの種類を表す列挙型
    /// </summary>
    public enum GameEventType
    {
        Shop,
        Battle,
        Treasure,  // 将来の拡張用
        Rest       // 将来の拡張用
    }
}
