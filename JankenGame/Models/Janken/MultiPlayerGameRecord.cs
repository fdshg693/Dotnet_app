namespace JankenGame.Models.Janken
{
    /// <summary>
    /// 複数プレイヤーのジャンケンゲーム1回分の結果を表すレコード
    /// </summary>
    public sealed record MultiPlayerGameRecord(
        Dictionary<string, JankenHand> PlayerHands, // プレイヤーID -> 出された手
        JankenHand? WinningHand, // 勝った手（引き分けの場合はnull）
        List<string> WinnerIds, // 勝者のプレイヤーID一覧
        DateTime Timestamp)
    {
        /// <summary>
        /// 新しいMultiPlayerGameRecordインスタンスを作成します。タイムスタンプは現在時刻に設定されます。
        /// </summary>
        public MultiPlayerGameRecord(Dictionary<string, JankenHand> playerHands, JankenHand? winningHand, List<string> winnerIds)
            : this(playerHands, winningHand, winnerIds, DateTime.Now)
        {
        }
    }

    /// <summary>
    /// 複数プレイヤーのジャンケンゲーム結果を管理するクラス
    /// </summary>
    public class MultiPlayerGameRecordList : List<MultiPlayerGameRecord>
    {
        /// <summary>
        /// ゲーム結果を記録に追加します
        /// </summary>
        public void AddRecord(Dictionary<string, JankenHand> playerHands, JankenHand? winningHand, List<string> winnerIds)
        {
            if (playerHands == null || playerHands.Count == 0)
            {
                throw new ArgumentNullException(nameof(playerHands), "Player hands cannot be null or empty.");
            }
            var record = new MultiPlayerGameRecord(playerHands, winningHand, winnerIds);
            this.Add(record);
        }

        /// <summary>
        /// 特定のプレイヤーが勝った回数を取得します
        /// </summary>
        public int GetWins(string playerId)
        {
            return this.Count(r => r.WinnerIds.Contains(playerId));
        }

        /// <summary>
        /// 特定のプレイヤーが敗けた回数を取得します
        /// </summary>
        public int GetLosses(string playerId)
        {
            return this.Count(r => r.WinningHand != null && !r.WinnerIds.Contains(playerId));
        }

        /// <summary>
        /// 特定のプレイヤーが引き分けになった回数を取得します
        /// </summary>
        public int GetDraws(string playerId)
        {
            return this.Count(r => r.WinningHand == null);
        }
    }
}
