namespace BoilerApplication.IOHelper
{
    public static class ConsolePresenter
    {
        public static void DisplayMessage(string s)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ResetColor();
        }
    }
}
