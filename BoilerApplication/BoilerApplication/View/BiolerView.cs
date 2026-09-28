using BoilerApplication.IOHelper;
using BoilerApplication.Models;
using BoilerApplication.Models.Enums;
using BoilerApplication.Repository;
using BoilerApplication.Services;

namespace BoilerApplication.View
{
    public class BiolerView
    {
        private Service _boilerService;

        private Boiler _boiler;

        private ILogger _logger;

        private static object _UILock = new object();

        public BiolerView(Service boilerService, Boiler boiler, ILogger logger)
        {
            _boilerService = boilerService;
            _boiler = boiler;
            _logger = logger;
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
                bool getOption = InputReader.GetInput("\nEnter the option : ",
                                                        Validator.GetEnumOption<MenuOptions>,
                                                        out MenuOptions option);

                if (!getOption)
                    continue;
                string result = "";
                try
                {
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
                    DisplayStateMessage($"After your operation" +
                         $"\nBoiler State: {_boiler.GetBoilerState()}" +
                         $"\nIl state {_boiler.GetInterLockState()}");


                }

                catch (InvalidOperationException ex)
                {
                    DisplayMessage("Machine failed Resetting to the normal state !!");
                    _boilerService.ResetBoiler();
                    await _logger.AppendLog(DateTime.Now, "[Error]", $"{ex}");
                }

                catch (Exception ex)
                {
                    DisplayMessage($"Unexpected Error : {ex.Message}");
                    _boilerService.ResetBoiler();
                    await _logger.AppendLog(DateTime.Now, "[Error]", $"{ex}");
                }
            }
        }

        private void DisplayMenu()
        {
            DisplayMessage("\n1.Start" +
                           "\n2.Stop" +
                           "\n3.SimulateError" +
                           "\n4.Toggle(open/close)" +
                           "\n5.Reset" +
                           "\n6.View Log" +
                           "\n7.Exit");
        }

        public static void DashBoardMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            DisplayMessage(message);
            Console.ResetColor();
        }

        public static void DisplayStateMessage(string stateMessage)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            DisplayMessage(stateMessage);
            Console.ResetColor();
        }

        private static void DisplayMessage(string message)
        {
            lock (_UILock)
            {
                Console.WriteLine(message);
            }
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
