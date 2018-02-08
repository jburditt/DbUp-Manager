using DbUp;
using DbUp.Helpers;
using System;

namespace DbUpManager
{
    public static class DbManager
    {
        public enum DbAction
        {
            Upgrade,
            Seed,
            CreateDatabase,
            CreateSchema,
            DropSchema,
            DropDatabase
        }

        public static void Setup(string connectionString)
        {
            ConsoleKey key = ConsoleKey.Enter;

            while (key != ConsoleKey.Escape)
            {
                Console.WriteLine($"Server: {MySqlHelper.Builder(connectionString).Server}\n");
                Console.WriteLine("What action would you like to perform?");
                Console.WriteLine("1. Create Database");
                Console.WriteLine("2. Create Schema");
                Console.WriteLine("3. Update Database");
                Console.WriteLine("4. Seed Data");
                Console.WriteLine("5. Drop Schema");
                Console.WriteLine("6. Drop Database");
                Console.WriteLine("7. Delete Data\n");
                Console.WriteLine("ESC. EXIT PROGRAM\n");

                key = Console.ReadKey().Key;

                Console.WriteLine("\n");

                // exit program
                if (key == ConsoleKey.Escape)
                    return;

                switch (key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("Database Name: ");
                        connectionString = CreateDatabase(connectionString, Console.ReadLine());
                        break;
                    case ConsoleKey.D2:
                        CreateSchema(connectionString);
                        break;
                    case ConsoleKey.D3:
                        Update(connectionString);
                        break;
                    case ConsoleKey.D4:
                        Seed(connectionString);
                        break;
                    case ConsoleKey.D5:
                        DropSchema(connectionString);
                        break;
                    case ConsoleKey.D6:
                        Console.WriteLine("Database Name: ");
                        DropDatabase(connectionString, Console.ReadLine());
                        break;
                    case ConsoleKey.D7:
                        DeleteData(connectionString);
                        break;
                }

                Console.WriteLine("Press the any key to continue.");
                Console.ReadKey();
                Console.Clear();
            }
        }

        public static void RunAction(string connectionString, DbAction action, string databaseName)
        {
            switch (action)
            {
                case DbAction.Seed:
                    DbManager.Seed(connectionString);
                    break;
                case DbAction.DropSchema:
                    DbManager.DropSchema(connectionString);
                    break;
                case DbAction.CreateDatabase:
                    DbManager.CreateDatabase(connectionString, databaseName);
                    break;
                case DbAction.CreateSchema:
                    DbManager.CreateSchema(connectionString);
                    break;
                case DbAction.DropDatabase:
                    DbManager.DropDatabase(connectionString, databaseName);
                    break;
                case DbAction.Upgrade:
                default:
                    DbManager.Update(connectionString);
                    break;
            }
        }

        public static string CreateDatabase(string connectionString, string databaseName, bool updateDatabase = true)
        {
            // use default connection string to create new database

            DisplayStatusMessage("Creating Database");

            MySqlHelper.ExecuteScript(connectionString, "CreateDatabase\\CreateDatabase.sql", new Tuple<string, string>("DatabaseName", databaseName));

            // run upgrade scripts on new database

            var connectionStringBuilder = MySqlHelper.Builder(connectionString);
            connectionStringBuilder.Database = databaseName;

            CreateSchema(connectionStringBuilder.ConnectionString);
            
            if (updateDatabase)
                Update(connectionStringBuilder.ConnectionString);

            return connectionStringBuilder.ConnectionString;
        }

        public static void DropDatabase(string connectionString, string databaseName)
        {
            MySqlHelper.ExecuteScript(connectionString, "DropDatabase\\DropDatabase.sql", new Tuple<string, string>("DatabaseName", databaseName));
        }

        public static void CreateSchema(string connectionString)
        {
            CompileScripts(connectionString, "Creating Schema", "CreateSchema\\");
        }

        public static void DropSchema(string connectionString)
        {
            CompileScripts(connectionString, "Dropping Schema", "DropSchema\\");
        }

        public static void DeleteData(string connectionString)
        {
            CompileScripts(connectionString, "Delete Data", "DeleteData\\");
        }

        public static void Seed(string connectionString)
        {
            CompileScripts(connectionString, "Seed Data", "Seed\\");
        }

        private static void Pause(int duration)
        {
            Console.Write("Pausing to allow new database to stabilize");

            for (int i = 0; i < duration; i++)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine();
        }

        public static void Update(string connectionString)
        {
            UpgradeDatabase(connectionString, "Compiling Scripts", "Update\\");
            CompileFunctions(connectionString);
            CompileSprocs(connectionString);
        }

        private static void UpgradeDatabase(string connectionString, string action, string folderPath)
        {
            DisplayStatusMessage(action);

            var upgrader = DeployChanges.To.MySqlDatabase(connectionString)
                    .WithScriptsFromFileSystem($"{ApplicationSettings.SolutionDirectory}\\Sql\\Scripts\\{folderPath}")
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            DisplayDatabaseUpgradeResult(result, action);
        }

        private static void CompileScripts(string connectionString, string action, string folderPath)
        {
            DisplayStatusMessage(action);

            var upgrader = DeployChanges.To.MySqlDatabase(connectionString)
                    .WithScriptsFromFileSystem($"{ApplicationSettings.SolutionDirectory}\\Sql\\Scripts\\{folderPath}")
                    .JournalTo(new NullJournal())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            DisplayDatabaseUpgradeResult(result, action);
        }

        private static void DisplayStatusMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void DisplayDatabaseUpgradeResult(DbUp.Engine.DatabaseUpgradeResult result, string stage)
        {
            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(stage + " COMPLETED");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
