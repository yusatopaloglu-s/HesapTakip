using System.Windows.Forms;

namespace HesapTakip
{
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblDatabaseType = new Label();
            cmbDatabaseType = new ComboBox();
            lblSqliteFilePath = new Label();
            txtSqliteFilePath = new TextBox();
            btnBrowseSqlite = new Button();
            lblServer = new Label();
            txtServer = new TextBox();
            lblDatabase = new Label();
            txtDatabase = new TextBox();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            btnTestConnection = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            chkUseWindowsAuth = new CheckBox();
            btn_crdb_sqlite = new Button();
            btn_crdb_mssql = new Button();
            btn_crdb_mysql = new Button();
            SuspendLayout();
            // 
            // lblDatabaseType
            // 
            lblDatabaseType.AutoSize = true;
            lblDatabaseType.Location = new Point(23, 23);
            lblDatabaseType.Margin = new Padding(4, 0, 4, 0);
            lblDatabaseType.Name = "lblDatabaseType";
            lblDatabaseType.Size = new Size(84, 15);
            lblDatabaseType.TabIndex = 15;
            lblDatabaseType.Text = "Veritabanı Tipi:";
            // 
            // cmbDatabaseType
            // 
            cmbDatabaseType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatabaseType.FormattingEnabled = true;
            cmbDatabaseType.Items.AddRange(new object[] { "SQLite", "MySQL", "MSSQL" });
            cmbDatabaseType.Location = new Point(140, 20);
            cmbDatabaseType.Margin = new Padding(4, 3, 4, 3);
            cmbDatabaseType.Name = "cmbDatabaseType";
            cmbDatabaseType.Size = new Size(233, 23);
            cmbDatabaseType.TabIndex = 0;
            cmbDatabaseType.SelectedIndexChanged += cmbDatabaseType_SelectedIndexChanged;
            // 
            // lblSqliteFilePath
            // 
            lblSqliteFilePath.AutoSize = true;
            lblSqliteFilePath.Location = new Point(23, 77);
            lblSqliteFilePath.Margin = new Padding(4, 0, 4, 0);
            lblSqliteFilePath.Name = "lblSqliteFilePath";
            lblSqliteFilePath.Size = new Size(105, 15);
            lblSqliteFilePath.TabIndex = 14;
            lblSqliteFilePath.Text = "SQLite Dosya Yolu:";
            // 
            // txtSqliteFilePath
            // 
            txtSqliteFilePath.Location = new Point(140, 73);
            txtSqliteFilePath.Margin = new Padding(4, 3, 4, 3);
            txtSqliteFilePath.Name = "txtSqliteFilePath";
            txtSqliteFilePath.Size = new Size(291, 23);
            txtSqliteFilePath.TabIndex = 1;
            // 
            // btnBrowseSqlite
            // 
            btnBrowseSqlite.Location = new Point(443, 71);
            btnBrowseSqlite.Margin = new Padding(4, 3, 4, 3);
            btnBrowseSqlite.Name = "btnBrowseSqlite";
            btnBrowseSqlite.Size = new Size(88, 27);
            btnBrowseSqlite.TabIndex = 2;
            btnBrowseSqlite.Text = "Gözat";
            btnBrowseSqlite.UseVisualStyleBackColor = true;
            btnBrowseSqlite.Click += btnBrowseSqlite_Click;
            // 
            // lblServer
            // 
            lblServer.AutoSize = true;
            lblServer.Location = new Point(23, 77);
            lblServer.Margin = new Padding(4, 0, 4, 0);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(50, 15);
            lblServer.TabIndex = 13;
            lblServer.Text = "Sunucu:";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(140, 73);
            txtServer.Margin = new Padding(4, 3, 4, 3);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(233, 23);
            txtServer.TabIndex = 1;
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(23, 111);
            lblDatabase.Margin = new Padding(4, 0, 4, 0);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(83, 15);
            lblDatabase.TabIndex = 12;
            lblDatabase.Text = "Veritabanı Adı:";
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(140, 108);
            txtDatabase.Margin = new Padding(4, 3, 4, 3);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(233, 23);
            txtDatabase.TabIndex = 2;
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Location = new Point(23, 146);
            lblUser.Margin = new Padding(4, 0, 4, 0);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(76, 15);
            lblUser.TabIndex = 11;
            lblUser.Text = "Kullanıcı Adı:";
            // 
            // txtUser
            // 
            txtUser.Location = new Point(140, 142);
            txtUser.Margin = new Padding(4, 3, 4, 3);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(233, 23);
            txtUser.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(23, 181);
            lblPassword.Margin = new Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(33, 15);
            lblPassword.TabIndex = 10;
            lblPassword.Text = "Şifre:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(140, 177);
            txtPassword.Margin = new Padding(4, 3, 4, 3);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(233, 23);
            txtPassword.TabIndex = 4;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(23, 215);
            lblPort.Margin = new Padding(4, 0, 4, 0);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(32, 15);
            lblPort.TabIndex = 9;
            lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(140, 212);
            txtPort.Margin = new Padding(4, 3, 4, 3);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(116, 23);
            txtPort.TabIndex = 5;
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new Point(140, 250);
            btnTestConnection.Margin = new Padding(4, 3, 4, 3);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(117, 35);
            btnTestConnection.TabIndex = 6;
            btnTestConnection.Text = "Baðlantýyý Test Et";
            btnTestConnection.UseVisualStyleBackColor = true;
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(292, 288);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(117, 35);
            btnSave.TabIndex = 7;
            btnSave.Text = "Kaydet";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(420, 288);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(117, 35);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Iptal";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // chkUseWindowsAuth
            // 
            chkUseWindowsAuth.AutoSize = true;
            chkUseWindowsAuth.Location = new Point(140, 48);
            chkUseWindowsAuth.Name = "chkUseWindowsAuth";
            chkUseWindowsAuth.Size = new Size(217, 19);
            chkUseWindowsAuth.TabIndex = 16;
            chkUseWindowsAuth.Text = "Windows Kimlik Doğrulaması Kullan";
            chkUseWindowsAuth.UseVisualStyleBackColor = true;
            chkUseWindowsAuth.Visible = false;
            chkUseWindowsAuth.CheckedChanged += ChkUseWindowsAuth_CheckedChanged;
            // 
            // btn_crdb_sqlite
            // 
            btn_crdb_sqlite.Location = new Point(292, 250);
            btn_crdb_sqlite.Name = "btn_crdb_sqlite";
            btn_crdb_sqlite.Size = new Size(117, 35);
            btn_crdb_sqlite.TabIndex = 17;
            btn_crdb_sqlite.Text = "Veri Tabanı Oluştur";
            btn_crdb_sqlite.UseVisualStyleBackColor = true;
            btn_crdb_sqlite.Visible = false;
            btn_crdb_sqlite.Click += btn_crdb_sqlite_Click;
            // 
            // btn_crdb_mssql
            // 
            btn_crdb_mssql.Location = new Point(292, 250);
            btn_crdb_mssql.Name = "btn_crdb_mssql";
            btn_crdb_mssql.Size = new Size(117, 35);
            btn_crdb_mssql.TabIndex = 18;
            btn_crdb_mssql.Text = "MSSQL için Oluştur";
            btn_crdb_mssql.UseVisualStyleBackColor = true;
            btn_crdb_mssql.Visible = false;
            btn_crdb_mssql.Click += btn_crdb_mssql_Click;
            // 
            // btn_crdb_mysql
            // 
            btn_crdb_mysql.Location = new Point(292, 250);
            btn_crdb_mysql.Name = "btn_crdb_mysql";
            btn_crdb_mysql.Size = new Size(117, 35);
            btn_crdb_mysql.TabIndex = 19;
            btn_crdb_mysql.Text = "MySQL için Oluştur";
            btn_crdb_mysql.UseVisualStyleBackColor = true;
            btn_crdb_mysql.Visible = false;
            btn_crdb_mysql.Click += btn_crdb_mysql_Click;
            // 
            // ConnectionSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(565, 346);
            Controls.Add(btn_crdb_mysql);
            Controls.Add(btn_crdb_mssql);
            Controls.Add(btn_crdb_sqlite);
            Controls.Add(chkUseWindowsAuth);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnTestConnection);
            Controls.Add(txtPort);
            Controls.Add(lblPort);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUser);
            Controls.Add(lblUser);
            Controls.Add(txtDatabase);
            Controls.Add(lblDatabase);
            Controls.Add(txtServer);
            Controls.Add(lblServer);
            Controls.Add(btnBrowseSqlite);
            Controls.Add(txtSqliteFilePath);
            Controls.Add(lblSqliteFilePath);
            Controls.Add(cmbDatabaseType);
            Controls.Add(lblDatabaseType);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectionSettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Veritabanı Bağlantı Ayarları";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion      

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
        private CheckBox chkUseWindowsAuth;
        private Button btn_crdb_sqlite;
        private Button btn_crdb_mssql;
        private Button btn_crdb_mysql;
    }
}
