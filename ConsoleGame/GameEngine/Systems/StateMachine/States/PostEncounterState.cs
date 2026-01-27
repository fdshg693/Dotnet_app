namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// エンカウント後の確認状態
    /// </summary>
    public class PostEncounterState : IGameState
    {
        public string Name => "PostEncounter";

        public IGameState Execute(GameFlowContext context)
        {
            context.ShowPlayerInfo();

            if (!context.ConfirmContinue())
            {
                context.WriteLine("\nGame ended by player choice.");
                return new GameOverState();
            }

            return new EncounterState();
        }
    }
}