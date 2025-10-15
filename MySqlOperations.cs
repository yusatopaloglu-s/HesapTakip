using MySql.Data.MySqlClient;
using System.Data;

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

        public void InitializeDatabase()
        {
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
                    { "ActivityCode","VARCHAR(6) DEFAULT NULL" }
                }, conn);

                // Transactions tablosu  
                EnsureTableAndColumns("Transactions", new Dictionary<string, string>
                {
                    { "TransactionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                    { "CustomerID", "INT" },
                    { "Date", "DATETIME" },
                    { "Description", "VARCHAR(255) NULL" },
                    { "Amount", "DECIMAL(18,2)" },
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
            using (var adapter = new MySqlDataAdapter("SELECT * FROM Customers", conn))
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

        public bool UpdateCustomer(int customerId, string newName, bool edefter, string taxid = null, string activitycode = null)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = transaction;

                            // Önce Transactions tablosundan sil
                            cmd.CommandText = "DELETE FROM Transactions WHERE CustomerID = @id";
                            cmd.Parameters.AddWithValue("@id", customerId);
                            cmd.ExecuteNonQuery();

                            // Sonra Customers tablosundan sil
                            cmd.CommandText = "DELETE FROM Customers WHERE CustomerID = @id";
                            cmd.ExecuteNonQuery();

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

        public DataTable GetTransactions(int customerId)
        {
            var dt = new DataTable();
            using (var conn = new MySqlConnection(_connectionString))
            using (var adapter = new MySqlDataAdapter(
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
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
            catch
            {
                return false;
            }
        }

        public bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = new MySqlCommand(
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
    }
}