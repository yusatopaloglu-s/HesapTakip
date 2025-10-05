using System.Diagnostics;

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
                Debug.WriteLine($"ConnectionSettingsForm DatabaseType GET: '{dbType}' -> '{guaranteedType}'");
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
            LoadCurrentSettings();
            UpdateFormByDatabaseType();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Mevcut ayarlarý yükle
                string currentType = AppConfigHelper.DatabaseType;

                if (!string.IsNullOrEmpty(currentType))
                {
                    cmbDatabaseType.SelectedItem = currentType;
                }
                else
                {
                    cmbDatabaseType.SelectedIndex = 0; // Varsayýlan SQLite
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
                    // Password security nedeniyle þifre yüklenmez
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ayarlar yüklenirken hata: {ex.Message}");
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

            // Port varsayýlan deðerleri
            if (isMySQL && string.IsNullOrEmpty(txtPort.Text))
                txtPort.Text = "3306";
            else if (isMSSQL && string.IsNullOrEmpty(txtPort.Text))
                txtPort.Text = "1433";

            // SQLite için varsayýlan dosya yolu
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
                sfd.Title = "SQLite Database Dosyasýný Seçin";
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
                    MessageBox.Show("Baðlantý testi baþarýlý!", "Baþarýlý",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Baðlantý testi baþarýsýz!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Baðlantý testi sýrasýnda hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTestConnection.Enabled = true;
            }
        }

        private bool ValidateInputs()
        {
            string databaseType = cmbDatabaseType.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(databaseType))
            {
                MessageBox.Show("Lütfen veritabaný tipi seçin!", "Uyarý",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (databaseType == "SQLite")
            {
                if (string.IsNullOrEmpty(txtSqliteFilePath.Text))
                {
                    MessageBox.Show("Lütfen SQLite dosya yolunu seçin!", "Uyarý",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(txtSqliteFilePath.Text);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        // Dizin oluþturulabilir mi kontrol et
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
                    MessageBox.Show("Lütfen sunucu adresini girin!", "Uyarý",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtDatabase.Text))
                {
                    MessageBox.Show("Lütfen veritabaný adýný girin!", "Uyarý",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtUser.Text))
                {
                    MessageBox.Show("Lütfen kullanýcý adýný girin!", "Uyarý",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtPort.Text))
                {
                    MessageBox.Show("Lütfen port numarasýný girin!", "Uyarý",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private string BuildConnectionString()
        {
            string databaseType = cmbDatabaseType.SelectedItem.ToString();
            Debug.WriteLine($"BuildConnectionString for: {databaseType}");

            if (databaseType == "SQLite")
            {
                string connectionString = $"Data Source={txtSqliteFilePath.Text};";
                Debug.WriteLine($"SQLite Connection String: {connectionString}");
                return connectionString;
            }
            else if (databaseType == "MySQL")
            {
                string connectionString = $"Server={txtServer.Text};Database={txtDatabase.Text};User={txtUser.Text};Password={txtPassword.Text};Port={txtPort.Text};Charset=utf8mb4;";
                Debug.WriteLine($"MySQL Connection String: {connectionString}");
                return connectionString;
            }
            else // MSSQL
            {
                string connectionString = $"Server={txtServer.Text},{txtPort.Text};Database={txtDatabase.Text};User={txtUser.Text};Password={txtPassword.Text};";
                Debug.WriteLine($"MSSQL Connection String: {connectionString}");
                return connectionString;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                // Deðerleri property'lere ata
                DatabaseType = cmbDatabaseType.SelectedItem.ToString();
                Server = txtServer.Text;
                Database = txtDatabase.Text;
                User = txtUser.Text;
                Password = txtPassword.Text;
                Port = txtPort.Text;
                SqliteFilePath = txtSqliteFilePath.Text;

                // SQLite için dosya dizinini oluþtur
                if (DatabaseType == "SQLite")
                {
                    try
                    {
                        string directory = Path.GetDirectoryName(SqliteFilePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                            Debug.WriteLine($"SQLite dizin oluþturuldu: {directory}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"SQLite dosya dizini oluþturulamadý: {ex.Message}", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // DialogResult'ý OK yap ve formu kapat
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
    partial class ConnectionSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblDatabaseType = new Label();
            this.cmbDatabaseType = new ComboBox();
            this.lblSqliteFilePath = new Label();
            this.txtSqliteFilePath = new TextBox();
            this.btnBrowseSqlite = new Button();
            this.lblServer = new Label();
            this.txtServer = new TextBox();
            this.lblDatabase = new Label();
            this.txtDatabase = new TextBox();
            this.lblUser = new Label();
            this.txtUser = new TextBox();
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.lblPort = new Label();
            this.txtPort = new TextBox();
            this.btnTestConnection = new Button();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // 
            // lblDatabaseType
            // 
            this.lblDatabaseType.AutoSize = true;
            this.lblDatabaseType.Location = new System.Drawing.Point(20, 20);
            this.lblDatabaseType.Name = "lblDatabaseType";
            this.lblDatabaseType.Size = new System.Drawing.Size(82, 13);
            this.lblDatabaseType.Text = "Veritabaný Tipi:";
            // 
            // cmbDatabaseType
            // 
            this.cmbDatabaseType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDatabaseType.FormattingEnabled = true;
            this.cmbDatabaseType.Items.AddRange(new object[] { "SQLite", "MySQL", "MSSQL" });
            this.cmbDatabaseType.Location = new System.Drawing.Point(120, 17);
            this.cmbDatabaseType.Name = "cmbDatabaseType";
            this.cmbDatabaseType.Size = new System.Drawing.Size(200, 21);
            this.cmbDatabaseType.TabIndex = 0;
            this.cmbDatabaseType.SelectedIndexChanged += new EventHandler(this.cmbDatabaseType_SelectedIndexChanged);
            // 
            // lblSqliteFilePath
            // 
            this.lblSqliteFilePath.AutoSize = true;
            this.lblSqliteFilePath.Location = new System.Drawing.Point(20, 50);
            this.lblSqliteFilePath.Name = "lblSqliteFilePath";
            this.lblSqliteFilePath.Size = new System.Drawing.Size(94, 13);
            this.lblSqliteFilePath.Text = "SQLite Dosya Yolu:";
            // 
            // txtSqliteFilePath
            // 
            this.txtSqliteFilePath.Location = new System.Drawing.Point(120, 47);
            this.txtSqliteFilePath.Name = "txtSqliteFilePath";
            this.txtSqliteFilePath.Size = new System.Drawing.Size(250, 20);
            this.txtSqliteFilePath.TabIndex = 1;
            // 
            // btnBrowseSqlite
            // 
            this.btnBrowseSqlite.Location = new System.Drawing.Point(380, 45);
            this.btnBrowseSqlite.Name = "btnBrowseSqlite";
            this.btnBrowseSqlite.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSqlite.TabIndex = 2;
            this.btnBrowseSqlite.Text = "Gözat";
            this.btnBrowseSqlite.UseVisualStyleBackColor = true;
            this.btnBrowseSqlite.Click += new EventHandler(this.btnBrowseSqlite_Click);
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(20, 50);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(44, 13);
            this.lblServer.Text = "Sunucu:";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(120, 47);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(200, 20);
            this.txtServer.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(20, 80);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(75, 13);
            this.lblDatabase.Text = "Veritabaný Adý:";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Location = new System.Drawing.Point(120, 77);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(200, 20);
            this.txtDatabase.TabIndex = 2;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(20, 110);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(67, 13);
            this.lblUser.Text = "Kullanýcý Adý:";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(120, 107);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(200, 20);
            this.txtUser.TabIndex = 3;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(20, 140);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(31, 13);
            this.lblPassword.Text = "Þifre:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(120, 137);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.TabIndex = 4;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(20, 170);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(120, 167);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 20);
            this.txtPort.TabIndex = 5;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(120, 200);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(100, 30);
            this.btnTestConnection.TabIndex = 6;
            this.btnTestConnection.Text = "Baðlantýyý Test Et";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new EventHandler(this.btnTestConnection_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(250, 250);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Kaydet";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(360, 250);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Ýptal";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            // 
            // ConnectionSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 300);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.btnBrowseSqlite);
            this.Controls.Add(this.txtSqliteFilePath);
            this.Controls.Add(this.lblSqliteFilePath);
            this.Controls.Add(this.cmbDatabaseType);
            this.Controls.Add(this.lblDatabaseType);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionSettingsForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Veritabaný Baðlantý Ayarlarý";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label lblDatabaseType;
        private ComboBox cmbDatabaseType;
        private Label lblSqliteFilePath;
        private TextBox txtSqliteFilePath;
        private Button btnBrowseSqlite;
        private Label lblServer;
        private TextBox txtServer;
        private Label lblDatabase;
        private TextBox txtDatabase;
        private Label lblUser;
        private TextBox txtUser;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblPort;
        private TextBox txtPort;
        private Button btnTestConnection;
        private Button btnSave;
        private Button btnCancel;
    }
}

