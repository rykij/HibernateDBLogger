using System.Configuration;

namespace HibernateDBLogger.Utilities
{
    public static class ConfigurationReader
    {
        //TODO: Change default connection string name (see also hibernate.cfg.xml)
        const string  DEFAULTCONNECTION = "defaultConnection";
        public static string GetConnectionString()
        {
            return GetConnectionString(DEFAULTCONNECTION);
        }

        public static string GetConnectionString(string Token)
        {
            string token = "connection string=\"";
            string tokenEnd = "\"";

            string entityFrameworkConnectionString = ConfigurationManager.ConnectionStrings[Token].ConnectionString;
            string connectionString = entityFrameworkConnectionString.Substring(entityFrameworkConnectionString.LastIndexOf(token) + token.Length);

            connectionString = connectionString.Substring(0, connectionString.LastIndexOf(tokenEnd));

            return connectionString;
        }

        public static string GetEntityConnectionString(string ConnectionToken)
        {
            string entityFrameworkConnectionString = ConfigurationManager.ConnectionStrings[ConnectionToken].ConnectionString;

            return entityFrameworkConnectionString;
        }

        public static string GetValue(string Key){
            return ConfigurationManager.AppSettings[Key].ToString();
        }
    }
}
