namespace JankenGame.Models.BlackJack
{
    public class BlackJackHand
    {
        public List<Card> Cards { get; private set; } = new();

        public void Add(Card c) => Cards.Add(c);

        public int Score
        {
            get
            {
                int sum = Cards.Sum(c => c.Value);
                // Ace を 1 として扱う調整
                int aceCount = Cards.Count(c => c.Rank == Rank.Ace);
                while (sum > 21 && aceCount > 0)
                {
                    sum -= 10;
                    aceCount--;
                }
                return sum;
            }
        }

        public bool IsBust => Score > 21;

        public void ResetCards()
        {
            Cards = new();
        }
    }
}
