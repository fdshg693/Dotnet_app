namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// ゲーム終了状態
    /// </summary>
    public class GameOverState : IGameState
    {
        public string Name => "GameOver";

        public IGameState? Execute(GameFlowContext context)
        {
            context.DisplayGameOver();
            return null;
        }
    }
}