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

        /// <summary>
        /// 勝者が存在するかどうか（全員引き分けの場合はfalse）
        /// </summary>
        public bool ExistsWinner => WinningHand != null && WinnerIds.Count > 0;

        /// <summary>
        /// プレイヤー視点の勝敗結果を取得します
        /// </summary>
        /// <param name="playerId">プレイヤーID</param>
        /// <returns>プレイヤーから見た勝敗結果</returns>
        public JankenResultEnum ToPlayerResult(string playerId)
        {
            // 勝者がいない場合は引き分け
            if (!ExistsWinner)
            {
                return JankenResultEnum.Draw;
            }

            // プレイヤーが勝者リストに含まれる場合は勝ち
            return WinnerIds.Contains(playerId)
                ? JankenResultEnum.Win
                : JankenResultEnum.Lose;
        }

        /// <summary>
        /// 引き分けの結果を作成します（タイムスタンプは現在時刻）
        /// </summary>
        public static MultiPlayerGameRecord CreateDraw(Dictionary<string, JankenHand> playerHands)
        {
            return new MultiPlayerGameRecord(playerHands, null, new List<string>());
        }

        /// <summary>
        /// 勝者ありの結果を作成します（タイムスタンプは現在時刻）
        /// </summary>
        public static MultiPlayerGameRecord CreateWinner(Dictionary<string, JankenHand> playerHands, JankenHand winningHand, List<string> winnerIds)
        {
            return new MultiPlayerGameRecord(playerHands, winningHand, winnerIds);
        }
    }

    /// <summary>
    /// 複数プレイヤーのジャンケンゲーム結果を管理する静的ヘルパークラス
    /// </summary>
    public static class MultiPlayerGameRecordHelper
    {
        /// <summary>
        /// ゲーム結果を記録に追加します
        /// </summary>
        public static MultiPlayerGameRecord AddRecord(
            Dictionary<string, JankenHand> playerHands, 
            JankenHand? winningHand, 
            List<string> winnerIds)
        {
            if (playerHands == null || playerHands.Count == 0)
            {
                throw new ArgumentNullException(nameof(playerHands), "Player hands cannot be null or empty.");
            }
            return new MultiPlayerGameRecord(playerHands, winningHand, winnerIds);
        }

        /// <summary>
        /// 特定のプレイヤーが勝った回数を取得します
        /// </summary>
        public static int GetWins(this IEnumerable<MultiPlayerGameRecord> records, string playerId)
        {
            return records.Count(r => r.WinnerIds.Contains(playerId));
        }

        /// <summary>
        /// 特定のプレイヤーが敗けた回数を取得します
        /// </summary>
        public static int GetLosses(this IEnumerable<MultiPlayerGameRecord> records, string playerId)
        {
            return records.Count(r => r.WinningHand != null && !r.WinnerIds.Contains(playerId));
        }

        /// <summary>
        /// 特定のプレイヤーが引き分けになった回数を取得します
        /// </summary>
        public static int GetDraws(this IEnumerable<MultiPlayerGameRecord> records, string playerId)
        {
            return records.Count(r => r.WinningHand == null);
        }
    }
}
