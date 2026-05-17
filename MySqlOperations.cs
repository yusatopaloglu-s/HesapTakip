using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;

namespace HesapTakip
{
    public class MySqlOperations : IDatabaseOperations
    {
        private string _connectionString;

        public MySqlOperations(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void EnsureDatabaseExists()
        {
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.Database;
            if (string.IsNullOrWhiteSpace(databaseName)) return;

            // Try open target DB
            try
            {
                using (var testConn = new MySqlConnection(_connectionString))
                {
                    testConn.Open();
                    return;
                }
            }
            catch (MySqlException ex)
            {
                Logger.Log($"MySql EnsureDatabaseExists: target DB open failed: {ex.Message} (Number {ex.Number})");
                // continue to attempt create
            }
            catch (Exception ex)
            {
                Logger.Log($"MySql EnsureDatabaseExists: target DB open failed: {ex.Message}");
            }

            // Connect to server without database
            var serverBuilder = new MySqlConnectionStringBuilder(_connectionString)
            {
                Database = ""
            };

            try
            {
                using (var conn = new MySqlConnection(serverBuilder.ConnectionString))
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Logger.Log($"MySql EnsureDatabaseExists: ensured database '{databaseName}' exists.");
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("Veritabanı oluşturulamadı: MySQL kullanıcı/sunucu yetkileri yetersiz veya kimlik doğrulama hatası. Lütfen MySQL sunucusunda uygun yetkilere sahip bir kullanıcı kullanın veya veritabanını elle oluşturun. Hata mesajı: " + ex.Message, ex);
            }
        }

        public void InitializeDatabase()
        {
            EnsureDatabaseExists();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Customers tablosu
                EnsureTableAndColumns("Customers", new Dictionary<string, string>
                {
                    { "CustomerID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "Name", "VARCHAR(255) NOT NULL" },
                    { "EDefter", "INT DEFAULT 0" },
                    { "Taxid","VARCHAR(11) DEFAULT NULL" },
                    { "ActivityCode","VARCHAR(6) DEFAULT NULL" },
                    { "IsDeleted", "TINYINT(1) DEFAULT 0" }
                }, conn);

                // Transactions tablosu  
                EnsureTableAndColumns("Transactions", new Dictionary<string, string>
                {
                    { "TransactionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "CustomerID", "INT" },
                    { "Date", "DATETIME" },
                    { "Description", "VARCHAR(255) NULL" },
                    { "Amount", "DECIMAL(18,2)" },
                    { "Period", "INT NULL" },
                    { "Type", "VARCHAR(50)" },
                    { "IsDeleted", "TINYINT(1) DEFAULT 0" }
                }, conn);

                // EDefterTakip tablosu
                EnsureTableAndColumns("EDefterTakip", new Dictionary<string, string>
                {
                    { "TransactionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "CustomerID", "INT" },
                    { "Date", "DATETIME" },
                    { "Kontor", "DECIMAL(18,2)" },
                    { "Type", "VARCHAR(255) NOT NULL" }
                }, conn);

                // Suggestions tablosu
                EnsureTableAndColumns("Suggestions", new Dictionary<string, string>
                {
                    { "SuggestionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "Description", "VARCHAR(255) NOT NULL UNIQUE" },
                    { "CreatedDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" }
                }, conn);

                // ExpenseCategories tablosu
                EnsureTableAndColumns("ExpenseCategories", new Dictionary<string, string>
                {
                    { "CategoryID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "Label", "VARCHAR(255) NOT NULL" },
                    { "Info", "VARCHAR(255) NOT NULL" }
                }, conn);
                // ExpenseCategories tablosunu JSON dosyasından doldur
                if (!TableHasData("ExpenseCategories", conn))
                {
                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expense_categories.json");
                    if (File.Exists(jsonFilePath))
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        var categories = JsonSerializer.Deserialize<List<ExpenseCategory>>(jsonContent);

                        using (var cmd = new MySqlCommand())
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
                   { "MatchingID", "INT PRIMARY KEY AUTO_INCREMENT" },
                   { "ItemName", "VARCHAR(255) NOT NULL" },
                   { "SubRecordType", "VARCHAR(255) NOT NULL" }
                   }, conn);

                // Periods table
                EnsureTableAndColumns("Periods", new Dictionary<string, string>
                {
                    { "PeriodYear", "INT PRIMARY KEY" },
                    { "DisplayName", "VARCHAR(255) NULL" }
                }, conn);

                // Templates tablosu
                EnsureTableAndColumns("Templates", new Dictionary<string, string>
                {
                    { "TemplateID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "Name", "VARCHAR(255) NOT NULL" },
                    { "IsBuiltIn", "TINYINT(1) DEFAULT 0" },
                    { "DefinitionJson", "TEXT NOT NULL" },
                    { "CreatedAt", "DATETIME DEFAULT CURRENT_TIMESTAMP" }
                }, conn);

                // TemplateColumns tablosu (opsiyonel)
                EnsureTableAndColumns("TemplateColumns", new Dictionary<string, string>
                {
                    { "TemplateColumnID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "TemplateID", "INT NOT NULL" },
                    { "ColumnName", "VARCHAR(255) NOT NULL" },
                    { "DataKey", "VARCHAR(255) NOT NULL" },
                    { "DisplayOrder", "INT DEFAULT 0" }
                }, conn);

                // FaturaFilters tablosu - Özel şablonlar için filtre kuralları
                EnsureTableAndColumns("FaturaFilters", new Dictionary<string, string>
{
                    { "FilterID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "TemplateID", "INT NOT NULL" },
                    { "ItemName", "VARCHAR(255) NOT NULL" },  
                    { "TaxRate", "DECIMAL(5,2) DEFAULT 0" },  
                    { "OutputColumnPrefix", "VARCHAR(50)" },
                    { "DisplayOrder", "INT DEFAULT 0" },
                    { "IsActive", "TINYINT(1) DEFAULT 1" },   
                    { "CreatedAt", "DATETIME DEFAULT CURRENT_TIMESTAMP" },
                    { "FOREIGN KEY (TemplateID) REFERENCES Templates(TemplateID) ON DELETE CASCADE", "" }
                }, conn);

                var templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
                var defaultTemplates = new List<(string Name, string Json)>
                {
                    ("Bilanço Satış", "table_bilancosatis.json"),
                    ("Bilanço Alış", "table_bilancoalis.json"),
                    ("Luca İşletme Satış", "table_isletmesatis.json"),
                    ("Luca İşletme Alış", "table_isletmesatis.json"),
                    ("Fatura Kalemleri Adet", "table_faturakalem.json")
                };

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Templates";
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        foreach (var tpl in defaultTemplates)
                        {
                            var jsonPath = Path.Combine(templateDir, tpl.Json);
                            if (!File.Exists(jsonPath))
                                throw new FileNotFoundException($"Şablon dosyası bulunamadı: {jsonPath}");

                            var definitionJson = File.ReadAllText(jsonPath);

                            cmd.CommandText = "INSERT INTO Templates (Name, IsBuiltIn, DefinitionJson) VALUES (@name, @isBuiltIn, @definitionJson)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@name", tpl.Name);
                            cmd.Parameters.AddWithValue("@isBuiltIn", 1);
                            cmd.Parameters.AddWithValue("@definitionJson", definitionJson);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

            }
        }         
                    
        public IDbConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public DataTable GetCustomers()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter("SELECT CustomerID,Name,EDefter,Taxid,ActivityCode,IsDeleted FROM Customers WHERE IsDeleted = 0", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }


        public bool AddCustomer(string name, bool edefter, string taxid = null, string activitycode = null)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "INSERT INTO Customers (Name, EDefter, Taxid, Activitycode) VALUES (@name, @edefter, @taxid, @activitycode)", conn))
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
            catch
            {
                return false;
            }
        }

        public bool UpdateCustomer(int customerId, string newName, bool edefter, string taxid = null, string activitycode = null, bool deleted = false)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "UPDATE Customers SET Name = @name, EDefter = @edefter, Taxid = @taxid, ActivityCode = @activitycode, IsDeleted = @deleted WHERE CustomerID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@edefter", edefter ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.Parameters.AddWithValue("@taxid", string.IsNullOrEmpty(taxid) ? (object)DBNull.Value : taxid);
                    cmd.Parameters.AddWithValue("@activitycode", string.IsNullOrEmpty(activitycode) ? (object)DBNull.Value : activitycode);
                    cmd.Parameters.AddWithValue("@deleted", deleted ? 1 : 0);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "UPDATE Customers SET IsDeleted = 1 WHERE CustomerID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetDeletedCustomers()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter("SELECT CustomerID, Name, EDefter, Taxid, ActivityCode, IsDeleted FROM Customers WHERE IsDeleted = 1", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable GetTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter(
                "SELECT TransactionID, Date, Description, Amount, Type, Period FROM Transactions WHERE CustomerID = @customerID AND IsDeleted = 0 ORDER BY Date ASC",
                conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@customerID", customerId);
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddTransaction(int customerId, DateTime date, string description, decimal amount, string type, int? period = null)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    @"INSERT INTO Transactions (CustomerID, Date, Description, Amount, Type, Period) 
                      VALUES (@cid, @date, @desc, @amount, @type, @period)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@cid", customerId);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@period", period.HasValue ? (object)period.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type, int? period = null)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    @"UPDATE Transactions SET Date = @date, Description = @desc, 
                      Amount = @amount, Type = @type, Period = @period WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@period", period.HasValue ? (object)period.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteTransaction(int transactionId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "UPDATE Transactions SET IsDeleted = 1 WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetSuggestions()
        {
            var suggestions = new List<string>();
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand("SELECT Description FROM Suggestions", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        suggestions.Add(reader["Description"].ToString());
                }
            }
            return suggestions;
        }

        public bool AddSuggestion(string description)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "INSERT INTO Suggestions (Description) VALUES (@desc)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@desc", description.Trim());
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveSuggestion(string description)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "DELETE FROM Suggestions WHERE Description = @desc", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public decimal CalculateTotalBalance(int customerId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    @"SELECT SUM(Amount * CASE WHEN Type = 'Gelir' THEN 1 ELSE -1 END) 
                      FROM Transactions WHERE CustomerID = @customerID AND IsDeleted = 0", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@customerID", customerId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                EnsureTableAndColumns(tableName, columns, conn);
            }
        }

