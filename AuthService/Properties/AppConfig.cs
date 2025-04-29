using DotNetEnv;
using System.Globalization;

namespace AuthService.Properties
{
    internal static class AppConfig
    {
        static AppConfig()
        {
            Env.Load();
        }
        public static string JwtIssuer => GetEnv("JWT_ISSUER");
        public static string JwtAudience => GetEnv("JWT_AUDIENCE");
        public static string JwtSecretKey => GetEnv("JWT_SECRET_KEY");
        public static string DistributedCacheConnectionString => GetEnv("DISTRIBUTED_CACHE_CONNECTION_STRING");

        public static string GoogleClientId => GetEnv("GOOGLE_CLIENT_ID");
        public static string GoogleClientSecret => GetEnv("GOOGLE_CLIENT_SECRET");
        public static Uri GoogleRedirectUri => new(GetEnv("GOOGLE_REDIRECT_URI"));


        public static int PepperLength => GetIntEnv("PEPPER_LENGTH");
        public static string PepperLetters => GetEnv("PEPPER_LETTERS");

        public static string DbConnectionString => GetEnv("DB_CONNECTION_STRING");
        public static int RefreshTokenLifetimeDays => GetIntEnv("REFRESH_TOKEN_LIFETIME_DAYS");
        public static int AccessTokenLifetimeMinutes => GetIntEnv("JWT_ACCESS_TOKEN_LIFETIME_MINUTES");

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