namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// ゲーム開始状態
    /// </summary>
    public class StartState : IGameState
    {
        public string Name => "Start";

        public IGameState Execute(GameFlowContext context)
        {
            context.WriteLine("\n=== Game Start ===");
            context.ShowPlayerInfo();

            return new EncounterState();
        }
    }
}