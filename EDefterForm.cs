using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static HesapTakip.MainForm;

namespace HesapTakip
{
    public partial class EDefterForm : Form
    {
        private string connectionString;
        private DataSet dataSet = new DataSet();
        public EDefterForm()
        {
            InitializeComponent();
            connectionString = Properties.Settings.Default.DatabasePath;
            InitializeDatabase();
            connection = new MySqlConnection(connectionString);
            LoadCustomers();
            dtpDate.Value = DateTime.Today;
            dgvKontorList.CellFormatting += dgvKontorList_CellFormatting;


        }
        private MySqlConnection connection;

        private void InitializeDatabase()
        {

        }

        public void LoadCustomers()
        {
            try
            {
                dgvFirmaList.Columns.Clear();


                using (var adapter = new MySqlDataAdapter("SELECT CustomerID, Name FROM Customers WHERE EDefter = 1", connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt); 
                    dgvFirmaList.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message + "\n" + "Ayarlar sıfırlandı. Uygulama kapatılıyor...");
                Properties.Settings.Default.Reset();
                Application.Exit();
            }
            if (dgvFirmaList.Columns.Contains("CustomerID"))
                dgvFirmaList.Columns["CustomerID"].Visible = false;
        }

        private void LoadTransactions(int customerID)
        {
            try
            {
                connection.Open();
                var transactionsAdapter = new MySqlDataAdapter(
                    $"SELECT TransactionID, Date, Kontor, Type FROM EDefterTakip WHERE CustomerID = {customerID}",
                    connection);
                dataSet.Tables["EDefterTakip"]?.Clear();
                transactionsAdapter.Fill(dataSet, "EDefterTakip");
                dgvKontorList.DataSource = dataSet.Tables["EDefterTakip"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            dgvKontorList.Columns["TransactionID"].Visible = false;
            dgvKontorList.Columns["Date"].HeaderText = "Tarih";
            dgvKontorList.Columns["Kontor"].HeaderText = "Kontor";
            dgvKontorList.Columns["Type"].Visible = false;

            foreach (DataGridViewRow row in dgvKontorList.Rows)
            {
                if (row.IsNewRow) continue;
                var typeCell = row.Cells["Type"];
                var kontorCell = row.Cells["Kontor"];
                if (typeCell.Value == null || kontorCell.Value == null) continue;

                switch (typeCell.Value.ToString().ToLower())
                {
                    case "ekle":
                        kontorCell.Style.ForeColor = Color.Green;
                        break;
                    case "cikar":
                        kontorCell.Style.ForeColor = Color.Red;
                        break;
                    default:
                        kontorCell.Style.ForeColor = dgvKontorList.DefaultCellStyle.ForeColor;
                        break;
                }
            }

            dgvKontorList.Sort(dgvKontorList.Columns["Date"], ListSortDirection.Ascending);
            dgvKontorList.Columns["Date"].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
        }
        private void CalculateAndDisplayTotal(int customerID)
        {
            try
            {
                connection.Open();
                using (var cmd = new MySqlCommand(
                    @"SELECT SUM(Kontor * CASE WHEN Type = 'ekle' THEN 1 ELSE -1 END) 
            FROM EDefterTakip 
            WHERE CustomerID = @customerID", connection))
                {
                    cmd.Parameters.AddWithValue("@customerID", customerID);
                    var result = cmd.ExecuteScalar();

                    decimal total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    lblTotal.Text = $"Kontor: {total.ToString("N2")} ₺";


                    lblTotal.ForeColor = total >= 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hesaplama hatası: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        private bool ValidateTransactionType()
        {
            if (GetSelectedTransactionType() == null)
            {
                MessageBox.Show("Lütfen işlem türünü seçiniz!");
                return false;
            }
            return true;
        }


        private string GetSelectedTransactionType()
        {
            return selectedTransactionType;
        }

        private void ClearTransactionInputs()
        {
            dtpDate.Value = DateTime.Today;
            txtkontor.Clear();

        }

        private string selectedTransactionType; 

        private void btnekle_Click(object sender, EventArgs e)
        {
            selectedTransactionType = "ekle";
            if (!decimal.TryParse(txtkontor.Text, out decimal kontor))
            {
                MessageBox.Show("Geçersiz tutar formatı!");
                return;
            }
            if (dgvFirmaList.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);

                if (!ValidateTransactionType()) return;

                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection; 
                        cmd.CommandText = @"INSERT INTO EDefterTakip 
                                            (CustomerID, Date, Kontor, Type) 
                                            VALUES 
                                            (@cid, @date, @kontor, @type)";

                        cmd.Parameters.AddWithValue("@cid", customerID);
                        cmd.Parameters.AddWithValue("@date", dtpDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@kontor", kontor);
                        cmd.Parameters.AddWithValue("@type", GetSelectedTransactionType());
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt eklenemedi: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }

                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                ClearTransactionInputs();
            }
        }

        private void btnsil_Click(object sender, EventArgs e)
        {
            selectedTransactionType = "cikar";
            if (dgvFirmaList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir müşteri seçin.");
                return;
            }

            int customerID = Convert.ToInt32(dgvFirmaList.SelectedRows[0].Cells["CustomerID"].Value);

            if (dgvKontorList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silinecek işlemi seçin.");
                return;
            }

            int transactionID = Convert.ToInt32(dgvKontorList.SelectedRows[0].Cells["TransactionID"].Value);

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                using (var cmd = new MySqlCommand("DELETE FROM EDefterTakip WHERE TransactionID = @transactionID AND CustomerID = @customerID", connection))
                {
                    cmd.Connection = connection;
                    cmd.Parameters.AddWithValue("@transactionID", transactionID);
                    cmd.Parameters.AddWithValue("@customerID", customerID);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();

                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt silinemedi: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvFirmaList.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);
                LoadTransactions(customerID);
                gbKontor.Enabled = true;
                CalculateAndDisplayTotal(customerID);
            }
        }
        private void txtkontor_Validating(object sender, CancelEventArgs e)
        {
            if (!decimal.TryParse(txtkontor.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out _))
            {
                MessageBox.Show(txtkontor, "Geçerli bir sayı giriniz");

                e.Cancel = true;
            }
            else
            {
                MessageBox.Show(txtkontor, "");
            }
        }


        private void txtAmount_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtkontor.Text, out decimal amount))
            {
                txtkontor.Text = amount.ToString("N2");
            }
        }

        private void dgvKontorList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvKontorList.Columns[e.ColumnIndex].Name == "Kontor")
            {
                var row = dgvKontorList.Rows[e.RowIndex];
                var typeCell = row.Cells["Type"].Value;
                if (typeCell != null)
                {
                    switch (typeCell.ToString().ToLower())
                    {
                        case "ekle":
                            e.CellStyle.ForeColor = Color.Green;
                            break;
                        case "cikar":
                            e.CellStyle.ForeColor = Color.Red;
                            break;
                        default:
                            e.CellStyle.ForeColor = dgvKontorList.DefaultCellStyle.ForeColor;
                            break;
                    }
                }
            }
        }

