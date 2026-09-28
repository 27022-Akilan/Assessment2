namespace BoilerApplication.Repository
{
    public class Logger : ILogger
    {
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        private readonly string _filePath;

        public Logger(string filePath)
        {
            _filePath = filePath;
        }

        public async Task AppendLog(DateTime timeStamp, string activity, string message)
        {
            await _fileLock.WaitAsync();

            try
            {
                if (!File.Exists(_filePath))
                {
                    using StreamWriter initialWriter = new StreamWriter(_filePath);
                    initialWriter.WriteLine("TimeStamp", "Event", "Event Data");
                    return;
                }

                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                writer.WriteLine($"{timeStamp},{activity},{message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<string[]> LoadFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return new string[0];
            }
            await _fileLock.WaitAsync();
            try
            {
                return await File.ReadAllLinesAsync(_filePath, default);
            }

            finally { _fileLock.Release(); }
        }
    }
}
