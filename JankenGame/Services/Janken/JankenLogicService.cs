using JankenGame.Models.Janken;

namespace JankenGame.Services.Janken
{
    /// <summary>
    /// ジャンケンの基本ロジックを提供するサービスクラス
    /// このクラスはジャンケンのルールを一元管理し、他のサービスから利用されます
    /// </summary>
    public class JankenLogicService
    {
        /// <summary>
        /// 指定された手に対して勝つ手を取得します
        /// </summary>
        /// <param name="hand">対戦相手の手</param>
        /// <returns>勝つ手</returns>
        public JankenHand GetWinningHand(JankenHand hand)
        {
            return hand switch
            {
                JankenHand.Rock => JankenHand.Paper,      // グーにはパーが勝つ
                JankenHand.Paper => JankenHand.Scissors,  // パーにはチョキが勝つ
                JankenHand.Scissors => JankenHand.Rock,   // チョキにはグーが勝つ
                _ => throw new ArgumentException("無効な手です", nameof(hand))
            };
        }

        /// <summary>
        /// 複数の手から勝った手を判定します（コアロジック）
        /// </summary>
        /// <param name="hands">ジャンケンで出された手の配列</param>
        /// <returns>ゲーム結果（勝者の有無と勝った手）</returns>
        /// <exception cref="ArgumentException">手の配列が空またはnullの場合</exception>
        public JankenGameResult GetWinningHands(params JankenHand[] hands)
        {
            if (hands == null || hands.Length == 0)
            {
                throw new ArgumentException("手の配列が空です", nameof(hands));
            }

            // 出された手の種類をカウント
            var distinctHands = hands.Distinct().ToList();

            // 全員同じ手 → 引き分け
            if (distinctHands.Count == 1)
            {
                return JankenGameResult.CreateDraw();
            }

            // 3種類全て出ている → 引き分け
            if (distinctHands.Count == 3)
            {
                return JankenGameResult.CreateDraw();
            }

            // 2種類の場合 → どちらかが勝つ
            // distinctHands[0] と distinctHands[1] のどちらが勝つかを判定
            var hand1 = distinctHands[0];
            var hand2 = distinctHands[1];

            // hand1 が hand2 に勝つ手かどうか判定
            if (hand1 == GetWinningHand(hand2))
            {
                return JankenGameResult.CreateWinner(hand1);
            }
            else
            {
                return JankenGameResult.CreateWinner(hand2);
            }
        }

        /// <summary>
        /// プレイヤーの手が相手の手に勝つかどうかを判定します
        /// </summary>
        /// <param name="playerHand">プレイヤーの手</param>
        /// <param name="opponentHand">相手の手</param>
        /// <returns>プレイヤーが勝つ場合はtrue</returns>
        public bool IsWinning(JankenHand playerHand, JankenHand opponentHand)
        {
            var gameResult = GetWinningHands(playerHand, opponentHand);
            return gameResult.ToPlayerResult(playerHand) == JankenResultEnum.Win;
        }
    }
}
