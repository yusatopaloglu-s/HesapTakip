using System.Data;
using System.Data.SQLite;
using System.Text.Json;

namespace HesapTakip
{
    public class SqliteOperations : IDatabaseOperations
    {
        private string _connectionString;

        public SqliteOperations(string connectionString)
        {
            Logger.Log($"SqliteOperations constructor called with: {connectionString}");
            _connectionString = connectionString;
            EnsureDatabaseFile();
        }


        private void EnsureDatabaseFile()
        {
            try
            {
                // Connection string'den dosya yolunu al
                string dataSource = GetDataSourceFromConnectionString();

                if (string.IsNullOrEmpty(dataSource))
                {
                    throw new ArgumentException("Geçersiz SQLite connection string: Data Source bulunamadı");
                }

                // Dizin yoksa oluştur
                string directory = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Logger.Log($"Dizin oluşturuldu: {directory}");
                }

                // Dosya yoksa oluştur
                if (!File.Exists(dataSource))
                {
                    SQLiteConnection.CreateFile(dataSource);
                    Logger.Log($"SQLite database oluşturuldu: {dataSource}");

                    // Tabloları oluştur
                    InitializeDatabase();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite database dosyası oluşturulamadı: {ex.Message}");
                throw;
            }
        }

        private string GetDataSourceFromConnectionString()
        {
            try
            {
                // Connection string'i parse et
                var builder = new SQLiteConnectionStringBuilder(_connectionString);
                return builder.DataSource;
            }
            catch (Exception ex)
            {
                Logger.Log($"Connection string parse hatası: {ex.Message}");
                return string.Empty;
            }
        }

