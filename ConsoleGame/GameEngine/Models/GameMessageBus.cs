namespace GameEngine.Models
{
    /// <summary>
    /// ドメインメッセージの発行バス
    /// </summary>
    public static class GameMessageBus
    {
        public static event Action<GameMessage>? MessagePublished;

        public static void Publish(string text, MessageType type)
        {
            var message = GameStateMapper.CreateMessage(text, type);
            MessagePublished?.Invoke(message);
        }

        public static void Publish(GameMessage message)
        {
            MessagePublished?.Invoke(message);
        }
    }
}
