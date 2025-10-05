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
            this.cmbDatabaseType.SelectedIndexChanged += new System.EventHandler(this.cmbDatabaseType_SelectedIndexChanged);
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
            this.btnBrowseSqlite.Click += new System.EventHandler(this.btnBrowseSqlite_Click);
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
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(250, 250);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Kaydet";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(360, 250);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Ýptal";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
    }
}