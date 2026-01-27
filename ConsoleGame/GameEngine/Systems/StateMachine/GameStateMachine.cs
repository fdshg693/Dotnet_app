namespace GameEngine.Systems.StateMachine
{
    /// <summary>
    /// 明示的な状態機械
    /// </summary>
    public class GameStateMachine
    {
        private IGameState? _currentState;
        private readonly GameFlowContext _context;

        public GameStateMachine(IGameState initialState, GameFlowContext context)
        {
            _currentState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Run()
        {
            while (_currentState != null)
            {
                var current = _currentState;
                var next = current.Execute(_context);

                _context.LogTransition(current.Name, next?.Name);

                _currentState = next;
            }
        }
    }
}