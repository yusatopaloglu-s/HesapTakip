﻿using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace HesapTakip
{


    public partial class EFaturaxmlForm : Form
    {

        public class InvoiceData
        {
            public string InvoiceType { get; set; }
            public string IssueDate { get; set; }
            public string InvoiceNumber { get; set; }
            public string SupplierTaxId { get; set; }
            public string SupplierName { get; set; }
            public string CustomerTaxId { get; set; }
            public string CustomerName { get; set; }
            public string cFirstName { get; set; }
            public string cFamilyName { get; set; }
            public string TaxExemptAmount0 { get; set; }
            public string TaxableAmount1 { get; set; }
            public string TaxAmount1 { get; set; }
            public string TaxableAmount8 { get; set; }
            public string TaxAmount8 { get; set; }
            public string TaxableAmount10 { get; set; }
            public string TaxAmount10 { get; set; }
            public string TaxableAmount18 { get; set; }
            public string TaxAmount18 { get; set; }
            public string TaxableAmount20 { get; set; }
            public string TaxAmount20 { get; set; }
            public string DepositAmount { get; set; }
            public string Oiv { get; set; }
            public string TotalPayable { get; set; }
            public string UUID { get; set; }
            public string PaymentMethod { get; set; }
            public string KdvExemptionTable { get; set; }
            public string KdvExemptionCode { get; set; }
            public string SaleType { get; set; }
            public string ActivityCode { get; set; }
            public string SubRecordType { get; set; }
            public string ItemName { get; set; }
            public string Quantity { get; set; }
            public string UnitPrice { get; set; }
            public string TaxableAmount { get; set; }
            public string TaxAmount { get; set; }
            public string Percent { get; set; }
        }

        private List<InvoiceData> BilancoSatisData = new List<InvoiceData>();
        private List<InvoiceData> BilancoAlisData = new List<InvoiceData>();
        private List<InvoiceData> LucaIsletmeSatisData = new List<InvoiceData>();
        private List<InvoiceData> LucaIsletmeAlisData = new List<InvoiceData>();
        private IDatabaseOperations _db;
        private Dictionary<string, string> _expenseMatchings; // ItemName -> SubRecordType

        public EFaturaxmlForm(IDatabaseOperations db)
        {
            _db = db;
            InitializeComponent();
            InitializeComboBox();
            LoadExpenseMatchings();


        }
        private void InitializeComboBox()
        {
            cmbTableSelector.Items.AddRange(new string[]
            {
                "Bilanço Satış",
                "Bilanço Alış",
                "Luca İşletme Satış",
                "Luca İşletme Alış",

            });
            cmbTableSelector.SelectedIndex = 0;

            UpdateDataGridView();

            if (_db != null)
            {
                if (_db.GetCustomers() is DataTable customers)
                {
                    // ComboBox'ı temizle
                    cbx_customerlist.Items.Clear();

                    // Boş seçenek ekle (null değer için)
                    cbx_customerlist.Items.Add(new { ID = -1, Name = "Seçiniz", ActivityCode = (string)null });

                    foreach (DataRow row in customers.Rows)
                    {
                        var activityCode = row["ActivityCode"] == DBNull.Value ? null : row["ActivityCode"].ToString();
                        cbx_customerlist.Items.Add(new
                        {
                            ID = row["CustomerID"],
                            Name = row["Name"],
                            ActivityCode = activityCode
                        });
                    }

                    cbx_customerlist.DisplayMember = "Name";
                    cbx_customerlist.ValueMember = "ActivityCode";
                    cbx_customerlist.SelectedIndex = 0;

                }
                else
                {
                    MessageBox.Show($"Müşteri verileri alınamadı:", "Hata");
                }
            }
            UpdateDataGridView();
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "XML Files|*.xml"
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        var xmlDoc = XDocument.Load(file);
                        ProcessXml(xmlDoc);
                    }
                    UpdateDataGridView();
                }
            }
        }
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"{cmbTableSelector.SelectedItem}_Export_{DateTime.Now:yyyyMMdd_HHmmss}_Part1.xlsx"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (dgvData.Rows.Count == 0)
                        {
                            MessageBox.Show("Seçili tabloda veri bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        int maxRowsPerFile = 600; // Başlık dahil toplam 600 satır
                        int maxDataRowsPerFile = maxRowsPerFile - 1; // Başlık hariç 599 veri satırı
                        int totalDataRows = dgvData.Rows.Count;
                        int fileCount = (int)Math.Ceiling((double)totalDataRows / maxDataRowsPerFile);

                        string baseFilePath = Path.Combine(
                            Path.GetDirectoryName(saveFileDialog.FileName),
                            Path.GetFileNameWithoutExtension(saveFileDialog.FileName).Replace("_Part1", "")
                        );

                        for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
                        {
                            string filePath = $"{baseFilePath}_Part{fileIndex + 1}.xlsx";

                            using (var package = new ExcelPackage(new FileInfo(filePath)))
                            {
                                var worksheet = package.Workbook.Worksheets.Add(cmbTableSelector.SelectedItem.ToString());

                                // Başlık satırını ekle (1 satır)
                                for (int col = 0; col < dgvData.Columns.Count; col++)
                                {
                                    worksheet.Cells[1, col + 1].Value = dgvData.Columns[col].HeaderText;
                                }

                                // Veri satırlarını ekle (599 satırlık parçalar halinde)
                                int startRow = fileIndex * maxDataRowsPerFile;
                                int endRow = Math.Min((fileIndex + 1) * maxDataRowsPerFile, totalDataRows);
                                int excelRow = 2; // Excel'de 2. satırdan başla (1. satır başlık)

                                for (int row = startRow; row < endRow; row++)
                                {
                                    for (int col = 0; col < dgvData.Columns.Count; col++)
                                    {
                                        worksheet.Cells[excelRow, col + 1].Value = dgvData.Rows[row].Cells[col].Value;
                                    }
                                    excelRow++;
                                }

                                worksheet.Cells.AutoFitColumns();
                                package.Save();
                            }
                        }

                        string message = fileCount > 1
                            ? $"{fileCount} adet Excel dosyası başarıyla oluşturuldu. (Her dosyada başlık dahil {maxRowsPerFile} satır)"
                            : "Excel dosyası başarıyla oluşturuldu.";

                        MessageBox.Show(message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Excel dosyası oluşturulurken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExportCSV_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                FileName = $"{cmbTableSelector.SelectedItem}_Export_{DateTime.Now:yyyyMMdd_HHmmss}_Part1.csv"
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (dgvData.Rows.Count == 0)
                        {
                            MessageBox.Show("Seçili tabloda veri bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int maxRowsPerFile = 600;
                        int maxDataRowsPerFile = maxRowsPerFile - 1;
                        int totalDataRows = dgvData.Rows.Count;
                        int fileCount = (int)Math.Ceiling((double)totalDataRows / maxDataRowsPerFile);

                        string baseFilePath = Path.Combine(
                            Path.GetDirectoryName(saveFileDialog.FileName),
                            Path.GetFileNameWithoutExtension(saveFileDialog.FileName).Replace("_Part1", "")
                        );

                        var trCulture = new CultureInfo("tr-TR"); 

                        for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
                        {
                            string filePath = $"{baseFilePath}_Part{fileIndex + 1}.csv";

                            using (var writer = CreateCsvWriter(filePath))
                            {
                                // Başlık satırını yaz
                                var headers = new List<string>();
                                for (int col = 0; col < dgvData.Columns.Count; col++)
                                {
                                    headers.Add(EscapeCsvField(dgvData.Columns[col].HeaderText, ';'));
                                }
                                writer.WriteLine(string.Join(";", headers));

                                // Veri satırlarını yaz (599 satırlık parçalar halinde)
                                int startRow = fileIndex * maxDataRowsPerFile;
                                int endRow = Math.Min((fileIndex + 1) * maxDataRowsPerFile, totalDataRows);

                                for (int row = startRow; row < endRow; row++)
                                {
                                    var fields = new List<string>();
                                    for (int col = 0; col < dgvData.Columns.Count; col++)
                                    {
                                        var cellValue = dgvData.Rows[row].Cells[col].Value;
                                        string formattedValue = FormatCellForCsv(cellValue, trCulture);
                                        fields.Add(EscapeCsvField(formattedValue, ';'));
                                    }
                                    writer.WriteLine(string.Join(";", fields));
                                }
                            }
                        }

                        string message = fileCount > 1
                            ? $"{fileCount} adet CSV dosyası başarıyla oluşturuldu. (Her dosyada başlık dahil {maxRowsPerFile} satır)"
                            : "CSV dosyası başarıyla oluşturuldu.";

                        MessageBox.Show(message, "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"CSV dosyası oluşturulurken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private StreamWriter CreateCsvWriter(string path, bool utf8Bom = false)
        {
            // For better compatibility with Excel in Turkish locale use Windows-1254 by default.
            // If utf8Bom == true, write UTF-8 with BOM.
            if (utf8Bom)
                return new StreamWriter(path, false, new UTF8Encoding(true));

            return new StreamWriter(path, false, Encoding.GetEncoding(1254));
        }

        private Encoding DetectFileEncoding(string path)
        {
            // Detect BOM if present. If none, assume Windows-1254 for Turkish CSV compatibility.
            var bom = new byte[4];
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    int read = fs.Read(bom, 0, Math.Min(4, (int)fs.Length));
                }
            }
            catch
            {
                return Encoding.GetEncoding(1254);
            }

            if (bom.Length >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;
            if (bom.Length >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode; // UTF-16 LE
            if (bom.Length >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode; // UTF-16 BE
            // default to Windows-1254 for Turkish CSV/Excel compatibility
            return Encoding.GetEncoding(1254);
        }

        private StreamWriter CreateUtf8Writer(string path)
        {
            return new StreamWriter(path, false, new UTF8Encoding(true));
        }

        private string FormatCellForCsv(object val, CultureInfo trCulture)
        {
            if (val == null || val == DBNull.Value) return "";

            string result;
            switch (val)
            {
                case DateTime dt:
                    result = dt.ToString("dd.MM.yyyy", trCulture);
                    break;
                case double d:
                    result = d.ToString("N2", trCulture);
                    break;
                case float f:
                    result = f.ToString("N2", trCulture);
                    break;
                case decimal m:
                    result = m.ToString("N2", trCulture);
                    break;
                case int i:
                    result = i.ToString(trCulture);
                    break;
                case long l:
                    result = l.ToString(trCulture);
                    break;
                case string s:
                    // String değerlerde BOM karakterini temizle
                    result = s.TrimStart('\uFEFF', '\u200B');
                    break;
                default:
                    // Sayısal değerler için genel kontrol
                    if (val.GetType().IsPrimitive && !(val is bool) && !(val is char))
                    {
                        if (double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
                        {
                            result = dbl.ToString("N2", trCulture);
                            break;
                        }
                    }
                    result = val.ToString()?.TrimStart('\uFEFF', '\u200B') ?? "";
                    break;
            }

            return result;
        }

        private string EscapeCsvField(string field, char separator = ';')
        {
            if (string.IsNullOrEmpty(field))
                return "";

           
            field = field.TrimStart('\uFEFF', '\u200B');



            if (field.Contains(separator) || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }


        private void CmbTableSelector_SelectedIndexChanged(object sender, EventArgs e)
        {

            var selectedItem = cbx_customerlist.SelectedItem;


            UpdateDataGridView();
        }

        
        private void CbxCustomerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbx_customerlist.SelectedItem != null)
            {
                dynamic selectedItem = cbx_customerlist.SelectedItem;
                string activityCode = selectedItem.ActivityCode ?? ""; // null kontrolü

                // TextBox'ı güncelle
                txtbox_act.Text = activityCode;

            }
            else
            {
                txtbox_act.Text = "";
            }

            UpdateDataGridView();

        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Tabloyu boşaltmak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {

                BilancoSatisData.Clear();
                BilancoAlisData.Clear();
                LucaIsletmeSatisData.Clear();
                LucaIsletmeAlisData.Clear();
                UpdateDataGridView();


            }


        }

        public class TextCleaner
        {
            public static string CleanText(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return input;

                // Noktalama işaretlerini kaldır, sadece harf, rakam ve boşlukları koru
                return Regex.Replace(input, @"[^\w\s]", "");
            }
        }
        private void btn_alttur_Click(object sender, EventArgs e)
        {
            if (_db == null)
            {
                MessageBox.Show("Veritabanı bağlantısı başlatılamadı!");
                return;
            }
            using (var categoryForm = new ExpenseCategoryForm(_db))
            {
                categoryForm.ShowDialog();
            }
            LoadExpenseMatchings(); 
        }
        private void LoadExpenseMatchings()
        {
            try
            {
                var matchingsTable = _db.GetExpenseMatchings();
                _expenseMatchings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in matchingsTable.Rows)
                {
                    string itemName = row["ItemName"].ToString();
                    string subRecordType = row["SubRecordType"].ToString();
                    if (!string.IsNullOrEmpty(itemName) && !_expenseMatchings.ContainsKey(itemName))
                    {
                        _expenseMatchings[itemName] = subRecordType;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eşleştirmeler yüklenirken hata: {ex.Message}");
            }
        }
        private void ProcessXml(XDocument xml)
        {
            try
            {
                var ns = xml.Root.GetDefaultNamespace();
                var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                var cac = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

                var invoiceType = xml.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.Value ?? "";
                var issueDate = xml.Descendants(cbc + "IssueDate").FirstOrDefault()?.Value ?? "";
                var invoiceNumber = xml.Descendants(cbc + "ID").FirstOrDefault()?.Value ?? "";
                var supplierTaxId = xml.Descendants(cac + "AccountingSupplierParty").Descendants(cac + "Party").Descendants(cac + "PartyIdentification").Descendants(cbc + "ID").FirstOrDefault()?.Value ?? "";
                var supplierNameNode = xml.Descendants(cac + "AccountingSupplierParty").Descendants(cac + "Party").Descendants(cac + "PartyName").Descendants(cbc + "Name").FirstOrDefault()?.Value ?? "";
                var supplierFirstName = xml.Descendants(cac + "AccountingSupplierParty").Descendants(cac + "Party").Descendants(cac + "Person").Descendants(cbc + "FirstName").FirstOrDefault()?.Value ?? "";
                var supplierFamilyName = xml.Descendants(cac + "AccountingSupplierParty").Descendants(cac + "Party").Descendants(cac + "Person").Descendants(cbc + "FamilyName").FirstOrDefault()?.Value ?? "";
                var customerTaxId = xml.Descendants(cac + "AccountingCustomerParty").Descendants(cac + "Party").Descendants(cac + "PartyIdentification").Descendants(cbc + "ID").FirstOrDefault()?.Value ?? "";
                var customerNameNode = TextCleaner.CleanText(xml.Descendants(cac + "AccountingCustomerParty").Descendants(cac + "Party").Descendants(cac + "PartyName").Descendants(cbc + "Name").FirstOrDefault()?.Value ?? ""); 
                var customerFirstName = TextCleaner.CleanText(xml.Descendants(cac + "AccountingCustomerParty").Descendants(cac + "Party").Descendants(cac + "Person").Descendants(cbc + "FirstName").FirstOrDefault()?.Value ?? "");
                var customerFamilyName = TextCleaner.CleanText(xml.Descendants(cac + "AccountingCustomerParty").Descendants(cac + "Party").Descendants(cac + "Person").Descendants(cbc + "FamilyName").FirstOrDefault()?.Value ?? "");
                var uuid = xml.Descendants(cbc + "UUID").FirstOrDefault()?.Value ?? "";
                var paymentMeansCode = xml.Descendants(cac + "PaymentMeans").Descendants(cbc + "PaymentMeansCode").FirstOrDefault()?.Value ?? "";

                var supplierName = string.IsNullOrEmpty(supplierNameNode) ? $"{supplierFirstName} {supplierFamilyName}".Trim() : supplierNameNode;
                var customerName = string.IsNullOrEmpty(customerNameNode) ? $"{customerFirstName} {customerFamilyName}".Trim() : customerNameNode;
                var cFirstName = string.IsNullOrEmpty(customerNameNode) ? $"{customerFirstName}".Trim() : customerNameNode;
                var cFamilyName = string.IsNullOrEmpty(customerNameNode) ? $"{customerFamilyName}".Trim() : customerNameNode;

                var paymentMethod = paymentMeansCode switch
                {
                    "48" => "KREDI",
                    "30" => "EFT",
                    "31" => "ÇEK",
                    _ => ""
                };

                double taxExemptAmount0 = 0.0;
                double taxableAmount1 = 0.0;
                double taxAmount1 = 0.0;
                double taxableAmount8 = 0.0;
                double taxAmount8 = 0.0;
                double taxableAmount10 = 0.0;
                double taxAmount10 = 0.0;
                double taxableAmount18 = 0.0;
                double taxAmount18 = 0.0;
                double taxableAmount20 = 0.0;
                double taxAmount20 = 0.0;
                double oiv = 0.0;

                // Process only invoice-level TaxTotal's TaxSubtotals
                var invoiceTaxTotal = xml.Root.Elements(cac + "TaxTotal").FirstOrDefault();
                var taxSubtotals = invoiceTaxTotal?.Elements(cac + "TaxSubtotal") ?? Enumerable.Empty<XElement>();
                foreach (var taxSubtotal in taxSubtotals)
                {
                    var percent = taxSubtotal.Descendants(cbc + "Percent").FirstOrDefault()?.Value ?? "0";
                    var taxableAmount = taxSubtotal.Descendants(cbc + "TaxableAmount").FirstOrDefault()?.Value ?? "0";
                    var taxAmount = taxSubtotal.Descendants(cbc + "TaxAmount").FirstOrDefault()?.Value ?? "0";
                    double percentVal = double.TryParse(percent, NumberStyles.Any, CultureInfo.InvariantCulture, out var p) ? p : 0.0;
                    double taxableVal = double.TryParse(taxableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var t) ? t : 0.0;
                    double taxVal = double.TryParse(taxAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : 0.0;

                   
                    if (percentVal == 0.0)
                    {
                        taxExemptAmount0 = taxableVal;
                    }
                    else if (percentVal == 1.0)
                    {
                        taxableAmount1 = taxableVal;
                        taxAmount1 = taxVal;
                    }
                    else if (percentVal == 8.0)
                    {
                        taxableAmount8 = taxableVal;
                        taxAmount8 = taxVal;
                    }
                    else if (percentVal == 10.0)
                    {
                        taxableAmount10 = taxableVal;
                        taxAmount10 = taxVal;
                    }
                    else if (percentVal == 18.0)
                    {
                        taxableAmount18 = taxableVal;
                        taxAmount18 = taxVal;
                    }
                    else if (percentVal == 20.0)
                    {
                        taxableAmount20 = taxableVal;
                        taxAmount20 = taxVal;
                    }
                }
                string activityCode = "";
                if (cbx_customerlist.SelectedItem != null)
                {
                    dynamic selectedItem = cbx_customerlist.SelectedItem;
                    activityCode = selectedItem.ActivityCode ?? "";
                }

                // TextBox'ı güncelle (görsel doğrulama için)
                txtbox_act.Text = activityCode;

                var totalTaxAmount = invoiceType == "TEVKIFAT" ? xml.Descendants(cac + "TaxTotal").Descendants(cbc + "TaxAmount").FirstOrDefault()?.Value ?? "0.00" : (taxAmount20 != 0.0 ? taxAmount20.ToString("F2", CultureInfo.InvariantCulture) : taxAmount18.ToString("F2", CultureInfo.InvariantCulture));
                var legalMonetaryTotal = xml.Descendants(cac + "LegalMonetaryTotal");
                var taxExclusiveAmount = legalMonetaryTotal.Descendants(cbc + "TaxExclusiveAmount").FirstOrDefault()?.Value ?? "0.00";
                var payableAmount = legalMonetaryTotal.Descendants(cbc + "PayableAmount").FirstOrDefault()?.Value ?? "0.00";
                var depositCode = xml.Descendants(cac + "TaxTotal").Descendants(cac + "TaxSubtotal").Descendants(cac + "TaxCategory").Descendants(cbc + "TaxExemptionReasonCode").FirstOrDefault()?.Value ?? "";
                var depositAmount = depositCode == "351" ? (double.TryParse(taxExclusiveAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var exclusive) && double.TryParse(taxableAmount18.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var tax18) && double.TryParse(taxableAmount8.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var tax8) ? (exclusive - (tax18 + tax8)).ToString("F2", CultureInfo.InvariantCulture) : "0.00") : "0.00";
                               
                var taxOffice = "";
                var lastName = string.IsNullOrEmpty(supplierNameNode) ? supplierFamilyName : "";
                var firstName = string.IsNullOrEmpty(supplierNameNode) ? supplierFirstName : supplierNameNode;
                var address = "";
                var (kdvExemptionTable, kdvExemptionCode, saleType) = (invoiceType == "ISTISNA") ? ("Tablo 8 (TAM İSTİSNA KAPSAMINA GİREN İŞLEMLER)", "301", "Tam İstisna Kapsamına Giren İşlemler") : ("", "", "Normal Satışlar");
                var culture = new CultureInfo("tr-TR");
                var formatter2 = new NumberFormatInfo { NumberDecimalDigits = 2, NumberGroupSeparator = ".", NumberDecimalSeparator = "," };

                string selectedTable = cmbTableSelector.SelectedItem?.ToString() ?? "";
                bool isSatis = selectedTable == "Bilanço Satış" || selectedTable == "Luca İşletme Satış";

                var defaultSubRecordType = "Mal Satışı"; // veya "Mal Alışı" için uygun değer

                if (isSatis)
                {
                    if (BilancoSatisData.Any(d => d.InvoiceNumber == invoiceNumber)) return;

                    BilancoSatisData.Add(new InvoiceData
                    {
                        InvoiceType = invoiceType,
                        IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                        InvoiceNumber = invoiceNumber,
                        CustomerTaxId = customerTaxId,
                        CustomerName = customerName,
                        TaxExemptAmount0 = taxExemptAmount0.ToString("N2", formatter2),
                        TaxableAmount1 = taxableAmount1.ToString("N2", formatter2),
                        TaxAmount1 = taxAmount1.ToString("N2", formatter2),
                        TaxableAmount8 = taxableAmount8.ToString("N2", formatter2),
                        TaxAmount8 = taxAmount8.ToString("N2", formatter2),
                        TaxableAmount10 = taxableAmount10.ToString("N2", formatter2),
                        TaxAmount10 = taxAmount10.ToString("N2", formatter2),
                        TaxableAmount18 = taxableAmount18.ToString("N2", formatter2),
                        TaxAmount18 = taxAmount18.ToString("N2", formatter2),
                        TaxableAmount20 = taxableAmount20.ToString("N2", formatter2),
                        TaxAmount20 = taxAmount20.ToString("N2", formatter2),
                        DepositAmount = double.TryParse(depositAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var dep) ? dep.ToString("N2", formatter2) : "0,00",
                        Oiv = oiv.ToString("N2", formatter2),
                        TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                        UUID = uuid,
                        PaymentMethod = paymentMethod,
                        KdvExemptionTable = kdvExemptionTable,
                        KdvExemptionCode = kdvExemptionCode,
                        SaleType = saleType
                    });
                }
                else
                {
                    if (BilancoAlisData.Any(d => d.InvoiceNumber == invoiceNumber)) return;

                    BilancoAlisData.Add(new InvoiceData
                    {
                        InvoiceType = invoiceType,
                        IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                        InvoiceNumber = invoiceNumber,
                        SupplierTaxId = supplierTaxId,
                        SupplierName = supplierName,
                        TaxExemptAmount0 = taxExemptAmount0.ToString("N2", formatter2),
                        TaxableAmount1 = taxableAmount1.ToString("N2", formatter2),
                        TaxAmount1 = taxAmount1.ToString("N2", formatter2),
                        TaxableAmount8 = taxableAmount8.ToString("N2", formatter2),
                        TaxAmount8 = taxAmount8.ToString("N2", formatter2),
                        TaxableAmount10 = taxableAmount10.ToString("N2", formatter2),
                        TaxAmount10 = taxAmount10.ToString("N2", formatter2),
                        TaxableAmount18 = taxableAmount18.ToString("N2", formatter2),
                        TaxAmount18 = taxAmount18.ToString("N2", formatter2),
                        TaxableAmount20 = taxableAmount20.ToString("N2", formatter2),
                        TaxAmount20 = taxAmount20.ToString("N2", formatter2),
                        DepositAmount = double.TryParse(depositAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var dep) ? dep.ToString("N2", formatter2) : "0,00",
                        Oiv = oiv.ToString("N2", formatter2),
                        TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                        UUID = uuid,
                        PaymentMethod = paymentMethod,
                        KdvExemptionTable = kdvExemptionTable,
                        KdvExemptionCode = kdvExemptionCode,
                        SaleType = saleType
                    });
                }

                var invoiceLines = xml.Descendants(cac + "InvoiceLine");
                foreach (var line in invoiceLines)
                {
                    var itemName = line.Descendants(cac + "Item").Descendants(cbc + "Name").FirstOrDefault()?.Value ?? "";
                    var quantity = line.Descendants(cbc + "InvoicedQuantity").FirstOrDefault()?.Value ?? "0";
                    var unitPrice = line.Descendants(cac + "Price").Descendants(cbc + "PriceAmount").FirstOrDefault()?.Value ?? "0";
                    var lineTaxableAmount = line.Descendants(cac + "TaxTotal").Descendants(cac + "TaxSubtotal").Descendants(cbc + "TaxableAmount").FirstOrDefault()?.Value ?? "0";
                    var lineTaxAmount = line.Descendants(cac + "TaxTotal").Descendants(cac + "TaxSubtotal").Descendants(cbc + "TaxAmount").FirstOrDefault()?.Value ?? "0";
                    var lineTaxPercent = line.Descendants(cac + "TaxTotal").Descendants(cac + "TaxSubtotal").Descendants(cbc + "Percent").FirstOrDefault()?.Value ?? "0";

                  // string itemName = item.Element("Description")?.Value?.Trim() ?? ""; // Örnek: itemName alımı
                    string subRecordType = "";

                    if (!string.IsNullOrEmpty(itemName) && _expenseMatchings != null && _expenseMatchings.TryGetValue(itemName, out string matchedType))
                    {
                        subRecordType = matchedType;
                    }
                    else
                    {
                        // Eşleşme yoksa varsayılan veya uyarı
                        subRecordType = ""; // Veya boş bırak
                                                              // Opsiyonel: Log veya uyarı
                       // Console.WriteLine($"Eşleşme bulunamadı: {itemName}");
                    }



                    if (isSatis)
                    {
                        LucaIsletmeSatisData.RemoveAll(d => d.InvoiceNumber == invoiceNumber);
                        if (taxableAmount1 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "1",
                                TaxableAmount = taxableAmount1.ToString("N2", formatter2),
                                TaxAmount = taxAmount1.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount8 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "8",
                                TaxableAmount = taxableAmount8.ToString("N2", formatter2),
                                TaxAmount = taxAmount8.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount10 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "10",
                                TaxableAmount = taxableAmount10.ToString("N2", formatter2),
                                TaxAmount = taxAmount10.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount18 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "18",
                                TaxableAmount = taxableAmount18.ToString("N2", formatter2),
                                TaxAmount = taxAmount18.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount20 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "20",
                                TaxableAmount = taxableAmount20.ToString("N2", formatter2),
                                TaxAmount = taxAmount20.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }

                        if (taxExemptAmount0 > 0.0)
                        {
                            LucaIsletmeSatisData.Add(new InvoiceData
                            {
                                InvoiceType = "GELİR",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                CustomerTaxId = customerTaxId,
                                CustomerName = customerName,
                                cFirstName = customerFirstName,
                                cFamilyName = customerFamilyName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                Percent = "0",
                                TaxableAmount = taxExemptAmount0.ToString("N2", formatter2),
                                TaxAmount = "0,00",
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = customerName  // For AÇIKLAMA
                            });
                        }
                    }
                    else
                    {
                        LucaIsletmeAlisData.RemoveAll(d => d.InvoiceNumber == invoiceNumber);
                        if (taxableAmount1 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "1",
                                TaxableAmount = taxableAmount1.ToString("N2", formatter2),
                                TaxAmount = taxAmount1.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount8 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "8",
                                TaxableAmount = taxableAmount8.ToString("N2", formatter2),
                                TaxAmount = taxAmount8.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount10 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "10",
                                TaxableAmount = taxableAmount10.ToString("N2", formatter2),
                                TaxAmount = taxAmount10.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount18 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "18",
                                TaxableAmount = taxableAmount18.ToString("N2", formatter2),
                                TaxAmount = taxAmount18.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName  // For AÇIKLAMA
                            });
                        }

                        if (taxableAmount20 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "20",
                                TaxableAmount = taxableAmount20.ToString("N2", formatter2),
                                TaxAmount = taxAmount20.ToString("N2", formatter2),
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName  // For AÇIKLAMA
                            });
                        }

                        if (taxExemptAmount0 > 0.0)
                        {
                            LucaIsletmeAlisData.Add(new InvoiceData
                            {
                                InvoiceType = "GİDER",
                                IssueDate = DateTime.TryParse(issueDate, out var date) ? date.ToString("dd.MM.yyyy", culture) : "",
                                InvoiceNumber = invoiceNumber,
                                SupplierTaxId = supplierTaxId,
                                SupplierName = lastName,  // SOYADI ÜNVAN
                                CustomerName = firstName,  // ADI DEVAMI
                                cFirstName = firstName,
                                cFamilyName = lastName,
                                KdvExemptionTable = kdvExemptionTable,
                                KdvExemptionCode = kdvExemptionCode,
                                SaleType = saleType,
                                ActivityCode = activityCode,
                                SubRecordType = subRecordType,
                                Percent = "0",
                                TaxableAmount = taxExemptAmount0.ToString("N2", formatter2),
                                TaxAmount = "0,00",
                                TotalPayable = double.TryParse(payableAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pay) ? pay.ToString("N2", formatter2) : "0,00",
                                PaymentMethod = paymentMethod,
                                ItemName = itemName // For AÇIKLAMA
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"XML işleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateDataGridView()
        {
            dgvData.Columns.Clear();
            dgvData.Rows.Clear();
            List<InvoiceData> data;
            string[] columns;

            switch (cmbTableSelector.SelectedItem?.ToString())
            {
                case "Bilanço Satış":
                    data = BilancoSatisData;
                    columns = new[] { "Fatura Türü", "Tarih", "Evrak No", "Alıcı VKN", "Alıcı Unvan", "Vergisiz 0%", "Vergisiz 1%", "Vergi 1%", "Vergisiz 10%", "Vergi 10%", "Vergisiz 20%", "Vergi 20%", "Depozito", "Cari Toplam", "Ödeme Türü" };
                    foreach (var column in columns)
                        dgvData.Columns.Add(column, column);
                    foreach (var item in data)
                        dgvData.Rows.Add(item.InvoiceType, item.IssueDate, item.InvoiceNumber, item.CustomerTaxId, item.CustomerName, item.TaxExemptAmount0, item.TaxableAmount1, item.TaxAmount1, item.TaxableAmount10, item.TaxAmount10, item.TaxableAmount20, item.TaxAmount20, item.DepositAmount, item.TotalPayable, item.PaymentMethod);
                    break;

                case "Bilanço Alış":
                    data = BilancoAlisData;
                    columns = new[] { "Fatura Türü", "Tarih", "Evrak No", "Satıcı VKN", "Satıcı Unvan", "Vergisiz 0%", "Vergisiz 1%", "Vergi 1%", "Vergisiz 10%", "Vergi 10%", "Vergisiz 20%", "Vergi 20%", "Depozito", "Cari Toplam", "Ödeme Türü" };
                    foreach (var column in columns)
                        dgvData.Columns.Add(column, column);
                    foreach (var item in data)
                        dgvData.Rows.Add(item.InvoiceType, item.IssueDate, item.InvoiceNumber, item.SupplierTaxId, item.SupplierName, item.TaxExemptAmount0, item.TaxableAmount1, item.TaxAmount1, item.TaxableAmount10, item.TaxAmount10, item.TaxableAmount20, item.TaxAmount20, item.DepositAmount, item.TotalPayable, item.PaymentMethod);
                    break;

                case "Luca İşletme Satış":
                    data = LucaIsletmeSatisData;
                    columns = new[] { "İŞLEM", "KATEGORİ", "BELGE TÜRÜ", "EVRAK TARİHİ", "KAYIT TARİHİ", "SERİ NO", "EVRAK NO", "TCKN/VKN", "VERGİ DAİRESİ", "SOYADI ÜNVAN", "ADI DEVAMI", "ADRES", "CARİ HESAP", "KDV İSTİSNASI", "KOD", "BELGE TÜRÜ(DB)", "ALIŞ/SATIŞ TÜRÜ", "KAYIT ALT TÜRÜ", "MAL VE HİZMET KODU", "AÇIKLAMA", "MİKTAR", "B.FİYAT", "TUTAR", "TEVKİFAT", "KDV ORANI", "KDV TUTARI", "TOPLAM TUTAR", "KREDİLİ TUTAR", "STOPAJ KODU", "STOPAJ TUTARI", "DÖNEMSELLIK İLKESİ", "FAALIYET KODU", "ÖDEME TÜRÜ" };
                    foreach (var column in columns)
                        dgvData.Columns.Add(column, column);
                    foreach (var item in data)
                    {
                        dgvData.Rows.Add(item.InvoiceType, "Defter Fişleri", "Satış", item.IssueDate, item.IssueDate, "", item.InvoiceNumber, item.CustomerTaxId, "", item.cFamilyName, item.cFirstName, "", "", item.KdvExemptionTable, item.KdvExemptionCode, "e-Arşiv Fatura", item.SaleType, "Mal Satışı", item.Percent, item.ItemName, "", "", item.TaxableAmount, "", item.Percent, item.TaxAmount, item.TotalPayable, "", "", "", "", item.ActivityCode, "");
                    }
                    break;


                case "Luca İşletme Alış":
                    data = LucaIsletmeAlisData;
                    columns = new[] { "İŞLEM", "KATEGORİ", "BELGE TÜRÜ", "EVRAK TARİHİ", "KAYIT TARİHİ", "SERİ NO", "EVRAK NO", "TCKN/VKN", "VERGİ DAİRESİ", "SOYADI ÜNVAN", "ADI DEVAMI", "ADRES", "CARİ HESAP", "KDV İSTİSNASI", "KOD", "BELGE TÜRÜ(DB)", "ALIŞ/SATIŞ TÜRÜ", "KAYIT ALT TÜRÜ", "MAL VE HİZMET KODU", "AÇIKLAMA", "MİKTAR", "B.FİYAT", "TUTAR", "TEVKİFAT", "KDV ORANI", "KDV TUTARI", "TOPLAM TUTAR", "KREDİLİ TUTAR", "STOPAJ KODU", "STOPAJ TUTARI", "DÖNEMSELLIK İLKESİ", "FAALIYET KODU", "ÖDEME TÜRÜ" };
                    foreach (var column in columns)
                        dgvData.Columns.Add(column, column);
                    foreach (var item in data)
                    {
                        dgvData.Rows.Add(item.InvoiceType, "Defter Fişleri", "Alış", item.IssueDate, item.IssueDate, "", item.InvoiceNumber, item.SupplierTaxId, "", item.cFamilyName, item.cFirstName, "", "", item.KdvExemptionTable, item.KdvExemptionCode, "e-Fatura", item.SaleType, item.SubRecordType, item.Percent, item.ItemName, "", "", item.TaxableAmount, "", item.Percent, item.TaxAmount, item.TotalPayable, "", "", "", "", item.ActivityCode ?? "", "");
                    }
                    break;

            }

            //  text alignment for numeric columns
            foreach (DataGridViewColumn column in dgvData.Columns)
            {
                if (column.Name.Contains("Vergisiz") || column.Name.Contains("Vergi") || column.Name.Contains("Tutar") || column.Name.Contains("Miktar") || column.Name.Contains("Birim Fiyat") || column.Name.Contains("Depozito") || column.Name.Contains("OIV"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        /* public async void BtnSplitAndSave_Click(object sender, EventArgs e)
         {
             using var ofd = new OpenFileDialog
             {
                 Filter = "Excel dosyaları|*.xlsx;*.xls|CSV dosyaları|*.csv",
                 Multiselect = false
             };
             if (ofd.ShowDialog() != DialogResult.OK) return;

             string sourcePath = ofd.FileName;
             string ext = Path.GetExtension(sourcePath).ToLowerInvariant();

             using var sfd = new SaveFileDialog
             {
                 Filter = ext == ".csv" ? "CSV Dosyası|*.csv" : "Excel Dosyası|*.xlsx",
                 FileName = Path.GetFileNameWithoutExtension(sourcePath) + "_Part1"
             };
             if (sfd.ShowDialog() != DialogResult.OK) return;

             string targetDir = Path.GetDirectoryName(sfd.FileName) ?? Environment.CurrentDirectory;
             string baseName = Path.GetFileNameWithoutExtension(sfd.FileName);

             progressBarSplit.Value = 0;
             progressBarSplit.Visible = true;
             lblSplitStatus.Text = "Başlatılıyor...";
             lblSplitStatus.Visible = true;
             btnSplitAndSave.Enabled = false;

             var progress = new Progress<int>(p =>
             {
                 progressBarSplit.Value = Math.Min(100, Math.Max(0, p));
                 lblSplitStatus.Text = $"İlerleme: {p}%";
             });

             try
             {
                 await Task.Run(() => SplitAndSaveFileAsync(sourcePath, targetDir, baseName, progress));
                 MessageBox.Show("Dosya parçalama tamamlandı.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Bölme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
             finally
             {
                 progressBarSplit.Visible = false;
                 lblSplitStatus.Visible = false;
                 btnSplitAndSave.Enabled = true;
             }
         }
         */
        /* private void SplitAndSaveFileAsync(string sourcePath, string targetDir, string baseName, IProgress<int> progress)
        {
            const int maxRowsWithHeader = 600; // başlık dahil
            string ext = Path.GetExtension(sourcePath).ToLowerInvariant();

            if (ext == ".csv")
            {
                // CSV: satır bazlı parça
                var allLines = File.ReadAllLines(sourcePath, Encoding.UTF8);
                if (allLines.Length == 0) return;

                string header = allLines[0];
                int totalData = Math.Max(0, allLines.Length - 1);
                int perFile = maxRowsWithHeader - 1;
                int fileCount = Math.Max(1, (int)Math.Ceiling((double)totalData / perFile));

                for (int i = 0; i < fileCount; i++)
                {
                    int start = 1 + i * perFile;
                    int end = Math.Min(allLines.Length - 1, start + perFile - 1);
                    var outPath = Path.Combine(targetDir, $"{baseName}_Part{i + 1}.csv");
                    using var sw = new StreamWriter(outPath, false, new UTF8Encoding(true));
                    sw.WriteLine(header);
                    for (int r = start; r <= end; r++) sw.WriteLine(allLines[r]);

                    progress.Report((int)((i + 1) / (double)fileCount * 100));
                }
            }
            else
            {
                // Excel: EPPlus ile parça
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var p = new ExcelPackage(new FileInfo(sourcePath));
                var ws = p.Workbook.Worksheets.FirstOrDefault();
                if (ws == null || ws.Dimension == null) return;

                int totalRows = ws.Dimension.End.Row;
                int totalCols = ws.Dimension.End.Column;
                int headerRows = 1;
                int dataRows = Math.Max(0, totalRows - headerRows);
                int perFile = maxRowsWithHeader - headerRows;
                int fileCount = Math.Max(1, (int)Math.Ceiling((double)dataRows / perFile));

                for (int fi = 0; fi < fileCount; fi++)
                {
                    var outPath = Path.Combine(targetDir, $"{baseName}_Part{fi + 1}.xlsx");
                    using var outPkg = new ExcelPackage(new FileInfo(outPath));
                    var outWs = outPkg.Workbook.Worksheets.Add(ws.Name ?? "Sheet1");

                    // Başlık kopyala
                    for (int c = 1; c <= totalCols; c++)
                        outWs.Cells[1, c].Value = ws.Cells[1, c].Value;

                    int startRow = fi * perFile + headerRows + 1;
                    int endRow = Math.Min(totalRows, startRow + perFile - 1);
                    int outRow = headerRows + 1;

                    for (int r = startRow; r <= endRow; r++)
                    {
                        for (int c = 1; c <= totalCols; c++)
                            outWs.Cells[outRow, c].Value = ws.Cells[r, c].Value;
                        outRow++;
                    }

                    outWs.Cells.AutoFitColumns();
                    outPkg.Save();

                    progress.Report((int)((fi + 1) / (double)fileCount * 100));
                }
            }
       */

        public async void BtnSplitAndSave_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Excel dosyaları|*.xlsx;*.xls|CSV dosyaları|*.csv",
                Multiselect = false
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string sourcePath = ofd.FileName;
            string ext = Path.GetExtension(sourcePath).ToLowerInvariant();

            // Excel dosyası ise kullanıcıya CSV'ye dönüştürme sorusu sor
            bool convertToCsv = false;
            if (ext == ".xlsx" || ext == ".xls")
            {
                var result = MessageBox.Show(
                    "Excel dosyasını CSV formatına dönüştürmek istiyor musunuz?",
                    "Dönüştürme Seçeneği",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                convertToCsv = (result == DialogResult.Yes);
            }

            using var sfd = new SaveFileDialog
            {
                Filter = convertToCsv ? "CSV Dosyası|*.csv" : "Excel Dosyası|*.xlsx",
                FileName = Path.GetFileNameWithoutExtension(sourcePath) + "_Part1" + (convertToCsv ? ".csv" : ".xlsx")
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            string targetDir = Path.GetDirectoryName(sfd.FileName) ?? Environment.CurrentDirectory;
            string baseName = Path.GetFileNameWithoutExtension(sfd.FileName);

            progressBarSplit.Value = 0;
            progressBarSplit.Visible = true;
            lblSplitStatus.Text = "Başlatılıyor...";
            lblSplitStatus.Visible = true;
            btnSplitAndSave.Enabled = false;

            var progress = new Progress<int>(p =>
            {
                progressBarSplit.Value = Math.Min(100, Math.Max(0, p));
                lblSplitStatus.Text = $"İlerleme: {p}%";
            });

            try
            {
                await Task.Run(() => SplitAndSaveFileAsync(sourcePath, targetDir, baseName, convertToCsv, progress));
                MessageBox.Show("Dosya parçalama tamamlandı.", "Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bölme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBarSplit.Visible = false;
                lblSplitStatus.Visible = false;
                btnSplitAndSave.Enabled = true;
            }
        }

        private void SplitAndSaveFileAsync(string sourcePath, string targetDir, string baseName, bool convertToCsv, IProgress<int> progress)
        {
            const int maxRowsWithHeader = 600; // başlık dahil
            string ext = Path.GetExtension(sourcePath).ToLowerInvariant();

            // CSV'ye dönüştürme seçeneği aktifse veya orijinal dosya CSV ise CSV formatında kaydet
            bool useCsvFormat = convertToCsv || ext == ".csv";

            if (useCsvFormat)
            {
                // Excel dosyasını CSV'ye dönüştür veya CSV dosyasını işle
                if (ext == ".xlsx" || ext == ".xls")
                {
                    // Excel'i CSV'ye dönüştür ve parçala
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var p = new ExcelPackage(new FileInfo(sourcePath));
                    var ws = p.Workbook.Worksheets.FirstOrDefault();
                    if (ws == null || ws.Dimension == null) return;

                    int totalRows = ws.Dimension.End.Row;
                    int totalCols = ws.Dimension.End.Column;
                    int headerRows = 1;
                    int dataRows = Math.Max(0, totalRows - headerRows);
                    int perFile = maxRowsWithHeader - headerRows;
                    int fileCount = Math.Max(1, (int)Math.Ceiling((double)dataRows / perFile));

                    var trCulture = new CultureInfo("tr-TR");

                    for (int fi = 0; fi < fileCount; fi++)
                    {
                        var outPath = Path.Combine(targetDir, $"{baseName}_Part{fi + 1}.csv");
                        using var writer = CreateCsvWriter(outPath);

                        // Başlık satırını yaz - Excel dosyasının 1. satırındaki hücreleri kullan
                        var headerValues = new List<string>();
                        for (int c = 1; c <= totalCols; c++)
                        {
                            var headerCellVal = ws.Cells[1, c].Value;
                            string headerText = FormatCellForCsv(headerCellVal, trCulture);
                            headerText = headerText.TrimStart('\uFEFF', '\u200B');
                            headerValues.Add(EscapeCsvField(headerText, ';'));
                        }
                        writer.WriteLine(string.Join(";", headerValues));

                        // Veri satırlarını yaz
                        int startRow = fi * perFile + headerRows + 1;
                        int endRow = Math.Min(totalRows, startRow + perFile - 1);

                        for (int r = startRow; r <= endRow; r++)
                        {
                            var rowValues = new List<string>();
                            for (int c = 1; c <= totalCols; c++)
                            {
                                rowValues.Add(EscapeCsvField(FormatCellForCsv(ws.Cells[r, c].Value, trCulture), ';'));
                            }
                            writer.WriteLine(string.Join(";", rowValues));
                        }

                        progress.Report((int)((fi + 1) / (double)fileCount * 100));
                    }
                }
                else
                {
                    // Orijinal CSV dosyasını parçala
                    var sourceEnc = DetectFileEncoding(sourcePath);
                    var allLines = File.ReadAllLines(sourcePath, sourceEnc);
                    if (allLines.Length == 0) return;

                    // Remove BOM from header line if present
                    string header = allLines[0].TrimStart('\uFEFF');
                    int totalData = Math.Max(0, allLines.Length - 1);
                    int perFile = maxRowsWithHeader - 1;
                    int fileCount = Math.Max(1, (int)Math.Ceiling((double)totalData / perFile));

                    for (int i = 0; i < fileCount; i++)
                    {
                        int start = 1 + (i * perFile);
                        int end = Math.Min(allLines.Length - 1, start + perFile - 1);
                        var outPath = Path.Combine(targetDir, $"{baseName}_Part{i + 1}.csv");
                        using var sw = CreateCsvWriter(outPath);
                        sw.WriteLine(header);
                        for (int r = start; r <= end; r++) sw.WriteLine(allLines[r]);

                        progress.Report((int)((i + 1) / (double)fileCount * 100));
                    }
                }
            }
            else
            {
                // Excel formatında parçala (orijinal kod)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var p = new ExcelPackage(new FileInfo(sourcePath));
                var ws = p.Workbook.Worksheets.FirstOrDefault();
                if (ws == null || ws.Dimension == null) return;

                int totalRows = ws.Dimension.End.Row;
                int totalCols = ws.Dimension.End.Column;
                int headerRows = 1;
                int dataRows = Math.Max(0, totalRows - headerRows);
                int perFile = maxRowsWithHeader - headerRows;
                int fileCount = Math.Max(1, (int)Math.Ceiling((double)dataRows / perFile));

                for (int fi = 0; fi < fileCount; fi++)
                {
                    var outPath = Path.Combine(targetDir, $"{baseName}_Part{fi + 1}.xlsx");
                    using var outPkg = new ExcelPackage(new FileInfo(outPath));
                    var outWs = outPkg.Workbook.Worksheets.Add(ws.Name ?? "Sheet1");

                    // Başlık kopyala
                    for (int c = 1; c <= totalCols; c++)
                        outWs.Cells[1, c].Value = ws.Cells[1, c].Value;

                    int startRow = fi * perFile + headerRows + 1;
                    int endRow = Math.Min(totalRows, startRow + perFile - 1);
                    int outRow = headerRows + 1;

                    for (int r = startRow; r <= endRow; r++)
                    {
                        for (int c = 1; c <= totalCols; c++)
                            outWs.Cells[outRow, c].Value = ws.Cells[r, c].Value;
                        outRow++;
                    }

                    outWs.Cells.AutoFitColumns();
                    outPkg.Save();

                    progress.Report((int)((fi + 1) / (double)fileCount * 100));
                }
            }
        }

        private string NormalizeHeader(string header)
        {
            if (string.IsNullOrEmpty(header))
                return header;

            
            header = header.TrimStart('\uFEFF', '\u200B');

            
            if (header.Contains("#"))
            {
                
                return header;
            }

            return header;
        }
        public void StartSplitInteractive()
        {
            BtnSplitAndSave_Click(this, EventArgs.Empty);
        }

        private Button btnUpload;
        private ComboBox cmbTableSelector;
        private GroupBox groupBox1;
        private Label label1;
        private DataGridView dgvData;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFaturaxmlForm));
            btnUpload = new Button();
            cmbTableSelector = new ComboBox();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            btnSplitAndSave = new Button();
            btn_alttur = new Button();
            BtnExportCSV = new Button();
            label2 = new Label();
            cbx_customerlist = new ComboBox();
            btnExportExcel = new Button();
            btn_clr = new Button();
            label1 = new Label();
            txtbox_act = new TextBox();
            dgvData = new DataGridView();
            progressBarSplit = new ProgressBar();
            lblSplitStatus = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            SuspendLayout();
            // 
            // btnUpload
            // 
            btnUpload.Location = new Point(3, 58);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(171, 29);
            btnUpload.TabIndex = 0;
            btnUpload.Text = "XML Dosyaları Yükle";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += UploadButton_Click;
            // 
            // cmbTableSelector
            // 
            cmbTableSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTableSelector.FormattingEnabled = true;
            cmbTableSelector.Location = new Point(3, 29);
            cmbTableSelector.Name = "cmbTableSelector";
            cmbTableSelector.Size = new Size(171, 23);
            cmbTableSelector.TabIndex = 1;
            cmbTableSelector.SelectedIndexChanged += CmbTableSelector_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Controls.Add(dgvData);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1332, 431);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "E-Fatura XML Excel Dönüştürücü";
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            groupBox2.Controls.Add(lblSplitStatus);
            groupBox2.Controls.Add(progressBarSplit);
            groupBox2.Controls.Add(btnSplitAndSave);
            groupBox2.Controls.Add(btn_alttur);
            groupBox2.Controls.Add(BtnExportCSV);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(cbx_customerlist);
            groupBox2.Controls.Add(btnExportExcel);
            groupBox2.Controls.Add(btn_clr);
            groupBox2.Controls.Add(cmbTableSelector);
            groupBox2.Controls.Add(btnUpload);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(txtbox_act);
            groupBox2.Dock = DockStyle.Top;
            groupBox2.Location = new Point(3, 19);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1326, 118);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            // 
            // btnSplitAndSave
            // 
            btnSplitAndSave.Location = new Point(670, 14);
            btnSplitAndSave.Name = "btnSplitAndSave";
            btnSplitAndSave.Size = new Size(100, 39);
            btnSplitAndSave.TabIndex = 11;
            btnSplitAndSave.Text = "Excel Parçala";
            btnSplitAndSave.UseVisualStyleBackColor = true;
            btnSplitAndSave.Click += BtnSplitAndSave_Click;
            // 
            // btn_alttur
            // 
            btn_alttur.Location = new Point(535, 14);
            btn_alttur.Name = "btn_alttur";
            btn_alttur.Size = new Size(100, 39);
            btn_alttur.TabIndex = 10;
            btn_alttur.Text = "Kayıt Alt Türü Eşleme";
            btn_alttur.UseVisualStyleBackColor = true;
            btn_alttur.Click += btn_alttur_Click;
            // 
            // BtnExportCSV
            // 
            BtnExportCSV.Location = new Point(199, 42);
            BtnExportCSV.Name = "BtnExportCSV";
            BtnExportCSV.Size = new Size(100, 24);
            BtnExportCSV.TabIndex = 9;
            BtnExportCSV.Text = "CSV";
            BtnExportCSV.UseVisualStyleBackColor = true;
            BtnExportCSV.Click += BtnExportCSV_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(335, 11);
            label2.Name = "label2";
            label2.Size = new Size(167, 30);
            label2.TabIndex = 7;
            label2.Text = "Müşteri Seç\r\nİşletmelerde Faaliyet Kodu için";
            // 
            // cbx_customerlist
            // 
            cbx_customerlist.DropDownStyle = ComboBoxStyle.DropDownList;
            cbx_customerlist.FormattingEnabled = true;
            cbx_customerlist.Location = new Point(335, 44);
            cbx_customerlist.Name = "cbx_customerlist";
            cbx_customerlist.Size = new Size(171, 23);
            cbx_customerlist.TabIndex = 6;
            cbx_customerlist.SelectedIndexChanged += CbxCustomerList_SelectedIndexChanged;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(199, 14);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(100, 24);
            btnExportExcel.TabIndex = 4;
            btnExportExcel.Text = "Excel";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += BtnExportExcel_Click;
            // 
            // btn_clr
            // 
            btn_clr.Location = new Point(199, 72);
            btn_clr.Name = "btn_clr";
            btn_clr.Size = new Size(100, 24);
            btn_clr.TabIndex = 5;
            btn_clr.Text = "Tabloyu Boşalt";
            btn_clr.UseVisualStyleBackColor = true;
            btn_clr.Click += BtnClear_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 11);
            label1.Name = "label1";
            label1.Size = new Size(78, 15);
            label1.TabIndex = 3;
            label1.Text = "Tablo Tipi Seç";
            // 
            // txtbox_act
            // 
            txtbox_act.Location = new Point(335, 51);
            txtbox_act.Name = "txtbox_act";
            txtbox_act.ReadOnly = true;
            txtbox_act.Size = new Size(100, 23);
            txtbox_act.TabIndex = 8;
            txtbox_act.Visible = false;
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.AllowUserToOrderColumns = true;
            dgvData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dgvData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvData.BackgroundColor = SystemColors.Control;
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Location = new Point(0, 150);
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dgvData.Size = new Size(1326, 281);
            dgvData.TabIndex = 2;
            // 
            // progressBarSplit
            // 
            progressBarSplit.Location = new Point(776, 30);
            progressBarSplit.Name = "progressBarSplit";
            progressBarSplit.Size = new Size(100, 23);
            progressBarSplit.TabIndex = 12;
            progressBarSplit.Visible = false;
            // 
            // lblSplitStatus
            // 
            lblSplitStatus.AutoSize = true;
            lblSplitStatus.Location = new Point(776, 14);
            lblSplitStatus.Name = "lblSplitStatus";
            lblSplitStatus.Size = new Size(0, 15);
            lblSplitStatus.TabIndex = 13;
            lblSplitStatus.Visible = false;
            lblSplitStatus.Text = "";
            // 
            // EFaturaxmlForm
            // 
            ClientSize = new Size(1332, 431);
            Controls.Add(groupBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "EFaturaxmlForm";
            Text = "E-Fatura XML & UBL -> Excel Dönüştürücü";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        private Button btnExportExcel;
        private Button btn_clr;
        private GroupBox groupBox2;
        private Label label2;
        private ComboBox cbx_customerlist;
        private TextBox txtbox_act;
        private Button BtnExportCSV;
        private Button btn_alttur;
        private Button btnSplitAndSave;
        private Label lblSplitStatus;
        private ProgressBar progressBarSplit;
    }
}