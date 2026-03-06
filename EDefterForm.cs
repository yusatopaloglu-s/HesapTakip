using System.ComponentModel;
using System.Data;
using System.Globalization;



namespace HesapTakip
{
    public partial class EDefterForm : Form
    {
        private IDatabaseOperations _db;

        public EDefterForm()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadCustomers();
            dtpDate.Value = DateTime.Today;
            dgvKontorList.CellFormatting += dgvKontorList_CellFormatting;
            dgvFirmaList.SelectionChanged += dgvCustomers_SelectionChanged;

        }
        private void InitializeDatabase()
        {
            try
            {
                string connectionString = AppConfigHelper.DatabasePath;
                string databaseType = AppConfigHelper.DatabaseType;

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseType))
                {
                    MessageBox.Show("Database ayarları bulunamadı. Lütfen bağlantı ayarlarını yapılandırın.");
                    this.Close();
                    return;
                }

                // Factory'den database instance'ı al
                _db = DatabaseFactory.Create(databaseType, connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database başlatma hatası: {ex.Message}");
                this.Close();
            }
        }

        private void LoadCustomers()
        {
            try
            {
                dgvFirmaList.Columns.Clear();

                var dt = _db.GetCustomers();

                // Filtre sonucu boşsa CopyToDataTable çağrmasın
                var filtered = dt.AsEnumerable()
                    .Where(row => row["EDefter"] != DBNull.Value && Convert.ToBoolean(row["EDefter"]));

                DataTable edefterRows;
                if (filtered.Any())
                {
                    edefterRows = filtered.CopyToDataTable();
                }
                else
                {
                    // Aynı şemada boş tablo oluştur
                    edefterRows = dt.Clone();
                }

                // Değişiklik: alfabetik sıralama (Name sütunu varsa)
                try
                {
                    if (edefterRows != null && edefterRows.Columns.Contains("Name"))
                    {
                        // DataView üzerinden Türkçe/varsayılan sıralama: Name ASC
                        var dv = edefterRows.DefaultView;
                        dv.Sort = "Name ASC";
                        dgvFirmaList.DataSource = dv;
                    }
                    else
                    {
                        dgvFirmaList.DataSource = edefterRows;
                    }
                }
                catch
                {
                    // Hata olursa doğrudan bağla (fallback)
                    dgvFirmaList.DataSource = edefterRows;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
                return;
            }

            // Tüm sütunlara güvenli erişim
            if (dgvFirmaList.Columns.Contains("CustomerID"))
                dgvFirmaList.Columns["CustomerID"].Visible = false;

            if (dgvFirmaList.Columns.Contains("EDefter"))
                dgvFirmaList.Columns["EDefter"].Visible = false;

            if (dgvFirmaList.Columns.Contains("Taxid"))
                dgvFirmaList.Columns["Taxid"].Visible = false;

            if (dgvFirmaList.Columns.Contains("ActivityCode"))
                dgvFirmaList.Columns["ActivityCode"].Visible = false;

            if (dgvFirmaList.Columns.Contains("IsDeleted"))
                dgvFirmaList.Columns["IsDeleted"].Visible = false;
        }

        private void LoadTransactions(int customerID)
        {
            try
            {
                var dt = _db.GetEDefterTransactions(customerID);
                dgvKontorList.DataSource = dt;
                FormatTransactionGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlemler yüklenirken hata: " + ex.Message);
            }
        }


        private void FormatTransactionGrid()
        {
            // Null ve column varlık kontrolleri
            if (dgvKontorList.Columns.Count == 0) return;

            if (dgvKontorList.Columns.Contains("TransactionID"))
                dgvKontorList.Columns["TransactionID"].Visible = false;

            if (dgvKontorList.Columns.Contains("Date"))
                dgvKontorList.Columns["Date"].HeaderText = "Tarih";

            if (dgvKontorList.Columns.Contains("Kontor"))
                dgvKontorList.Columns["Kontor"].HeaderText = "Kontor";

            if (dgvKontorList.Columns.Contains("Type"))
                dgvKontorList.Columns["Type"].Visible = false;

            // Row sayısı kontrolü
            if (dgvKontorList.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvKontorList.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Cell varlık ve null kontrolleri
                    if (row.Cells["Type"]?.Value != null && row.Cells["Kontor"] != null)
                    {
                        var typeValue = row.Cells["Type"].Value.ToString().ToLower();
                        var kontorCell = row.Cells["Kontor"];

                        kontorCell.Style.ForeColor = typeValue switch
                        {
                            "ekle" => Color.Green,
                            "cikar" => Color.Red,
                            _ => dgvKontorList.DefaultCellStyle.ForeColor
                        };
                    }
                }
            }

            //Column varlık kontrolü
            if (dgvKontorList.Columns.Contains("Date"))
            {
                dgvKontorList.Sort(dgvKontorList.Columns["Date"], ListSortDirection.Ascending);
                dgvKontorList.Columns["Date"].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
        }


        private void CalculateAndDisplayTotal(int customerID)
        {
            try
            {
                decimal total = _db.CalculateEDefterTotal(customerID);
                lblTotal.Text = $"Kontor: {total.ToString("N2")} ";
                lblTotal.ForeColor = total >= 0 ? Color.DarkGreen : Color.DarkRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hesaplama hatası: " + ex.Message);
            }
        }
        private void AddParameter(IDbCommand command, string parameterName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            command.Parameters.Add(param);
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

            // Kültüre duyarlı ve binlik/para sembollerini kabul eden parse
            if (!decimal.TryParse(txtkontor.Text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal kontor))
            {
                MessageBox.Show("Geçersiz tutar formatı!");
                return;
            }

            if (dgvFirmaList.CurrentRow != null)
            {
                var customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);

                if (!ValidateTransactionType()) return;

                bool success = _db.AddEDefterTransaction(customerID, dtpDate.Value, kontor, GetSelectedTransactionType());

                if (success)
                {
                    LoadTransactions(customerID);
                    CalculateAndDisplayTotal(customerID);
                    ClearTransactionInputs();
                    MessageBox.Show("Kontör başarıyla eklendi!");
                }
                else
                {
                    MessageBox.Show("Kayıt eklenemedi!");
                }
            }
        }

        private void btnsil_Click(object sender, EventArgs e)
        {
            if (dgvKontorList.CurrentRow == null || dgvKontorList.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Lütfen silinecek işlemi seçin.");
                return;
            }

            int transactionID = Convert.ToInt32(dgvKontorList.CurrentRow.Cells["TransactionID"].Value);
            int customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);

            bool success = _db.DeleteEDefterTransaction(transactionID);

            if (success)
            {
                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                MessageBox.Show("Kayıt başarıyla silindi!");
            }
            else
            {
                MessageBox.Show("Kayıt silinemedi!");
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
            // DÜZELTİLMİŞ: Daha açıklayıcı hata mesajı
            if (!decimal.TryParse(txtkontor.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal result) || result <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir pozitif sayı giriniz!", "Geçersiz Giriş");
                e.Cancel = true;
            }
        }

        private void txtAmount_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtkontor.Text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal amount))
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

            // Kültüre duyarlı parse kullan
            if (!decimal.TryParse(txtkontor.Text, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal kontor))
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
                    using (var connection = _db.GetConnection())
                    {
                        connection.Open();

                        string query = @"INSERT INTO EDefterTakip 
                                        (CustomerID, Date, Kontor, Type) 
                                        VALUES 
                                        (@cid, @date, @kontor, @type)";

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = query;
                            AddParameter(cmd, "@cid", customerID);
                            AddParameter(cmd, "@date", dtpDate.Value);
                            AddParameter(cmd, "@kontor", kontor);
                            AddParameter(cmd, "@type", GetSelectedTransactionType());
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadTransactions(customerID);
                    CalculateAndDisplayTotal(customerID);
                    ClearTransactionInputs();
                    MessageBox.Show("Kontör başarıyla çıkarıldı!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kayıt eklenemedi: " + ex.Message);
                }
            }
        }
        private void btnhepsi1_Click(object sender, EventArgs e)
        {
            var transactions = new List<EDefterTransaction>();

            foreach (DataGridViewRow row in dgvFirmaList.Rows)
            {
                if (row.IsNewRow || row.Cells["CustomerID"].Value == null) continue;

                int customerID = Convert.ToInt32(row.Cells["CustomerID"].Value);
                transactions.Add(new EDefterTransaction
                {
                    CustomerID = customerID,
                    Date = dtpDate.Value,
                    Kontor = 1,
                    Type = "cikar"
                });
            }

            bool success = _db.BulkUpdateEDefterTransactions(transactions);

            if (success)
            {
                if (dgvFirmaList.CurrentRow != null)
                {
                    int selectedCustomerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);
                    LoadTransactions(selectedCustomerID);
                    CalculateAndDisplayTotal(selectedCustomerID);
                }
                MessageBox.Show($"{transactions.Count} firmadan 1 kontör çıkarıldı!");
            }
            else
            {
                MessageBox.Show("Toplu işlem sırasında hata oluştu!");
            }
        }

        private void btnedit_Click(object sender, EventArgs e)
        {
            // Seçili satır ve geçerli firma kontrolü
            if (dgvKontorList.CurrentRow == null || dgvKontorList.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Lütfen düzenlenecek işlemi seçin.");
                return;
            }

            if (dgvFirmaList.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir firma seçin.");
                return;
            }

            // Mevcut Kontor değerini oku (güvenli)
            object cellVal = dgvKontorList.CurrentRow.Cells["Kontor"]?.Value;
            decimal currentKontor = 0m;
            if (cellVal is decimal decVal)
            {
                currentKontor = decVal;
            }
            else
            {
                string s = cellVal?.ToString() ?? "0";
                if (!decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out currentKontor))
                {
                    // fallback invariant
                    decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out currentKontor);
                }
            }

            // Kullanıcıdan yeni değeri al
            if (!TryShowEditKontorDialog(currentKontor, out decimal newKontor))
            {
                // Kullanıcı iptal etti veya doğrulama başarısız oldu
                return;
            }

            // TransactionID oku
            int transactionID;
            try
            {
                transactionID = Convert.ToInt32(dgvKontorList.CurrentRow.Cells["TransactionID"].Value);
            }
            catch
            {
                MessageBox.Show("Seçilen kayıttan TransactionID okunamadı.");
                return;
            }

            int customerID = Convert.ToInt32(dgvFirmaList.CurrentRow.Cells["CustomerID"].Value);

            try
            {
                using (var connection = _db.GetConnection())
                {
                    connection.Open();

                    string query = @"UPDATE EDefterTakip 
                                     SET Kontor = @kontor 
                                     WHERE TransactionID = @id";

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        AddParameter(cmd, "@kontor", newKontor);
                        AddParameter(cmd, "@id", transactionID);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadTransactions(customerID);
                CalculateAndDisplayTotal(customerID);
                ClearTransactionInputs();
                MessageBox.Show("Kontör değeri başarıyla güncellendi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message);
            }
        }

        // ADD: Sınıfa ekleyin (private yardımcı metot)
        private bool TryShowEditKontorDialog(decimal currentValue, out decimal newValue)
        {
            newValue = 0m;

            using (var dlg = new Form())
            {
                dlg.Text = "Kontör Düzenle";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new System.Drawing.Size(320, 130);

                var lbl = new Label
                {
                    Text = "Yeni Kontör değeri:",
                    AutoSize = false,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Left = 10,
                    Top = 10,
                    Width = 300,
                    Height = 20
                };

                var txt = new TextBox
                {
                    Left = 10,
                    Top = 35,
                    Width = 300,
                    Text = currentValue.ToString("N2", CultureInfo.CurrentCulture)
                };

                var btnSave = new Button
                {
                    Text = "Kaydet",
                    DialogResult = DialogResult.OK,
                    Left = 140,
                    Width = 80,
                    Top = 70
                };

                var btnCancel = new Button
                {
                    Text = "İptal",
                    DialogResult = DialogResult.Cancel,
                    Left = 230,
                    Width = 80,
                    Top = 70
                };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(txt);
                dlg.Controls.Add(btnSave);
                dlg.Controls.Add(btnCancel);

                dlg.AcceptButton = btnSave;
                dlg.CancelButton = btnCancel;

                var res = dlg.ShowDialog(this);
                if (res != DialogResult.OK)
                    return false;

                // Doğrulama: kültüre duyarlı parse
                if (!decimal.TryParse(txt.Text?.Trim(), NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal parsed) || parsed <= 0m)
                {
                    MessageBox.Show("Lütfen geçerli bir pozitif sayı giriniz!", "Geçersiz Giriş", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                newValue = parsed;
                return true;
            }
        }
    }
}
