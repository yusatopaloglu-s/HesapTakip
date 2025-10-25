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
                Logger.Log("Uygulama ba�l�yor...");

                QuestPDF.Settings.License = LicenseType.Community;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);

                Logger.Log("Temel ayarlar yap�ld�...");

                // ESK� AYARLARI YEN� KONUMA TA�I
                AppConfigHelper.MigrateOldSettings();
                Logger.Log("Migration tamamland�...");

                // Database ba�lant� kontrol� -- h�zl� path: sadece konfigurasyon var m� kontrol et
                if (!HasValidDatabaseConfiguration())
                {
                    Logger.Log("Ge�erli ba�lant� yok, ConnectionSettingsForm a��l�yor...");
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Ba�lant� ayarlar� girilmedi. Uygulama kapat�l�yor.");
                        return;
                    }
                }
                else
                {
                    Logger.Log("Connection configuration bulundu; startup connection testi atland� (daha h�zl� a��l��). MainForm i�inde ba�lant� kontrol� yap�lacak.");
                }

                Logger.Log("MainForm a��l�yor...");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Logger.Log($"KR�T�K HATA: {ex.Message}");
                Logger.Log($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"Uygulama ba�lat�lamad�: {ex.Message}",
                    "Ba�latma Hatas�", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ba�lant� ayarlar� formunu g�ster 
        private static bool ShowConnectionSettingsForm()
        {
            using (var settingsForm = new ConnectionSettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    Logger.Log("Kullan�c� ayarlar� kaydetti...");

                    // Form zaten ayarlar� kaydetti (ConnectionSettingsForm.btnSave �a��r�yor).
                    // Buradan kaydetme yapm�yoruz, sadece konfig�rasyonu test ediyoruz.

                    Logger.Log($"Ayarlar kaydedildi - Type: '{settingsForm.DatabaseType}'");

                    // Hemen kaydedilen ayarlar� kontrol et - hafif test
                    string currentType = AppConfigHelper.DatabaseType;
                    string currentConn = AppConfigHelper.DatabasePath;
                    Logger.Log($"Hemen sonra - DatabaseType: '{currentType}'");
                    Logger.Log($"Hemen sonra - DatabasePath: '{currentConn}'");

                    // Kaydedilen ayarlar� test et (hafif - yaln�zca gerekli oldu�unda)
                    if (TestDatabaseConnection())
                    {
                        Logger.Log("Ba�lant� testi ba�ar�l�!");
                        return true;
                    }
                    else
                    {
                        Logger.Log("Ba�lant� testi ba�ar�s�z!");
                        MessageBox.Show("Ba�lant� testi ba�ar�s�z. L�tfen ayarlar� tekrar kontrol edin.",
                            "Ba�lant� Hatas�", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Tekrar deneme
                        return ShowConnectionSettingsForm();
                    }
                }
                else
                {
                    Logger.Log("Kullan�c� iptal etti...");
                    return false;
                }
            }
        }

        // Ge�erli database konfig�rasyonu kontrol� 
        private static bool HasValidDatabaseConfiguration()
        {
            string connectionString = AppConfigHelper.DatabasePath;
            string databaseType = AppConfigHelper.DatabaseType;

            Logger.Log($"Configuration kontrol� - Type: {databaseType}, Connection: {!string.IsNullOrEmpty(connectionString)}");

            // Hem connection string hem de database type olmal�
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
            {
                Logger.Log("Connection string veya database type bo�!");
                return false;
            }

            // SQLite i�in farkl� kontrol
            if (databaseType == "SQLite")
            {
                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Logger.Log($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Logger.Log("SQLite Data Source bo�!");
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Logger.Log($"SQLite dizin olu�turuldu: {directory}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log($"SQLite dizin kontrol hatas�: {ex.Message}");
                    return false;
                }
            }

            // MySQL/MSSQL i�in temel kontroller
            bool hasValidComponents = connectionString.Contains("Server=") &&
                                   connectionString.Contains("Database=");

            Logger.Log($"MySQL/MSSQL bile�en kontrol�: {hasValidComponents}");
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
                    Logger.Log("Connection string veya database type bo�!");
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

        // SQLite ba�lant� testi 
        private static bool TestSQLiteConnection(string connectionString)
        {
            try
            {
                Logger.Log("SQLite ba�lant� testi ba�l�yor...");

                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Logger.Log($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Logger.Log("SQLite Data Source bo�!");
                    return false;
                }

                // Dosya ve dizin kontrol�
                string directory = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Log($"SQLite dizin olu�turuldu: {directory}");
                }

                // Hafif ba�lant� testi (sadece a�/kapa)
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    Logger.Log("SQLite ba�lant�s� a��ld� (test)");

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
                Logger.Log($"SQLite ba�lant� testi SQL hatas�: {sqlEx.Message}");
                Logger.Log($"SQLite Error Code: {sqlEx.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite ba�lant� testi genel hatas�: {ex.Message}");
                Logger.Log($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // MySQL ba�lant� testi - (sadece TestConnection �a�r�s�, initialize yok)
        private static bool TestMySQLConnection(string connectionString)
        {
            try
            {
                Logger.Log("MySQL ba�lant� testi ba�l�yor...");
                var db = new MySqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Logger.Log("MySQL ba�lant� testi ba�ar�l�");
                }
                else
                {
                    Logger.Log("MySQL ba�lant� testi ba�ar�s�z");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"MySQL ba�lant� testi hatas�: {ex.Message}");
                return false;
            }
        }

        // MSSQL ba�lant� testi - (sadece TestConnection �a�r�s�, initialize yok)
        private static bool TestMSSQLConnection(string connectionString)
        {
            try
            {
                Logger.Log("MSSQL ba�lant� testi ba�l�yor...");
                var db = new MsSqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Logger.Log("MSSQL ba�lant� testi ba�ar�l�");
                }
                else
                {
                    Logger.Log("MSSQL ba�lant� testi ba�ar�s�z");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"MSSQL ba�lant� testi hatas�: {ex.Message}");
                return false;
            }
        }
    }
}