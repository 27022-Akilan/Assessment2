namespace BoilerApplication.IOHelper
{
    public class Validator
    {
        public static (bool IsSuccess, T Value, string Message) GetEnumOption<T>(string input) where T : struct, Enum
        {
            var (isSuccess, value, message) = ParseEnum<T>(input);
            if (!isSuccess)
                return (false, default, message);

            if (!Enum.IsDefined<T>(value))
                return (false, default, "Enter one of the options shown on the screen");

            return (true, value, "success");
        }

        public static (bool IsSuccess, T Value, string Message) ParseEnum<T>(string input) where T : struct, Enum
        {
            if (!Enum.TryParse<T>(input, true, out T result))
                return (false, default, "Not a valid option");

            return (true, result, "success");
        }
    }
}
