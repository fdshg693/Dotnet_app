namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ブラックジャックのゲーム進行状態
    /// </summary>
    public enum BlackJackGameState
    {
        /// <summary>
        /// ゲーム開始前
        /// </summary>
        Waiting,

        /// <summary>
        /// アンティ（入場料）支払い
        /// </summary>
        Ante,

        /// <summary>
        /// 初期配牌
        /// </summary>
        InitialDeal,

        /// <summary>
        /// ベッティング中
        /// </summary>
        BettingRound,

        /// <summary>
        /// プレイヤーのカードアクション（ヒット/スタンド）
        /// </summary>
        PlayersTurn,

        /// <summary>
        /// ディーラーターン
        /// </summary>
        DealerTurn,

        /// <summary>
        /// 結果表示と精算
        /// </summary>
        Showdown,

        /// <summary>
        /// ゲーム終了
        /// </summary>
        GameOver
    }
}
