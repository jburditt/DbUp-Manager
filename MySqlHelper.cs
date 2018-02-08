using MySql.Data.MySqlClient;
using System;
using System.IO;

namespace DbUpManager
{
    public static class MySqlHelper
    {
        public static string ConnectionString(string server, uint port, string userId, string password, string database)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder();
            connectionStringBuilder.Server = server;
            connectionStringBuilder.Port = port;
            connectionStringBuilder.UserID = userId;
            connectionStringBuilder.Password = password;
            connectionStringBuilder.Database = database;

            return connectionStringBuilder.ConnectionString;
        }

        public static MySqlConnectionStringBuilder Builder(string connectionString)
        {
            return new MySqlConnectionStringBuilder(connectionString);
        }

        public static int ExecuteScript(string connectionString, string scriptFilePath, params Tuple<string, string>[] variableReplacements)
        {
            var mySqlConnection = new MySqlConnection(connectionString);
            var filePath = $"{ ApplicationSettings.SolutionDirectory }\\Sql\\Scripts\\{ scriptFilePath }";

            var query = File.ReadAllText(filePath);
            foreach (var variable in variableReplacements)
                query = query.Replace($"${variable.Item1}$", $"{variable.Item2}");
            
            var script = new MySqlScript(mySqlConnection, query);

            return script.Execute();
        }
    }
}