        private void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns, MySqlConnection conn)
        {
            using (var cmd = new MySqlCommand())
            {
                cmd.Connection = conn;

                // Tablo var mı kontrolü
                cmd.CommandText = $"SHOW TABLES LIKE '{tableName}'";
                var exists = cmd.ExecuteScalar() != null;

                if (!exists)
                {
                    var columnsDef = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                    cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDef}) CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Kolon kontrolü
                    cmd.CommandText = $"SHOW COLUMNS FROM {tableName}";
                    var reader = cmd.ExecuteReader();
                    var existingColumns = new HashSet<string>();
                    while (reader.Read())
                    {
                        existingColumns.Add(reader["Field"].ToString());
                    }
                    reader.Close();

                    foreach (var kv in columns)
                    {
                        if (!existingColumns.Contains(kv.Key))
                        {
                            cmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {kv.Key} {kv.Value}";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public DataTable GetEDefterTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter(
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
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
            catch
            {
                return false;
            }
        }

        public bool DeleteEDefterTransaction(int transactionId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "DELETE FROM EDefterTakip WHERE TransactionID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public decimal CalculateEDefterTotal(int customerId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    @"SELECT SUM(Kontor * CASE WHEN Type = 'ekle' THEN 1 ELSE -1 END) 
              FROM EDefterTakip WHERE CustomerID = @customerID", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@customerID", customerId);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public bool BulkUpdateEDefterTransactions(List<EDefterTransaction> transactions)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var trans in transactions)
                            {
                                using (var cmd = new MySqlCommand(
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
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TableHasData(string tableName, MySqlConnection conn)
        {
            using (var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {tableName}", conn))
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
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter("SELECT CategoryID, Label, Info FROM ExpenseCategories", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
        public bool AddExpenseMatching(string itemName, string subRecordType)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter("SELECT ItemName, SubRecordType FROM ExpenseMatching", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable GetPeriods()
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter("SELECT PeriodYear, DisplayName FROM Periods ORDER BY PeriodYear DESC", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddPeriod(int periodYear, string displayName = null)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand("INSERT IGNORE INTO Periods (PeriodYear, DisplayName) VALUES (@year, @disp)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@year", periodYear);
                    cmd.Parameters.AddWithValue("@disp", string.IsNullOrEmpty(displayName) ? (object)DBNull.Value : displayName);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public DataTable GetTemplates()
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter("SELECT TemplateID, Name, IsBuiltIn, DefinitionJson, CreatedAt FROM Templates ORDER BY IsBuiltIn DESC, Name ASC", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddTemplate(string name, string definitionJson, bool isBuiltIn = false)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand("INSERT INTO Templates (Name, IsBuiltIn, DefinitionJson) VALUES (@name, @isBuiltIn, @definitionJson)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@isBuiltIn", isBuiltIn ? 1 : 0);
                    cmd.Parameters.AddWithValue("@definitionJson", definitionJson);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite AddTemplate hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTemplate(int templateId, string name, string definitionJson)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand("UPDATE Templates SET Name = @name, DefinitionJson = @definitionJson WHERE TemplateID = @id AND IsBuiltIn = 0", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@definitionJson", definitionJson);
                    cmd.Parameters.AddWithValue("@id", templateId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite UpdateTemplate hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTemplate(int templateId)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                using (var cmd = new SQLiteCommand("DELETE FROM Templates WHERE TemplateID = @id AND IsBuiltIn = 0", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", templateId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SQLite DeleteTemplate hatası: {ex.Message}");
                return false;
            }
        }

        public DataTable GetTemplateColumns(int templateId)
        {
            var dt = new DataTable();
            using (var conn = new SQLiteConnection(_connectionString))
            using (var adapter = new SQLiteDataAdapter("SELECT TemplateColumnID, TemplateID, ColumnName, DataKey, DisplayOrder FROM TemplateColumns WHERE TemplateID = @templateId ORDER BY DisplayOrder ASC", conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@templateId", templateId);
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable GetFiltersForTemplate(int templateId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter(
                "SELECT FilterID, TemplateID, ItemName, TaxRate, OutputColumnPrefix, DisplayOrder, IsActive, CreatedAt " +
                "FROM FaturaFilters WHERE TemplateID = @templateId AND IsActive = 1 " +
                "ORDER BY DisplayOrder ASC, FilterID ASC", conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@templateId", templateId);
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddFilter(int templateId, string itemName, decimal taxRate, string outputColumnPrefix, int displayOrder = 0)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "INSERT INTO FaturaFilters (TemplateID, ItemName, TaxRate, OutputColumnPrefix, DisplayOrder) " +
                    "VALUES (@templateId, @itemName, @taxRate, @prefix, @displayOrder)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@templateId", templateId);
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.Parameters.AddWithValue("@taxRate", taxRate);
                    cmd.Parameters.AddWithValue("@prefix", outputColumnPrefix);
                    cmd.Parameters.AddWithValue("@displayOrder", displayOrder);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"MySql AddFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateFilter(int filterId, string itemName, decimal taxRate, string outputColumnPrefix, int displayOrder)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
                    "UPDATE FaturaFilters SET ItemName = @itemName, TaxRate = @taxRate, OutputColumnPrefix = @prefix, DisplayOrder = @displayOrder " +
                    "WHERE FilterID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.Parameters.AddWithValue("@taxRate", taxRate);
                    cmd.Parameters.AddWithValue("@prefix", outputColumnPrefix);
                    cmd.Parameters.AddWithValue("@displayOrder", displayOrder);
                    cmd.Parameters.AddWithValue("@id", filterId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"MySql UpdateFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteFilter(int filterId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand("DELETE FROM FaturaFilters WHERE FilterID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", filterId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"MySql DeleteFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool ToggleFilterActive(int filterId, bool isActive)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand("UPDATE FaturaFilters SET IsActive = @active WHERE FilterID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@active", isActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", filterId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"MySql ToggleFilterActive hatası: {ex.Message}");
                return false;
            }
        }

        public bool ReorderFilters(int templateId, Dictionary<int, int> filterIdToOrder)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var kvp in filterIdToOrder)
                            {
                                using (var cmd = new MySqlCommand(
                                    "UPDATE FaturaFilters SET DisplayOrder = @displayOrder WHERE FilterID = @filterId AND TemplateID = @templateId", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@displayOrder", kvp.Value);
                                    cmd.Parameters.AddWithValue("@filterId", kvp.Key);
                                    cmd.Parameters.AddWithValue("@templateId", templateId);
                                    cmd.ExecuteNonQuery();
                                }
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
                Logger.Log($"MySql ReorderFilters hatası: {ex.Message}");
                return false;
            }
        }
    }
}