        public bool TestConnection()
        {
            try
            {
                Logger.Log("SQLite TestConnection başlıyor...");
                Logger.Log($"SQLite Connection String: {_connectionString}");

                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    Logger.Log("SQLite bağlantısı açıldı");

                    // Basit bir test sorgusu
                    using (var cmd = new SQLiteCommand("SELECT 1", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        Logger.Log($"SQLite test sorgusu sonucu: {result}");
                    }

                    // Tabloları kontrol et
                    using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Logger.Log("Mevcut tablolar:");
                        while (reader.Read())
                        {
                            Logger.Log($" - {reader[0]}");
                        }
                    }

                    Logger.Log("SQLite bağlantı testi başarılı.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite bağlantı testi hatası: {ex.Message}");
                Logger.Log($"Hata türü: {ex.GetType()}");
                Logger.Log($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public void InitializeDatabase()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                // Customers tablosu - SQLite uyumlu
                EnsureTableAndColumns("Customers", new Dictionary<string, string>
                {
                    { "CustomerID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                    { "Name", "TEXT NOT NULL" },
                    { "EDefter", "INTEGER DEFAULT 0" },
                    { "Taxid","TEXT DEFAULT NULL" },
                    { "ActivityCode","TEXT DEFAULT NULL" }
                }, conn);

                // Transactions tablosu - SQLite uyumlu
                EnsureTableAndColumns("Transactions", new Dictionary<string, string>
                {
                    { "TransactionID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                    { "CustomerID", "INTEGER" },
                    { "Date", "DATETIME" },
                    { "Description", "TEXT" },
                    { "Amount", "DECIMAL(18,2)" },
                    { "Type", "TEXT" },
                    { "IsDeleted", "INTEGER DEFAULT 0" }
                }, conn);

                // EDefterTakip tablosu - SQLite uyumlu
                EnsureTableAndColumns("EDefterTakip", new Dictionary<string, string>
                {
                    { "TransactionID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                    { "CustomerID", "INTEGER" },
                    { "Date", "DATETIME" },
                    { "Kontor", "DECIMAL(18,2)" },
                    { "Type", "TEXT NOT NULL" }
                }, conn);

                // Suggestions tablosu - SQLite uyumlu
                EnsureTableAndColumns("Suggestions", new Dictionary<string, string>
                {
                    { "SuggestionID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                    { "Description", "TEXT NOT NULL UNIQUE" },
                    { "CreatedDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" }
                }, conn);
                // ExpenseCategories tablosu
                EnsureTableAndColumns("ExpenseCategories", new Dictionary<string, string>
                {
                    { "CategoryID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
                    { "Label", "TEXT NOT NULL" },
                    { "Info", "TEXT NOT NULL" }
                }, conn);

                // ExpenseCategories tablosunu JSON dosyasından doldur
                if (!TableHasData("ExpenseCategories", conn))
                {
                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expense_categories.json");
                    if (File.Exists(jsonFilePath))
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        var categories = JsonSerializer.Deserialize<List<ExpenseCategory>>(jsonContent);

                        using (var cmd = new SQLiteCommand())
                        {
                            cmd.Connection = conn;
                            foreach (var category in categories)
                            {
                                cmd.CommandText = "INSERT INTO ExpenseCategories (Label, Info) VALUES (@label, @info)";
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@label", category.Label ?? "");
                                cmd.Parameters.AddWithValue("@info", category.Info ?? "");
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("expense_categories.json file not found in the application directory.");
                    }
                }
                // ExpenseMatching tablosu
                EnsureTableAndColumns("ExpenseMatching", new Dictionary<string, string>
        {
            { "MatchingID", "INTEGER PRIMARY KEY AUTOINCREMENT" },
            { "ItemName", "TEXT NOT NULL" },
            { "SubRecordType", "TEXT NOT NULL" }
        }, conn);
            }
        }

        public IDbConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public DataTable GetCustomers()
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter("SELECT * FROM Customers", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddCustomer(string name, bool edefter, string taxid = null, string activitycode = null)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Customers (Name, EDefter, Taxid, ActivityCode) VALUES (@name, @edefter, @taxid, @activitycode)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@name", name.Trim());
                    cmd.Parameters.AddWithValue("@edefter", edefter ? 1 : 0);
                    cmd.Parameters.AddWithValue("@taxid", string.IsNullOrEmpty(taxid) ? (object)DBNull.Value : taxid);
                    cmd.Parameters.AddWithValue("@activitycode", string.IsNullOrEmpty(activitycode) ? (object)DBNull.Value : activitycode);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite AddCustomer hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateCustomer(int customerId, string newName, bool edefter, string taxid = null, string activitycode = null)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "UPDATE Customers SET Name = @name, EDefter = @edefter, Taxid = @taxid, ActivityCode = @activitycode WHERE CustomerID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@edefter", edefter ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.Parameters.AddWithValue("@taxid", string.IsNullOrEmpty(taxid) ? (object)DBNull.Value : taxid);
                    cmd.Parameters.AddWithValue("@activitycode", string.IsNullOrEmpty(activitycode) ? (object)DBNull.Value : activitycode);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite UpdateCustomer hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Önce Transactions tablosundan sil
                            using (var cmd1 = new SQLiteCommand("DELETE FROM Transactions WHERE CustomerID = @id", conn, transaction))
                            {
                                cmd1.Parameters.AddWithValue("@id", customerId);
                                cmd1.ExecuteNonQuery();
                            }

                            // Sonra Customers tablosundan sil
                            using (var cmd2 = new SQLiteCommand("DELETE FROM Customers WHERE CustomerID = @id", conn, transaction))
                            {
                                cmd2.Parameters.AddWithValue("@id", customerId);
                                cmd2.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite DeleteCustomer hatası: {ex.Message}");
                return false;
            }
        }

        public DataTable GetTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter(
                "SELECT TransactionID, Date, Description, Amount, Type FROM Transactions WHERE CustomerID = @customerID AND IsDeleted = 0 ORDER BY Date ASC",
                conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@customerID", customerId);
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddTransaction(int customerId, DateTime date, string description, decimal amount, string type)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    @"INSERT INTO Transactions (CustomerID, Date, Description, Amount, Type) 
                      VALUES (@cid, @date, @desc, @amount, @type)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@cid", customerId);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite AddTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    @"UPDATE Transactions SET Date = @date, Description = @desc, 
                      Amount = @amount, Type = @type WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite UpdateTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTransaction(int transactionId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "UPDATE Transactions SET IsDeleted = 1 WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite DeleteTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public List<string> GetSuggestions()
        {
            var suggestions = new List<string>();
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand("SELECT Description FROM Suggestions", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            suggestions.Add(reader["Description"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite GetSuggestions hatası: {ex.Message}");
            }
            return suggestions;
        }

        public bool AddSuggestion(string description)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO Suggestions (Description) VALUES (@desc)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@desc", description.Trim());
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite AddSuggestion hatası: {ex.Message}");
                return false;
            }
        }

        public bool RemoveSuggestion(string description)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "DELETE FROM Suggestions WHERE Description = @desc", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@desc", description);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite RemoveSuggestion hatası: {ex.Message}");
                return false;
            }
        }

        public decimal CalculateTotalBalance(int customerId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    @"SELECT SUM(Amount * CASE WHEN Type = 'Gelir' THEN 1 ELSE -1 END) 
                      FROM Transactions WHERE CustomerID = @customerID AND IsDeleted = 0", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@customerID", customerId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value && result != null ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite CalculateTotalBalance hatası: {ex.Message}");
                return 0;
            }
        }

        public void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                EnsureTableAndColumns(tableName, columns, conn);
            }
        }

        private void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns, SQLiteConnection conn)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = conn;

                try
                {
                    // Tablo var mı kontrolü - SQLite syntax
                    cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    var exists = cmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        var columnsDef = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                        cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDef})";
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                        Logger.Log($"{tableName} tablosu oluşturuldu.");
                    }
                    else
                    {
                        // Kolon kontrolü - SQLite syntax
                        cmd.CommandText = $"PRAGMA table_info({tableName})";
                        cmd.Parameters.Clear();
                        using (var reader = cmd.ExecuteReader())
                        {
                            var existingColumns = new HashSet<string>();
                            while (reader.Read())
                            {
                                existingColumns.Add(reader["name"].ToString());
                            }

                            foreach (var kv in columns)
                            {
                                if (!existingColumns.Contains(kv.Key))
                                {
                                    // SQLite'da ALTER TABLE ADD COLUMN
                                    cmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {kv.Key} {kv.Value}";
                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                        Logger.Log($"{tableName} tablosuna {kv.Key} sütunu eklendi.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log($"{tableName}.{kv.Key} sütunu eklenirken hata: {ex.Message}");
                                    }
                                    finally
                                    {
                                        cmd.Parameters.Clear();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"EnsureTableAndColumns hatası ({tableName}): {ex.Message}");
                    throw;
                }
            }
        }

        public bool DeleteEDefterTransaction(int transactionId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "DELETE FROM EDefterTakip WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite DeleteEDefterTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public decimal CalculateEDefterTotal(int customerId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    @"SELECT SUM(Kontor * CASE WHEN Type = 'ekle' THEN 1 ELSE -1 END) 
              FROM EDefterTakip WHERE CustomerID = @customerID", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@customerID", customerId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value && result != null ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite CalculateEDefterTotal hatası: {ex.Message}");
                return 0;
            }
        }

