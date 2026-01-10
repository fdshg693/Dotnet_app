using GameEngine.Constants;
using GameEngine.Interfaces;

namespace GameEngine.Models
{
    public class Enemy : IEnemy
    {
        public string Name { get; private set; }
        public int HP { get; private set; }

        public int BaseHP { get; private set; }
        public int BaseAP { get; private set; }
        public int BaseDP { get; private set; }
        public IAttackStrategy _attackStrategy { get; private set; }

        public bool IsAlive => HP > 0;
        public int Experience { get; private set; } = 0;
        public int Gold { get; private set; } = 0;

        public Enemy(string name, int hp, IAttackStrategy attackStrategy, int experience, int aP, int dP)
        {
            Name = name;
            HP = hp;
            BaseHP = hp;
            _attackStrategy = attackStrategy;
            Experience = experience;
            Gold = (experience / GameConstants.EnemyGoldBaseMultiplier) + new Random().Next(GameConstants.EnemyGoldRandomMin, GameConstants.EnemyGoldRandomMax);
            BaseAP = aP;
            BaseDP = dP;
        }
        public void Attack(ICharacter character)
        {
            character.TakeDamage(_attackStrategy.ExecuteAttack() + BaseAP);
        }
        public void TakeDamage(int amount)
        {
            int damage = amount - BaseDP;
            HP -= damage;
            if (HP < 0) HP = 0;
            Console.WriteLine($"{Name} takes {damage} damage! Remaining HP: {HP}");
        }
        public void Heal(int amount)
        {
            HP += amount;
            if (HP > BaseHP) HP = BaseHP;
        }
        public void ChangeAttackStrategy(string AttackStrategyName)
        {
            _attackStrategy = AttackStrategy.GetAttackStrategy(AttackStrategyName);
        }
    }
}