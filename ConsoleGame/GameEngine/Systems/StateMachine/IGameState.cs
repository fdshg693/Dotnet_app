namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// ゲームの状態を表すインターフェース
    /// </summary>
    public interface IGameState
    {
        string Name { get; }

        /// <summary>
        /// 状態の処理を実行し、次の状態を返す
        /// </summary>
        IGameState? Execute(GameFlowContext context);
    }
}