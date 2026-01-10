using GameEngine.Constants;
using GameEngine.Interfaces;

namespace GameEngine.Models
{
    public static class AttackStrategy
    {
        public static IAttackStrategy GetAttackStrategy(string attackType)
        {
            return attackType switch
            {
                "Melee" => new MeleeAttackStrategy(),
                "Magic" => new MagicAttackStrategy(),
                _ => new DefaultAttackStrategy()
            };
        }
    }
    public class DefaultAttackStrategy : IAttackStrategy
    {
        public int ExecuteAttack() => new Random().Next(GameConstants.AttackDamage.DefaultMin, GameConstants.AttackDamage.DefaultMax);
        public string GetAttackStrategyName()
        {
            return "Default";
        }
    }
    public class MeleeAttackStrategy : IAttackStrategy
    {
        public int ExecuteAttack() => new Random().Next(GameConstants.AttackDamage.MeleeMin, GameConstants.AttackDamage.MeleeMax);
        public string GetAttackStrategyName()
        {
            return "Melee";
        }
    }

    public class MagicAttackStrategy : IAttackStrategy
    {
        public int ExecuteAttack() => new Random().Next(GameConstants.AttackDamage.MagicMin, GameConstants.AttackDamage.MagicMax);

        public string GetAttackStrategyName()
        {
            return "Magic";
        }
    }
}
