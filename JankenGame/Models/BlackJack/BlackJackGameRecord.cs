namespace JankenGame.Models.BlackJack
{
    /// <summary>
    /// ブラックジャックのゲーム結果を記録
    /// </summary>
    public record BlackJackGameRecord(
        DateTime PlayedAt,
        int PlayerScore,
        int DealerScore,
        string Result, // "プレイヤー勝利" / "ディーラー勝利" / "引き分け"
        bool PlayerBust,
        bool DealerBust
    );
}
