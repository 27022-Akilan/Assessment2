
namespace BoilerApplication.Repository
{
    public class Logger : ILogger
    {
        private readonly string _filePath;
        public Logger(string filePath)
        {
            _filePath = filePath;
        }

        public void AppendLog(DateTime dateTime, string description, string message)
        {
            throw new NotImplementedException();
        }

        public void LoadFromFile()
        {
            throw new NotImplementedException();
        }
    }
}
