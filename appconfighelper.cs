using System.Diagnostics;
using System.Xml;

namespace HesapTakip
{
    public static class AppConfigHelper
    {
        // Sabit klasör yolu - her versiyonda aynı kalacak
        private static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HesapTakip");

        public static string ConfigFilePath => Path.Combine(AppDataFolder, "HesapTakip.config");

        static AppConfigHelper()
        {
            // Klasörü oluştur (eğer yoksa)
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        public static string DatabasePath
        {
            get
            {
                try
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(ConfigFilePath);
                        var node = doc.SelectSingleNode("//DatabasePath");
                        return node?.InnerText ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Config okuma hatası: {ex.Message}");
                }

                return string.Empty;
            }
            set
            {
                try
                {
                    XmlDocument doc = new XmlDocument();

                    if (File.Exists(ConfigFilePath))
                    {
                        doc.Load(ConfigFilePath);
                    }
                    else
                    {
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                        doc.AppendChild(doc.CreateElement("Configuration"));
                    }

                    var node = doc.SelectSingleNode("//DatabasePath");
                    if (node == null)
                    {
                        node = doc.CreateElement("DatabasePath");
                        doc.DocumentElement.AppendChild(node);
                    }

                    node.InnerText = value;
                    doc.Save(ConfigFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Config kaydetme hatası: {ex.Message}");
                }
            }
        }

        public static string DatabaseType
        {
            get
            {
                try
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(ConfigFilePath);
                        var node = doc.SelectSingleNode("//DatabaseType");
                        string dbType = node?.InnerText?.Trim() ?? "SQLite";

                        // GARANTİLİ GET: Her zaman doğru format
                        string guaranteedType = GuaranteeDatabaseType(dbType);
                        Debug.WriteLine($"AppConfigHelper DatabaseType GET: '{dbType}' -> '{guaranteedType}'");
                        return guaranteedType;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DatabaseType okuma hatası: {ex.Message}");
                }

                Debug.WriteLine("AppConfigHelper DatabaseType: Defaulting to SQLite");
                return "SQLite";
            }
            set
            {
                try
                {
                    string cleanValue = value?.Trim() ?? "SQLite";

                    // GARANTİLİ SET: Her zaman doğru format
                    string guaranteedType = GuaranteeDatabaseType(cleanValue);
                    Debug.WriteLine($"AppConfigHelper setting DatabaseType: '{cleanValue}' -> '{guaranteedType}'");

                    XmlDocument doc = new XmlDocument();

                    if (File.Exists(ConfigFilePath))
                    {
                        doc.Load(ConfigFilePath);
                    }
                    else
                    {
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                        doc.AppendChild(doc.CreateElement("Configuration"));
                    }

                    var node = doc.SelectSingleNode("//DatabaseType");
                    if (node == null)
                    {
                        node = doc.CreateElement("DatabaseType");
                        doc.DocumentElement.AppendChild(node);
                    }

                    node.InnerText = guaranteedType;
                    doc.Save(ConfigFilePath);

                    Debug.WriteLine($"DatabaseType successfully set to: '{guaranteedType}'");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DatabaseType kaydetme hatası: {ex.Message}");
                }
            }
        }

