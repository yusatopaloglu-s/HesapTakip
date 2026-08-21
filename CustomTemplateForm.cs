using HesapTakip;
using System.Data;
using System.Text;
using System.Text.Json;

public class CustomTemplateForm : Form
{
    private IDatabaseOperations _db;
    private ComboBox cmbBaseTemplate;
    private TextBox txtNewTemplateName;
    private FlowLayoutPanel flowFilters;
    private Button btnAddFilter;
    private Button btnSave;
    private Button btnCancel;

    public CustomTemplateForm(IDatabaseOperations db, IEnumerable<string> baseTemplates)
    {
        _db = db;
        InitializeComponents();

        cmbBaseTemplate.Items.AddRange(baseTemplates.ToArray());
        if (cmbBaseTemplate.Items.Count > 0) cmbBaseTemplate.SelectedIndex = 0;
    }

    private void InitializeComponents()
    {
        this.Text = "Yeni Şablon Oluştur";
        this.Width = 700;
        this.Height = 500;
        this.StartPosition = FormStartPosition.CenterParent;

        var lblBase = new Label { Text = "Baz alınacak şablon:", Left = 10, Top = 14, AutoSize = true };
        cmbBaseTemplate = new ComboBox { Left = 150, Top = 10, Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblName = new Label { Text = "Yeni şablon adı:", Left = 10, Top = 50, AutoSize = true };
        txtNewTemplateName = new TextBox { Left = 150, Top = 46, Width = 400 };

        var lblFilters = new Label { Text = "Filtreler (kalem tespiti):", Left = 10, Top = 88, AutoSize = true };

        flowFilters = new FlowLayoutPanel
        {
            Left = 10,
            Top = 110,
            Width = 660,
            Height = 300,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        btnAddFilter = new Button { Text = "Filtre Ekle", Left = 10, Top = 420, Width = 100 };
        btnAddFilter.Click += (s, e) => AddFilterRow();

        btnSave = new Button { Text = "Kaydet", Left = 430, Top = 420, Width = 100 };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button { Text = "İptal", Left = 540, Top = 420, Width = 100 };
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.AddRange(new Control[] { lblBase, cmbBaseTemplate, lblName, txtNewTemplateName, lblFilters, flowFilters, btnAddFilter, btnSave, btnCancel });


        AddFilterRow();
    }

    private void AddFilterRow(string itemName = "", string prefix = "", decimal taxRate = 20)
    {
        var panel = new Panel { Width = flowFilters.ClientSize.Width - 25, Height = 36 };

        var txtItem = new TextBox { Left = 0, Top = 6, Width = 260, Text = itemName, PlaceholderText = "Kalem adı (ör. Kargo Bedeli)" };
        var cmbRate = new ComboBox { Left = 270, Top = 6, Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbRate.Items.AddRange(new object[] { "0", "1", "8", "10", "20" });
        cmbRate.SelectedItem = taxRate.ToString();

        var txtPrefix = new TextBox { Left = 360, Top = 6, Width = 160, Text = prefix, PlaceholderText = "Çıktı sütun öneki (ör. Kargo)" };

        var btnRemove = new Button { Text = "-", Left = 530, Top = 4, Width = 36, Height = 28 };
        btnRemove.Click += (s, e) => { flowFilters.Controls.Remove(panel); };

        panel.Controls.AddRange(new Control[] { txtItem, cmbRate, txtPrefix, btnRemove });
        flowFilters.Controls.Add(panel);
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {

        var newName = txtNewTemplateName.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName))
        {
            MessageBox.Show("Şablon adı boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }


        var filters = new List<(string ItemName, decimal TaxRate, string OutputPrefix)>();
        foreach (Panel p in flowFilters.Controls)
        {
            var txtItem = p.Controls.OfType<TextBox>().FirstOrDefault();
            var cmbRate = p.Controls.OfType<ComboBox>().FirstOrDefault();
            var txtPrefix = p.Controls.OfType<TextBox>().Skip(1).FirstOrDefault();

            if (txtItem == null || cmbRate == null || txtPrefix == null) continue;
            var itemName = txtItem.Text.Trim();
            if (string.IsNullOrWhiteSpace(itemName)) continue;
            if (!decimal.TryParse(cmbRate.SelectedItem?.ToString() ?? "0", out var r)) r = 0;
            var prefix = txtPrefix.Text.Trim();
            filters.Add((itemName, r, prefix));
        }

        if (filters.Count == 0)
        {
            var res = MessageBox.Show("Hiç filtre eklemediniz. Yine de kaydetmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;
        }


        var baseTemplateName = cmbBaseTemplate.SelectedItem?.ToString() ?? "Bilanço Satış";
        var target = baseTemplateName switch
        {
            "Bilanço Satış" => "BilancoSatisData",
            "Bilanço Alış" => "BilancoAlisData",
            "Luca İşletme Satış" => "LucaIsletmeSatisData",
            "Luca İşletme Alış" => "LucaIsletmeAlisData",
            "Fatura Kalemleri Adet" => "InvoiceItemsQuantityData",
            _ => "BilancoSatisData"
        };
        List<object>? columnDefinitions = LoadBaseTemplateColumns(baseTemplateName);

        if (columnDefinitions != null)
        {
            foreach (var filter in filters)
            {
                var prefix = string.IsNullOrWhiteSpace(filter.OutputPrefix)
                    ? "Filtre"
                    : filter.OutputPrefix;

                columnDefinitions.Add(new
                {
                    ColumnName = $"{prefix} Matrahı",
                    DataKey = $"Filter:{prefix}:Matrah"
                });

                columnDefinitions.Add(new
                {
                    ColumnName = $"{prefix} KDV",
                    DataKey = $"Filter:{prefix}:Kdv"
                });
            }
        }

        if (columnDefinitions == null || columnDefinitions.Count == 0)
        {
            MessageBox.Show("Baz şablonun sütunları alınamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var def = new
        {
            Name = newName,
            Target = target,
            Columns = columnDefinitions,
            FilterRules = filters.Select((f, idx) => new
            {
                PropertyName = "ItemName",
                FilterValue = f.ItemName,
                ComparisonType = "Contains",
                ColumnPrefix = string.IsNullOrWhiteSpace(f.OutputPrefix)
                    ? $"Filtre{idx + 1}"
                    : f.OutputPrefix,
                TaxRate = f.TaxRate,
                IncludeInResult = false
            }).ToList()
        };

        string defJson = JsonSerializer.Serialize(def, new JsonSerializerOptions { WriteIndented = true });

        /*9    try
            {
               int newTemplateId = _db.AddTemplate(newName, defJson, isBuiltIn: false);

                if (newTemplateId > 0)
                {
                    int order = 0;
                    foreach (var f in filters)
                    {
                        bool filterAdded = _db.AddFilter(newTemplateId, f.ItemName, f.TaxRate, f.OutputPrefix ?? "Filter", order++);
                        if (!filterAdded)
                        {
                            Logger.Log($"⚠ Filtre kaydedilemedi: {f.ItemName}");
                        }
                    }

                    MessageBox.Show(
                        $"Şablon '{newName}' başarıyla oluşturuldu.\n{filters.Count} filtre kaydedildi.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    this.DialogResult = DialogResult.OK; 
                }

                else
                {
                    MessageBox.Show($"Şablon kaydedilemedi. DB döndürülen ID: {newTemplateId}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şablon kaydetme hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */

        string templateDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "templates",
        "custom");

        Directory.CreateDirectory(templateDirectory);

        string safeFileName = string.Concat(
            newName.Select(character =>
                Path.GetInvalidFileNameChars().Contains(character)
                    ? '_'
                    : character));

        string templatePath = Path.Combine(
            templateDirectory,
            $"{safeFileName}.json");

        File.WriteAllText(
            templatePath,
            defJson,
            new UTF8Encoding(false));

        MessageBox.Show(
            $"Şablon '{newName}' başarıyla oluşturuldu.",
            "Başarılı",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        DialogResult = DialogResult.OK;
    }
    private List<object>? LoadBaseTemplateColumns(string baseTemplateName)
    {
        return baseTemplateName switch
        {
            "Bilanço Satış" => new List<object>
            {
                new { ColumnName = "Fatura Türü", DataKey = "InvoiceType" },
                new { ColumnName = "Tarih", DataKey = "IssueDate" },
                new { ColumnName = "Evrak No", DataKey = "InvoiceNumber" },
                new { ColumnName = "Alıcı VKN", DataKey = "CustomerTaxId" },
                new { ColumnName = "Alıcı Unvan", DataKey = "CustomerName" },
                new { ColumnName = "Vergisiz 0%", DataKey = "TaxExemptAmount0" },
                new { ColumnName = "Vergisiz 1%", DataKey = "TaxableAmount1" },
                new { ColumnName = "Vergi 1%", DataKey = "TaxAmount1" },
                new { ColumnName = "Vergisiz 10%", DataKey = "TaxableAmount10" },
                new { ColumnName = "Vergi 10%", DataKey = "TaxAmount10" },
                new { ColumnName = "Vergisiz 20%", DataKey = "TaxableAmount20" },
                new { ColumnName = "Vergi 20%", DataKey = "TaxAmount20" },
                new { ColumnName = "Depozito", DataKey = "DepositAmount" },
                new { ColumnName = "Cari Toplam", DataKey = "TotalPayable" },
                new { ColumnName = "Ödeme Türü", DataKey = "PaymentMethod" }
            },

            "Bilanço Alış" => new List<object>
            {
                new { ColumnName = "Fatura Türü", DataKey = "InvoiceType" },
                new { ColumnName = "Tarih", DataKey = "IssueDate" },
                new { ColumnName = "Evrak No", DataKey = "InvoiceNumber" },
                new { ColumnName = "Satıcı VKN", DataKey = "SupplierTaxId" },
                new { ColumnName = "Satıcı Unvan", DataKey = "SupplierName" },
                new { ColumnName = "Vergisiz 0%", DataKey = "TaxExemptAmount0" },
                new { ColumnName = "Vergisiz 1%", DataKey = "TaxableAmount1" },
                new { ColumnName = "Vergi 1%", DataKey = "TaxAmount1" },
                new { ColumnName = "Vergisiz 10%", DataKey = "TaxableAmount10" },
                new { ColumnName = "Vergi 10%", DataKey = "TaxAmount10" },
                new { ColumnName = "Vergisiz 20%", DataKey = "TaxableAmount20" },
                new { ColumnName = "Vergi 20%", DataKey = "TaxAmount20" },
                new { ColumnName = "Depozito", DataKey = "DepositAmount" },
                new { ColumnName = "Cari Toplam", DataKey = "TotalPayable" },
                new { ColumnName = "Ödeme Türü", DataKey = "PaymentMethod" }
            },
            _ => null
        };
    }
}