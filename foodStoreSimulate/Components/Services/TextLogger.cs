namespace foodStoreSimulate.Components.Services
{
    public interface ILogger
    {
        void AddLog(string message);
    }

    public class TextLogger : ILogger
    {
        public string LogText { get; private set; } = "";

        public void AddLog(string message)
        {
            LogText += message + "\n";
        }
    }

}
