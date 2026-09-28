namespace BoilerApplication.Repository
{
    public interface ILogger
    {
        Task<string[]> LoadFromFile();

        Task AppendLog(DateTime dateTime, string description, string message);
    }
}
