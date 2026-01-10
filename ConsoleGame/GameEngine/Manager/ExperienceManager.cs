using GameEngine.Constants;

namespace GameEngine.Manager
{
    public class ExperienceManager
    {
        public int TotalExperience { get; private set; } = 0;
        public int Level { get; private set; } = 1;
        /// <summary>
        /// Gain experience points and check for level up.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>LevelUp</returns>
        public int GainExperience(int amount)
        {
            Console.WriteLine($"You gain {amount} experience");
            TotalExperience += amount;
            if (TotalExperience >= GameConstants.ExperienceRequiredForLevelUp)
            {
                Level++;
                TotalExperience -= GameConstants.ExperienceRequiredForLevelUp;
                Console.WriteLine($"Levele UP to level {Level}!");
                return 1;
            }
            return 0;
        }
        public void ShowInfo()
        {
            Console.WriteLine($"Total Experience: {TotalExperience}");
            Console.WriteLine($"Level: {Level}");
        }
    }
}
