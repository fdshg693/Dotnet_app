namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// プレイヤーのベッティング状態を管理
    /// </summary>
    public class PlayerBettingState
    {
        /// <summary>
        /// プレイヤー
        /// </summary>
        public BlackJackPlayer Player { get; set; }

        /// <summary>
        /// 所持チップ
        /// </summary>
        public int TotalChips { get; set; }

        /// <summary>
        /// このラウンドで賭けた額
        /// </summary>
        public int CurrentBet { get; set; }

        /// <summary>
        /// 降りたか
        /// </summary>
        public bool HasFolded { get; set; }

        /// <summary>
        /// オールインか
        /// </summary>
        public bool IsAllIn { get; set; }

        /// <summary>
        /// この周で行動したか
        /// </summary>
        public bool HasActedThisRound { get; set; }

        public PlayerBettingState(BlackJackPlayer player, int initialChips = 1000)
        {
            Player = player;
            TotalChips = initialChips;
            CurrentBet = 0;
            HasFolded = false;
            IsAllIn = false;
            HasActedThisRound = false;
        }
    }
}
