namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ブラックジャックのプレイヤー情報と統計を管理
    /// </summary>
    public class BlackJackPlayer : BlackJackParticipant
    {
        /// <summary>
        /// 所持チップ
        /// </summary>
        public int Chips { get; set; } = 1000;

        /// <summary>
        /// 現在のベット額
        /// </summary>
        public int CurrentBet { get; set; } = 0;

        /// <summary>
        /// フォールドしたか
        /// </summary>
        public bool HasFolded { get; set; } = false;

        public BlackJackPlayer()
        {
            Name = "プレイヤー";
        }
    }
}
