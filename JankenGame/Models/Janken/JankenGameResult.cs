namespace JankenGame.Models.Janken
{
    /// <summary>
    /// 複数プレイヤーのジャンケンゲームの結果を表すクラス
    /// </summary>
    public class JankenGameResult
    {
        /// <summary>
        /// 勝者が存在するかどうか（全員引き分けの場合はfalse）
        /// </summary>
        public bool ExistsWinner { get; }

        /// <summary>
        /// 勝った手（引き分けの場合はnull）
        /// </summary>
        public JankenHand? WinningHand { get; }

        /// <summary>
        /// JankenGameResultのコンストラクタ
        /// </summary>
        /// <param name="existsWinner">勝者が存在するか</param>
        /// <param name="winningHand">勝った手（引き分けの場合はnull）</param>
        public JankenGameResult(bool existsWinner, JankenHand? winningHand)
        {
            ExistsWinner = existsWinner;
            WinningHand = winningHand;
        }

        /// <summary>
        /// 引き分けの結果を作成します
        /// </summary>
        /// <returns>引き分けを表すJankenGameResult</returns>
        public static JankenGameResult CreateDraw()
        {
            return new JankenGameResult(false, null);
        }

        /// <summary>
        /// 勝者ありの結果を作成します
        /// </summary>
        /// <param name="winningHand">勝った手</param>
        /// <returns>勝者ありを表すJankenGameResult</returns>
        public static JankenGameResult CreateWinner(JankenHand winningHand)
        {
            return new JankenGameResult(true, winningHand);
        }

        /// <summary>
        /// プレイヤー視点の勝敗結果を取得します
        /// </summary>
        /// <param name="playerHand">プレイヤーの手</param>
        /// <returns>プレイヤーから見た勝敗結果</returns>
        public JankenResultEnum ToPlayerResult(JankenHand playerHand)
        {
            // 勝者がいない場合は引き分け
            if (!ExistsWinner)
            {
                return JankenResultEnum.Draw;
            }

            // プレイヤーの手が勝った手と一致する場合は勝ち
            return WinningHand == playerHand
                ? JankenResultEnum.Win
                : JankenResultEnum.Lose;
        }
    }
}