        // YENİ METOD: Database tipini garantiye al
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
                return dbType; // Olduğu gibi dön
        }
        private static void MigrateFromUserConfig(string userConfigPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(userConfigPath);

                // Eski ayarları oku
                var settingNodes = doc.SelectNodes("//setting[@name='DatabasePath']");
                if (settingNodes != null && settingNodes.Count > 0)
                {
                    string oldValue = settingNodes[0].InnerText;
                    if (!string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(DatabasePath))
                    {
                        // Yeni konuma kaydet
                        DatabasePath = oldValue;

                        // DatabaseType'i varsayılan olarak MySQL ayarla
                        DatabaseType = "MySQL";

                        Debug.WriteLine("Eski ayarlar yeni konuma taşındı.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"User config migration hatası: {ex.Message}");
            }
        }


        public static void SaveConnectionString(string server, string database, string user, string password, string port, string databaseType, string sqliteFilePath = "")
        {
            DatabaseType = databaseType;

            string connectionString;
            if (databaseType == "SQLite")
            {
                // SQLite için dosya yolunu kontrol et ve normalize et
                if (string.IsNullOrEmpty(sqliteFilePath))
                {
                    // Varsayılan dosya yolu
                    sqliteFilePath = Path.Combine(AppDataFolder, $"{database}.sqlite");
                }

                // Dosya yolunu tam path yap
                sqliteFilePath = Path.GetFullPath(sqliteFilePath);

                connectionString = $"Data Source={sqliteFilePath};";

                Debug.WriteLine($"SQLite Connection String: {connectionString}");
            }
            else if (databaseType == "MySQL")
            {
                connectionString = $"Server={server};Database={database};User={user};Password={password};Port={port};Charset=utf8mb4;";
            }
            else // MSSQL
            {
                connectionString = $"Server={server},{port};Database={database};User={user};Password={password};";
            }

            DatabasePath = connectionString;
        }

        // SQLite için Data Source değerini alma
        public static string GetDataSourceFromConnectionString()
        {
            try
            {
                if (string.IsNullOrEmpty(DatabasePath))
                {
                    Debug.WriteLine("GetDataSourceFromConnectionString: DatabasePath boş!");
                    return string.Empty;
                }

                Debug.WriteLine($"GetDataSourceFromConnectionString parsing: {DatabasePath}");

                var pairs = DatabasePath.Split(';');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2 &&
                        keyValue[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                    {
                        string dataSource = keyValue[1].Trim();
                        Debug.WriteLine($"GetDataSourceFromConnectionString found: {dataSource}");
                        return dataSource;
                    }
                }

                Debug.WriteLine("GetDataSourceFromConnectionString: Data Source bulunamadı!");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetDataSourceFromConnectionString hatası: {ex.Message}");
                return string.Empty;
            }
        }

        // Connection string'den bileşenleri çıkarma metodları
        public static string GetServerFromConnectionString()
        {
            return GetValueFromConnectionString("Server");
        }

        public static string GetDatabaseFromConnectionString()
        {
            return GetValueFromConnectionString("Database");
        }

        public static string GetUserFromConnectionString()
        {
            return GetValueFromConnectionString("User");
        }

        public static string GetPortFromConnectionString()
        {
            return GetValueFromConnectionString("Port");
        }

        private static string GetValueFromConnectionString(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(DatabasePath)) return string.Empty;

                var pairs = DatabasePath.Split(';');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return keyValue[1].Trim();
                    }
                }
            }
            catch
            {
                // Hata durumunda boş string dön
            }
            return string.Empty;
        }

        public static bool HasValidConnectionString()
        {
            if (string.IsNullOrEmpty(DatabasePath))
                return false;

            // SQLite için farklı kontrol
            if (DatabaseType == "SQLite")
            {
                return DatabasePath.Contains("Data Source=");
            }

            // MySQL/MSSQL için kontrol
            return DatabasePath.Contains("Server=");
        }

        // Eski ayarları yeni konuma taşıma (migration)
        public static void MigrateOldSettings()
        {
            try
            {
                // Eski ClickOnce settings yolunu bul
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string[] oldPaths = Directory.GetDirectories(localAppData, "HesapTakip_Url_*", SearchOption.TopDirectoryOnly);

                foreach (string oldPath in oldPaths)
                {
                    string[] versionDirs = Directory.GetDirectories(oldPath);
                    foreach (string versionDir in versionDirs)
                    {
                        string userConfigPath = Path.Combine(versionDir, "user.config");
                        if (File.Exists(userConfigPath))
                        {
                            // Eski user.config'ten ayarları oku ve yeni konuma taşı
                            MigrateFromUserConfig(userConfigPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Migration hatası: {ex.Message}");
            }
        }
    }
}