using JankenGame.Models.Janken;

namespace JankenGame.Services.Janken
{
    /// <summary>
    /// ジャンケンチャレンジモードのビジネスロジックを処理するサービスクラス
    /// </summary>
    public class JankenChallengeService
    {
        private readonly Random _random = new();
        private readonly JankenLogicService _logicService;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public JankenChallengeService()
        {
            _logicService = new JankenLogicService();
        }

        /// <summary>
        /// ランダムにコンピューターの手を生成する
        /// </summary>
        /// <returns>コンピューターの手</returns>
        public JankenHand GenerateComputerHand()
        {
            return (JankenHand)_random.Next(0, 3);
        }

        /// <summary>
        /// プレイヤーの選択がコンピューターに勝つ手かどうかを判定する
        /// </summary>
        /// <param name="playerHand">プレイヤーが選んだ手</param>
        /// <param name="computerHand">コンピューターの手</param>
        /// <returns>プレイヤーが勝つ手を選んだ場合はtrue</returns>
        public bool IsCorrectAnswer(JankenHand playerHand, JankenHand computerHand)
        {
            return _logicService.IsWinning(playerHand, computerHand);
        }

        /// <summary>
        /// コンピューターの手に対して勝つ手を取得する（答え）
        /// </summary>
        /// <param name="computerHand">コンピューターの手</param>
        /// <returns>勝つ手</returns>
        public JankenHand GetWinningHand(JankenHand computerHand)
        {
            return _logicService.GetWinningHand(computerHand);
        }
    }
}
