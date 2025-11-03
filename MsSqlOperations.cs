using System.Data;
using System.Data.SqlClient;
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
                }
            }
            catch (SqlException ex) when (ex.Number == 4060)
            {
                // 4060 = Cannot open database requested by the login. The login failed.
                Logger.Log($"MsSql InitializeDatabase: Cannot open database after creation/open attempt: {ex.Message} (Number {ex.Number})");

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

        /*  public bool DeleteCustomer(int customerId)
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
                              // Önce Transactions tablosundan sil
                              using (var cmd1 = new SqlCommand("DELETE FROM Transactions WHERE CustomerID = @id", conn, transaction))
                              {
                                  cmd1.Parameters.AddWithValue("@id", customerId);
                                  cmd1.ExecuteNonQuery();
                              }

                              // Sonra Customers tablosundan sil
                              using (var cmd2 = new SqlCommand("DELETE FROM Customers WHERE CustomerID = @id", conn, transaction))
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
                  System.Diagnostics.Debug.WriteLine($"MSSQL DeleteCustomer hatası: {ex.Message}");
                  return false;
              }
          }
          */

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
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
                System.Diagnostics.Debug.WriteLine($"MSSQL AddTransaction hatası: {ex.Message}");
                return false;
            }
        }

        public bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(
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
    }
}