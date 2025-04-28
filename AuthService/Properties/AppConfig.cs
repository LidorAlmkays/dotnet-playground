using System.Globalization;

namespace AuthService.Properties
{
    public class AppConfig
    {
        public string JwtIssuer { get; }
        public string JwtAudience { get; }
        public string JwtSecretKey { get; }
        public string DistributedCacheConnectionString { get; }

        public string GoogleClientId { get; }
        public string GoogleClientSecret { get; }

        public int PepperLength { get; }
        public string PepperLetters { get; }

        public string DbConnectionString { get; }
        public int AccessTokenLifetimeMinutes { get; }
        public int RefreshTokenLifetimeDays { get; }
        public AppConfig()
        {
            JwtIssuer = GetEnv("JWT_ISSUER");
            JwtAudience = GetEnv("JWT_AUDIENCE");
            JwtSecretKey = GetEnv("JWT_SECRET_KEY");

            GoogleClientId = GetEnv("GOOGLE_CLIENT_ID");
            GoogleClientSecret = GetEnv("GOOGLE_CLIENT_SECRET");
            PepperLetters = GetEnv("PEPPER_LETTERS");

            DistributedCacheConnectionString = GetEnv("DISTRIBUTED_CACHE_CONNECTION_STRING");


            var dbType = GetEnv("DB_TYPE");
            var dbName = GetEnv("DB_NAME");
            var dbPort = GetIntEnv("DB_PORT");
            var dbUsername = GetEnv("DB_USERNAME");
            var dbPassword = GetEnv("DB_PASSWORD");

            DbConnectionString = $"Server={dbType}; Port={dbPort}; User Id={dbUsername}; Password={dbPassword}; Database={dbName}";

            PepperLength = GetIntEnv("PEPPER_LENGTH");
            if (PepperLength <= 0)
                throw new ArgumentOutOfRangeException("PEPPER_LENGTH must be greater than 0.");
            AccessTokenLifetimeMinutes = GetIntEnv("JWT_ACCESS_TOKEN_LIFETIME_MINUTES");
            if (AccessTokenLifetimeMinutes <= 0)
                throw new ArgumentOutOfRangeException("JWT_ACCESS_TOKEN_LIFETIME_MINUTES must be greater than 0.");
            RefreshTokenLifetimeDays = GetIntEnv("REFRESH_TOKEN_LIFETIME_DAYS");
            if (AccessTokenLifetimeMinutes <= 0)
                throw new ArgumentOutOfRangeException("RefreshTokenLifetimeDays must be greater than 0.");

        }

        private static string GetEnv(string key) =>
            Environment.GetEnvironmentVariable(key)
            ?? throw new InvalidOperationException($"{key} environment variable is missing.");

        private static int GetIntEnv(string key)
        {
            var value = GetEnv(key);
            return int.TryParse(value, out var result)
                ? result
                : throw new FormatException($"{key} must be an integer.");
        }
    }
}