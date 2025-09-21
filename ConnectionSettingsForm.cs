namespace HesapTakip
{
    public partial class ConnectionSettingsForm : Form
    {
        public string Server => txtServer.Text;
        public string Database => txtDatabase.Text;
        public string User => txtUser.Text;
        public string Password => txtPassword.Text;
        public string Port => txtPort.Text;

        public ConnectionSettingsForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Database) ||
                string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Port))
            {
                MessageBox.Show("Tüm alanlarý doldurun!");
                return;
            }
            this.DialogResult = DialogResult.OK;
            //this.Close();
        }

        private TextBox txtServer;
        private TextBox txtDatabase;
        private TextBox txtUser;
        private TextBox txtPassword;
        private TextBox txtPort;
        private Button btnSave;


        private void InitializeComponent()
        {
            txtServer = new TextBox();
            txtDatabase = new TextBox();
            txtUser = new TextBox();
            txtPassword = new TextBox();
            txtPort = new TextBox();
            btnSave = new Button();
            SuspendLayout();
            // 
            // txtServer
            // 
            txtServer.Location = new Point(12, 12);
            txtServer.Name = "txtServer";
            txtServer.PlaceholderText = "Server/IP";
            txtServer.Size = new Size(100, 23);
            txtServer.TabIndex = 0;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(12, 41);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.PlaceholderText = "Veri Tabaný Adý";
            txtDatabase.Size = new Size(100, 23);
            txtDatabase.TabIndex = 1;
            // 
            // txtUser
            // 
            txtUser.Location = new Point(12, 70);
            txtUser.Name = "txtUser";
            txtUser.PlaceholderText = "Kullanýcý Adý";
            txtUser.Size = new Size(100, 23);
            txtUser.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(12, 99);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Þifre";
            txtPassword.Size = new Size(100, 23);
            txtPassword.TabIndex = 3;
            // 
            // txtPort
            // 
            txtPort.Enabled = false;
            txtPort.Location = new Point(12, 128);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(100, 23);
            txtPort.TabIndex = 4;
            txtPort.Text = "3306";
            txtPort.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 157);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 23);
            btnSave.TabIndex = 5;
            btnSave.Text = "Kaydet / Baþlat";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // ConnectionSettingsForm
            // 
            ClientSize = new Size(124, 198);
            Controls.Add(btnSave);
            Controls.Add(txtPort);
            Controls.Add(txtPassword);
            Controls.Add(txtUser);
            Controls.Add(txtDatabase);
            Controls.Add(txtServer);
            Name = "ConnectionSettingsForm";
            ResumeLayout(false);
            PerformLayout();

        }
    }
}