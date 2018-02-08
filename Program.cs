using Fclp;
using Microsoft.Extensions.Configuration;
using System;
using static DbUpManager;

namespace DbUpManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ParseArguments(args);
            var connectionString = arguments.ConnectionString ?? ApplicationSettings.Configuration.GetConnectionString("PromoCode");

            // setup mode, ask for input from user to configure program
            if (arguments.Setup)
                DbManager.Setup(connectionString);
            // run scripts
            else
                DbManager.RunAction(connectionString, arguments.Action, arguments.DatabaseName);

#if DEBUG
            Console.WriteLine("Press the any key to continue.");
            Console.ReadLine();
#endif
        }

        public class ApplicationArguments
        {
            public bool Setup { get; set; }
            public DbAction Action { get; set; }
            public string DatabaseName { get; set; }
            public string ConnectionString { get; set; }
        }

        static ApplicationArguments ParseArguments(string[] args)
        {
            // create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Setup)
                .As('s', "setup");

            p.Setup(arg => arg.Action)
                .As('a', "action");

            p.Setup(arg => arg.DatabaseName)
                .As('d', "dbname");

            p.Setup(arg => arg.ConnectionString)
                .As('c', "conn");
                
            var result = p.Parse(args);

            if (result.HasErrors)
                Console.WriteLine($"Error parsing arguments: {result.ErrorText}");

            return p.Object;
        }
    }
}
