namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ベッティングラウンドの状態を管理
    /// </summary>
    public class BettingRound
    {
        /// <summary>
        /// 現在の最高ベット額
        /// </summary>
        public int CurrentBet { get; set; }

        /// <summary>
        /// ポット（賭け金総額）
        /// </summary>
        public int Pot { get; set; }

        /// <summary>
        /// プレイヤーのベッティング状態
        /// </summary>
        public List<PlayerBettingState> PlayerStates { get; set; }

        /// <summary>
        /// 現在のプレイヤーインデックス
        /// </summary>
        public int CurrentPlayerIndex { get; set; }

        /// <summary>
        /// 全員の行動完了
        /// </summary>
        public bool RoundComplete { get; set; }

        public BettingRound()
        {
            CurrentBet = 0;
            Pot = 0;
            PlayerStates = new List<PlayerBettingState>();
            CurrentPlayerIndex = 0;
            RoundComplete = false;
        }
    }
}
