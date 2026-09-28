using BoilerApplication.ConstantData;

namespace BoilerApplication.IOHelper
{
    public class InputReader
    {
        public static bool GetInput<T>(
            string prompt,
            Func<string, (bool IsSuccess, T Value, string Message)> tryConvert,
            out T result)
        {
            for (int attempt = 1; attempt <= Constants.MaxAttempts; attempt++)
            {
                Console.Write(prompt);
                string userInput = Console.ReadLine() ?? string.Empty;

                var (isSuccess, value, message) = tryConvert(userInput);
                if (!isSuccess)
                {
                    DisplayMessage(message, Constants.MaxAttempts - attempt);
                    continue;
                }

                result = value;
                return true;
            }

            result = default!;
            DisplayMessage("Your attempts are completed, returning to main menu", 0);
            return false;
        }

        public static void DisplayMessage(string message, int attemptLeft)
        {
            Console.WriteLine($"{message} and Attempts left is : {attemptLeft}");
        }
    }
}
