using MySql.Data.MySqlClient;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HesapTakip

{
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using QuestPDF.Infrastructure;
    using System.Diagnostics;

    public partial class MainForm : Form
    {
        private string connectionString;
        private static bool _versionChecked = false;

        public MainForm()
        {
            InitializeComponent();
            connectionString = Properties.Settings.Default.DatabasePath;
            InitializeDatabase();
            LoadCustomers();
            InitializeAutoComplete();
            LoadSuggestions();
            dtpDate.Value = DateTime.Today;
            dgvTransactions.CellFormatting += dgvTransactions_CellFormatting;
            if (!_versionChecked)
            {
                _versionChecked = true;
                _ = CheckForUpdate();
            }

        }
                       
        private MySqlConnection connection;       
        private DataSet dataSet = new DataSet();
        private List<string> _suggestions = new List<string>();
                              
        private void InitializeDatabase()
        {
            connection = new MySqlConnection(connectionString);

            // Customers tablosu ve sütunları
            EnsureTableAndColumns("Customers", new Dictionary<string, string>
            {
                { "CustomerID", "INT PRIMARY KEY AUTO_INCREMENT" },
                { "Name", "VARCHAR(255) NOT NULL" },
                { "EDefter", "INT DEFAULT 0" }
            });

            // Transactions tablosu ve sütunları
            EnsureTableAndColumns("Transactions", new Dictionary<string, string>
            {
                { "TransactionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                { "CustomerID", "INT" },
                { "Date", "DATETIME" },
                { "Description", "VARCHAR(255) NULL" },
                { "Amount", "DECIMAL(18,2)" },
                { "Type", "VARCHAR(50)" },
                { "IsDeleted", "TINYINT(1) DEFAULT 0" }
            });

            // EDefterTakip tablosu ve sütunları
            EnsureTableAndColumns("EDefterTakip", new Dictionary<string, string>
            {
                { "TransactionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                { "CustomerID", "INT" },
                { "Date", "DATETIME" },
                { "Kontor", "DECIMAL(18,2)" },
                { "Type", "VARCHAR(255) NOT NULL" }
            });

            // Suggestions tablosu ve sütunları (MySQL uyumlu)
            EnsureTableAndColumns("Suggestions", new Dictionary<string, string>
            {
                { "SuggestionID", "INT PRIMARY KEY AUTO_INCREMENT" },
                { "Description", "VARCHAR(255) NOT NULL UNIQUE" }, // Changed from TEXT to VARCHAR(255)
                { "CreatedDate", "DATETIME DEFAULT CURRENT_TIMESTAMP" }
            });
        }



        private void btnResetSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            MessageBox.Show("Ayarlar sıfırlandı. Uygulama kapatılıyor...");
            Application.Exit();
        }

        public void LoadCustomers()
        {
            try
            {
                dgvCustomers.Columns.Clear();

                // Müşteri listesini yükle
                
                using (var adapter = new MySqlDataAdapter("SELECT * FROM Customers", connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCustomers.DataSource = dt;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message + "\n" + "Ayarlar sıfırlandı. Uygulama kapatılıyor...");
                Properties.Settings.Default.Reset();
                Application.Exit();
            }
            if (dgvCustomers.Columns["CustomerID"] != null)
            {
                dgvCustomers.Columns["CustomerID"].Visible = false;
                dgvCustomers.Columns["EDefter"].Visible = false;
            }
        }

        private void LoadTransactions(int customerID)
        {
            try
            {
                using (var adapter = new MySqlDataAdapter(
                    "SELECT TransactionID, Date, Description, Amount, Type FROM Transactions WHERE CustomerID = @customerID AND IsDeleted = 0",
                    connection))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@customerID", customerID);
                    dataSet.Tables["Transactions"]?.Clear();
                    adapter.Fill(dataSet, "Transactions");
                    dgvTransactions.DataSource = dataSet.Tables["Transactions"];
                }
                FormatTransactionGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
            }
        }

        private void FormatTransactionGrid()
        {
            if (dgvTransactions.Columns["TransactionID"] != null)
                dgvTransactions.Columns["TransactionID"].Visible = false;
            if (dgvTransactions.Columns["Type"] != null)
                dgvTransactions.Columns["Type"].Visible = false;
            if (dgvTransactions.Columns["Description"] != null)
                dgvTransactions.Columns["Description"].Width = 200;
            if (dgvTransactions.Columns["Date"] != null)
                dgvTransactions.Columns["Date"].HeaderText = "Tarih";
            if (dgvTransactions.Columns["Description"] != null)
                dgvTransactions.Columns["Description"].HeaderText = "Açıklama";
            if (dgvTransactions.Columns["Amount"] != null)
                dgvTransactions.Columns["Amount"].HeaderText = "Tutar";

            foreach (DataGridViewRow row in dgvTransactions.Rows)
            {
                if (row.IsNewRow) continue;
                var typeCell = row.Cells["Type"];
                var amountCell = row.Cells["Amount"];
                if (typeCell?.Value == null || amountCell?.Value == null) continue;
                amountCell.Style.ForeColor = typeCell.Value.ToString().ToLower() switch
                {
                    "gelir" => System.Drawing.Color.Green,
                    "gider" => System.Drawing.Color.Red,
                    _ => dgvTransactions.DefaultCellStyle.ForeColor
                };
            }

            if (dgvTransactions.Columns["Date"] != null)
            {
                dgvTransactions.Sort(dgvTransactions.Columns["Date"], ListSortDirection.Ascending);
                dgvTransactions.Columns["Date"].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
        }
        private string GetSelectedTransactionType()
        {
            if (rbIncome.Checked) return "Gelir";
            if (rbExpense.Checked) return "Gider";
            return null; // Validasyon için
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
        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            using (var inputForm = new InputForm("Müşteri Adı:"))
            {
                // E-Defter seçimi için ek kontrol ekle
                var chkEDefter = new CheckBox
                {
                    Text = "E-Defter",
                    Left = 20,
                    Top = 80,
                    Width = 200
                };
                inputForm.Controls.Add(chkEDefter);

                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputForm.InputText))
                {

                    {

                        {
                            connection.Open();
                            using (var cmd = new MySqlCommand(
                                "INSERT INTO Customers (Name, EDefter) VALUES (@name, @edefter)",
                                connection))
                            {
                                cmd.Parameters.AddWithValue("@name", inputForm.InputText.Trim());
                                cmd.Parameters.AddWithValue("@edefter", chkEDefter.Checked ? 1 : 0);
                                cmd.ExecuteNonQuery();
                            }
                            connection.Close();
                        }
                        LoadCustomers();
                    }
                }
            }
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;
            var customerID = dgvCustomers.CurrentRow.Cells["CustomerID"].Value.ToString();
            var customerName = dgvCustomers.CurrentRow.Cells["Name"].Value?.ToString() ?? "this customer";
            var confirmResult = MessageBox.Show(
                $"{customerName} müşterisini ve tüm hareketlerini silmek istediğinize emin misiniz?\nBu işlem geri alınamaz!",
                "Müşteri Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            try
            {
               
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = "DELETE FROM Transactions WHERE CustomerID = @id";
                            cmd.Parameters.AddWithValue("@id", customerID);
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "DELETE FROM Customers WHERE CustomerID = @id";
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                    connection.Close();
                }
                LoadCustomers();
                dgvTransactions.DataSource = null;
                MessageBox.Show($"{customerName} Başarıyla Silindi.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme Başarısız: {ex.Message}",
                        "HATA",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }

        }

        
        private void CalculateAndDisplayTotal(int customerID)
        {
            try
            {
                connection.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    @"SELECT SUM(Amount * CASE WHEN Type = 'Gelir' THEN 1 ELSE -1 END) 
                      FROM Transactions 
                      WHERE CustomerID = @customerID AND IsDeleted = 0", connection)) 
                {
                    cmd.Parameters.AddWithValue("@customerID", customerID);
                    var result = cmd.ExecuteScalar();

                    decimal total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    lblTotal.Text = $"Toplam Bakiye: {total:N2} ₺";

                    // Negatif bakiyeler için renk değişimi
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


        private void btnAddTransaction_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Geçersiz tutar formatı!");
                return;
            }
            if (dgvCustomers.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);

                if (!ValidateTransactionType()) return;
                                
                using (var cmd = new MySqlCommand(
                    @"INSERT INTO Transactions 
              (CustomerID, Date, Description, Amount, Type) 
              VALUES 
              (@cid, @date, @desc, @amount, @type)", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@cid", customerID);
                    cmd.Parameters.AddWithValue("@date", dtpDate.Value);
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@type", GetSelectedTransactionType());
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                ClearTransactionInputs();
            }
        }
        private void btnDeleteTransaction_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.CurrentRow == null)
            {
                MessageBox.Show("Lütfen silmek için bir hareket seçin!");
                return;
            }
            var confirmResult = MessageBox.Show(
                "Seçili hareketi silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButtons.YesNo
            );
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    int transactionID = Convert.ToInt32(dgvTransactions.CurrentRow.Cells["TransactionID"].Value);
                    int customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);

                    using (var cmd = new MySqlCommand("UPDATE Transactions SET IsDeleted = 1 WHERE TransactionID = @id", connection))
                    {
                        connection.Open();
                        cmd.Parameters.AddWithValue("@id", transactionID);
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    LoadTransactions(customerID);
                    CalculateAndDisplayTotal(customerID);
                    MessageBox.Show("Hareket başarıyla silindi!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Silme hatası: " + ex.Message);
                }
            }
        }
        public void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
                LoadTransactions(customerID);
                gbTransactions.Enabled = true;
                CalculateAndDisplayTotal(customerID);
            }
        }

        private void ClearTransactionInputs()
        {
            dtpDate.Value = DateTime.Today;
            txtDescription.Clear();
            txtAmount.Clear();

        }

        private void txtAmount_Validating(object sender, CancelEventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out _))
            {
                MessageBox.Show(txtAmount, "Geçerli bir sayı giriniz");

                e.Cancel = true;
            }
            else
            {
                MessageBox.Show(txtAmount, "");
            }
        }
        private void txtAmount_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                txtAmount.Text = amount.ToString("N2");
            }
        }

        // RadioButton'ların arkaplan rengini değiştir
        private void UpdateTypeValidationUI()
        {
            QuestPDF.Infrastructure.Color errorColor = QuestPDF.Infrastructure.Color.FromRGB(255, 182, 193); 

            rbIncome.BackColor = GetSelectedTransactionType() != null
                ? SystemColors.Window
                : System.Drawing.Color.FromArgb(errorColor.Red, errorColor.Green, errorColor.Blue);

            rbExpense.BackColor = rbIncome.BackColor;
        }


        // Her değişiklikte tetikle
        private void rbIncome_CheckedChanged(object sender, EventArgs e) => UpdateTypeValidationUI();
        private void rbExpense_CheckedChanged(object sender, EventArgs e) => UpdateTypeValidationUI();
        // Label'a geçiş efekti 
        private void UpdateTotalWithAnimation(decimal newTotal)
        {
            Color targetColor = newTotal >= 0 ? Color.FromRGB(255, 182, 193) : Color.FromRGB(255, 182, 193);

            lblTotal.BeginInvoke((Action)(() =>
            {
                lblTotal.ForeColor = System.Drawing.Color.FromArgb(targetColor.Alpha, targetColor.Red, targetColor.Green, targetColor.Blue);
                lblTotal.Text = $"Toplam Bakiye: {newTotal:N2} ₺";
            }));
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null)
            {
                MessageBox.Show("Lütfen önce bir müşteri seçin!", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
                string customerName = dgvCustomers.CurrentRow.Cells["Name"].Value.ToString();

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Dosyaları|*.xlsx";
                    sfd.FileName = $"{customerName}_HesapHareketleri_{DateTime.Today:yyyyMMddHHmm}.xlsx";
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog() != DialogResult.OK) return;

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        var transactionsSheet = excel.Workbook.Worksheets.Add("Hareketler");
                        DataTable dtTransactions = GetTransactionsDataTable(customerID);

                        // Title row
                        transactionsSheet.Cells["A1"].Value = $"{customerName} - Hesap Hareketleri";
                        transactionsSheet.Cells["A1:E1"].Merge = true;
                        transactionsSheet.Cells["A1"].Style.Font.Bold = true;
                        transactionsSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Add headers and data starting at row 2
                        transactionsSheet.Cells["A2"].LoadFromDataTable(dtTransactions, true);

                        // DYNAMIC RANGE HANDLING
                        int totalRows = dtTransactions.Rows.Count;
                        int startDataRow = 3; // Data starts at row 3 (row 1 = title, row 2 = headers)
                        int endDataRow = startDataRow + totalRows - 1;

                        // Only apply formatting if there's data
                        if (totalRows > 0)
                        {
                            // Format date column (column A)
                            ExcelRange dateRange = transactionsSheet.Cells[$"A{startDataRow}:A{endDataRow}"];
                            dateRange.Style.Numberformat.Format = "dd.mm.yyyy";

                            // Format amount column (column D)
                            ExcelRange amountRange = transactionsSheet.Cells[$"D{startDataRow}:D{endDataRow}"];
                            amountRange.Style.Numberformat.Format = "#,##0.00 ₺";

                            // Apply color coding to amount column
                            for (int row = startDataRow; row <= endDataRow; row++)
                            {
                                var typeCell = transactionsSheet.Cells[$"E{row}"].Value?.ToString();
                                var amountCell = transactionsSheet.Cells[$"D{row}"];

                                if (typeCell == "Gelir")
                                {
                                    amountCell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                                }
                                else if (typeCell == "Gider")
                                {
                                    amountCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                }
                            }
                        }

                        // Column settings
                        transactionsSheet.Column(1).Width = 12; // Tarih
                        transactionsSheet.Column(2).Width = 40; // Açıklama
                        transactionsSheet.Column(3).Width = 15; // Tutar
                        transactionsSheet.Column(4).AutoFit();  // Type (hidden)
                        transactionsSheet.Column(5).Hidden = true; // Hide Type column

                        // Auto-fit all columns
                        transactionsSheet.Cells[transactionsSheet.Dimension.Address].AutoFitColumns();

                        // Save file
                        excel.SaveAs(new FileInfo(sfd.FileName));
                    }

                    MessageBox.Show("Excel aktarımı başarıyla tamamlandı!", "Başarılı",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel aktarımı sırasında hata oluştu:\n{ex.Message}",
                                "Hata",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        
        private DataTable GetCustomersDataTable()
        {
            DataTable dt = new DataTable();
            dt.TableName = "Customers";

            using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM Customers", connection))
            {
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
                connection.Close();
            }
            return dt;
        }

       
        private DataTable GetTransactionsDataTable(int customerID)
        {
            DataTable dt = new DataTable();
            using (var cmd = new MySqlCommand(
                "SELECT Date, Description, Amount, Type FROM Transactions WHERE CustomerID = @customerID ORDER BY Date ASC",
                connection))
            {
                cmd.Parameters.AddWithValue("@customerID", customerID);
                connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
                connection.Close();
            }
            return dt;
        }

        public class PDFGenerator
        {
            public static void GeneratePdf(DataTable transactions, string customerName, string savePath)
            {
                try
                {
                    // Toplam bakiyeyi hesapla
                    decimal total = 0;
                    foreach (DataRow row in transactions.Rows)
                    {
                        if (decimal.TryParse(row["Amount"].ToString(), out decimal amount))
                        {
                            string type = row["Type"].ToString();
                            if (type.Equals("Gelir", StringComparison.OrdinalIgnoreCase))
                                total += amount;
                            else if (type.Equals("Gider", StringComparison.OrdinalIgnoreCase))
                                total -= amount;
                        }
                    }

                    Document.Create(document =>
                    {
                        document.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            // Header
                            page.Header()
                                .Column(column =>
                                {
                                    column.Item().Text($"{customerName} - Hesap Hareketleri")
                                        .Bold().FontSize(16).FontColor(Colors.Blue.Darken3);

                                    column.Item().PaddingTop(5).Text(DateTime.Today.ToString("dd.MM.yyyy HH:mm"))
                                        .FontSize(9).FontColor(Colors.Grey.Medium);
                                });

                            // Main Content
                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Column(col =>
                                {
                                    col.Item().Table(table =>
                                    {
                                        // Column Definitions
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f); // Tarih
                                            columns.RelativeColumn(3);    // Açıklama
                                            columns.RelativeColumn(1.5f); // Tutar
                                            columns.RelativeColumn(1.2f); // Tür
                                        });

                                        // Table Header with styling
                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten3)
                                                .Padding(5).Text("Tarih").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten3)
                                                .Padding(5).Text("Açıklama").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten3)
                                                .Padding(5).Text("Tutar").Bold();
                                            header.Cell().Background(Colors.Grey.Lighten3)
                                                .Padding(5).Text("Tür").Bold();
                                        });

                                        // Handle empty transactions
                                        if (transactions.Rows.Count == 0)
                                        {
                                            table.Cell().ColumnSpan(4)
                                                .PaddingVertical(10)
                                                .Text("Hesap hareketi bulunamadı")
                                                .Italic().AlignCenter();
                                        }

                                        // Data Rows
                                        foreach (DataRow row in transactions.Rows)
                                        {
                                            // Format date properly
                                            DateTime dateValue;
                                            bool validDate = DateTime.TryParse(row["Date"].ToString(), out dateValue);
                                            string dateStr = validDate ? dateValue.ToString("dd.MM.yyyy") : "Geçersiz Tarih";

                                            // Get transaction type
                                            string type = row["Type"].ToString();

                                            // Set amount color based on type
                                            Color amountColor = Colors.Black;
                                            if (type.Equals("Gelir", StringComparison.OrdinalIgnoreCase))
                                                amountColor = Colors.Green.Darken2;
                                            else if (type.Equals("Gider", StringComparison.OrdinalIgnoreCase))
                                                amountColor = Colors.Red.Darken2;

                                            // Format amount
                                            decimal amountValue;
                                            bool validAmount = decimal.TryParse(row["Amount"].ToString(), out amountValue);
                                            string amountStr = validAmount ? $"{amountValue:N2} ₺" : "Geçersiz Tutar";

                                            // Create cells
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5).Text(dateStr);

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5).Text(row["Description"].ToString());

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5).AlignRight()
                                                .Text(amountStr).FontColor(amountColor);

                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5).AlignCenter()
                                                .Text(type);
                                        }
                                    });

                                    // Toplam Bakiye satırı
                                    col.Item().PaddingTop(20).AlignRight().Text($"Toplam Bakiye: {total:N2} ₺")
                                        .Bold().FontSize(12)
                                        .FontColor(total >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                });

                            // Footer
                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("Sayfa ");
                                    x.CurrentPageNumber();
                                    x.Span(" / ");
                                    x.TotalPages();
                                });
                        });
                    }).GeneratePdf(savePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"PDF oluşturma hatası: {ex.Message}", ex);
                }
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null)
            {
                MessageBox.Show("Lütfen önce bir müşteri seçin!", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Dosyaları|*.pdf";
                sfd.FileName = $"{dgvCustomers.CurrentRow.Cells["Name"].Value}_Hareketler_{DateTime.Today:yyyyMMddHHmm}.pdf";
                sfd.OverwritePrompt = true;

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Get customer data
                    string customerName = dgvCustomers.CurrentRow.Cells["Name"].Value.ToString();
                    int customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);

                    // Get transactions
                    DataTable transactions = GetTransactionsDataTable(customerID);

                    // Generate PDF
                    PDFGenerator.GeneratePdf(transactions, customerName, sfd.FileName);

                    // Show success message
                    MessageBox.Show("PDF başarıyla oluşturuldu!", "Başarılı",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optional: Open the PDF
                    // Process.Start(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturma hatası:\n{ex.Message}",
                                    "Hata",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }
                        
        public static class DatabaseHelper
        {
            public static List<string> GetSuggestions(MySqlConnection conn)
            {
                var suggestions = new List<string>();
                bool closeAfter = false;
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                        closeAfter = true;
                    }

                    using (var cmd = new MySqlCommand("SELECT Description FROM Suggestions", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            suggestions.Add(reader["Description"].ToString());
                    }
                }
                finally
                {
                    if (closeAfter) conn.Close();
                }

                return suggestions;
            }

            public static bool AddSuggestion(MySqlConnection conn, string description)
            {
                try
                {
                    bool closeAfter = false;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                        closeAfter = true;
                    }

                    using (var cmd = new MySqlCommand("INSERT INTO Suggestions (Description) VALUES (@desc)", conn))
                    {
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.ExecuteNonQuery();
                    }

                    if (closeAfter) conn.Close();
                    return true;
                }
                catch (MySql.Data.MySqlClient.MySqlException)
                {
                    // Genellikle UNIQUE ihlali gibi durumlarda false döndür
                    return false;
                }
            }

            public static void RemoveSuggestion(MySqlConnection conn, string description)
            {
                bool closeAfter = false;
                try
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                        closeAfter = true;
                    }

                    using (var cmd = new MySqlCommand("DELETE FROM Suggestions WHERE Description = @desc", conn))
                    {
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    if (closeAfter) conn.Close();
                }
            }
        }

        private void InitializeAutoComplete()
        {
            txtDescription.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtDescription.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtDescription.AutoCompleteCustomSource = new AutoCompleteStringCollection();
        }

        private void LoadSuggestions()
        {
            var suggestions = DatabaseHelper.GetSuggestions(connection);
            txtDescription.AutoCompleteCustomSource.Clear();
            txtDescription.AutoCompleteCustomSource.AddRange(suggestions.ToArray());
            lstSuggestions.DataSource = suggestions;
        }

        // EKLEME BUTONU
        private void btnAddDescript_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                bool success = DatabaseHelper.AddSuggestion(connection, txtDescription.Text.Trim());

                if (success)
                {
                    LoadSuggestions(); // Listeyi yenile
                    txtDescription.Clear();
                }
                else
                    MessageBox.Show("Bu açıklama zaten mevcut!");
            }
        }

        // ÇIKARMA BUTONU
        private void btnRemoveDescript_Click(object sender, EventArgs e)
        {
            if (lstSuggestions.SelectedItem != null)
            {
                string selected = lstSuggestions.SelectedItem.ToString();
                DatabaseHelper.RemoveSuggestion(connection, selected);
                LoadSuggestions(); // Listeyi yenile
            }
        }
        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            var filtered = DatabaseHelper.GetSuggestions(connection)
                .Where(s => s.Contains(txtDescription.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            lstSuggestions.DataSource = filtered;
        }

        public class InputForm : Form
        {
            private TextBox textBox = new TextBox();
            private Button okButton = new Button();

            public string InputText => textBox.Text;

            public InputForm(string prompt)
            {
                InitializeComponents(prompt);
            }

            private void InitializeComponents(string prompt)
            {
                this.Text = "Giriş";
                this.Size = new System.Drawing.Size(300, 150);

                var label = new Label { Text = prompt, Left = 20, Top = 20, Width = 260 };
                textBox = new TextBox { Left = 20, Top = 50, Width = 260 };
                okButton = new Button { Text = "Tamam", Left = 110, Top = 80, Width = 80 };

                okButton.Click += (sender, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

                this.Controls.Add(label);
                this.Controls.Add(textBox);
                this.Controls.Add(okButton);

            }
        }
        public partial class EditCustomerForm : Form
        {
            private int _customerId;
            private string _currentName;
            private bool _edefter;
            private TextBox txtCustomerName;
            private CheckBox chkEDefter;
            private Button btnSave;

            public EditCustomerForm(int customerId, string currentName, bool edefter)
            {
                txtCustomerName = new TextBox();
                txtCustomerName.Location = new Point(20, 20);
                txtCustomerName.Size = new System.Drawing.Size(200, 25);
                this.Controls.Add(txtCustomerName);

                chkEDefter = new CheckBox();
                chkEDefter.Text = "E-Defter Müşterisi";
                chkEDefter.Location = new Point(20, 50);
                chkEDefter.Size = new System.Drawing.Size(200, 25);
                chkEDefter.Checked = edefter;
                this.Controls.Add(chkEDefter);

                _customerId = customerId;
                _currentName = currentName;
                _edefter = edefter;
                txtCustomerName.Text = _currentName;

                btnSave = new Button();
                btnSave.Location = new Point(20, 80);
                btnSave.Size = new System.Drawing.Size(200, 25);
                btnSave.Text = "Kaydet";
                btnSave.Click += (sender, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
                this.Controls.Add(btnSave);
            }

            public string UpdatedName => txtCustomerName.Text.Trim();
            public bool UpdatedEDefter => chkEDefter.Checked;
        }
        private void btnEditCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!");
                return;
            }

            int customerId = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
            string currentName = dgvCustomers.CurrentRow.Cells["Name"].Value.ToString();
            bool currentEDefter = Convert.ToBoolean(dgvCustomers.CurrentRow.Cells["EDefter"].Value);

            using (EditCustomerForm editForm = new EditCustomerForm(customerId, currentName, currentEDefter))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateCustomer(customerId, editForm.UpdatedName, editForm.UpdatedEDefter);
                    LoadCustomers(); // Listeyi yenile
                }
            }
        }
        private void UpdateCustomer(int customerId, string newName, bool edefter)
        {
            try
            {
                using (var cmd = new MySqlCommand(
                    "UPDATE Customers SET Name = @name, EDefter = @edefter WHERE CustomerID = @id", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@edefter", edefter ? 1 : 0); // Boolean to Integer
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                //MessageBox.Show("Müşteri başarıyla güncellendi!");
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        //
        private int GetCurrentCustomerId()
        {
            if (dgvCustomers.CurrentRow == null || dgvCustomers.CurrentRow.Cells["CustomerID"].Value == null)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!");
                return -1; // Hata durumunda -1 döndür
            }
            return Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
        }
        private void btnEditTransaction_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir hareket seçin!");
                return;
            }

            DataGridViewRow row = dgvTransactions.CurrentRow;
            int transactionId = Convert.ToInt32(row.Cells["TransactionID"].Value);
            DateTime date = Convert.ToDateTime(row.Cells["Date"].Value);
            string desc = row.Cells["Description"].Value.ToString();

            // Amount'ı her zaman InvariantCulture ile string'e çevir
            string amount = "";
            if (row.Cells["Amount"].Value is decimal dec)
                amount = dec.ToString(CultureInfo.InvariantCulture);
            else
                amount = Convert.ToDecimal(row.Cells["Amount"].Value).ToString(CultureInfo.InvariantCulture);

            string type = row.Cells["Type"].Value.ToString();

            using (EditTransactionForm editForm = new EditTransactionForm(date, desc, amount, type))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    UpdateTransaction(transactionId, editForm.TransactionDate, editForm.Description, editForm.Amount, editForm.Type);
                    LoadTransactions(GetCurrentCustomerId());
                }
            }
        }
        private void UpdateTransaction(int transactionId, DateTime date, string desc, string amount, string type)
        {
            try
            {
                // Nokta ve virgül ayrımını düzelt
                if (!decimal.TryParse(amount.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountValue))
                {
                    MessageBox.Show("Tutar değeri geçersiz!");
                    return;
                }

                using (var cmd = new MySqlCommand(
                    @"UPDATE Transactions 
              SET Date = @date, 
                  Description = @desc, 
                  Amount = @amount, 
                  Type = @type 
              WHERE TransactionID = @id", connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@amount", amountValue); // DÜZELTİLDİ
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@id", transactionId);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
                MessageBox.Show("Hareket başarıyla güncellendi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme hatası: " + ex.Message);
            }
        }

        public partial class EditTransactionForm : Form
        {
            public DateTime TransactionDate;
            public string Description;
            public string Amount;
            public string Type;

            public EditTransactionForm(DateTime date, string desc, string amount, string type)
            {


                // Kontrolleri manuel oluştur (Tasarım dosyası olmadan)
                this.Size = new System.Drawing.Size(350, 200);

                Label lblDate = new Label { Text = "Tarih:", Location = new Point(10, 10) };
                DateTimePicker dtpDate = new DateTimePicker { Location = new Point(110, 10), Width = 170 };

                Label lblDesc = new Label { Text = "Açıklama:", Location = new Point(10, 40) };
                TextBox txtDescription = new TextBox { Location = new Point(110, 40), Width = 150 };

                Label lblAmount = new Label { Text = "Tutar:", Location = new Point(10, 70) };
                TextBox nudAmount = new TextBox { Location = new Point(110, 70), Width = 150 };

                Label lblType = new Label { Text = "Tür:", Location = new Point(10, 100) };
                ComboBox cbType = new ComboBox { Location = new Point(110, 100), Width = 150 };
                cbType.Items.AddRange(new[] { "Gelir", "Gider" });

                Button btnSave = new Button { Text = "Kaydet", Location = new Point(110, 130) };

                // Değerleri ata
                dtpDate.Value = date;
                txtDescription.Text = desc;
                nudAmount.Text = amount;
                cbType.SelectedItem = type;

                // Event handler
                btnSave.Click += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(txtDescription.Text) && (string.IsNullOrEmpty(nudAmount.Text)) || cbType.SelectedIndex == -1)
                    {
                        MessageBox.Show("Tüm alanları doldurun!");
                        return;
                    }

                    TransactionDate = dtpDate.Value;
                    Description = txtDescription.Text;
                    Amount = nudAmount.Text;
                    Type = cbType.Text;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                // Kontrolleri forma ekle
                this.Controls.AddRange(new Control[] { lblDate, dtpDate, lblDesc, txtDescription, lblAmount, nudAmount, lblType, cbType, btnSave });
            }
        }

        private DataTable ImportExcelData(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            DataTable dt = new DataTable();
            dt.Columns.Add("Tarih", typeof(DateTime));
            dt.Columns.Add("Açıklama", typeof(string));
            dt.Columns.Add("Tutar", typeof(decimal));
            dt.Columns.Add("Tür", typeof(string)); // "Borç" veya "Alacak"

            FileInfo file = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 3; row <= rowCount; row++) // 1. satır başlık
                {
                    // Sütunlardan verileri oku (1-indexed)
                    DateTime tarih = worksheet.Cells[row, 1].GetValue<DateTime>();
                    string aciklama = worksheet.Cells[row, 2].Text;
                    decimal tutar = worksheet.Cells[row, 3].GetValue<decimal>();
                    //decimal alacak = worksheet.Cells[row, 3].GetValue<decimal>();
                    string tur = worksheet.Cells[row, 4].Text;
                                       
                    // DataTable'a ekle
                    dt.Rows.Add(tarih, aciklama, tutar, tur);
                }
            }
            return dt;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Dosyaları|*.xlsx;*.xls";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        DataTable importedData = ImportExcelData(ofd.FileName);
                        dgvTransactions.DataSource = importedData;
                        MessageBox.Show($"{importedData.Rows.Count} kayıt başarıyla yüklendi!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata: " + ex.Message);
                    }
                }
            }
        }

        private bool SaveToDatabase(DataTable data)
        {


            var customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);




            foreach (DataRow row in data.Rows)
            {
                using (var cmd = new MySqlCommand(

                                        @"INSERT INTO Transactions
                                        (CustomerID, Date, Description, Amount, Type)
                                        VALUES 
                                        (@cid,@date, @desc, @amount, @type)", connection))

                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@cid", customerID);
                    cmd.Parameters.AddWithValue("@date", row["Tarih"]);
                    cmd.Parameters.AddWithValue("@desc", row["Açıklama"]);
                    cmd.Parameters.AddWithValue("@amount", row["Tutar"]);
                    cmd.Parameters.AddWithValue("@type", row["Tür"]);
                    cmd.ExecuteNonQuery();
                    connection.Close();

                }
            }


            LoadTransactions(customerID);
            CalculateAndDisplayTotal(customerID);
            return true;
        }


        private bool ValidateExcelFormat(ExcelWorksheet worksheet)
        {
            // Başlık kontrolü
            if (worksheet.Cells[1, 1].Text != "Tarih" ||
                worksheet.Cells[1, 4].Text != "Açıklama" ||
                worksheet.Cells[1, 5].Text != "Borç" ||
                worksheet.Cells[1, 6].Text != "Alacak")
            {
                MessageBox.Show("Excel formatı uyumsuz! Doğru sütun başlıklarını kontrol edin.");
                return false;
            }
            return true;
        }

        private void btnSaveToDb_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.DataSource == null)
            {
                MessageBox.Show("Önce Excel'den veri aktarın!");
                return;
            }

            DataTable data = (DataTable)dgvTransactions.DataSource;
            if (SaveToDatabase(data))
            {
                MessageBox.Show($"{data.Rows.Count} kayıt başarıyla veritabanına kaydedildi!");
                // Listeyi yenile
                LoadTransactions(GetCurrentCustomerId());
            }

        }
        private void dgvTransactions_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvTransactions.Columns[e.ColumnIndex].Name == "Amount")
            {
                var row = dgvTransactions.Rows[e.RowIndex];
                var typeCell = row.Cells["Type"].Value;
                if (typeCell != null)
                {
                    switch (typeCell.ToString().ToLower())
                    {
                        case "gelir":
                            e.CellStyle.ForeColor = System.Drawing.Color.Green;
                            break;
                        case "gider":
                            e.CellStyle.ForeColor = System.Drawing.Color.Red;
                            break;
                        default:
                            e.CellStyle.ForeColor = dgvTransactions.DefaultCellStyle.ForeColor;
                            break;
                    }
                }
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {   /*
            // İsteğe bağlı: Uygulama başladığında sessizce kontrol et
            await CheckForUpdate();
            */
        }
       
        private void eDefterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var eDefterForm = new EDefterForm();
            eDefterForm.Show(); // Yeni pencereyi açar
        }

        private void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns)
        {
            // Tablo var mı kontrolü
            using (var cmd = new MySqlCommand())
            {
                cmd.Connection = connection;
                connection.Open();
                //
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
                connection.Close();
            }
        }

        public static async Task CheckForUpdate(IProgress<int> progress = null, IProgress<string> statusProgress = null)
        {
            string repoOwner = "yusatopaloglu-s";
            string repoName = "HesapTakip";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("HesapTakip");
                    client.Timeout = TimeSpan.FromSeconds(30);

                    // Versiyon kontrolü
                    statusProgress?.Report("Versiyon kontrol ediliyor...");
                    progress?.Report(10);

                    var json = await client.GetStringAsync($"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest");
                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string latestVersion = release.tag_name;
                    string downloadUrl = release.assets[0].browser_download_url;

                    // Versiyon karşılaştırması
                    if (latestVersion != Application.ProductVersion)
                    {
                        statusProgress?.Report("Yeni sürüm bulundu...");
                        progress?.Report(30);

                        var result = MessageBox.Show($"Yeni sürüm bulundu: {latestVersion}\nGüncellemek ister misiniz?", "Güncelleme", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            await DownloadAndInstallUpdate(downloadUrl, progress, statusProgress);
                        }
                        else
                        {
                            progress?.Report(100);
                            statusProgress?.Report("Güncelleme iptal edildi");
                        }
                    }
                    else
                    {
                        progress?.Report(100);
                        statusProgress?.Report("Uygulamanız güncel");
                        MessageBox.Show("Uygulamanız güncel.");
                    }
                }
            }
            catch (Exception ex)
            {
                statusProgress?.Report("Hata oluştu");
                MessageBox.Show($"Güncelleme kontrolü sırasında hata: {ex.Message}");
            }
        }

        private static async Task DownloadAndInstallUpdate(string downloadUrl, IProgress<int> progress, IProgress<string> statusProgress)
        {
            string tempZipFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(downloadUrl));

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    statusProgress?.Report("Güncelleme indiriliyor...");
                    progress?.Report(40);

                    // ZIP dosyasını indir
                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(tempZipFile, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            var buffer = new byte[8192];
                            var totalBytesRead = 0L;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (canReportProgress)
                                {
                                    var progressPercentage = (int)((double)totalBytesRead / totalBytes * 100);
                                    var overallProgress = 40 + (int)(progressPercentage * 0.5);
                                    progress?.Report(overallProgress);
                                    statusProgress?.Report($"İndiriliyor: {progressPercentage}%");
                                }
                            }
                        }
                    }

                    statusProgress?.Report("Dosyalar hazırlanıyor...");
                    progress?.Report(90);

                    // ZIP'i çıkart
                    string tempExtractPath = Path.Combine(Path.GetTempPath(), "HesapTakip_Update");
                    if (Directory.Exists(tempExtractPath))
                    {
                        Directory.Delete(tempExtractPath, true);
                    }

                    System.IO.Compression.ZipFile.ExtractToDirectory(tempZipFile, tempExtractPath);

                    // Güncelleme batch dosyasını oluştur ve çalıştır
                    CreateAndRunUpdateBatch(tempExtractPath);

                    statusProgress?.Report("Güncelleme başlatılıyor...");
                    progress?.Report(100);

                    // Uygulamayı kapat (batch dosyası gerisini halleder)
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                statusProgress?.Report("Hata oluştu");
                MessageBox.Show($"Güncelleme sırasında hata oluştu: {ex.Message}");
                CleanupTempFiles(tempZipFile);
            }
        }

        private static void CreateAndRunUpdateBatch(string updateFilesPath)
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string batchContent = $@"
@echo off
chcp 65001 >nul
echo HesapTakip Güncelleme Aracı
echo ===========================
echo.

