using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Api
{
    public static class Configuration
    {
        public static void SetupConfig(WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            var configuration = builder.Configuration;
            builder.Services.AddSingleton<IConfiguration>(configuration);
            Env.Load();
        }

        public static string GetServiceName()
        {
            string serviceName = GetStringEnvironmentVariable("SERVICE_NAME");
            return serviceName;
        }

        public static (string issuer, string audience, string secretKey) GetJWTConfig()
        {
            string issuer = GetStringEnvironmentVariable("JWT_ISSUER");
            string audience = GetStringEnvironmentVariable("JWT_AUDIENCE");
            string secretKey = GetStringEnvironmentVariable("JWT_SECRET_KEY");
            return (issuer, audience, secretKey);
        }

        public static (string ClientId, string ClientSecret) GetGoogleOAUTHConfig()
        {
            var clientId = GetStringEnvironmentVariable("GOOGLE_CLIENT_ID");
            var clientSecret = GetStringEnvironmentVariable("GOOGLE_CLIENT_SECRET");
            return (clientId, clientSecret);
        }

        public static (int pepperLength, string pepperLetters) GetSaltAndPepperConfig()
        {
            int pepperLength = GetIntEnvironmentVariable("PEPPER_LENGTH");
            if (pepperLength <= 0)
            {
                throw new ArgumentOutOfRangeException(null, "The pepper length number cannot be less than 0.");
            }
            string pepperLetters = GetStringEnvironmentVariable("PEPPER_LETTERS");
            return (pepperLength, pepperLetters);
        }
        public static string GetDBConnectionString()
        {
            string dbType = GetStringEnvironmentVariable("DB_TYPE");
            string dbName = GetStringEnvironmentVariable("DB_NAME");
            int dbPort = GetIntEnvironmentVariable("DB_PORT");
            string dbUsername = GetStringEnvironmentVariable("DB_USERNAME");
            string dbPassword = GetStringEnvironmentVariable("DB_PASSWORD");

            string connectionString = $"Server={dbType}; Port={dbPort}; User Id={dbUsername}; Password={dbPassword}; Database={dbName}";
            return connectionString;
        }
        private static string GetStringEnvironmentVariable(string varName)
        {
            string variable = Environment.GetEnvironmentVariable(varName)
                    ?? throw new InvalidOperationException(varName + " environment variable is missing.");
            return variable;
        }
        private static int GetIntEnvironmentVariable(string varName)
        {
            int variable = int.Parse(
                     Environment.GetEnvironmentVariable(varName)
                    ?? throw new InvalidOperationException(varName + " environment variable is missing."),
                    CultureInfo.InvariantCulture);
            return variable;
        }
    }
}