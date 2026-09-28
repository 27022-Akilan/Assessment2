using BoilerApplication.IOHelper;
using BoilerApplication.Models;
using BoilerApplication.Models.Enums;

namespace BoilerApplication
{
    public class View
    {
        private Service _boilerService;

        private static object _UILock = new object();

        private Boiler _boiler;
        public View(Service boilerService, Boiler boiler)
        {
            this._boilerService = boilerService;
            _boiler = boiler;
            _boilerService.ReflectTime += DisplayDashBoard;
        }

        private void DisplayDashBoard(string timeLeft, BoilerState state)
        {
            CleanDashBoard();
            if (!Monitor.TryEnter(_UILock))
            {
                return;
            }
            try
            {
                (int left, int right) = Console.GetCursorPosition();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine($"State : {state} | Time left out  : {timeLeft}");
                Console.ResetColor();
                Console.SetCursorPosition(left, right);
            }
            finally
            {
                Monitor.Exit(_UILock);
            }
        }

        private void CleanDashBoard()
        {
            Console.WriteLine();
        }

        public async Task RunApplication()
        {
            bool isRunning = true;
            while (isRunning)
            {
                DisplayMenu();
                bool getOption = InputReader.GetInput<MenuOptions>("\nEnter the option : ",
                                                        Validator.GetEnumOption<MenuOptions>,
                                                        out MenuOptions option);

                if (!getOption)
                    continue;
                string result = "";
                try
                {

                    DisplayMessage($"Last Status" +
                        $"\nBoiler State: {_boiler.GetBoilerState()}" +
                        $"\nIl state {_boiler.GetInterLockState()}");

                    switch (option)
                    {
                        case MenuOptions.Start:
                            result = await _boilerService.Start();
                            break;
                        case MenuOptions.Stop:
                            result = await _boilerService.Stop();
                            break;
                        case MenuOptions.ErrorSimulation:
                            _boilerService.ThrowError();
                            break;
                        case MenuOptions.Toggle:
                            result = await _boilerService.ToggleInterLockState();
                            break;
                        case MenuOptions.Reset:
                            result = _boilerService.ResetBoiler();
                            break;
                        case MenuOptions.ViewLog:
                            await DisplayLog();
                            break;
                        case MenuOptions.Exit:
                            isRunning = false;
                            DisplayMessage("Exiting the application");
                            break;
                        default:
                            DisplayMessage("Enter the correct choice");
                            break;
                    }

                    DisplayMessage(result);
                    DisplayMessage($"After your operation" +
                         $"\nBoiler State: {_boiler.GetBoilerState()}" +
                         $"\nIl state {_boiler.GetInterLockState()}");


                }

                catch (InvalidOperationException ex)
                {
                    DisplayMessage("Machine failed Resetting to the normal state !!");
                    _boilerService.ResetBoiler();
                }

                catch (Exception e)
                {
                    DisplayMessage($"Unexpected Error : {e.Message}");
                }
            }
        }

        private void DisplayMenu()
        {
            DisplayMessage("\n1.Start" +
                           "\n2.Stop" +
                           "\n3.SimulateError" +
                           "\n4.Toggle" +
                           "\n5.Reset" +
                           "\n6.View Log" +
                           "\n7.Exit");
        }

        public static void DisplaySuccessMessage(string s)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        private static void DisplayMessage(string s)
        {
            Console.WriteLine(s);
        }

        private async Task DisplayLog()
        {
            string[] data = await _boilerService.LoadFromFile();

            lock (_UILock)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    Console.WriteLine(data[i]);
                }
            }
        }
    }
}
