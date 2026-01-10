namespace JankenGame.Models.BlackJack
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }
    public enum Rank
    {
        Two = 2, Three, Four, Five, Six,
        Seven, Eight, Nine, Ten,
        Jack = 10, Queen = 10, King = 10, Ace = 11
    }

    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }
        public int Value => (int)Rank;
        public string Name => $"{Rank} of {Suit}";
    }
}
