using BoilerApplication.Models;
using BoilerApplication.Repository;

namespace BoilerApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            ILogger logger = new Logger("BoilerLog.txt");
            Boiler boiler = new Boiler();

            Service service = new Service(boiler, logger);

            View view = new View(service, boiler);
            view.RunApplication();
        }
    }
}
