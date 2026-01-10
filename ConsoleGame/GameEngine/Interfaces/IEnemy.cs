namespace GameEngine.Interfaces
{
    public interface IEnemy : ICharacter
    {
        int Experience { get; }
        int Gold { get; }
        IAttackStrategy _attackStrategy { get; }
    }
}
