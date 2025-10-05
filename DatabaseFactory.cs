using System.Diagnostics;

namespace HesapTakip
{
    public static class DatabaseFactory
    {
        public static IDatabaseOperations Create(string databaseType, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string boş olamaz");

            Debug.WriteLine($"DatabaseFactory creating: {databaseType}");

            // GARANTİLİ TİP BELİRLEME
            string guaranteedType = GuaranteeDatabaseType(databaseType);
            Debug.WriteLine($"Guaranteed database type: {guaranteedType}");

            switch (guaranteedType.ToUpperInvariant())
            {
                case "SQLITE":
                    Debug.WriteLine("Creating SqliteOperations instance...");
                    return new SqliteOperations(connectionString);
                case "MYSQL":
                    Debug.WriteLine("Creating MySqlOperations instance...");
                    return new MySqlOperations(connectionString);
                case "MSSQL":
                    Debug.WriteLine("Creating MsSqlOperations instance...");
                    return new MsSqlOperations(connectionString);
                default:
                    throw new NotSupportedException($"Desteklenmeyen database tipi: {databaseType} -> {guaranteedType}");
            }
        }

        private static string GuaranteeDatabaseType(string dbType)
        {
            if (string.IsNullOrEmpty(dbType))
                return "SQLite";

            string lower = dbType.ToLowerInvariant();

            if (lower.Contains("sqlite"))
                return "SQLite";
            else if (lower.Contains("mysql"))
                return "MySQL";
            else if (lower.Contains("mssql") || lower.Contains("sqlserver"))
                return "MSSQL";
            else
                return dbType;
        }
    }
}