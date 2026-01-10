namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ブラックジャックの参加者（プレイヤー・ディーラー）の基底クラス
    /// </summary>
    public abstract class BlackJackParticipant
    {
        public string Name { get; set; } = string.Empty;
        public List<Card> Cards { get; set; } = new();

        /// <summary>
        /// 現在のスコア
        /// </summary>
        public int Score
        {
            get
            {
                int sum = Cards.Sum(c => c.Value);
                int aceCount = Cards.Count(c => c.Rank == Rank.Ace);
                while (sum > 21 && aceCount > 0)
                {
                    sum -= 10;
                    aceCount--;
                }
                return sum;
            }
        }

        /// <summary>
        /// バストしているか判定
        /// </summary>
        public bool IsBust => Score > 21;

        /// <summary>
        /// ブラックジャックか判定（21点かつ2枚）
        /// </summary>
        public bool IsBlackjack => Score == 21 && Cards.Count == 2;

        /// <summary>
        /// 勝った回数
        /// </summary>
        public int Wins { get; set; } = 0;

        /// <summary>
        /// 負けた回数
        /// </summary>
        public int Losses { get; set; } = 0;

        /// <summary>
        /// 引き分けた回数
        /// </summary>
        public int Draws { get; set; } = 0;

        /// <summary>
        /// 総プレイ数
        /// </summary>
        public int TotalGames => Wins + Losses + Draws;

        /// <summary>
        /// 勝率（%表示）
        /// </summary>
        public double WinRate => TotalGames == 0 ? 0 : (Wins * 100.0) / TotalGames;

        /// <summary>
        /// カードをリセット
        /// </summary>
        public void ResetCards() => Cards = new();

        /// <summary>
        /// 勝利を記録
        /// </summary>
        public void RecordWin() => Wins++;

        /// <summary>
        /// 敗北を記録
        /// </summary>
        public void RecordLoss() => Losses++;

        /// <summary>
        /// 引き分けを記録
        /// </summary>
        public void RecordDraw() => Draws++;

        /// <summary>
        /// 統計をリセット
        /// </summary>
        public void ResetStats()
        {
            Wins = 0;
            Losses = 0;
            Draws = 0;
        }
    }
}
