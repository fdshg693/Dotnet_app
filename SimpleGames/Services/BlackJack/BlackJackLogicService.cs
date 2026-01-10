using JankenGame.Models.BlackJack;

namespace JankenGame.Services.BlackJack
{
    /// <summary>
    /// ブラックジャックの勝敗判定などの純粋なロジックを提供するサービス
    /// </summary>
    public class BlackJackLogicService
    {
        /// <summary>
        /// ゲームの結果を判定
        /// </summary>
        public string DetermineWinner(int playerScore, int dealerScore, bool playerBust, bool dealerBust)
        {
            if (playerBust)
                return "あなたはバースト！ディーラーの勝ち…";
            
            if (dealerBust)
                return "ディーラーがバースト！あなたの勝ち🎉";
            
            if (playerScore > dealerScore)
                return "あなたの勝ち🎉";
            
            if (playerScore < dealerScore)
                return "ディーラーの勝ち…";
            
            return "引き分け（プッシュ）";
        }

        /// <summary>
        /// ディーラーがヒットすべきかどうかを判定
        /// </summary>
        public bool ShouldDealerHit(int dealerScore)
        {
            return dealerScore < 17;
        }

        /// <summary>
        /// 複数プレイヤーの勝者を判定し、勝敗を記録
        /// </summary>
        /// <returns>勝者のリスト（引き分けのプレイヤーも含む）</returns>
        public List<BlackJackPlayer> DetermineWinners(List<BlackJackPlayer> activePlayers, BlackJackDealer dealer)
        {
            var winners = new List<BlackJackPlayer>();

            foreach (var player in activePlayers)
            {
                string result = DetermineWinner(
                    player.Score,
                    dealer.Score,
                    player.IsBust,
                    dealer.IsBust
                );

                if (result.Contains("あなたの勝ち") || result.Contains("プレイヤーの勝ち"))
                {
                    winners.Add(player);
                    player.RecordWin();
                    dealer.RecordLoss();
                }
                else if (result.Contains("ディーラーの勝ち") || result.Contains("バースト"))
                {
                    player.RecordLoss();
                    dealer.RecordWin();
                }
                else
                {
                    // 引き分けの場合もベットを返す（勝者リストに追加）
                    winners.Add(player);
                    player.RecordDraw();
                    dealer.RecordDraw();
                }
            }

            return winners;
        }

        /// <summary>
        /// 全プレイヤーの勝敗を記録
        /// </summary>
        public void RecordGameResults(List<BlackJackPlayer> players, BlackJackDealer dealer)
        {
            foreach (var player in players)
            {
                string result = DetermineWinner(
                    player.Score,
                    dealer.Score,
                    player.IsBust,
                    dealer.IsBust
                );

                if (result.Contains("あなたの勝ち") || result.Contains("プレイヤーの勝ち"))
                {
                    player.RecordWin();
                    dealer.RecordLoss();
                }
                else if (result.Contains("ディーラーの勝ち") || result.Contains("バースト"))
                {
                    player.RecordLoss();
                    dealer.RecordWin();
                }
                else
                {
                    player.RecordDraw();
                    dealer.RecordDraw();
                }
            }
        }
    }
}
