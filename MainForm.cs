using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.ComponentModel;
using System.Data;
using System.Globalization;



namespace HesapTakip

{
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using QuestPDF.Infrastructure;
    using System.Diagnostics;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        private IDatabaseOperations _db;
        private DataSet dataSet = new DataSet();
        private List<string> _suggestions = new List<string>();
        private static bool _versionChecked = false;
        private async void MainForm_Load(object sender, EventArgs e)
        {
            progressBar1.Visible = false;
            // Açılışta sessizce versiyon kontrolü yap
            _ = CheckForUpdatesSilentAsync();
        }
        public MainForm()
        {
            InitializeComponent();
            InitializeApplication();
        }
        private void InitializeApplication()
        {
            //Factory pattern ile database başlat
            if (!InitializeDatabase())
            {
                // Database başlatılamazsa uygulamayı kapat
                Application.Exit();
                return;
            }

            LoadCustomers();
            InitializeAutoComplete();
            LoadSuggestions();

            dtpDate.Value = DateTime.Today;
            dgvTransactions.CellFormatting += dgvTransactions_CellFormatting;
            toolStripStatusLabelVersion.Text = $"v{GetCurrentVersion()}";
        }
        private bool InitializeDatabase()
        {
            try
            {
                string connectionString = AppConfigHelper.DatabasePath;
                string databaseType = AppConfigHelper.DatabaseType;

                Debug.WriteLine($"MainForm InitializeDatabase - Type: {databaseType}, Connection: {connectionString}");

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
                {
                    MessageBox.Show("Database ayarları bulunamadı. Lütfen bağlantı ayarlarını yapılandırın.");
                    return OpenConnectionSettings();
                }

                // Factory'den database instance'ı al
                _db = DatabaseFactory.Create(databaseType, connectionString);
                Debug.WriteLine($"Database instance created: {_db.GetType().Name}");

                // Database'i initialize et
                _db.InitializeDatabase();
                Debug.WriteLine("Database initialized successfully");

                // Bağlantı testi
                if (!_db.TestConnection())
                {
                    MessageBox.Show("Database bağlantısı başarısız. Lütfen ayarları kontrol edin.");
                    return OpenConnectionSettings();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database başlatma hatası: {ex.Message}\nLütfen bağlantı ayarlarını kontrol edin.");
                Debug.WriteLine($"InitializeDatabase ERROR: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return OpenConnectionSettings();
            }
        }
        // Connection settings formunu aç
        private bool OpenConnectionSettings()
        {
            using (var settingsForm = new ConnectionSettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Yeni ayarlarla database'i yeniden başlat
                    return InitializeDatabase();
                }
                else
                {
                    return false;
                }
            }
        }


        private void btnResetSettings_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Tüm ayarlar sıfırlanacak ve bağlantı bilgileri silinecek. Emin misiniz?",
                "Ayarları Sıfırla",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Özel config dosyasını sil
                    if (File.Exists(AppConfigHelper.ConfigFilePath))
                    {
                        File.Delete(AppConfigHelper.ConfigFilePath);
                    }

                    // Eski settings'i sıfırla
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Ayarlar sıfırlandı. Uygulama kapatılıyor...");
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ayarlar sıfırlanırken hata oluştu: {ex.Message}");
                }
            }
        }

        public void LoadCustomers()
        {
            try
            {
                dgvCustomers.Columns.Clear();
                var dt = _db.GetCustomers();
                dgvCustomers.DataSource = dt;

                if (dgvCustomers.Columns["CustomerID"] != null)
                {
                    dgvCustomers.Columns["CustomerID"].Visible = false;
                    dgvCustomers.Columns["EDefter"].Visible = false;
                    dgvCustomers.Columns["Taxid"].Visible = false;
                    dgvCustomers.Columns["ActivityCode"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteriler yüklenirken hata: " + ex.Message);
            }
        }

        private void LoadTransactions(int customerID)
        {
            try
            {
                var dt = _db.GetTransactions(customerID);
                dataSet.Tables["Transactions"]?.Clear();
                dataSet.Tables.Add(dt);
                dgvTransactions.DataSource = dt;
                FormatTransactionGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlemler yüklenirken hata: " + ex.Message);
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
            return null;
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
                var Taxid = new TextBox
                {
                    Text = "",
                    Left = 20,
                    Top = 50,
                    Width = 200,
                    PlaceholderText = "TCKN / Vergi No (Opsiyonel - Max 11 karakter)",
                    MaxLength = 11 // En fazla 11 karakter
                };
                inputForm.Controls.Add(Taxid);

                var ActivityCode = new TextBox
                {
                    Text = "",
                    Left = 20,
                    Top = 110,
                    Width = 200,
                    PlaceholderText = "Faaliyet Kodu (Opsiyonel - Max 6 karakter)",
                    MaxLength = 6 // En fazla 6 karakter
                };
                inputForm.Controls.Add(ActivityCode);

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
                    // TaxID ve ActivityCode değerlerini al (boş bırakılabilir)
                    string taxIdText = Taxid.Text.Trim();
                    string activityCodeText = ActivityCode.Text.Trim();

                    // TaxID sadece rakam kontrolü (opsiyonel)
                    if (!string.IsNullOrEmpty(taxIdText) && !taxIdText.All(char.IsDigit))
                    {
                        MessageBox.Show("TCKN/Vergi No sadece rakamlardan oluşmalıdır!", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // ActivityCode sadece rakam kontrolü (opsiyonel)
                    if (!string.IsNullOrEmpty(activityCodeText) && !activityCodeText.All(char.IsDigit))
                    {
                        MessageBox.Show("Faaliyet Kodu sadece rakamlardan oluşmalıdır!", "Hata",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Boş değerleri null olarak gönder
                    string taxId = string.IsNullOrEmpty(taxIdText) ? null : taxIdText;
                    string activityCode = string.IsNullOrEmpty(activityCodeText) ? null : activityCodeText;

                    bool success = _db.AddCustomer(inputForm.InputText.Trim(), chkEDefter.Checked, taxId, activityCode);
                    if (success)
                    {
                        LoadCustomers();
                        //MessageBox.Show("Müşteri başarıyla eklendi!");
                    }
                    else
                    {
                        MessageBox.Show("Müşteri eklenirken hata oluştu!");
                    }
                }
            }
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;

            var customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);
            var customerName = dgvCustomers.CurrentRow.Cells["Name"].Value?.ToString() ?? "bu müşteri";

            var confirmResult = MessageBox.Show(
                $"{customerName} müşterisini ve tüm hareketlerini silmek istediğinize emin misiniz?\nBu işlem geri alınamaz!",
                "Müşteri Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            try
            {
                bool success = _db.DeleteCustomer(customerID);
                if (success)
                {
                    LoadCustomers();
                    dgvTransactions.DataSource = null;
                    // MessageBox.Show($"{customerName} başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateAndDisplayTotal(int customerID)
        {
            try
            {
                decimal total = _db.CalculateTotalBalance(customerID);
                lblTotal.Text = $"Toplam Bakiye: {total:N2} ₺";
                lblTotal.ForeColor = total >= 0 ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hesaplama hatası: " + ex.Message);
            }
        }


        private void btnAddTransaction_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Geçersiz tutar formatı!");
                return;
            }

            if (dgvCustomers.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!");
                return;
            }

            if (!ValidateTransactionType()) return;

            var customerID = Convert.ToInt32(dgvCustomers.CurrentRow.Cells["CustomerID"].Value);

            bool success = _db.AddTransaction(customerID, dtpDate.Value, txtDescription.Text, amount, GetSelectedTransactionType());

            if (success)
            {
                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                ClearTransactionInputs();
                // MessageBox.Show("İşlem başarıyla eklendi!");
            }
            else
            {
                MessageBox.Show("İşlem eklenirken hata oluştu!");
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

            if (confirmResult != DialogResult.Yes) return;

            try
            {
                int transactionID = Convert.ToInt32(dgvTransactions.CurrentRow.Cells["TransactionID"].Value);
                int customerID = GetCurrentCustomerId();

                bool success = _db.DeleteTransaction(transactionID);
                if (success)
                {
                    LoadTransactions(customerID);
                    CalculateAndDisplayTotal(customerID);
                    // MessageBox.Show("Hareket başarıyla silindi!");
                }
                else
                {
                    MessageBox.Show("Silme işlemi başarısız!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme hatası: " + ex.Message);
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
                            ExcelRange dateRange = transactionsSheet.Cells[$"B{startDataRow}:B{endDataRow}"];
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
                        transactionsSheet.Column(1).AutoFit();
                        transactionsSheet.Column(2).Width = 12; // Tarih
                        transactionsSheet.Column(3).Width = 40; // Açıklama
                        transactionsSheet.Column(4).Width = 15; // Tutar
                        transactionsSheet.Column(5).AutoFit();  // Type (hidden)
                        transactionsSheet.Column(6).Hidden = true; // Hide Type column

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

        private DataTable GetTransactionsDataTable(int customerID)
        {
            try
            {
                var dt = _db.GetTransactions(customerID);

                // Header'ları Türkçe'ye çevir
                if (dt.Columns.Contains("TransactionID"))
                    dt.Columns["TransactionID"].ColumnName = "İşlemID";
                if (dt.Columns.Contains("Date"))
                    dt.Columns["Date"].ColumnName = "Tarih";
                if (dt.Columns.Contains("Description"))
                    dt.Columns["Description"].ColumnName = "Açıklama";
                if (dt.Columns.Contains("Amount"))
                    dt.Columns["Amount"].ColumnName = "Tutar";
                if (dt.Columns.Contains("Type"))
                    dt.Columns["Type"].ColumnName = "Tür";

                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri alınırken hata: {ex.Message}");
                return new DataTable();
            }
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

        private void InitializeAutoComplete()
        {
            txtDescription.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtDescription.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtDescription.AutoCompleteCustomSource = new AutoCompleteStringCollection();
        }

        private void LoadSuggestions()
        {
            try
            {
                var suggestions = _db.GetSuggestions();
                txtDescription.AutoCompleteCustomSource.Clear();
                txtDescription.AutoCompleteCustomSource.AddRange(suggestions.ToArray());
                lstSuggestions.DataSource = suggestions;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Öneriler yüklenirken hata: " + ex.Message);
            }
        }

        private void btnAddDescript_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                bool success = _db.AddSuggestion(txtDescription.Text.Trim());
                if (success)
                {
                    LoadSuggestions();
                    txtDescription.Clear();
                    // MessageBox.Show("Öneri başarıyla eklendi!");
                }
                else
                {
                    MessageBox.Show("Bu açıklama zaten mevcut!");
                }
            }
        }

        private void btnRemoveDescript_Click(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("btnRemoveDescript_Click started");

                if (lstSuggestions.SelectedItem != null)
                {
                    string selected = lstSuggestions.SelectedItem.ToString();
                    Debug.WriteLine($"Selected suggestion to remove: {selected}");

                    // Önce onay al
                    var result = MessageBox.Show(
                        $"'{selected}' önerisini silmek istediğinizden emin misiniz?",
                        "Öneri Silme",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Debug.WriteLine("User confirmed deletion");

                        // Database tipini kontrol et
                        string databaseType = AppConfigHelper.DatabaseType;
                        Debug.WriteLine($"Current database type: {databaseType}");

                        bool success = _db.RemoveSuggestion(selected);
                        Debug.WriteLine($"RemoveSuggestion result: {success}");

                        if (success)
                        {
                            LoadSuggestions();
                            // MessageBox.Show("Öneri başarıyla silindi!", "Başarılı",
                            // MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Öneri silinirken hata oluştu veya öneri bulunamadı!", "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("User cancelled deletion");
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen silmek için bir öneri seçin!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"btnRemoveDescript_Click CRITICAL ERROR: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"Öneri silinirken beklenmeyen hata: {ex.Message}", "Kritik Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Debug.WriteLine("btnRemoveDescript_Click completed");
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var allSuggestions = _db.GetSuggestions();
                var filtered = allSuggestions
                    .Where(s => s.Contains(txtDescription.Text, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                lstSuggestions.DataSource = filtered;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Öneri filtreleme hatası: {ex.Message}");
            }
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
            private string _currentTaxId;
            private string _currentActivityCode;
            private TextBox txtCustomerName;
            private CheckBox chkEDefter;
            private Button btnSave;
            private TextBox txtCustomerTaxid;
            private TextBox txtCustomerActivityCode;


            public EditCustomerForm(int customerId, string currentName, bool edefter, string taxId, string activityCode)
            {
                _customerId = customerId;
                _currentName = currentName;
                _edefter = edefter;
                _currentTaxId = taxId;
                _currentActivityCode = activityCode;

                InitializeForm();
            }

            private void InitializeForm()
            {
                this.Text = "Müşteri Düzenle";
                this.Size = new System.Drawing.Size(300, 250);
                this.StartPosition = FormStartPosition.CenterParent;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                // Müşteri Adı
                Label lblName = new Label { Text = "Müşteri Adı:", Location = new Point(20, 20), Width = 100 };
                txtCustomerName = new TextBox { Location = new Point(120, 20), Width = 150, Text = _currentName };
                this.Controls.Add(lblName);
                this.Controls.Add(txtCustomerName);

                // E-Defter CheckBox
                chkEDefter = new CheckBox { Text = "E-Defter Müşterisi", Location = new Point(20, 50), Width = 200, Checked = _edefter };
                this.Controls.Add(chkEDefter);

                // TaxID
                Label lblTaxId = new Label { Text = "TCKN/Vergi No:", Location = new Point(20, 80), Width = 100 };
                txtCustomerTaxid = new TextBox
                {
                    Location = new Point(120, 80),
                    Width = 150,
                    Text = _currentTaxId,
                    MaxLength = 11,
                    PlaceholderText = "Max 11 karakter"
                };
                this.Controls.Add(lblTaxId);
                this.Controls.Add(txtCustomerTaxid);

                // Activity Code
                Label lblActivityCode = new Label { Text = "Faaliyet Kodu:", Location = new Point(20, 110), Width = 100 };
                txtCustomerActivityCode = new TextBox
                {
                    Location = new Point(120, 110),
                    Width = 150,
                    Text = _currentActivityCode,
                    MaxLength = 6,
                    PlaceholderText = "Max 6 karakter"
                };
                this.Controls.Add(lblActivityCode);
                this.Controls.Add(txtCustomerActivityCode);

                // Kaydet Butonu
                btnSave = new Button { Text = "Kaydet", Location = new Point(120, 150), Width = 80 };
                btnSave.Click += BtnSave_Click;
                this.Controls.Add(btnSave);

                // İptal Butonu
                Button btnCancel = new Button { Text = "İptal", Location = new Point(210, 150), Width = 60 };
                btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
                this.Controls.Add(btnCancel);
            }
            private void BtnSave_Click(object sender, EventArgs e)
            {
                // Validasyon
                if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
                {
                    MessageBox.Show("Müşteri adı boş olamaz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // TaxID validasyon (sadece rakam)
                if (!string.IsNullOrEmpty(txtCustomerTaxid.Text) && !txtCustomerTaxid.Text.All(char.IsDigit))
                {
                    MessageBox.Show("TCKN/Vergi No sadece rakamlardan oluşmalıdır!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ActivityCode validasyon (sadece rakam)
                if (!string.IsNullOrEmpty(txtCustomerActivityCode.Text) && !txtCustomerActivityCode.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Faaliyet Kodu sadece rakamlardan oluşmalıdır!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            public string UpdatedName => txtCustomerName.Text.Trim();
            public bool UpdatedEDefter => chkEDefter.Checked;
            public string UpdatedTaxId => string.IsNullOrEmpty(txtCustomerTaxid.Text.Trim()) ? null : txtCustomerTaxid.Text.Trim();
            public string UpdatedActivityCode => string.IsNullOrEmpty(txtCustomerActivityCode.Text.Trim()) ? null : txtCustomerActivityCode.Text.Trim();
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

            // Null kontrolü ile TaxID ve ActivityCode değerlerini al
            object taxIdValue = dgvCustomers.CurrentRow.Cells["TaxID"].Value;
            object activityCodeValue = dgvCustomers.CurrentRow.Cells["ActivityCode"].Value;

            string currentTaxId = taxIdValue != null && taxIdValue != DBNull.Value ? taxIdValue.ToString() : "";
            string currentActivityCode = activityCodeValue != null && activityCodeValue != DBNull.Value ? activityCodeValue.ToString() : "";

            using (EditCustomerForm editForm = new EditCustomerForm(customerId, currentName, currentEDefter, currentTaxId, currentActivityCode))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    bool success = _db.UpdateCustomer(customerId, editForm.UpdatedName, editForm.UpdatedEDefter, editForm.UpdatedTaxId, editForm.UpdatedActivityCode);
                    if (success)
                    {
                        LoadCustomers();
                        // MessageBox.Show("Müşteri başarıyla güncellendi!");
                    }
                    else
                    {
                        MessageBox.Show("Müşteri güncellenirken hata oluştu!");
                    }
                }
            }
        }
        private int GetCurrentCustomerId()
        {
            if (dgvCustomers.CurrentRow == null || dgvCustomers.CurrentRow.Cells["CustomerID"].Value == null)
            {
                MessageBox.Show("Lütfen bir müşteri seçin!");
                return -1;
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
                    // Amount formatını düzelt
                    if (!decimal.TryParse(editForm.Amount.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountValue))
                    {
                        MessageBox.Show("Tutar değeri geçersiz!");
                        return;
                    }

                    bool success = _db.UpdateTransaction(transactionId, editForm.TransactionDate, editForm.Description, amountValue, editForm.Type);
                    if (success)
                    {
                        LoadTransactions(GetCurrentCustomerId());
                        // MessageBox.Show("Hareket başarıyla güncellendi!");
                    }
                    else
                    {
                        MessageBox.Show("Hareket güncellenirken hata oluştu!");
                    }
                }
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
            try
            {
                var customerID = GetCurrentCustomerId();
                if (customerID == -1) return false;

                int successCount = 0;
                foreach (DataRow row in data.Rows)
                {
                    // DataTable kolon isimlerini kontrol et
                    DateTime date = row["Tarih"] != DBNull.Value ? Convert.ToDateTime(row["Tarih"]) : DateTime.Now;
                    string description = row["Açıklama"]?.ToString() ?? "";

                    if (!decimal.TryParse(row["Tutar"]?.ToString(), out decimal amount))
                        continue;

                    string type = row["Tür"]?.ToString() ?? "Gelir";

                    bool success = _db.AddTransaction(customerID, date, description, amount, type);
                    if (success) successCount++;
                }

                // Listeyi yenile
                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);

                MessageBox.Show($"{successCount} kayıt başarıyla veritabanına kaydedildi!");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}");
                return false;
            }
        }

        //BURAYI MODÜLER HALE GETİRECEZ - TARİH AÇIKLAMA AYNI KALSIN , BORÇ ALACAK TANNIMI İÇİN ESKİ KODDAN AYIRMA FONKSİYONUNU GETİR.
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

        private void eDefterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var eDefterForm = new EDefterForm();
            eDefterForm.Show();
        }

        private void eFaturaXMLExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_db == null || !InitializeDatabase()) // Bağlantı kontrolü
            {
                MessageBox.Show("Veritabanı bağlantısı kurulamadı. Lütfen ayarları kontrol edin.");
                return;
            }

            var efaturaxmlForm = new EFaturaxmlForm(_db); // _db'yi geçir
            efaturaxmlForm.Show();
        }
        public static async Task CheckForUpdate(IProgress<int> progress, IProgress<string> statusProgress)
        {
            string repoOwner = "yusatopaloglu-s";
            string repoName = "HesapTakip";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("HesapTakip");
                    client.Timeout = TimeSpan.FromSeconds(30);

                    statusProgress?.Report("Versiyon kontrol ediliyor...");
                    progress?.Report(10);

                    var json = await client.GetStringAsync($"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest");
                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string latestVersion = release.tag_name;
                    string downloadUrl = release.assets[0].browser_download_url;

                    // Versiyon karşılaştırması
                    Version currentVersion = GetCurrentVersion();
                    Version gitVersion = ParseVersion(latestVersion);

                    if (gitVersion > currentVersion)
                    {
                        statusProgress?.Report("Yeni sürüm bulundu...");
                        progress?.Report(30);

                        var result = MessageBox.Show($"Yeni sürüm bulundu: {latestVersion}\nMevcut sürüm: {currentVersion}\nGüncellemek ister misiniz?", "Güncelleme", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            await DownloadAndInstallUpdate(downloadUrl, progress, statusProgress);
                        }
                        else
                        {
                            progress?.Report(100);
                            statusProgress?.Report("Güncelleme iptal edildi");

                            await Task.Delay(2000);
                            progress?.Report(0);
                            statusProgress?.Report("");
                        }
                    }
                    else
                    {
                        progress?.Report(100);
                        statusProgress?.Report("Uygulamanız güncel");

                        await Task.Delay(1000);
                        MessageBox.Show("Uygulamanız güncel.");

                        await Task.Delay(1000);
                        progress?.Report(0);
                        statusProgress?.Report("");
                    }
                }
            }
            catch (Exception ex)
            {
                statusProgress?.Report("Hata oluştu");
                progress?.Report(0);
                MessageBox.Show($"Güncelleme kontrolü sırasında hata: {ex.Message}");

                await Task.Delay(1000);
                progress?.Report(0);
                statusProgress?.Report("");
            }
        }
        private async Task CheckForUpdatesSilentAsync()
        {
            string repoOwner = "yusatopaloglu-s";
            string repoName = "HesapTakip";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("HesapTakip");
                    client.Timeout = TimeSpan.FromSeconds(10); // Daha kısa timeout

                    var json = await client.GetStringAsync($"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest");
                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string latestVersion = release.tag_name;

                    Version currentVersion = GetCurrentVersion();
                    Version gitVersion = ParseVersion(latestVersion);

                    if (gitVersion > currentVersion)
                    {
                        // UI thread'inde status label'ı güncelle
                        this.Invoke(new Action(() =>
                        {
                            toolStripStatusLabelVersion.Text = $"v{currentVersion} - Yeni sürüm bulundu!";
                            toolStripStatusLabelVersion.ForeColor = System.Drawing.Color.OrangeRed;
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda sessiz kal
                Debug.WriteLine($"Sessiz güncelleme hatası: {ex.Message}");
            }
        }

        // Versiyon metodlarını da ekleyelim
        private static Version GetCurrentVersion()
        {
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            if (assemblyVersion == null || assemblyVersion.ToString() == "0.0.0.0")
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (Version.TryParse(versionInfo.FileVersion, out Version fileVersion))
                {
                    return fileVersion;
                }
            }

            return assemblyVersion ?? new Version(1, 0, 0);
        }

        // Git tag'ını Version formatına parse et
        private static Version ParseVersion(string versionString)
        {
            try
            {

                string cleanVersion = versionString.Trim().TrimStart('v', 'V');


                string versionOnly = System.Text.RegularExpressions.Regex.Match(cleanVersion, @"[\d\.]+").Value;

                return Version.Parse(versionOnly);
            }
            catch
            {

                return new Version(0, 0, 0);
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
                    CreateAndRunUpdateBatch(tempExtractPath, progress, statusProgress);

                    statusProgress?.Report("Güncelleme başlatılıyor...");
                    progress?.Report(100);

                    // Progress bar'ı kapatmak için biraz bekle
                    await Task.Delay(1000);

                    // Uygulamayı kapat (batch dosyası gerisini halleder)
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                statusProgress?.Report("Hata oluştu");
                progress?.Report(0); // Hata durumunda progress'i sıfırla

                // Hata mesajını göster ama progress bilgisini sıfırla
                MessageBox.Show($"Güncelleme sırasında hata oluştu: {ex.Message}");

                // Progress bar'ı temizle
                statusProgress?.Report("");
                progress?.Report(0);

                CleanupTempFiles(tempZipFile);
            }
        }

        private static void CreateAndRunUpdateBatch(string updateFilesPath, IProgress<int> progress, IProgress<string> statusProgress)
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

echo Ayarlar korunuyor...

:: Özel config dosyasını yedekle
if exist ""{appPath}\HesapTakip.config"" (
    copy ""{appPath}\HesapTakip.config"" ""{appPath}\HesapTakip.config.backup"" >nul
    echo Config yedeklendi
)

:: Yeni dosyaları kopyala
echo Güncelleme dosyaları kopyalanıyor...
xcopy ""{updateFilesPath}\*"" ""{appPath}"" /Y /E /I /Q

:: Config dosyasını geri yükle
if exist ""{appPath}\HesapTakip.config.backup"" (
    copy ""{appPath}\HesapTakip.config.backup"" ""{appPath}\HesapTakip.config"" >nul
    del ""{appPath}\HesapTakip.config.backup"" >nul
    echo Config geri yüklendi
)

if %errorlevel% equ 0 (
    echo.
    echo ✓ Güncelleme başarıyla tamamlandı!
    echo ✓ Uygulama yeniden başlatılıyor...
    echo.
    
    timeout /t 2 /nobreak >nul
    cd /d ""{appPath}""
    start """" ""HesapTakip.exe""
) else (
    echo.
    echo ✗ Hata: Güncelleme sırasında problem oluştu!
    pause
)

echo Temizlik yapılıyor...
if exist ""{updateFilesPath}"" rmdir /s /q ""{updateFilesPath}""

exit
";

            string batchFile = Path.Combine(Path.GetTempPath(), "HesapTakip_Update.bat");
            File.WriteAllText(batchFile, batchContent, System.Text.Encoding.UTF8);

            statusProgress?.Report("Güncelleme işlemi başlatılıyor...");

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchFile}\"",
                WorkingDirectory = appPath,
                UseShellExecute = false,
                CreateNoWindow = false
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
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            statusLabel.Text = "Güncelleme kontrol ediliyor...";
            statusLabel.Visible = true;

            var progress = new Progress<int>(percent =>
            {
                // Doğrudan güncelle - ToolStripProgressBar thread-safe
                progressBar1.Value = percent;
            });

            var statusProgress = new Progress<string>(status =>
            {
                statusLabel.Text = status;
                statusLabel.Visible = !string.IsNullOrEmpty(status);
            });

            try
            {
                await CheckForUpdate(progress, statusProgress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
                progressBar1.Visible = false;
                statusLabel.Visible = false;
            }
            finally
            {
                await Task.Delay(2000);
                progressBar1.Visible = false;
                statusLabel.Visible = false;
            }
        }

        private void link_yusa_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            string url = "https://github.com/yusatopaloglu-s/HesapTakip";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
