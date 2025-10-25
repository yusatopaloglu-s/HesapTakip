using QuestPDF.Infrastructure;
using System.Data.SQLite;

namespace HesapTakip
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Logger.Log("Uygulama baþlýyor...");

                QuestPDF.Settings.License = LicenseType.Community;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);

                Logger.Log("Temel ayarlar yapýldý...");

                // ESKÝ AYARLARI YENÝ KONUMA TAÞI
                AppConfigHelper.MigrateOldSettings();
                Logger.Log("Migration tamamlandý...");

                // Database baðlantý kontrolü -- hýzlý path: sadece konfigurasyon var mý kontrol et
                if (!HasValidDatabaseConfiguration())
                {
                    Logger.Log("Geçerli baðlantý yok, ConnectionSettingsForm açýlýyor...");
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Baðlantý ayarlarý girilmedi. Uygulama kapatýlýyor.");
                        return;
                    }
                }
                else
                {
                    Logger.Log("Connection configuration bulundu; startup connection testi atlandý (daha hýzlý açýlýþ). MainForm içinde baðlantý kontrolü yapýlacak.");
                }

                Logger.Log("MainForm açýlýyor...");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Logger.Log($"KRÝTÝK HATA: {ex.Message}");
                Logger.Log($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"Uygulama baþlatýlamadý: {ex.Message}",
                    "Baþlatma Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Baðlantý ayarlarý formunu göster 
        private static bool ShowConnectionSettingsForm()
        {
            using (var settingsForm = new ConnectionSettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    Logger.Log("Kullanýcý ayarlarý kaydetti...");

                    // Form zaten ayarlarý kaydetti (ConnectionSettingsForm.btnSave çaðýrýyor).
                    // Buradan kaydetme yapmýyoruz, sadece konfigürasyonu test ediyoruz.

                    Logger.Log($"Ayarlar kaydedildi - Type: '{settingsForm.DatabaseType}'");

                    // Hemen kaydedilen ayarlarý kontrol et - hafif test
                    string currentType = AppConfigHelper.DatabaseType;
                    string currentConn = AppConfigHelper.DatabasePath;
                    Logger.Log($"Hemen sonra - DatabaseType: '{currentType}'");
                    Logger.Log($"Hemen sonra - DatabasePath: '{currentConn}'");

                    // Kaydedilen ayarlarý test et (hafif - yalnýzca gerekli olduðunda)
                    if (TestDatabaseConnection())
                    {
                        Logger.Log("Baðlantý testi baþarýlý!");
                        return true;
                    }
                    else
                    {
                        Logger.Log("Baðlantý testi baþarýsýz!");
                        MessageBox.Show("Baðlantý testi baþarýsýz. Lütfen ayarlarý tekrar kontrol edin.",
                            "Baðlantý Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Tekrar deneme
                        return ShowConnectionSettingsForm();
                    }
                }
                else
                {
                    Logger.Log("Kullanýcý iptal etti...");
                    return false;
                }
            }
        }

        // Geçerli database konfigürasyonu kontrolü 
        private static bool HasValidDatabaseConfiguration()
        {
            string connectionString = AppConfigHelper.DatabasePath;
            string databaseType = AppConfigHelper.DatabaseType;

            Logger.Log($"Configuration kontrolü - Type: {databaseType}, Connection: {!string.IsNullOrEmpty(connectionString)}");

            // Hem connection string hem de database type olmalý
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
            {
                Logger.Log("Connection string veya database type boþ!");
                return false;
            }

            // SQLite için farklý kontrol
            if (databaseType == "SQLite")
            {
                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Logger.Log($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Logger.Log("SQLite Data Source boþ!");
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Logger.Log($"SQLite dizin oluþturuldu: {directory}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log($"SQLite dizin kontrol hatasý: {ex.Message}");
                    return false;
                }
            }

            // MySQL/MSSQL için temel kontroller
            bool hasValidComponents = connectionString.Contains("Server=") &&
                                   connectionString.Contains("Database=");

            Logger.Log($"MySQL/MSSQL bileþen kontrolü: {hasValidComponents}");
            return hasValidComponents;
        }


        private static bool TestDatabaseConnection()
        {
            try
            {
                string connectionString = AppConfigHelper.DatabasePath;
                string databaseType = AppConfigHelper.DatabaseType;

                Logger.Log($"=== TestDatabaseConnection ===");
                Logger.Log($"Database Type: '{databaseType}'");
                Logger.Log($"Connection String: '{connectionString}'");

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
                {
                    Logger.Log("Connection string veya database type boþ!");
                    return false;
                }

                string dbType = databaseType.Trim().ToUpperInvariant();
                Logger.Log($"Trimmed Database Type (Invariant): '{dbType}'");

                if (dbType.Equals("SQLITE", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Testing SQLite connection...");
                    return TestSQLiteConnection(connectionString);
                }
                else if (dbType.Equals("MYSQL", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Testing MySQL connection...");
                    return TestMySQLConnection(connectionString);
                }
                else if (dbType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Testing MSSQL connection...");
                    return TestMSSQLConnection(connectionString);
                }
                else
                {
                    Logger.Log($"Bilinmeyen database tipi: '{databaseType}' -> '{dbType}'");

                    // Manuel kontrol
                    if (databaseType.IndexOf("sqlite", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Logger.Log("Manuel olarak SQLite tespit edildi!");
                        return TestSQLiteConnection(connectionString);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"=== TestDatabaseConnection ERROR ===");
                Logger.Log($"Error: {ex.Message}");
                Logger.Log($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // SQLite baðlantý testi 
        private static bool TestSQLiteConnection(string connectionString)
        {
            try
            {
                Logger.Log("SQLite baðlantý testi baþlýyor...");

                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Logger.Log($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Logger.Log("SQLite Data Source boþ!");
                    return false;
                }

                // Dosya ve dizin kontrolü
                string directory = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Log($"SQLite dizin oluþturuldu: {directory}");
                }

                // Hafif baðlantý testi (sadece aç/kapa)
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    Logger.Log("SQLite baðlantýsý açýldý (test)");

                    using (var cmd = new SQLiteCommand("SELECT 1", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Logger.Log($"SQLite test sorgusu sonucu: {result}");
                    }

                    return true;
                }
            }
            catch (SQLiteException sqlEx)
            {
                Logger.Log($"SQLite baðlantý testi SQL hatasý: {sqlEx.Message}");
                Logger.Log($"SQLite Error Code: {sqlEx.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite baðlantý testi genel hatasý: {ex.Message}");
                Logger.Log($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // MySQL baðlantý testi - (sadece TestConnection çaðrýsý, initialize yok)
        private static bool TestMySQLConnection(string connectionString)
        {
            try
            {
                Logger.Log("MySQL baðlantý testi baþlýyor...");
                var db = new MySqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Logger.Log("MySQL baðlantý testi baþarýlý");
                }
                else
                {
                    Logger.Log("MySQL baðlantý testi baþarýsýz");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"MySQL baðlantý testi hatasý: {ex.Message}");
                return false;
            }
        }

        // MSSQL baðlantý testi - (sadece TestConnection çaðrýsý, initialize yok)
        private static bool TestMSSQLConnection(string connectionString)
        {
            try
            {
                Logger.Log("MSSQL baðlantý testi baþlýyor...");
                var db = new MsSqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Logger.Log("MSSQL baðlantý testi baþarýlý");
                }
                else
                {
                    Logger.Log("MSSQL baðlantý testi baþarýsýz");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"MSSQL baðlantý testi hatasý: {ex.Message}");
                return false;
            }
        }
    }
}