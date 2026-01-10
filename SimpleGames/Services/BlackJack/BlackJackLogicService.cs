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
        /// ブラックジャック（21）かどうかを判定
        /// </summary>
        public bool IsBlackjack(int score, int cardCount)
        {
            return score == 21 && cardCount == 2;
        }
    }
}
