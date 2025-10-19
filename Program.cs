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
                Debug.WriteLine("Uygulama ba�l�yor...");

                QuestPDF.Settings.License = LicenseType.Community;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);

                Debug.WriteLine("Temel ayarlar yap�ld�...");

                // ESK� AYARLARI YEN� KONUMA TA�I
                AppConfigHelper.MigrateOldSettings();
                Debug.WriteLine("Migration tamamland�...");

                // Database ba�lant� kontrol�
                if (!HasValidDatabaseConfiguration())
                {
                    Debug.WriteLine("Ge�erli ba�lant� yok, ConnectionSettingsForm a��l�yor...");
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Ba�lant� ayarlar� girilmedi. Uygulama kapat�l�yor.");
                        return;
                    }
                }
                else if (!TestDatabaseConnection())
                {
                    Debug.WriteLine("Ba�lant� testi ba�ar�s�z, ConnectionSettingsForm a��l�yor...");
                    MessageBox.Show("Veritaban� ba�lant�s� ba�ar�s�z. L�tfen ayarlar� kontrol edin.",
                        "Ba�lant� Hatas�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (!ShowConnectionSettingsForm())
                    {
                        MessageBox.Show("Ba�lant� ayarlar� girilmedi. Uygulama kapat�l�yor.");
                        return;
                    }
                }

                Debug.WriteLine("Ba�lant� ba�ar�l�, MainForm a��l�yor...");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"KR�T�K HATA: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

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
                    Debug.WriteLine("Kullan�c� ayarlar� kaydetti...");

                    // SQLite i�in dosya yolunu da g�nder
                    // --- D�ZELTME BA�LANGI� ---
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
                        useWindowsAuth // MSSQL i�in Windows Auth bilgisini ilet
                    );
                    // --- D�ZELTME SONU ---


                    Debug.WriteLine($"Ayarlar kaydedildi - Type: '{settingsForm.DatabaseType}'");

                    // DEBUG: Hemen test et
                    string currentType = AppConfigHelper.DatabaseType;
                    string currentConn = AppConfigHelper.DatabasePath;
                    Debug.WriteLine($"Hemen sonra - DatabaseType: '{currentType}'");
                    Debug.WriteLine($"Hemen sonra - DatabasePath: '{currentConn}'");

                    // Kaydedilen ayarlar� test et
                    if (TestDatabaseConnection())
                    {
                        Debug.WriteLine("Ba�lant� testi ba�ar�l�!");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Ba�lant� testi ba�ar�s�z!");
                        MessageBox.Show("Ba�lant� testi ba�ar�s�z. L�tfen ayarlar� tekrar kontrol edin.",
                            "Ba�lant� Hatas�", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ShowConnectionSettingsForm(); // Tekrar deneme
                    }
                }
                else
                {
                    Debug.WriteLine("Kullan�c� iptal etti...");
                    return false;
                }
            }
        }

        // Ge�erli database konfig�rasyonu kontrol� 
        private static bool HasValidDatabaseConfiguration()
        {
            string connectionString = AppConfigHelper.DatabasePath;
            string databaseType = AppConfigHelper.DatabaseType;

            Debug.WriteLine($"Configuration kontrol� - Type: {databaseType}, Connection: {!string.IsNullOrEmpty(connectionString)}");

            // Hem connection string hem de database type olmal�
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
            {
                Debug.WriteLine("Connection string veya database type bo�!");
                return false;
            }

            // SQLite i�in farkl� kontrol
            if (databaseType == "SQLite")
            {
                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Debug.WriteLine($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Debug.WriteLine("SQLite Data Source bo�!");
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                        Debug.WriteLine($"SQLite dizin olu�turuldu: {directory}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SQLite dizin kontrol hatas�: {ex.Message}");
                    return false;
                }
            }

            // MySQL/MSSQL i�in temel kontroller
            bool hasValidComponents = connectionString.Contains("Server=") &&
                                   connectionString.Contains("Database=");

            Debug.WriteLine($"MySQL/MSSQL bile�en kontrol�: {hasValidComponents}");
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
                    Debug.WriteLine("Connection string veya database type bo�!");
                    return false;
                }

                // T�RK�E KARAKTER SORUNU ��Z�M� - Culture invariant
                string dbType = databaseType.Trim().ToUpperInvariant();
                Debug.WriteLine($"Trimmed Database Type (Invariant): '{dbType}'");

                // Karakterleri tek tek debug et
                Debug.WriteLine("DatabaseType karakter analizi:");
                foreach (char c in dbType)
                {
                    Debug.WriteLine($" - Char: '{c}' Code: {(int)c}");
                }

                // Culture-invariant kar��la�t�rma
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

        // SQLite ba�lant� testi 
        private static bool TestSQLiteConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("SQLite ba�lant� testi ba�l�yor...");

                string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                Debug.WriteLine($"SQLite Data Source: {dataSource}");

                if (string.IsNullOrEmpty(dataSource))
                {
                    Debug.WriteLine("SQLite Data Source bo�!");
                    return false;
                }

                // Dosya ve dizin kontrol�
                string directory = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"SQLite dizin olu�turuldu: {directory}");
                }

                // Ba�lant� testi
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("SQLite ba�lant�s� a��ld�");

                    // Basit bir sorgu �al��t�r
                    using (var cmd = new SQLiteCommand("SELECT 1", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Debug.WriteLine($"SQLite test sorgusu sonucu: {result}");
                    }

                    // Tablolar� kontrol et
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
                Debug.WriteLine($"SQLite ba�lant� testi SQL hatas�: {sqlEx.Message}");
                Debug.WriteLine($"SQLite Error Code: {sqlEx.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLite ba�lant� testi genel hatas�: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // MySQL ba�lant� testi -
        private static bool TestMySQLConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("MySQL ba�lant� testi ba�l�yor...");
                var db = new MySqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Debug.WriteLine("MySQL ba�lant� testi ba�ar�l�");
                    db.InitializeDatabase();
                    Debug.WriteLine("MySQL database initialize edildi");
                }
                else
                {
                    Debug.WriteLine("MySQL ba�lant� testi ba�ar�s�z");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MySQL ba�lant� testi hatas�: {ex.Message}");
                return false;
            }
        }

        // MSSQL ba�lant� testi - 
        private static bool TestMSSQLConnection(string connectionString)
        {
            try
            {
                Debug.WriteLine("MSSQL ba�lant� testi ba�l�yor...");
                var db = new MsSqlOperations(connectionString);
                bool result = db.TestConnection();

                if (result)
                {
                    Debug.WriteLine("MSSQL ba�lant� testi ba�ar�l�");
                    db.InitializeDatabase();
                    Debug.WriteLine("MSSQL database initialize edildi");
                }
                else
                {
                    Debug.WriteLine("MSSQL ba�lant� testi ba�ar�s�z");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSSQL ba�lant� testi hatas�: {ex.Message}");
                return false;
            }
        }
    }
}