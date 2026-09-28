using BoilerApplication.Models;
using BoilerApplication.Repository;
using BoilerApplication.Services;
using BoilerApplication.View;
namespace BoilerApplication
{
    /// <summary>
    /// Represents the entry point of the application,
    /// </summary>
    public class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("=============== Boiler Control Initialized ===============");

                ILogger logger = new Logger("BoilerLog.csv");
                Boiler boiler = new Boiler();

                Service service = new Service(boiler, logger);

                await logger.LoadFromFile();
                service.ResetBoiler();
                await logger.AppendLog(DateTime.Now, "Status change", "Reseted to LockOut State");
                await logger.AppendLog(DateTime.Now, "Inter Lock Status Change", "Reseted to Open state");


                BiolerView view = new BiolerView(service, boiler, logger);
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
