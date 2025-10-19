using QuestPDF.Infrastructure;
using System.Data.SQLite;
using System.Diagnostics;

namespace HesapTakip
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Debug.WriteLine("Uygulama baþlýyor...");

                QuestPDF.Settings.License = LicenseType.Community;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);

                Debug.WriteLine("Temel ayarlar yapýldý...");

                // ESKÝ AYARLARI YENÝ KONUMA TAÞI
                AppConfigHelper.MigrateOldSettings();
                Debug.WriteLine("Migration tamamlandý...");

                // Database baðlantý kontrolü
                if (!HasValidDatabaseConfiguration())
                {
                    Debug.WriteLine("Geçerli baðlantý yok, ConnectionSettingsForm açýlýyor...");
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Baðlantý ayarlarý girilmedi. Uygulama kapatýlýyor.");
                        return;
                    }
                }
                else if (!TestDatabaseConnection())
                {
                    Debug.WriteLine("Baðlantý testi baþarýsýz, ConnectionSettingsForm açýlýyor...");
                    MessageBox.Show("Veritabaný baðlantýsý baþarýsýz. Lütfen ayarlarý kontrol edin.",
                        "Baðlantý Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Baðlantý ayarlarý girilmedi. Uygulama kapatýlýyor.");
                        return;
                    }
                }

                Debug.WriteLine("Baðlantý baþarýlý, MainForm açýlýyor...");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KRÝTÝK HATA: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

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
                    Debug.WriteLine("Kullanýcý ayarlarý kaydetti...");

                    // SQLite için dosya yolunu da gönder
                    // --- DÜZELTME BAÞLANGIÇ ---
                    bool useWindowsAuth = false;
                    if (settingsForm.DatabaseType == "MSSQL")
                        useWindowsAuth = AppConfigHelper.IsWindowsAuthEnabled;

                    AppConfigHelper.SaveConnectionString(
                        settingsForm.Server,
                        settingsForm.Database,
                        settingsForm.User,
                        settingsForm.Password,
                        settingsForm.Port,
                        settingsForm.DatabaseType,
                        settingsForm.SqliteFilePath,
                        useWindowsAuth // MSSQL için Windows Auth bilgisini ilet
                    );
                    // --- DÜZELTME SONU ---


                    Debug.WriteLine($"Ayarlar kaydedildi - Type: '{settingsForm.DatabaseType}'");

                    // DEBUG: Hemen test et
                    string currentType = AppConfigHelper.DatabaseType;
                    string currentConn = AppConfigHelper.DatabasePath;
                    Debug.WriteLine($"Hemen sonra - DatabaseType: '{currentType}'");
                    Debug.WriteLine($"Hemen sonra - DatabasePath: '{currentConn}'");

                    // Kaydedilen ayarlarý test et
                    if (TestDatabaseConnection())
                    {
                        Debug.WriteLine("Baðlantý testi baþarýlý!");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Baðlantý testi baþarýsýz!");
                        MessageBox.Show("Baðlantý testi baþarýsýz. Lütfen ayarlarý tekrar kontrol edin.",
                            "Baðlantý Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ShowConnectionSettingsForm(); // Tekrar deneme
                    }
                }
                else
                {
                    Debug.WriteLine("Kullanýcý iptal etti...");
                    return false;
                }
            }
        }

        // Geçerli database konfigürasyonu kontrolü 
        private static bool HasValidDatabaseConfiguration()
        {
            string connectionString = AppConfigHelper.DatabasePath;
            string databaseType = AppConfigHelper.DatabaseType;

            Debug.WriteLine($"Configuration kontrolü - Type: {databaseType}, Connection: {!string.IsNullOrEmpty(connectionString)}");

            // Hem connection string hem de database type olmalý
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
            {
                Debug.WriteLine("Connection string veya database type boþ!");
                return false;
            }

            // SQLite için farklý kontrol
            if (databaseType == "SQLite")
            {
                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Debug.WriteLine($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Debug.WriteLine("SQLite Data Source boþ!");
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Debug.WriteLine($"SQLite dizin oluþturuldu: {directory}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SQLite dizin kontrol hatasý: {ex.Message}");
                    return false;
                }
            }

            // MySQL/MSSQL için temel kontroller
            bool hasValidComponents = connectionString.Contains("Server=") &&
                                   connectionString.Contains("Database=");

            Debug.WriteLine($"MySQL/MSSQL bileþen kontrolü: {hasValidComponents}");
            return hasValidComponents;
        }


        private static bool TestDatabaseConnection()
        {
            try
            {
                string connectionString = AppConfigHelper.DatabasePath;
                string databaseType = AppConfigHelper.DatabaseType;

                Debug.WriteLine($"=== TestDatabaseConnection ===");
                Debug.WriteLine($"Database Type: '{databaseType}'");
                Debug.WriteLine($"Connection String: '{connectionString}'");

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
                {
                    Debug.WriteLine("Connection string veya database type boþ!");
                    return false;
                }

                // TÜRKÇE KARAKTER SORUNU ÇÖZÜMÜ - Culture invariant
                string dbType = databaseType.Trim().ToUpperInvariant();
                Debug.WriteLine($"Trimmed Database Type (Invariant): '{dbType}'");

                // Karakterleri tek tek debug et
                Debug.WriteLine("DatabaseType karakter analizi:");
                foreach (char c in dbType)
                {
                    Debug.WriteLine($" - Char: '{c}' Code: {(int)c}");
                }

                // Culture-invariant karþýlaþtýrma
                if (dbType.Equals("SQLITE", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Testing SQLite connection...");
                    return TestSQLiteConnection(connectionString);
                }
                else if (dbType.Equals("MYSQL", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Testing MySQL connection...");
                    return TestMySQLConnection(connectionString);
                }
                else if (dbType.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Testing MSSQL connection...");
                    return TestMSSQLConnection(connectionString);
                }
                else
                {
                    Debug.WriteLine($"Bilinmeyen database tipi: '{databaseType}' -> '{dbType}'");

                    // Manuel kontrol
                    if (databaseType.IndexOf("sqlite", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Debug.WriteLine("Manuel olarak SQLite tespit edildi!");
                        return TestSQLiteConnection(connectionString);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== TestDatabaseConnection ERROR ===");
                Debug.WriteLine($"Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // SQLite baðlantý testi 
        private static bool TestSQLiteConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("SQLite baðlantý testi baþlýyor...");

                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Debug.WriteLine($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Debug.WriteLine("SQLite Data Source boþ!");
                    return false;
                }

                // Dosya ve dizin kontrolü
                string directory = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"SQLite dizin oluþturuldu: {directory}");
                }

                // Baðlantý testi
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("SQLite baðlantýsý açýldý");

                    // Basit bir sorgu çalýþtýr
                    using (var cmd = new SQLiteCommand("SELECT 1", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Debug.WriteLine($"SQLite test sorgusu sonucu: {result}");
                    }

                    // Tablolarý kontrol et
                    Debug.WriteLine("Mevcut tablolar kontrol ediliyor...");
                    using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Debug.WriteLine($" - Tablo: {reader[0]}");
                        }
                    }

                    // Database'i initialize et
                    Debug.WriteLine("SQLite database initialize ediliyor...");
                    var db = new SqliteOperations(connectionString);
                    db.InitializeDatabase();
                    Debug.WriteLine("SQLite database initialize edildi");

                    return true;
                }
            }
            catch (SQLiteException sqlEx)
            {
                Debug.WriteLine($"SQLite baðlantý testi SQL hatasý: {sqlEx.Message}");
                Debug.WriteLine($"SQLite Error Code: {sqlEx.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLite baðlantý testi genel hatasý: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // MySQL baðlantý testi -
        private static bool TestMySQLConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("MySQL baðlantý testi baþlýyor...");
                var db = new MySqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Debug.WriteLine("MySQL baðlantý testi baþarýlý");
                    db.InitializeDatabase();
                    Debug.WriteLine("MySQL database initialize edildi");
                }
                else
                {
                    Debug.WriteLine("MySQL baðlantý testi baþarýsýz");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MySQL baðlantý testi hatasý: {ex.Message}");
                return false;
            }
        }

        // MSSQL baðlantý testi - 
        private static bool TestMSSQLConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("MSSQL baðlantý testi baþlýyor...");
                var db = new MsSqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Debug.WriteLine("MSSQL baðlantý testi baþarýlý");
                    db.InitializeDatabase();
                    Debug.WriteLine("MSSQL database initialize edildi");
                }
                else
                {
                    Debug.WriteLine("MSSQL baðlantý testi baþarýsýz");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSSQL baðlantý testi hatasý: {ex.Message}");
                return false;
            }
        }
    }
}