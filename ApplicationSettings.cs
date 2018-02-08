using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DbUpManager
{
    public class ApplicationSettings
    {
        public static IConfigurationRoot Configuration { get; private set; }
        public static string ConnectionString { get; private set; }

        public static string SolutionDirectory =
#if DEBUG
            Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppContext.BaseDirectory.LastIndexOf("bin"))).ToString()).ToString();
#else
            AppDomain.CurrentDomain.BaseDirectory;
#endif

        static ApplicationSettings()
        {
            Build();
        }

        public static void Build()
        {
            var filePath = $"{SolutionDirectory}\\Services\\";
            var environmentName = "Development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(filePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

            Configuration = builder.Build();

            ConnectionString = Configuration.GetConnectionString("PromoCode") ?? Environment.GetEnvironmentVariable("PromoCode");
        }
    }
}
