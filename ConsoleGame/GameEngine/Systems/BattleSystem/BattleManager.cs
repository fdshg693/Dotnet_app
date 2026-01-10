using GameEngine.Factory;
using GameEngine.Interfaces;

namespace GameEngine.Systems.BattleSystem
{
    /// <summary>
    /// 戦闘全体の管理を行うクラス
    /// </summary>
    public class BattleManager
    {
        private readonly IPlayer _player;

        public BattleManager(IPlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary>
        /// 戦闘を開始する
        /// </summary>
        public BattleResult StartBattle()
        {
            try
            {
                IEnemy enemy = EnemyFactory.CreateRandomEnemy();
                Console.WriteLine($"A wild {enemy.Name} appears!\n");

                return ExecuteBattle(enemy);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting battle: {ex.Message}");
                return new BattleResult(BattleOutcome.Error, null);
            }
        }

        /// <summary>
        /// 指定された敵との戦闘を実行する
        /// </summary>
        private BattleResult ExecuteBattle(IEnemy enemy)
        {
            var battleTurn = 0;

            while (_player.IsAlive && enemy.IsAlive)
            {
                battleTurn++;
                Console.WriteLine($"\n--- Turn {battleTurn} ---");

                // プレイヤーのターン
                ExecutePlayerTurn(enemy);

                // 敵が倒された場合
                if (!enemy.IsAlive)
                {
                    return HandleVictory(enemy);
                }

                // 敵のターン
                ExecuteEnemyTurn(enemy);

                // プレイヤーが倒された場合
                if (!_player.IsAlive)
                {
                    return HandleDefeat(enemy);
                }

                // ターン終了時の状態表示
                DisplayBattleStatus(enemy);
            }

            // 通常はここには到達しない
            return new BattleResult(BattleOutcome.Error, enemy);
        }

        /// <summary>
        /// プレイヤーのターンを実行
        /// </summary>
        private void ExecutePlayerTurn(IEnemy enemy)
        {
            // 攻撃戦略をプレイヤーに選択させる
            var attackStrategyName = UserInteraction.SelectAttackStrategy();
            _player.ChangeAttackStrategy(attackStrategyName);

            // 攻撃実行
            int enemyHPBefore = enemy.HP;
            _player.Attack(enemy);
            int damageDealt = enemyHPBefore - enemy.HP;

            Console.WriteLine($"{_player.Name} attacks {enemy.Name} with {attackStrategyName}!");
            Console.WriteLine($"Dealt {damageDealt} damage!");
            Console.WriteLine("-------------------------------------------------------------------");
        }

        /// <summary>
        /// 敵のターンを実行
        /// </summary>
        private void ExecuteEnemyTurn(IEnemy enemy)
        {
            int playerHPBefore = _player.HP;
            enemy.Attack(_player);
            int damageReceived = playerHPBefore - _player.HP;

            Console.WriteLine($"{enemy.Name} attacks {_player.Name} with {enemy._attackStrategy.GetAttackStrategyName()}!");
            Console.WriteLine($"Received {damageReceived} damage!");
            Console.WriteLine("-------------------------------------------------------------------");
        }

        /// <summary>
        /// 戦闘状態を表示
        /// </summary>
        private void DisplayBattleStatus(IEnemy enemy)
        {
            Console.WriteLine($"\nStatus:");
            Console.WriteLine($"  {_player.Name}: {_player.HP} HP");
            Console.WriteLine($"  {enemy.Name}: {enemy.HP} HP");
        }

        /// <summary>
        /// 勝利時の処理
        /// </summary>
        private BattleResult HandleVictory(IEnemy enemy)
        {
            Console.WriteLine($"\n{enemy.Name} has been defeated!");
            GameRecord.RecordWin();
            GameRecord.ShowRecord();
            
            _player.DefeatEnemy(enemy);
            
            return new BattleResult(BattleOutcome.Victory, enemy);
        }

        /// <summary>
        /// 敗北時の処理
        /// </summary>
        private BattleResult HandleDefeat(IEnemy enemy)
        {
            Console.WriteLine($"\n{_player.Name} has fallen...");
            GameRecord.RecordLoss();
            GameRecord.ShowRecord();
            
            return new BattleResult(BattleOutcome.Defeat, enemy);
        }
    }

    /// <summary>
    /// 戦闘結果を表すクラス
    /// </summary>
    public class BattleResult
    {
        public BattleOutcome Outcome { get; }
        public IEnemy? Enemy { get; }

        public BattleResult(BattleOutcome outcome, IEnemy? enemy)
        {
            Outcome = outcome;
            Enemy = enemy;
        }

        public bool IsVictory => Outcome == BattleOutcome.Victory;
        public bool IsDefeat => Outcome == BattleOutcome.Defeat;
        public bool IsError => Outcome == BattleOutcome.Error;
    }

    /// <summary>
    /// 戦闘の結果を表す列挙型
    /// </summary>
    public enum BattleOutcome
    {
        Victory,
        Defeat,
        Error
    }
}
