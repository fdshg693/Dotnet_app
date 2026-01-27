using GameEngine.Interfaces;
using GameEngine.Manager;
using GameEngine.Models;
using GameEngine.Systems;

namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// 状態機械の実行コンテキスト
    /// </summary>
    public class GameFlowContext
    {
        private readonly Action<IEnumerable<GameMessage>> _renderMessages;

        public IPlayer Player { get; }
        public EventManager EventManager { get; }
        public IGameInput Input { get; }
        public SaveDataManager? SaveDataManager { get; }

        public GameFlowContext(
            IPlayer player,
            EventManager eventManager,
            IGameInput input,
            SaveDataManager? saveDataManager,
            Action<IEnumerable<GameMessage>> renderMessages)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            EventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            SaveDataManager = saveDataManager;
            _renderMessages = renderMessages ?? throw new ArgumentNullException(nameof(renderMessages));
        }

        public bool IsPlayerAlive => Player.IsAlive;

        public void ShowPlayerInfo()
        {
            Player.ShowInfo();
        }

        public void RenderMessages(IEnumerable<GameMessage> messages)
        {
            _renderMessages(messages);
        }

        public EventResult TriggerRandomEvent()
        {
            return EventManager.TriggerRandomEvent();
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void LogTransition(string fromState, string? toState)
        {
            var next = string.IsNullOrWhiteSpace(toState) ? "End" : toState;
            Console.WriteLine($"[State] {fromState} -> {next}");
        }

        /// <summary>
        /// 続行確認（継続・停止・一時保存）
        /// </summary>
        public bool ConfirmContinue()
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
            if (SaveDataManager == null)
            {
                Console.WriteLine("\n✗ セーブ機能が利用できません。");
                Console.WriteLine("  MongoDBへの接続を確認してください。");
                return;
            }

            try
            {
                bool success = await SaveDataManager.SavePlayerDataAsync(Player, "auto_save");
                
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
        public void DisplayGameOver()
        {
            Console.WriteLine("\n===========================================");
            
            if (Player.IsAlive)
            {
                Console.WriteLine("       Thank you for playing!");
            }
            else
            {
                Console.WriteLine("            GAME OVER");
            }
            
            Console.WriteLine("===========================================");
            
            Player.ShowInfo();
            RenderMessages(GameRecord.GetRecordMessages());
            
            Console.WriteLine("\nFinal Stats:");
            Console.WriteLine($"  Gold Earned: {Player.ReturnTotalGold()}");
            Console.WriteLine($"  Potions Remaining: {Player.ReturnTotalPotions()}");
        }
    }
}