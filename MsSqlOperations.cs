using DocumentFormat.OpenXml.Drawing.Diagrams;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.Json;

namespace HesapTakip
{
    public class MsSqlOperations : IDatabaseOperations
    {
        private string _connectionString;

        public MsSqlOperations(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TestConnection()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
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

        // Returns true if the method had to create the database (i.e. it did not previously exist)
        private bool EnsureDatabaseExists()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName)) return false;

            // If target DB is already accessible, nothing to do
            try
            {
                using (var testConn = new SqlConnection(_connectionString))
                {
                    testConn.Open();
                    return false; // already existed / accessible
                }
            }
            catch (SqlException ex)
            {
                Logger.Log($"MsSql EnsureDatabaseExists: target DB open failed: {ex.Message} (Number {ex.Number})");
                // fall through to attempt create
            }
            catch (Exception ex)
            {
                Logger.Log($"MsSql EnsureDatabaseExists: target DB open failed: {ex.Message}");
            }

            // Build connection to master
            var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };

            try
            {
                using (var conn = new SqlConnection(masterBuilder.ConnectionString))
                {
                    conn.Open();
                    // Create database if not exists
                    string sql = $@"IF DB_ID(N'{databaseName}') IS NULL
                        BEGIN
                          CREATE DATABASE [{databaseName}];
                            END";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Logger.Log($"MsSql EnsureDatabaseExists: ensured database '{databaseName}' exists.");
                return true; // created now (or ensured)
            }
            catch (SqlException ex)
            {
                // 15247 = The server principal does not have CREATE DATABASE permission
                if (ex.Number == 15247)
                {
                    throw new InvalidOperationException("Veritabanı oluşturulamadı: çalıştıran Windows hesabının SQL Server üzerinde 'CREATE DATABASE' yetkisi yok. Lütfen ya SQL Server'da yetki verin, ya SQL Authentication kullanın veya veritabanını elle oluşturun. Hata mesajı: " + ex.Message, ex);
                }

                throw; // rethrow other SQL exceptions
            }
        }

        public void InitializeDatabase()
        {
            // Ensure DB exists (create if missing) before schema setup
            bool createdNow = EnsureDatabaseExists();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Customers tablosu - MSSQL uyumlu
                    EnsureTableAndColumns("Customers", new Dictionary<string, string>
                    {
                        { "CustomerID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "Name", "NVARCHAR(255) NOT NULL" },
                        { "EDefter", "INT DEFAULT 0" },
                        { "Taxid","NVARCHAR(11) DEFAULT NULL" },
                        { "ActivityCode","NVARCHAR(6) DEFAULT NULL" },
                        { "IsDeleted", "BIT DEFAULT 0" }
                    }, conn);

                    // Transactions tablosu - MSSQL uyumlu
                    EnsureTableAndColumns("Transactions", new Dictionary<string, string>
                    {
                        { "TransactionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "CustomerID", "INT" },
                        { "Date", "DATETIME" },
                        { "Description", "NVARCHAR(255) NULL" },
                        { "Amount", "DECIMAL(18,2)" },
                        { "Period", "INT NULL" },
                        { "Type", "NVARCHAR(50)" },
                        { "IsDeleted", "BIT DEFAULT 0" }
                    }, conn);

                    // EDefterTakip tablosu - MSSQL uyumlu
                    EnsureTableAndColumns("EDefterTakip", new Dictionary<string, string>
                    {
                        { "TransactionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "CustomerID", "INT" },
                        { "Date", "DATETIME" },
                        { "Kontor", "DECIMAL(18,2)" },
                        { "Type", "NVARCHAR(255) NOT NULL" }
                    }, conn);

                    // Suggestions tablosu - MSSQL uyumlu
                    EnsureTableAndColumns("Suggestions", new Dictionary<string, string>
                    {
                        { "SuggestionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "Description", "NVARCHAR(255) NOT NULL UNIQUE" },
                        { "CreatedDate", "DATETIME DEFAULT GETDATE()" }
                    }, conn);

                    // ExpenseCategories tablosu
                    EnsureTableAndColumns("ExpenseCategories", new Dictionary<string, string>
                    {
                        { "CategoryID", "INT PRIMARY KEY IDENTITY(1,1)" },
                        { "Label", "NVARCHAR(255) NOT NULL" },
                        { "Info", "NVARCHAR(255) NOT NULL" }
                    }, conn);

                    // ExpenseCategories tablosunu JSON dosyasından doldur
                    if (!TableHasData("ExpenseCategories", conn))
                    {
                        string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expense_categories.json");
                        if (File.Exists(jsonFilePath))
                        {
                            string jsonContent = File.ReadAllText(jsonFilePath);
                            var categories = JsonSerializer.Deserialize<List<ExpenseCategory>>(jsonContent);

                            using (var cmd = new SqlCommand())
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
                        { "MatchingID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "ItemName", "NVARCHAR(255) NOT NULL" },
                        { "SubRecordType", "NVARCHAR(255) NOT NULL" }
                    }, conn);

                    // Periods table
                    EnsureTableAndColumns("Periods", new Dictionary<string, string>
                    {
                        { "PeriodYear", "INT PRIMARY KEY" },
                        { "DisplayName", "NVARCHAR(255) NULL" }
                    }, conn);

                    // Templates tablosu
                    EnsureTableAndColumns("Templates", new Dictionary<string, string>
                    {
                        { "TemplateID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "Name", "NVARCHAR(255) NOT NULL" },
                        { "IsBuiltIn", "BIT DEFAULT 0" },
                        { "DefinitionJson", "NVARCHAR(MAX) NOT NULL" },
                        { "CreatedAt", "DATETIME DEFAULT GETDATE()" }
                    }, conn);

                    // TemplateColumns tablosu (opsiyonel)
                    EnsureTableAndColumns("TemplateColumns", new Dictionary<string, string>
                    {
                        { "TemplateColumnID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "TemplateID", "INT NOT NULL" },
                        { "ColumnName", "NVARCHAR(255) NOT NULL" },
                        { "DataKey", "NVARCHAR(255) NOT NULL" },
                        { "DisplayOrder", "INT DEFAULT 0" }
                    }, conn);

                    // FaturaFilters tablosu - Özel şablonlar için filtre kuralları
                    EnsureTableAndColumns("FaturaFilters", new Dictionary<string, string>
{
                        { "FilterID", "INT IDENTITY(1,1) PRIMARY KEY" },
                        { "TemplateID", "INT NOT NULL" },
                        { "ItemName", "NVARCHAR(255) NOT NULL" },
                        { "TaxRate", "DECIMAL(5,2) DEFAULT 0" },
                        { "OutputColumnPrefix", "NVARCHAR(50)" },
                        { "DisplayOrder", "INT DEFAULT 0" },
                        { "IsActive", "BIT DEFAULT 1" },
                        { "CreatedAt", "DATETIME DEFAULT GETDATE()" }
                    }, conn);

                    // Foreign Key (MSSQL için ayrı ekle)
                    using (var cmd = new SqlCommand(@"
                        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
                                       WHERE CONSTRAINT_NAME = 'FK_FaturaFilters_Templates')
                        BEGIN
                            ALTER TABLE FaturaFilters 
                            ADD CONSTRAINT FK_FaturaFilters_Templates 
                            FOREIGN KEY (TemplateID) REFERENCES Templates(TemplateID) ON DELETE CASCADE
                        END
                    ", conn))
                    {
                        try { cmd.ExecuteNonQuery(); } catch { }
                    }

                    // Varsayılan şablonlar
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

            catch (SqlException ex) when (ex.Number == 4060)
            {
                // 4060 = Cannot open database requested by the login. The login failed.
                Logger.Log($"MsSql InitializeDatabase: Cannot open database after creation/open attempt: {ex.Message} (Number {ex.Number}");
                // If we just created the database but the login cannot open it (common when Windows auth and user mapping not set),
                // attempt to initialize schema using master connection and fully-qualified object names as a fallback.
                if (createdNow)
                {
                    try
                    {
                        var builder = new SqlConnectionStringBuilder(_connectionString);
                        string databaseName = builder.InitialCatalog;
                        Logger.Log($"MsSql InitializeDatabase: attempting master-connection fallback initialization for DB '{databaseName}'");
                        InitializeDatabaseUsingMaster(databaseName);
                        return;
                    }
                    catch (Exception inner)
                    {
                        Logger.Log($"MsSql InitializeDatabase (fallback) failed: {inner.Message}");
                        throw; // rethrow so caller sees failure
                    }
                }

                throw; // not created now and cannot open -> rethrow
            }



        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public DataTable GetCustomers()
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT CustomerID,Name,EDefter,Taxid,ActivityCode,IsDeleted FROM Customers WHERE IsDeleted = 0", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddCustomer(string name, bool edefter, string taxid = null, string activitycode = null)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL AddCustomer hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateCustomer(int customerId, string newName, bool edefter, string taxid = null, string activitycode = null, bool deleted = false)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL UpdateCustomer hatası: {ex.Message}");
                return false;
            }
        }


        public bool DeleteCustomer(int customerId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
                    "UPDATE Customers SET IsDeleted = 1 WHERE CustomerID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL DeleteCustomer hatası: {ex.Message}");
                return false;
            }
        }

        public DataTable GetDeletedCustomers()
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT CustomerID, Name, EDefter, Taxid, ActivityCode, IsDeleted FROM Customers WHERE IsDeleted = 1", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public DataTable GetTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(
                "SELECT TransactionID, Date, Description, Amount, Type, Period FROM Transactions WHERE CUSTOMERID = @customerID AND IsDeleted = 0 ORDER BY Date ASC",
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
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL AddTransaction hatası: {ex.Message}");
                return false;
            }
         }

        public bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type, int? period = null)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL UpdateTransaction hatası: {ex.Message}");
                return false;
            }
         }

        public bool DeleteTransaction(int transactionId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL DeleteTransaction hatası: {ex.Message}");
                return false;
            }
        }

        // Suggestions işlemleri
        public List<string> GetSuggestions()
        {
            var suggestions = new List<string>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT Description FROM Suggestions", conn))
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
                System.Diagnostics.Debug.WriteLine($"MSSQL GetSuggestions hatası: {ex.Message}");
            }
            return suggestions;
        }

        public bool AddSuggestion(string description)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("INSERT INTO Suggestions (Description) VALUES (@desc)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@desc", description.Trim());
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL AddSuggestion hatası: {ex.Message}");
                return false;
            }
        }

        public bool RemoveSuggestion(string description)
        {
            try
            {
                Debug.WriteLine($"MSSQL RemoveSuggestion called for: {description}");

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    Debug.WriteLine("MSSQL connection opened successfully in RemoveSuggestion");

                    using (var cmd = new SqlCommand("DELETE FROM Suggestions WHERE Description = @desc", conn))
                    {
                        cmd.Parameters.AddWithValue("@desc", description);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        Debug.WriteLine($"MSSQL RemoveSuggestion - Rows affected: {rowsAffected}");

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MSSQL RemoveSuggestion Genel Hatası: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // --- E-Defter methods (restored) ---
        public DataTable GetEDefterTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(
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
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL AddEDefterTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteEDefterTransaction(int transactionId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL DeleteEDefterTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public decimal CalculateEDefterTotal(int customerId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL CalculateEDefterTotal hatası: {ex.Message}");
                return 0;
            }
        }

        public bool BulkUpdateEDefterTransactions(List<EDefterTransaction> transactions)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var trans in transactions)
                            {
                                using (var cmd = new SqlCommand(
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
                            System.Diagnostics.Debug.WriteLine($"MSSQL BulkUpdateEDefterTransactions hatası: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL BulkUpdateEDefterTransactions hatası: {ex.Message}");
                return false;
            }
        }

        public decimal CalculateTotalBalance(int customerId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL CalculateTotalBalance hatası: {ex.Message}");
                return 0;
            }
        }

        public void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                EnsureTableAndColumns(tableName, columns, conn);
            }
        }

        private void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns, SqlConnection conn)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                // Tablo var mı kontrolü - MSSQL syntax
                cmd.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                var exists = (int)cmd.ExecuteScalar() > 0;

                if (!exists)
                {
                    var columnsDef = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                    cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDef})";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Kolon kontrolü - MSSQL syntax
                    cmd.CommandText = $@"
                        SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = '{tableName}'";

                    var reader = cmd.ExecuteReader();
                    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        existingColumns.Add(reader["COLUMN_NAME"].ToString());
                    }
                    reader.Close();

                    foreach (var kv in columns)
                    {
                        if (!existingColumns.Contains(kv.Key))
                        {
                            // MSSQL'de ALTER TABLE ADD COLUMN
                            cmd.CommandText = $"ALTER TABLE {tableName} ADD {kv.Key} {kv.Value}";
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"MSSQL kolon ekleme hatası: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        // Fallback initialization using master connection and fully-qualified object names
        private void InitializeDatabaseUsingMaster(string databaseName)
        {
            var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };

            using (var conn = new SqlConnection(masterBuilder.ConnectionString))
            {
                conn.Open();

                // Customers
                EnsureTableAndColumnsMaster("Customers", new Dictionary<string, string>
                {
                    { "CustomerID", "INT IDENTITY(1,1) PRIMARY KEY" },
                    { "Name", "NVARCHAR(255) NOT NULL" },
                    { "EDefter", "INT DEFAULT 0" },
                    { "Taxid","NVARCHAR(11) DEFAULT NULL" },
                    { "ActivityCode","NVARCHAR(6) DEFAULT NULL" }
                }, conn, databaseName);

                // Transactions
                EnsureTableAndColumnsMaster("Transactions", new Dictionary<string, string>
                {
                    { "TransactionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                    { "CustomerID", "INT" },
                    { "Date", "DATETIME" },
                    { "Description", "NVARCHAR(255) NULL" },
                    { "Amount", "DECIMAL(18,2)" },
                    { "Period", "INT NULL" },
                    { "Type", "NVARCHAR(50)" },
                    { "IsDeleted", "BIT DEFAULT 0" }
                }, conn, databaseName);

                // EDefterTakip
                EnsureTableAndColumnsMaster("EDefterTakip", new Dictionary<string, string>
                {
                    { "TransactionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                    { "CustomerID", "INT" },
                    { "Date", "DATETIME" },
                    { "Kontor", "DECIMAL(18,2)" },
                    { "Type", "NVARCHAR(255) NOT NULL" }
                }, conn, databaseName);

                // Suggestions
                EnsureTableAndColumnsMaster("Suggestions", new Dictionary<string, string>
                {
                    { "SuggestionID", "INT IDENTITY(1,1) PRIMARY KEY" },
                    { "Description", "NVARCHAR(255) NOT NULL UNIQUE" },
                    { "CreatedDate", "DATETIME DEFAULT GETDATE()" }
                }, conn, databaseName);

                // ExpenseCategories
                EnsureTableAndColumnsMaster("ExpenseCategories", new Dictionary<string, string>
                {
                    { "CategoryID", "INT PRIMARY KEY IDENTITY(1,1)" },
                    { "Label", "NVARCHAR(255) NOT NULL" },
                    { "Info", "NVARCHAR(255) NOT NULL" }
                }, conn, databaseName);

                // Fill ExpenseCategories from JSON if empty
                if (!TableHasDataMaster("ExpenseCategories", conn, databaseName))
                {
                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "expense_categories.json");
                    if (File.Exists(jsonFilePath))
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        var categories = JsonSerializer.Deserialize<List<ExpenseCategory>>(jsonContent);

                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = conn;
                            foreach (var category in categories)
                            {
                                cmd.CommandText = $"INSERT INTO [{databaseName}].dbo.ExpenseCategories (Label, Info) VALUES (@label, @info)";
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

                // ExpenseMatching
                EnsureTableAndColumnsMaster("ExpenseMatching", new Dictionary<string, string>
                {
                    { "MatchingID", "INT IDENTITY(1,1) PRIMARY KEY" },
                    { "ItemName", "NVARCHAR(255) NOT NULL" },
                    { "SubRecordType", "NVARCHAR(255) NOT NULL" }
                }, conn, databaseName);
            }
        }

        private void EnsureTableAndColumnsMaster(string tableName, Dictionary<string, string> columns, SqlConnection masterConn, string databaseName)
        {
            using (var cmd = masterConn.CreateCommand())
            {
                // Tablo var mı kontrolü
                cmd.CommandText = $"SELECT COUNT(*) FROM [{databaseName}].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo'";
                var exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                if (!exists)
                {
                    var columnsDef = string.Join(", ", columns.Select(kv => $"{kv.Key} {kv.Value}"));
                    cmd.CommandText = $"CREATE TABLE [{databaseName}].dbo.[{tableName}] ({columnsDef})";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = $"SELECT COLUMN_NAME FROM [{databaseName}].INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo'";
                    var reader = cmd.ExecuteReader();
                    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        existingColumns.Add(reader["COLUMN_NAME"].ToString());
                    }
                    reader.Close();

                    foreach (var kv in columns)
                    {
                        if (!existingColumns.Contains(kv.Key))
                        {
                            cmd.CommandText = $"ALTER TABLE [{databaseName}].dbo.[{tableName}] ADD {kv.Key} {kv.Value}";
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"MSSQL master kolon ekleme hatası: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private bool TableHasData(string tableName, SqlConnection conn)
        {
            using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", conn))
            {
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool TableHasDataMaster(string tableName, SqlConnection masterConn, string databaseName)
        {
            using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{databaseName}].dbo.[{tableName}]", masterConn))
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
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT CategoryID, Label, Info FROM ExpenseCategories", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
        public DataTable GetPeriods()
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT PeriodYear, DisplayName FROM Periods ORDER BY PeriodYear DESC", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }

        public bool AddPeriod(int periodYear, string displayName = null)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("INSERT INTO Periods (PeriodYear, DisplayName) VALUES (@year, @disp)", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@year", periodYear);
                    cmd.Parameters.AddWithValue("@disp", string.IsNullOrEmpty(displayName) ? (object)DBNull.Value : displayName);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL AddPeriod hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteExpenseMatching(string itemName)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT ItemName, SubRecordType FROM ExpenseMatching", conn))
            {
                adapter.Fill(dt);
            }
            return dt;
        }
        public bool AddExpenseMatching(string itemName, string subRecordType)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
            using (var conn = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(
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
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL AddFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateFilter(int filterId, string itemName, decimal taxRate, string outputColumnPrefix, int displayOrder)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL UpdateFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool DeleteFilter(int filterId)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("DELETE FROM FaturaFilters WHERE FilterID = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", filterId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MSSQL DeleteFilter hatası: {ex.Message}");
                return false;
            }
        }

        public bool ToggleFilterActive(int filterId, bool isActive)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("UPDATE FaturaFilters SET IsActive = @active WHERE FilterID = @id", conn))
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
                System.Diagnostics.Debug.WriteLine($"MSSQL ToggleFilterActive hatası: {ex.Message}");
                return false;
            }
        }

        public bool ReorderFilters(int templateId, Dictionary<int, int> filterIdToOrder)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var kvp in filterIdToOrder)
                            {
                                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL ReorderFilters hatası: {ex.Message}");
                return false;
            }
        }
    }
}