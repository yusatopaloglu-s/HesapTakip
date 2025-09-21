using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HesapTakip
{
    public partial class KontorForm : Form
    {
        private SQLiteConnection connection;
        public KontorForm(string connectionString)
        {
            InitializeComponent();
            connection = new SQLiteConnection(connectionString);
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            dgvCustomers.Columns.Clear();
            using (var adapter = new SQLiteDataAdapter("SELECT CustomerID, Name FROM Customers", connection))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvCustomers.DataSource = dt;
            }
            dgvCustomers.Columns["CustomerID"].Visible = false;
        }

        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow != null)
            {
                int customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
                LoadKontorList(customerID);
            }
        }

        private void InitializeComponent()
        {
            dgvKontor = new DataGridView();
            customerPanel = new Panel();
            label4 = new Label();
            dgvCustomers = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvKontor).BeginInit();
            customerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).BeginInit();
            SuspendLayout();
            // 
            // dgvKontor
            // 
            dgvKontor.BackgroundColor = SystemColors.Control;
            dgvKontor.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKontor.Location = new Point(269, 299);
            dgvKontor.Name = "dgvKontor";
            dgvKontor.Size = new Size(310, 203);
            dgvKontor.TabIndex = 0;
            // 
            // customerPanel
            // 
            customerPanel.AutoSize = true;
            customerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            customerPanel.Controls.Add(label4);
            customerPanel.Controls.Add(dgvCustomers);
            customerPanel.Location = new Point(8, 8);
            customerPanel.Name = "customerPanel";
            customerPanel.Size = new Size(258, 494);
            customerPanel.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(80, 0);
            label4.Name = "label4";
            label4.Size = new Size(82, 15);
            label4.TabIndex = 4;
            label4.Text = "Müþteri Listesi";
            // 
            // dgvCustomers
            // 
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomers.BackgroundColor = SystemColors.Control;
            dgvCustomers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCustomers.ColumnHeadersVisible = false;
            dgvCustomers.Location = new Point(0, 19);
            dgvCustomers.Name = "dgvCustomers";
            dgvCustomers.ReadOnly = true;
            dgvCustomers.RowHeadersVisible = false;
            dgvCustomers.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.Size = new Size(255, 472);
            dgvCustomers.TabIndex = 0;
            // 
            // KontorForm
            // 
            ClientSize = new Size(704, 554);
            Controls.Add(customerPanel);
            Controls.Add(dgvKontor);
            Name = "KontorForm";
            ((System.ComponentModel.ISupportInitialize)dgvKontor).EndInit();
            customerPanel.ResumeLayout(false);
            customerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        private void LoadKontorList(int customerID)
        {
            dgvKontor.Columns.Clear();
            using (var adapter = new SQLiteDataAdapter(
                "SELECT TransactionID, Date, Kontor FROM EDefterTakip WHERE CustomerID = @cid", connection))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@cid", customerID);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvKontor.DataSource = dt;
            }
        }
        private DataGridView dgvKontor;
        private Panel customerPanel;
        private Label label4;
        private DataGridView dgvCustomers;
    }
}