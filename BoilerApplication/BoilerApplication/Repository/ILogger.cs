namespace BoilerApplication.Repository
{
    public interface ILogger
    {
        void LoadFromFile();

        void AppendLog(DateTime dateTime, string description, string message);
    }
}
