namespace BoilerApplication.Repository
{
    public class Logger : ILogger
    {
        /// <summary>
        /// A lock to the file where a single thread can enter one at a time.
        /// </summary>
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        private readonly string _filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="filePath"></param>
        public Logger(string filePath)
        {
            _filePath = filePath;
        }

        /// <inheritdoc/>
        public async Task AppendLogAsync(DateTime timeStamp, string activity, string message)
        {
            await _fileLock.WaitAsync();

            try
            {
                if (!File.Exists(_filePath))
                {
                    using StreamWriter initialWriter = new StreamWriter(_filePath);
                    await initialWriter.WriteLineAsync("TimeStamp,Event,Event Data");
                    return;
                }

                using StreamWriter writer = new StreamWriter(_filePath, append: true);
                await writer.WriteLineAsync($"{timeStamp},{activity},{message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<string[]> LoadFromFileAsync()
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