        private void btncikar_Click(object sender, EventArgs e)
        {
            selectedTransactionType = "cikar";
            if (!decimal.TryParse(txtkontor.Text, out decimal kontor))
            {
                MessageBox.Show("Geçersiz tutar formatı!");
                return;
            }
            if (dgvFirmaList.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);

                if (!ValidateTransactionType()) return;

                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = @"INSERT INTO EDefterTakip 
                                            (CustomerID, Date, Kontor, Type) 
                                            VALUES 
                                            (@cid, @date, @kontor, @type)";

                        cmd.Parameters.AddWithValue("@cid", customerID);
                        cmd.Parameters.AddWithValue("@date", dtpDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@kontor", kontor);
                        cmd.Parameters.AddWithValue("@type", GetSelectedTransactionType());
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt eklenemedi: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }

                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                ClearTransactionInputs();
            }


        }
        private void btnhepsi1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvFirmaList.Rows)
            {
                if (row.IsNewRow) continue;
                int customerID = Convert.ToInt32(row.Cells["CustomerID"].Value);

                // Her müşteri için 1 adet "cikar" işlemi ekle
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = @"INSERT INTO EDefterTakip 
                                    (CustomerID, Date, Kontor, Type) 
                                    VALUES 
                                    (@cid, @date, @kontor, @type)";

                        cmd.Parameters.AddWithValue("@cid", customerID);
                        cmd.Parameters.AddWithValue("@date", dtpDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@kontor", 1); // Her firma için 1 düş
                        cmd.Parameters.AddWithValue("@type", "cikar");
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Firma ID {customerID} için işlem eklenemedi: {ex.Message}");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }

            // Seçili müşteri varsa tabloyu güncelle
            if (dgvFirmaList.CurrentRow != null)
            {
                int selectedCustomerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);
                LoadTransactions(selectedCustomerID);
                CalculateAndDisplayTotal(selectedCustomerID);
            }
        }
    }
}
