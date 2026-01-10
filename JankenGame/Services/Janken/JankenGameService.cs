using JankenGame.Models.Janken;

namespace JankenGame.Services.Janken
{
    /// <summary>
    /// ジャンケンゲームのビジネスロジックを処理するサービスクラス
    /// </summary>
    public class JankenGameService
    {
        private readonly JankenLogicService _logicService;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public JankenGameService()
        {
            _logicService = new JankenLogicService();
        }

        /// <summary>
        /// 複数プレイヤーのジャンケン結果を判定します
        /// </summary>
        /// <param name="players">プレイヤー一覧</param>
        /// <returns>勝った手と勝者のプレイヤーID一覧</returns>
        public (JankenHand? winningHand, List<string> winnerIds) DetermineMultiPlayerWinner(List<JankenPlayer> players)
        {
            var hands = players.Select(p => p.Hand).OfType<JankenHand>().ToArray();
            if (hands.Length != players.Count)
            {
                throw new ArgumentException("すべてのプレイヤーが手を出す必要があります");
            }

            var gameResult = _logicService.GetWinningHands(hands);
            
            if (!gameResult.ExistsWinner)
            {
                return (null, new List<string>());
            }

            var winnerIds = players
                .Where(p => p.Hand == gameResult.WinningHand)
                .Select(p => p.Id)
                .ToList();

            return (gameResult.WinningHand, winnerIds);
        }
    }
}
