namespace GameEngine.Systems
{
    public static class GameRecord
    {
        public static int TotalWins { get; private set; }
        public static int TotalLosses { get; private set; }
        public static int TotalGames => TotalWins + TotalLosses;
        public static void RecordWin()
        {
            TotalWins++;
        }
        public static void RecordLoss()
        {
            TotalLosses++;
        }
        public static void ShowRecord()
        {
            Console.WriteLine($"Total Wins: {TotalWins}");
            Console.WriteLine($"Total Losses: {TotalLosses}");
            Console.WriteLine($"Total Games: {TotalGames}");
            Console.WriteLine($"Win Rate: {(TotalGames == 0 ? 0 : (double)TotalWins / TotalGames * 100):F2}%");
        }
    }
}
