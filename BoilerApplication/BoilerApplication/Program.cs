using BoilerApplication.Models;
using BoilerApplication.Repository;

namespace BoilerApplication
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, World!");

                ILogger logger = new Logger("BoilerLog.csv");
                Boiler boiler = new Boiler();

                Service service = new Service(boiler, logger);

                await logger.LoadFromFile();
                service.ResetBoiler();
                await logger.AppendLog(DateTime.Now, "Status change", "Reseted to LockOut State");
                await logger.AppendLog(DateTime.Now, "Inter Lock Status Change", "Reseted to Open state");


                View view = new View(service, boiler);
                await view.RunApplication();
            }

            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unathorized access to the file occured closing the file " + e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong " + e);
            }
        }
    }
}
