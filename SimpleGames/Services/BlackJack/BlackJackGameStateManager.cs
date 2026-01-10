using JankenGame.Models.BlackJack;

namespace JankenGame.Services.BlackJack
{
    /// <summary>
    /// ブラックジャックのゲーム状態とプレイヤー管理を行うサービス
    /// プレイヤーリスト、ディーラー、現在のプレイヤーインデックス、ゲーム状態を管理
    /// </summary>
    public class BlackJackGameStateManager
    {
        public List<BlackJackPlayer> Players { get; private set; }
        public BlackJackDealer Dealer { get; private set; }
        public int CurrentPlayerIndex { get; private set; }
        public BlackJackGameState GameState { get; private set; }

        public BlackJackPlayer? CurrentPlayer => 
            CurrentPlayerIndex >= 0 && CurrentPlayerIndex < Players.Count 
                ? Players[CurrentPlayerIndex] 
                : null;

        public BlackJackGameStateManager()
        {
            Players = new List<BlackJackPlayer>();
            Dealer = new BlackJackDealer();
            GameState = BlackJackGameState.Waiting;
            CurrentPlayerIndex = -1;
        }

        /// <summary>
        /// プレイヤーを追加
        /// </summary>
        public void AddPlayer(BlackJackPlayer player)
        {
            Players.Add(player);
        }

        /// <summary>
        /// ゲーム状態を設定
        /// </summary>
        public void SetGameState(BlackJackGameState state)
        {
            GameState = state;
        }

        /// <summary>
        /// 次のプレイヤーに移動
        /// </summary>
        /// <returns>全プレイヤーのターンが終了した場合はtrue</returns>
        public bool MoveToNextPlayer()
        {
            CurrentPlayerIndex++;
            return CurrentPlayerIndex >= Players.Count;
        }

        /// <summary>
        /// 現在のプレイヤーインデックスを設定（ベッティングラウンド同期用）
        /// </summary>
        public void SetCurrentPlayerIndex(int index)
        {
            CurrentPlayerIndex = index;
        }

        /// <summary>
        /// 全プレイヤーがバストしているかチェック
        /// </summary>
        public bool AreAllPlayersBust()
        {
            return Players.All(p => p.IsBust);
        }

        /// <summary>
        /// ゲームをリセット（カードを除く状態のみ）
        /// </summary>
        public void ResetGame()
        {
            // 全プレイヤーのカードをリセット
            foreach (var player in Players)
            {
                player.ResetCards();
            }
            Dealer.ResetCards();

            // 状態をリセット
            GameState = BlackJackGameState.Waiting;
            CurrentPlayerIndex = -1;
        }

        /// <summary>
        /// プレイヤーターンの開始（初期インデックス設定）
        /// </summary>
        public void StartPlayersTurn()
        {
            CurrentPlayerIndex = 0;
            GameState = BlackJackGameState.PlayersTurn;
        }

        /// <summary>
        /// ディーラーターンの開始
        /// </summary>
        public void StartDealerTurn()
        {
            GameState = BlackJackGameState.DealerTurn;
        }

        /// <summary>
        /// ゲーム終了状態に設定
        /// </summary>
        public void EndGame()
        {
            GameState = BlackJackGameState.GameOver;
        }

        /// <summary>
        /// アンティ状態に設定
        /// </summary>
        public void StartAnte()
        {
            GameState = BlackJackGameState.Ante;
        }

        /// <summary>
        /// 初期配牌状態に設定
        /// </summary>
        public void StartInitialDeal()
        {
            GameState = BlackJackGameState.InitialDeal;
        }

        /// <summary>
        /// ベッティングラウンド開始
        /// </summary>
        public void StartBettingRound()
        {
            CurrentPlayerIndex = 0;
            GameState = BlackJackGameState.BettingRound;
        }

        /// <summary>
        /// 精算状態に設定
        /// </summary>
        public void StartShowdown()
        {
            GameState = BlackJackGameState.Showdown;
        }
    }
}
