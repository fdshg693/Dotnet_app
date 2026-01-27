using GameEngine.Interfaces;
using GameEngine.Manager;
using GameEngine.Configuration;
using GameEngine.Models;

namespace GameEngine.Systems
{
    /// <summary>
    /// ゲームのメインループと全体進行を管理するクラス
    /// </summary>
    public class GameSystem
    {
        private readonly IPlayer _player;
        private readonly EventManager _eventManager;
        private readonly IGameInput _input;
        private readonly SaveDataManager? _saveDataManager;

        public GameSystem(IPlayer player, IGameInput input)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _eventManager = new EventManager(_player, _input);

            GameMessageBus.MessagePublished += OnMessagePublished;
            
            // SaveDataManagerの初期化（MongoDBが利用できない場合はnull）
            try
            {
                var config = GameConfigLoader.Instance;
                _saveDataManager = new SaveDataManager(
                    config.MongoDB.ConnectionString,
                    config.MongoDB.DatabaseName,
                    config.MongoDB.CollectionName
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: セーブ機能を初期化できませんでした: {ex.Message}");
                Console.WriteLine("ゲームはセーブ機能なしで続行されます。");
                _saveDataManager = null;
            }
        }

        /// <summary>
        /// ランダムイベントを発生させる（後方互換性のため残されているメソッド）
        /// </summary>
        public void Encounter(IPlayer player)
        {
            var result = _eventManager.TriggerRandomEvent();
            RenderMessages(result.Messages);
        }

        /// <summary>
        /// ゲームのメインループを実行する
        /// </summary>
        public void RunGameLoop()
        {
            Console.WriteLine("\n=== Game Start ===");
            _player.ShowInfo();

            while (_player.IsAlive)
            {
                Console.WriteLine("\n--- New Encounter ---");
                
                var eventResult = _eventManager.TriggerRandomEvent();
                RenderMessages(eventResult.Messages);
                bool continueGame = eventResult.ContinueGame;

                if (!continueGame || !_player.IsAlive)
                {
                    break;
                }

                // プレイヤー情報の表示
                _player.ShowInfo();

                // 続行確認（オプション）
                if (!ConfirmContinue())
                {
                    Console.WriteLine("\nGame ended by player choice.");
                    break;
                }
            }

            DisplayGameOver();
        }

        /// <summary>
        /// 続行確認（継続・停止・一時保存）
        /// </summary>
        /// <returns>
        /// ゲームを続行する場合はtrue、終了する場合はfalse
        /// </returns>
        private bool ConfirmContinue()
        {
            string action = UserInteraction.SelectGameAction();

            switch (action)
            {
                case "continue":
                    return true;

                case "save_continue":
                    SaveGameAsync().Wait();
                    return true;

                case "save_quit":
                    SaveGameAsync().Wait();
                    return false;

                case "quit":
                    Console.WriteLine("\nゲームを終了します。");
                    return false;

                default:
                    Console.WriteLine("\n無効な選択です。ゲームを続行します。");
                    return true;
            }
        }

        /// <summary>
        /// ゲームデータを保存する
        /// </summary>
        private async Task SaveGameAsync()
        {
            if (_saveDataManager == null)
            {
                Console.WriteLine("\n✗ セーブ機能が利用できません。");
                Console.WriteLine("  MongoDBへの接続を確認してください。");
                return;
            }

            try
            {
                var saveData = _player.GetSaveData("auto_save");
                bool success = await _saveDataManager.SavePlayerDataAsync(_player, "auto_save");
                
                if (success)
                {
                    Console.WriteLine("✓ ゲームデータを保存しました。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ セーブに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// ゲームオーバー画面を表示
        /// </summary>
        private void DisplayGameOver()
        {
            Console.WriteLine("\n===========================================");
            
            if (_player.IsAlive)
            {
                Console.WriteLine("       Thank you for playing!");
            }
            else
            {
                Console.WriteLine("            GAME OVER");
            }
            
            Console.WriteLine("===========================================");
            
            _player.ShowInfo();
            RenderMessages(GameRecord.GetRecordMessages());
            
            Console.WriteLine("\nFinal Stats:");
            Console.WriteLine($"  Gold Earned: {_player.ReturnTotalGold()}");
            Console.WriteLine($"  Potions Remaining: {_player.ReturnTotalPotions()}");
        }

        private void OnMessagePublished(GameMessage message)
        {
            RenderMessages(new[] { message });
        }

        private static void RenderMessages(IEnumerable<GameMessage> messages)
        {
            foreach (var message in messages)
            {
                Console.WriteLine(message.Text);
            }
        }
    }
}
