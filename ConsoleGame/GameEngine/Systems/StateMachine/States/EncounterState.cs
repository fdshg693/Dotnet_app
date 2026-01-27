namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// エンカウント（イベント）状態
    /// </summary>
    public class EncounterState : IGameState
    {
        public string Name => "Encounter";

        public IGameState Execute(GameFlowContext context)
        {
            context.WriteLine("\n--- New Encounter ---");

            var eventResult = context.TriggerRandomEvent();
            context.RenderMessages(eventResult.Messages);

            if (!eventResult.ContinueGame || !context.IsPlayerAlive)
            {
                return new GameOverState();
            }

            return new PostEncounterState();
        }
    }
}