namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ベッティングアクションの種類
    /// </summary>
    public enum BettingAction
    {
        /// <summary>
        /// 降りる
        /// </summary>
        Fold,

        /// <summary>
        /// 揃える（現在のベット額に合わせる）
        /// </summary>
        Call,

        /// <summary>
        /// 上乗せ（現在のベット額より多く賭ける）
        /// </summary>
        Raise,

        /// <summary>
        /// 追加ベットなし（現在のベット額が0の場合のみ）
        /// </summary>
        Check,

        /// <summary>
        /// 全額賭け
        /// </summary>
        AllIn
    }
}
