using GameEngine.Interfaces;
using GameEngine.Manager;
using GameEngine.Configuration;
using GameEngine.Models;
using GameEngine.Systems.StateMachine;

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
            var context = new GameFlowContext(
                _player,
                _eventManager,
                _input,
                _saveDataManager,
                RenderMessages);

            var stateMachine = new GameStateMachine(new StartState(), context);
            stateMachine.Run();
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
