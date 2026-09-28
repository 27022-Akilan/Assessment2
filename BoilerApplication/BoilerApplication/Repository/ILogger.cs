namespace BoilerApplication.Repository
{
    public interface ILogger
    {
        /// <summary>
        /// Loads the content from the file.
        /// </summary>
        /// <returns>Array of string representing the logs.</returns>
        Task<string[]> LoadFromFileAsync();

        /// <summary>
        /// Appends the log to the file.
        /// </summary>
        /// <param name="timeStamp">Timestamp of the log.</param>
        /// <param name="event">Event that have occured.</param>
        /// <param name="message">Message describing the event.</param>
        /// <returns></returns>
        Task AppendLogAsync(DateTime timeStamp, string @event, string message);
    }
}
