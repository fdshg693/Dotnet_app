namespace JankenGame.Models.Janken
{
    /// <summary>
    /// ジャンケンゲームの1回のプレイ結果を表すレコード
    /// </summary>
    public sealed record JankenRecord(
        JankenHand PlayerHand,
        JankenHand ComputerHand,
        JankenResultEnum Outcome,
        DateTime Timestamp)
    {
        /// <summary>
        /// 新しいJankenRecordインスタンスを作成します。タイムスタンプは現在時刻に設定されます。
        /// </summary>
        public JankenRecord(JankenHand playerHand, JankenHand computerHand, JankenResultEnum outcome)
            : this(playerHand, computerHand, outcome, DateTime.Now)
        {
        }
    }

    public class JankenRecordList : List<JankenRecord>
    {
        public void AddRecord(JankenHand? playerHand, JankenHand? computerHand, JankenResultEnum outcome)
        {
            if (playerHand == null)
            {
                throw new ArgumentNullException(nameof(playerHand), "Player hand cannot be null.");
            }
            if (computerHand == null)
            {
                throw new ArgumentNullException(nameof(computerHand), "Computer hand cannot be null.");
            }
            var record = new JankenRecord(playerHand.Value, computerHand.Value, outcome);
            this.Add(record);
        }
        public int TotalWins => this.Count(r => r.Outcome == JankenResultEnum.Win);
        public int TotalLosses => this.Count(r => r.Outcome == JankenResultEnum.Lose);
        public int TotalDraws => this.Count(r => r.Outcome == JankenResultEnum.Draw);
    }
}
