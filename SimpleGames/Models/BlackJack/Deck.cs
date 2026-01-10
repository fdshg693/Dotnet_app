namespace JankenGame.Models.BlackJack
{
    public class Deck
    {
        private Stack<Card> _cards;
        private static readonly Random _rnd = new();

        /// <summary>
        /// 全カード（52枚）でデッキを構築
        /// </summary>
        public Deck() : this(CreateAllCards())
        {
        }

        /// <summary>
        /// 指定されたカードリストからデッキを構築（自動的にシャッフル）
        /// </summary>
        public Deck(List<Card> cards)
        {
            _cards = new Stack<Card>(cards.OrderBy(_ => _rnd.Next()));
        }

        /// <summary>
        /// 全52枚のカードを生成
        /// </summary>
        public static List<Card> CreateAllCards()
        {
            var cards = new List<Card>();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                    cards.Add(new Card { Suit = s, Rank = r });
            return cards;
        }

        public Card Draw() => _cards.Pop();
        
        public int RemainingCards => _cards.Count;
    }
}