        public bool BulkUpdateEDefterTransactions(List<EDefterTransaction> transactions)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var trans in transactions)
                            {
                                using (var cmd = new SQLiteCommand(
                                    @"INSERT INTO EDefterTakip (CustomerID, Date, Kontor, Type) 
                              VALUES (@cid, @date, @kontor, @type)", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@cid", trans.CustomerID);
                                    cmd.Parameters.AddWithValue("@date", trans.Date);
                                    cmd.Parameters.AddWithValue("@kontor", trans.Kontor);
                                    cmd.Parameters.AddWithValue("@type", trans.Type);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Logger.Log($"SQLite BulkUpdateEDefterTransactions hatası: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite BulkUpdateEDefterTransactions hatası: {ex.Message}");
                return false;
            }
        }

        public DataTable GetEDefterTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter(
                "SELECT TransactionID, Date, Kontor, Type FROM EDefterTakip WHERE CustomerID = @customerID ORDER BY Date ASC",
                conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@customerID", customerId);
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddEDefterTransaction(int customerId, DateTime date, decimal kontor, string type)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    @"INSERT INTO EDefterTakip (CustomerID, Date, Kontor, Type) 
              VALUES (@cid, @date, @kontor, @type)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@cid", customerId);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@kontor", kontor);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite AddEDefterTransaction hatası: {ex.Message}");
                return false;
            }
        }
        private bool TableHasData(string tableName, SQLiteConnection conn)
        {
            using (var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {tableName}", conn))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private class ExpenseCategory
        {
            public string Label { get; set; }
            public string Info { get; set; }
        }
        public DataTable GetCategories()
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter("SELECT CategoryID, Label, Info FROM ExpenseCategories", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
        public bool AddExpenseMatching(string itemName, string subRecordType)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "INSERT INTO ExpenseMatching (ItemName, SubRecordType) VALUES (@itemName, @subRecordType)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.Parameters.AddWithValue("@subRecordType", subRecordType);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteExpenseMatching(string itemName)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand(
                    "DELETE FROM ExpenseMatching WHERE ItemName = @itemName", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetExpenseMatchings()
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter("SELECT ItemName, SubRecordType FROM ExpenseMatching", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

    }
}