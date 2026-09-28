using BoilerApplication.IOHelper;
using BoilerApplication.Models;
using BoilerApplication.Models.Enums;
using BoilerApplication.Repository;
using BoilerApplication.Services;

namespace BoilerApplication.View
{
    /// <summary>
    /// Represents the UI for the Boiler .
    /// </summary>
    public class BoilerView
    {
        private BoilerService _boilerService;

        private Boiler _boiler;

        private ILogger _logger;

        private static object _UILock = new object();


        /// <summary>
        /// Initializes a new instance of the <see cref="BoilerView"/> class.
        /// </summary>
        /// <param name="boilerService">Boiler service</param>
        /// <param name="boiler">Boiler info</param>
        /// <param name="logger">Logger</param>
        public BoilerView(BoilerService boilerService, Boiler boiler, ILogger logger)
        {
            _boilerService = boilerService;
            _boiler = boiler;
            _logger = logger;
            _boilerService.OnProcessing += DisplayDashBoard;
        }

        public async Task RunApplicationAsync()
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
                            result = await _boilerService.StartAsync();
                            break;
                        case MenuOptions.Stop:
                            result = await _boilerService.StopAsync();
                            break;
                        case MenuOptions.ErrorSimulation:
                            _boilerService.ThrowError();
                            break;
                        case MenuOptions.Toggle:
                            result = await _boilerService.ToggleInterLockStateAsync();
                            break;
                        case MenuOptions.Reset:
                            result = _boilerService.ResetBoiler();
                            break;
                        case MenuOptions.ViewLog:
                            await DisplayLogAsync();
                            break;
                        case MenuOptions.Exit:
                            isRunning = false;
                            DisplayMessage("Exiting the application");
                            break;
                        default:
                            DisplayMessage("Enter the correct choice");
                            break;
                    }

                    Console.Clear();
                    DisplayMessage($"\n{result}\n");
                    DisplayStateMessage($"After your operation" +
                         $"\nBoiler State: {_boiler.GetBoilerState()}" +
                         $"\nInter Lock State : {_boiler.GetInterLockState()}");


                }

                catch (InvalidOperationException ex)
                {
                    DisplayMessage("Machine failed Resetting to the normal state !!");
                    _boilerService.ResetBoiler();
                    await _logger.AppendLogAsync(DateTime.Now, "[Error]", $"{ex}");
                }

                catch (Exception ex)
                {
                    DisplayMessage($"Unexpected Error : {ex.Message}");
                    _boilerService.ResetBoiler();
                    await _logger.AppendLogAsync(DateTime.Now, "[Error]", $"{ex}");
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


        /// <summary>
        /// Displays the DashBoard.
        /// </summary>
        /// <param name="timeLeft">Time to be displayed.</param>
        /// <param name="state">State of the boiler.</param>
        private void DisplayDashBoard(string timeLeft, BoilerState state)
        {
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

        /// <summary>
        /// To display the State messages in the blue color.
        /// </summary>
        /// <param name="stateMessage">Sate info of the boiler.</param>
        private static void DisplayStateMessage(string stateMessage)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            DisplayMessage(stateMessage);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the Message in default color.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        private static void DisplayMessage(string message)
        {
            lock (_UILock)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Displays the Log to the user.
        /// </summary>
        /// <returns></returns>
        private async Task DisplayLogAsync()
        {
            string[] data = await _boilerService.LoadFromFileAsync();

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
