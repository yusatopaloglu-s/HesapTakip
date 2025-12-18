namespace HesapTakip
{
    public partial class ConnectionSettingsForm : Form
    {
        public string Server { get; private set; }
        public string Database { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }
        public string Port { get; private set; }
        public string DatabaseType
        {
            get
            {
                string dbType = cmbDatabaseType.SelectedItem?.ToString()?.Trim() ?? "SQLite";
                string guaranteedType = GuaranteeDatabaseType(dbType);
                Logger.Log($"ConnectionSettingsForm DatabaseType GET: '{dbType}' -> '{guaranteedType}'");
                return guaranteedType;
            }
            private set { }
        }

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
                return dbType;
        }
        public string SqliteFilePath { get; private set; }

        public ConnectionSettingsForm()
        {
            InitializeComponent();
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Font = new Font("Segoe UI", 9F);
            }

            LoadCurrentSettings();
            UpdateFormByDatabaseType();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Mevcut ayarları yükle
                string currentType = AppConfigHelper.DatabaseType;

                if (!string.IsNullOrEmpty(currentType))
                {
                    cmbDatabaseType.SelectedItem = currentType;
                }
                else
                {
                    cmbDatabaseType.SelectedIndex = 0; // Varsayılan SQLite
                }

                if (currentType == "SQLite")
                {
                    string dataSource = AppConfigHelper.GetDataSourceFromConnectionString();
                    if (!string.IsNullOrEmpty(dataSource))
                    {
                        txtSqliteFilePath.Text = dataSource;
                    }
                }
                else
                {
                    txtServer.Text = AppConfigHelper.GetServerFromConnectionString();
                    txtDatabase.Text = AppConfigHelper.GetDatabaseFromConnectionString();
                    txtUser.Text = AppConfigHelper.GetUserFromConnectionString();
                    txtPort.Text = AppConfigHelper.GetPortFromConnectionString();
                    // Password security nedeniyle Şifre yüklenmez
                    // Windows auth kontrolü
                    chkUseWindowsAuth.Checked = AppConfigHelper.IsWindowsAuthEnabled;
                    if (chkUseWindowsAuth.Checked)
                    {
                        txtUser.Text = "";
                        txtPassword.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Ayarlar yüklenirken hata: {ex.Message}");
            }
        }

        private void UpdateFormByDatabaseType()
        {
            string selectedType = cmbDatabaseType.SelectedItem?.ToString() ?? "SQLite";

            bool isSQLite = selectedType == "SQLite";
            bool isMySQL = selectedType == "MySQL";
            bool isMSSQL = selectedType == "MSSQL";

            // SQLite kontrollerini göster/gizle
            lblSqliteFilePath.Visible = isSQLite;
            txtSqliteFilePath.Visible = isSQLite;
            btnBrowseSqlite.Visible = isSQLite;

            // MySQL/MSSQL kontrollerini göster/gizle
            lblServer.Visible = !isSQLite;
            txtServer.Visible = !isSQLite;
            lblDatabase.Visible = !isSQLite;
            txtDatabase.Visible = !isSQLite;
            lblUser.Visible = !isSQLite;
            txtUser.Visible = !isSQLite;
            lblPassword.Visible = !isSQLite;
            txtPassword.Visible = !isSQLite;
            lblPort.Visible = !isSQLite;
            txtPort.Visible = !isSQLite;
            btnTestConnection.Visible = !isSQLite;
            btn_crdb_sqlite.Visible = isSQLite; // Sadece SQLite seçiliyse göster
            btn_crdb_mysql.Visible = isMySQL; // Sadece MySQL seçiliyse göster
            btn_crdb_mssql.Visible = isMSSQL; // Sadece MSSQL seçiliyse göster
            // Windows Kimlik Doğrulaması Checkbox'ı sadece MSSQL için göster
            chkUseWindowsAuth.Visible = isMSSQL;
            if (isMSSQL)
            {
                ChkUseWindowsAuth_CheckedChanged(null, null); // Alanları güncelle
            }

            // Port varsayılan değerleri -
            if (isMySQL)
            {
                if (string.IsNullOrEmpty(txtPort.Text) || txtPort.Text == "1433")
                    txtPort.Text = "3306";
            }
            else if (isMSSQL)
            {
                if (string.IsNullOrEmpty(txtPort.Text) || txtPort.Text == "3306")
                    txtPort.Text = "1433";
            }

            // SQLite için varsayılan dosya yolu
            if (isSQLite && string.IsNullOrEmpty(txtSqliteFilePath.Text))
            {
                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "HesapTakip", "hesaptakip.sqlite");
                txtSqliteFilePath.Text = defaultPath;
            }


        }

        private void cmbDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFormByDatabaseType();
        }

        private void btnBrowseSqlite_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "SQLite Database|*.sqlite|All Files|*.*";
                sfd.Title = "SQLite Database Dosyasını Seçin";
                sfd.DefaultExt = "sqlite";
                sfd.AddExtension = true;

                if (!string.IsNullOrEmpty(txtSqliteFilePath.Text))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(txtSqliteFilePath.Text);
                    sfd.FileName = Path.GetFileName(txtSqliteFilePath.Text);
                }
                else
                {
                    sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtSqliteFilePath.Text = sfd.FileName;
                }
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            string databaseType = cmbDatabaseType.SelectedItem.ToString();
            string connectionString = BuildConnectionString();

            try
            {
                Cursor = Cursors.WaitCursor;
                btnTestConnection.Enabled = false;

                bool testResult = false;

                if (databaseType == "MySQL")
                {
                    var db = new MySqlOperations(connectionString);
                    testResult = db.TestConnection();
                }
                else if (databaseType == "MSSQL")
                {
                    var db = new MsSqlOperations(connectionString);
                    testResult = db.TestConnection();
                }

                if (testResult)
                {
                    MessageBox.Show("Bağlantı testi başarılı!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Bağlantı testi başarısız!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı testi sırasında hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTestConnection.Enabled = true;
            }
        }
        private void ChkUseWindowsAuth_CheckedChanged(object sender, EventArgs e)
        {
            bool useWindowsAuth = chkUseWindowsAuth.Checked;
            // Kullanıcı adı ve şifre alanlarını etkinleştir/devre dışı bırak
            lblUser.Visible = !useWindowsAuth;
            txtUser.Visible = !useWindowsAuth;
            lblPassword.Visible = !useWindowsAuth;
            txtPassword.Visible = !useWindowsAuth;
        }

        private bool ValidateInputs()
        {
            string databaseType = cmbDatabaseType.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(databaseType))
            {
                MessageBox.Show("Lütfen veritabanı tipi seçin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (databaseType == "SQLite")
            {
                if (string.IsNullOrEmpty(txtSqliteFilePath.Text))
                {
                    MessageBox.Show("Lütfen SQLite dosya yolunu seçin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(txtSqliteFilePath.Text);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        // Dizin oluşturulabilir mi kontrol et
                        Directory.CreateDirectory(directory);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Geçersiz dosya yolu: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtServer.Text))
                {
                    MessageBox.Show("Lütfen sunucu adresini girin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtDatabase.Text))
                {
                    MessageBox.Show("Lütfen veritabanı adını girin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtPort.Text))
                {
                    MessageBox.Show("Lütfen port numarasını girin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (databaseType == "MSSQL" && chkUseWindowsAuth.Checked)
                {
                    // Windows auth için kullanıcı adı ve şifre zorunlu değil
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(txtUser.Text))
                    {
                        MessageBox.Show("Lütfen kullanıcı adını girin!", "Uyarı",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }

            return true;
        }
        private string BuildConnectionString()
        {
            string databaseType = cmbDatabaseType.SelectedItem.ToString();
            Logger.Log($"BuildConnectionString for: {databaseType}");
            Logger.Log($"chkUseWindowsAuth.Checked: {chkUseWindowsAuth.Checked}");

            if (databaseType == "SQLite")
            {
                string connectionString = $"Data Source={txtSqliteFilePath.Text};";
                Logger.Log($"SQLite Connection String: {connectionString}");
                return connectionString;
            }
            else if (databaseType == "MySQL")
            {
                string connectionString = $"Server={txtServer.Text};Database={txtDatabase.Text};User={txtUser.Text};Password={txtPassword.Text};Port={txtPort.Text};Charset=utf8mb4;";
                Logger.Log($"MySQL Connection String: {connectionString}");
                return connectionString;
            }
            else // MSSQL
            {
                string connectionString;
                bool windowsAuth = chkUseWindowsAuth.Checked && databaseType == "MSSQL";
                Logger.Log($"MSSQL Windows Auth durumu: {windowsAuth}");

                if (windowsAuth)
                {
                    connectionString = $"Server={txtServer.Text},{txtPort.Text};Database={txtDatabase.Text};Integrated Security=true;";
                }
                else
                {
                    connectionString = $"Server={txtServer.Text},{txtPort.Text};Database={txtDatabase.Text};User Id={txtUser.Text};Password={txtPassword.Text};";
                }
                Logger.Log($"MSSQL Connection String: {connectionString}");
                return connectionString;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                // Değerleri property'lere ata
                DatabaseType = cmbDatabaseType.SelectedItem.ToString();
                Server = txtServer.Text;
                Database = txtDatabase.Text;
                User = txtUser.Text;
                Password = txtPassword.Text;
                Port = txtPort.Text;
                SqliteFilePath = txtSqliteFilePath.Text;

                // MSSQL için Windows auth ayarını kaydet
                if (DatabaseType == "MSSQL")
                {
                    AppConfigHelper.IsWindowsAuthEnabled = chkUseWindowsAuth.Checked;
                    if (chkUseWindowsAuth.Checked)
                    {
                        User = "";
                        Password = "";
                    }
                    Logger.Log($"MSSQL Windows Auth: {chkUseWindowsAuth.Checked}");
                }

                // SQLite için dosya dizinini oluştur
                if (DatabaseType == "SQLite")
                {
                    try
                    {
                        string directory = Path.GetDirectoryName(SqliteFilePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"SQLite dosya dizini oluşturulamadı: {ex.Message}", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // BAğLANTI DıZESıNı OLUşTUR VE TEST ET
                string connectionString = BuildConnectionString();
                Logger.Log($"Kaydetmeden önce connection string: {connectionString}");

                // Test bağlantısını yap (SQLite için test ekledim)
                bool testResult = false;
                if (DatabaseType == "SQLite")
                {
                    var db = new SqliteOperations(connectionString);
                    testResult = db.TestConnection();
                }
                else if (DatabaseType == "MySQL")
                {
                    var db = new MySqlOperations(connectionString);
                    testResult = db.TestConnection();
                }
                else if (DatabaseType == "MSSQL")
                {
                    var db = new MsSqlOperations(connectionString);
                    testResult = db.TestConnection();
                }

                if (!testResult)
                {
                    MessageBox.Show($"Bağlantı testi başarısız! Connection String: {connectionString}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // AYARLARI KAYDET
                AppConfigHelper.SaveConnectionString(Server, Database, User, Password, Port, DatabaseType, SqliteFilePath, chkUseWindowsAuth.Checked && DatabaseType == "MSSQL");
                Logger.Log("Kullanıcı ayarları kaydedildi...");

                // DialogResult'ı OK yap ve formu kapat
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken hata: {ex.Message}\nStack Trace: {ex.StackTrace}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void btn_crdb_sqlite_Click(object sender, EventArgs e)
        {
            string filePath = txtSqliteFilePath.Text?.Trim();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("Lütfen bir dosya yolu girin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string fullPath = Path.GetFullPath(filePath);
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(fullPath))
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(fullPath);
                    Logger.Log($"SQLite file created: {fullPath}");
                }
                else
                {
                    Logger.Log($"SQLite file already exists: {fullPath}");
                }

                // Normalize textbox to full path so saving uses the same path
                txtSqliteFilePath.Text = fullPath;

                // Build a safe connection string and try to open it to surface errors early
                string connStr = $"Data Source={fullPath};Version=3;FailIfMissing=False;Pooling=True;";
                Logger.Log($"Attempting to open SQLite connection: {connStr}");

                using (var conn = new System.Data.SQLite.SQLiteConnection(connStr))
                {
                    conn.Open();
                    Logger.Log("Opened SQLite connection after create.");
                    using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT sqlite_version()", conn))
                    {
                        var version = cmd.ExecuteScalar();
                        Logger.Log($"SQLite version: {version}");
                    }
                }

                // Initialize schema (creates tables if missing)
                var db = new SqliteOperations(connStr);
                db.InitializeDatabase();

                MessageBox.Show("SQLite veritabanı oluşturuldu ve initialize edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Data.SQLite.SQLiteException sqlEx)
            {
                Logger.Log($"SQLite hata: {sqlEx.Message} Code: {sqlEx.ErrorCode}");
                MessageBox.Show($"SQLite hatası: {sqlEx.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Logger.Log($"Veritabanı oluşturulamadı: {ex.Message}");
                MessageBox.Show($"Veritabanı oluşturulamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Yeni: MySQL veritabanı oluşturma butonu handler
        private void btn_crdb_mysql_Click(object sender, EventArgs e)
        {
            // Validate inputs first
            if (!ValidateInputs())
                return;

            string connectionString = BuildConnectionString();

            try
            {
                Cursor = Cursors.WaitCursor;
                btn_crdb_mysql.Enabled = false;

                var db = new MySqlOperations(connectionString);
                db.InitializeDatabase();

                MessageBox.Show("MySQL veritabanı oluşturuldu ve initialize edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Log($"MySQL veritabanı oluşturulamadı: {ex.Message}");
                MessageBox.Show($"MySQL veritabanı oluşturulamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btn_crdb_mysql.Enabled = true;
            }
        }

        // Yeni: MSSQL veritabanı oluşturma butonu handler
        private void btn_crdb_mssql_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            string connectionString = BuildConnectionString();

            try
            {
                Cursor = Cursors.WaitCursor;
                btn_crdb_mssql.Enabled = false;

                var db = new MsSqlOperations(connectionString);
                db.InitializeDatabase();

                MessageBox.Show("MSSQL veritabanı oluşturuldu ve initialize edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Log($"MSSQL veritabanı oluşturulamadı: {ex.Message}");
                MessageBox.Show($"MSSQL veritabanı oluşturulamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btn_crdb_mssql.Enabled = true;
            }
        }
    }
}