echo Uygulama kapatılıyor...
timeout /t 2 /nobreak >nul

taskkill /f /im ""HesapTakip.exe"" >nul 2>&1
taskkill /f /im ""HesapTakip"" >nul 2>&1

echo Dosyalar kopyalanıyor...
xcopy ""{updateFilesPath}\*"" ""{appPath}"" /Y /E /I /Q

if %errorlevel% equ 0 (
    echo Güncelleme başarıyla tamamlandı!
    echo Uygulama yeniden başlatılıyor...
    
    cd /d ""{appPath}""
    start """" ""HesapTakip.exe""
) else (
    echo Hata: Dosyalar kopyalanamadı!
    pause
)

echo Temizlik yapılıyor...
rmdir /s /q ""{updateFilesPath}""

exit
";

            string batchFile = Path.Combine(Path.GetTempPath(), "HesapTakip_Update.bat");
            File.WriteAllText(batchFile, batchContent, System.Text.Encoding.UTF8);

            // Batch dosyasını çalıştır
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchFile}\"",
                WorkingDirectory = appPath,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        private static void CleanupTempFiles(string tempZipFile)
        {
            try
            {
                if (File.Exists(tempZipFile))
                    File.Delete(tempZipFile);
            }
            catch
            {
                // Temizlik hatasını görmezden gel
            }
        }
        private async void CheckUpdateButton_Click(object sender, EventArgs e)
        {
            // Progress bar'ı sıfırla ve göster
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            statusLabel.Text = "Güncelleme kontrol ediliyor...";

            var progress = new Progress<int>(percent =>
            {
                progressBar1.Value = percent;
            });

            var statusProgress = new Progress<string>(status =>
            {
                statusLabel.Text = status;
            });

            try
            {
                await CheckForUpdate(progress, statusProgress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
            finally
            {
                // İşlem tamamlandığında progress bar'ı gizle
                if (progressBar1.Value == 100)
                {
                    await Task.Delay(2000); // Tamamlandı mesajını göstermek için bekle
                    progressBar1.Visible = false;
                }
            }
        }


    }

}









