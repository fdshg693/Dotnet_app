using GameEngine.Configuration;
using GameEngine.Constants;
using GameEngine.Interfaces;
using GameEngine.Manager;
using GameEngine.Models;
using GameEngine.Systems;

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
                
                // 設定を事前に読み込む（エラーチェックのため）
                var config = GameConfigLoader.Instance;
                Console.WriteLine("Configuration loaded successfully!\n");

                // プレイヤー名の入力
                Console.Write("Enter your name: ");
                string? input = Console.ReadLine();
                string playerName = string.IsNullOrWhiteSpace(input)
                    ? "Hero"
                    : input.Trim();

                // プレイヤーの初期化
                IPlayer player = CreatePlayer(playerName);
                
                // ゲームシステムの初期化
                var gameSystem = new GameSystem(player);

                // ゲームループの実行
                gameSystem.RunGameLoop();

                Console.WriteLine("\nThank you for playing! Press any key to exit.");
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

        /// <summary>
        /// プレイヤーオブジェクトを作成する
        /// </summary>
        private static IPlayer CreatePlayer(string name)
        {
            var config = GameConfigLoader.Instance;

            var experienceManager = new ExperienceManager();
            var inventoryManager = new InventoryManager();
            
            return new Player(
                name,
                config.Player.InitialHP,
                AttackStrategy.GetAttackStrategy("Default"),
                experienceManager,
                inventoryManager);
        }
    }
}
