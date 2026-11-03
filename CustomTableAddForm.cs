using HesapTakip;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace HesapTakip
{
    public partial class CustomTableAddForm : Form
    {
        private IDatabaseOperations _db;
        private ComboBox cmbBaseTemplate;
        private TextBox txtNewTemplateName;
        private FlowLayoutPanel flowFilters;
        private Button btnAddFilter;
        private Button btnSave;
        private Button btnCancel;

        public CustomTableAddForm(IDatabaseOperations db, IEnumerable<string> baseTemplates)
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

            // Custom template klasörünü oluştur
            string templateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", "custom");
            Directory.CreateDirectory(templateDirectory);

            string safeFileName = string.Concat(
                newName.Select(character =>
                    Path.GetInvalidFileNameChars().Contains(character)
                        ? '_'
                        : character));

            string templatePath = Path.Combine(
                templateDirectory,
                $\"{safeFileName}.json\");

            // Şablon tanımı (basit örnek - gerçek uygulamada LoadBuiltinTemplatesFromFiles'den alınan veriler kullanılmalı)
            string defJson = $"{{\"Name\": \"{newName}\", \"Columns\": []}}";
            File.WriteAllText(templatePath, defJson, new UTF8Encoding(false));

            MessageBox.Show(
                $\"Şablon '{newName}' başarıyla oluşturuldu.\",
                "Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
        }
    }
}