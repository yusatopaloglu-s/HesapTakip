using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

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
                // Mevcut ayarlar� y�kle
                string currentType = AppConfigHelper.DatabaseType;

                if (!string.IsNullOrEmpty(currentType))
                {
                    cmbDatabaseType.SelectedItem = currentType;
                }
                else
                {
                    cmbDatabaseType.SelectedIndex = 0; // Varsay�lan SQLite
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
                    // Password security nedeniyle �ifre y�klenmez
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ayarlar y�klenirken hata: {ex.Message}");
            }
        }

        private void UpdateFormByDatabaseType()
        {
            string selectedType = cmbDatabaseType.SelectedItem?.ToString() ?? "SQLite";

            bool isSQLite = selectedType == "SQLite";
            bool isMySQL = selectedType == "MySQL";
            bool isMSSQL = selectedType == "MSSQL";

            // SQLite kontrollerini g�ster/gizle
            lblSqliteFilePath.Visible = isSQLite;
            txtSqliteFilePath.Visible = isSQLite;
            btnBrowseSqlite.Visible = isSQLite;

            // MySQL/MSSQL kontrollerini g�ster/gizle
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

            // Port varsay�lan de�erleri - SADECE BO�SA veya T�P DE���T�YSE
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

            // SQLite i�in varsay�lan dosya yolu
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
                sfd.Title = "SQLite Database Dosyas�n� Se�in";
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
                    MessageBox.Show("Ba�lant� testi ba�ar�l�!", "Ba�ar�l�",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ba�lant� testi ba�ar�s�z!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ba�lant� testi s�ras�nda hata: {ex.Message}", "Hata",
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
                MessageBox.Show("L�tfen veritaban� tipi se�in!", "Uyar�",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (databaseType == "SQLite")
            {
                if (string.IsNullOrEmpty(txtSqliteFilePath.Text))
                {
                    MessageBox.Show("L�tfen SQLite dosya yolunu se�in!", "Uyar�",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                try
                {
                    string directory = Path.GetDirectoryName(txtSqliteFilePath.Text);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        // Dizin olu�turulabilir mi kontrol et
                        Directory.CreateDirectory(directory);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ge�ersiz dosya yolu: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtServer.Text))
                {
                    MessageBox.Show("L�tfen sunucu adresini girin!", "Uyar�",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtDatabase.Text))
                {
                    MessageBox.Show("L�tfen veritaban� ad�n� girin!", "Uyar�",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtUser.Text))
                {
                    MessageBox.Show("L�tfen kullan�c� ad�n� girin!", "Uyar�",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(txtPort.Text))
                {
                    MessageBox.Show("L�tfen port numaras�n� girin!", "Uyar�",
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
                // De�erleri property'lere ata
                DatabaseType = cmbDatabaseType.SelectedItem.ToString();
                Server = txtServer.Text;
                Database = txtDatabase.Text;
                User = txtUser.Text;
                Password = txtPassword.Text;
                Port = txtPort.Text;
                SqliteFilePath = txtSqliteFilePath.Text;

                // SQLite i�in dosya dizinini olu�tur
                if (DatabaseType == "SQLite")
                {
                    try
                    {
                        string directory = Path.GetDirectoryName(SqliteFilePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                            Debug.WriteLine($"SQLite dizin olu�turuldu: {directory}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"SQLite dosya dizini olu�turulamad�: {ex.Message}", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // DialogResult'� OK yap ve formu kapat
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
}