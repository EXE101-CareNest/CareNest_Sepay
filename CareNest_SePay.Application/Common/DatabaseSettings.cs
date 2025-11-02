namespace CareNest_SePay.Application.Common
{
    public class DatabaseSettings
    {
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;

        /// <summary>
        /// Build PostgreSQL connection string from settings
        /// </summary>
        public string BuildConnectionString()
        {
            return $"Host={Ip};Port={Port};Username={User};Password={Password};Database={Database};SSL Mode=Require;";
        }
    }
}

