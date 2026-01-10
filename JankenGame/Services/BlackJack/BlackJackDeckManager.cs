using JankenGame.Models.BlackJack;

namespace JankenGame.Services.BlackJack
{
    /// <summary>
    /// ブラックジャックのデッキ管理を行うサービス
    /// デッキからカードを引く、デッキの再構築、場のカード管理を担当
    /// </summary>
    public class BlackJackDeckManager
    {
        private Deck _deck;
        private readonly List<Card> _cardsInPlay = new();

        public BlackJackDeckManager()
        {
            _deck = new Deck();
        }

        /// <summary>
        /// カードを引く（デッキが空になったら再構築）
        /// </summary>
        public Card DrawCard()
        {
            Card card;
            try
            {
                card = _deck.Draw();
            }
            catch (InvalidOperationException)
            {
                // デッキが空になったら、場にあるカードを除いて再構築
                RebuildDeck();
                card = _deck.Draw();
            }
            
            _cardsInPlay.Add(card);
            return card;
        }

        /// <summary>
        /// デッキを再構築（場にあるカードを除く）
        /// </summary>
        private void RebuildDeck()
        {
            // 全カードを生成
            var allCards = Deck.CreateAllCards();

            // 場にあるカードを除外
            var availableCards = allCards
                .Where(card => !_cardsInPlay.Any(inPlay => 
                    inPlay.Suit == card.Suit && inPlay.Rank == card.Rank))
                .ToList();

            _deck = new Deck(availableCards);
        }

        /// <summary>
        /// デッキをリセット（場のカードをクリア）
        /// </summary>
        public void ResetDeck()
        {
            _cardsInPlay.Clear();
            _deck = new Deck();
        }

        /// <summary>
        /// 場にあるカードをクリア（ゲームリセット時に使用）
        /// </summary>
        public void ClearCardsInPlay()
        {
            _cardsInPlay.Clear();
        }
    }
}
