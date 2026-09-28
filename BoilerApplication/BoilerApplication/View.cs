using BoilerApplication.IOHelper;
using BoilerApplication.Models;
using BoilerApplication.Models.Enums;

namespace BoilerApplication
{
    public class View
    {
        private Service _boilerService;

        private Boiler _boiler;
        public View(Service boilerService, Boiler boiler)
        {
            this._boilerService = boilerService;
            _boiler = boiler;
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

                    ConsolePresenter.DisplayMessage($"Last Status" +
                        $"\nBoiler State: {_boiler.GetBoilerState()}" +
                        $"\nIl state {_boiler.GetInterLockState()}");

                    switch (option)
                    {
                        case MenuOptions.Start:
                            result = await _boilerService.Start();
                            break;
                        case MenuOptions.Stop:
                            result = _boilerService.Stop();
                            break;
                        case MenuOptions.ErrorSimulation:
                            _boilerService.ThrowError();
                            break;
                        case MenuOptions.Toggle:
                            result = _boilerService.ToggleInterLockState();
                            break;
                        case MenuOptions.Reset:
                            result = _boilerService.ResetBoiler();
                            break;
                        case MenuOptions.Exit:
                            isRunning = false;
                            Console.WriteLine("Exiting the application");
                            break;
                        default:
                            Console.WriteLine("Enter the correct choice");
                            break;
                    }

                    Console.WriteLine(result);
                    ConsolePresenter.DisplayMessage($"After your operation" +
                         $"\nBoiler State: {_boiler.GetBoilerState()}" +
                         $"\nIl state {_boiler.GetInterLockState()}");


                }

                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Machine failed Resetting to the normal state !!");
                    _boilerService.ResetBoiler();
                }

            }
        }

        private void DisplayMenu()
        {
            Console.Write("\n1.Start" +
                           "\n2.Stop" +
                           "\n3.SimulateError" +
                           "\n4.Toggle" +
                           "\n5.Reset" +
                           "\n6.View Log" +
                           "\n7.Exit");
        }
    }
}
