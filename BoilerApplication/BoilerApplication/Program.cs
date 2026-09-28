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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("=============== Boiler Control Initialized ===============");

                ILogger logger = new Logger("BoilerLog.csv");
                Boiler boiler = new Boiler();

                BoilerService service = new BoilerService(boiler, logger);

                await logger.LoadFromFileAsync();
                service.ResetBoiler();
                await logger.AppendLogAsync(DateTime.Now, "Status change", "Reseted to LockOut State");
                await logger.AppendLogAsync(DateTime.Now, "Inter Lock Status Change", "Reseted to Open state");


                BoilerView view = new BoilerView(service, boiler, logger);
                await view.RunApplicationAsync();
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
