using System.Data;
using System.Linq;

namespace HesapTakip
{
    public partial class ExpenseCategoryForm : Form
    {
        private IDatabaseOperations _db;
        private DataTable _expenseCategories;
        private DataTable _matchList;
        public ExpenseCategoryForm(IDatabaseOperations db)
        {
            InitializeComponent();
            _db = db;

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (dgv_expensecatlist == null || dvg_matchlist == null || textBox1 == null)
            {
                MessageBox.Show("DataGridView bileşenleri initialize edilemedi!");
                return;
            }
            LoadCategories();
            LoadMatchings();
            if (Controls.Find("txtSearch", true).Length > 0)
            {
                var txtSearch = Controls.Find("txtSearch", true)[0] as TextBox;
                txtSearch.TextChanged += TxtSearch_TextChanged;
            }
        }

        private void LoadCategories()
        {
            try
            {
                _expenseCategories = _db.GetCategories() ?? new DataTable(); // Null ise boş tablo oluştur
                dgv_expensecatlist.DataSource = _expenseCategories.Copy();
                if (dgv_expensecatlist.Columns.Contains("CategoryID"))
                    dgv_expensecatlist.Columns["CategoryID"].Visible = false;
                if (dgv_expensecatlist.Columns.Contains("Label"))
                {
                    dgv_expensecatlist.Columns["Label"].HeaderText = "Kayıt Alt Türü";
                    dgv_expensecatlist.Columns["Label"].Width = 150;
                }
                if (dgv_expensecatlist.Columns.Contains("Info"))
                {
                    dgv_expensecatlist.Columns["Info"].HeaderText = "Kanun Md.";
                    dgv_expensecatlist.Columns["Info"].Width = 150;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategoriler yüklenirken hata oluştu: {ex.Message}");
            }
        }
        private void LoadMatchings()
        {
            try
            {
                _matchList = _db.GetExpenseMatchings() ?? new DataTable();
                dvg_matchlist.DataSource = _matchList;
                /* dvg_matchlist.DataBindingComplete += (s, e) =>*/

                if (dvg_matchlist.Columns.Contains("MatchingID"))
                {
                    dvg_matchlist.Columns["MatchingID"].Visible = false;
                }
                if (dvg_matchlist.Columns.Contains("ItemName"))
                {
                    dvg_matchlist.Columns["ItemName"].HeaderText = "Fatura Kalem Adı";
                    dvg_matchlist.Columns["ItemName"].Width = 150;
                }
                if (dvg_matchlist.Columns.Contains("SubRecordType"))
                {
                    dvg_matchlist.Columns["SubRecordType"].HeaderText = "Kayıt Alt Türü";
                    dvg_matchlist.Columns["SubRecordType"].Width = 150;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eşleştirmeler yüklenirken hata oluştu: {ex.Message}");
            }
        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string itemName = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(itemName))
            {
                MessageBox.Show("Lütfen geçerli bir fatura kalemi girin!");
                return;
            }

            if (dgv_expensecatlist.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir kategori seçin!");
                return;
            }

            string subRecordType = dgv_expensecatlist.SelectedRows[0].Cells["Label"].Value.ToString();
            string normalizedItemName = itemName.ToLower().Trim();
            if (_matchList.AsEnumerable().Any(row => row.Field<string>("ItemName").ToLower().Trim() == normalizedItemName))
            {
                MessageBox.Show("Bu fatura adı zaten eşleştirilmiş!");
                return;
            }

            if (_db.AddExpenseMatching(itemName, subRecordType))
            {

                _matchList.Rows.Add(itemName, subRecordType);
                textBox1.Text = ""; // TextBox'ı temizle

                LoadMatchings();
                // MessageBox.Show("Eşleşme başarıyla eklendi!");
            }
            else
            {
                MessageBox.Show("Eşleşme eklenirken hata oluştu!");
            }
        }
        private void BtnRmv_Click(object sender, EventArgs e)
        {
            if (dvg_matchlist.SelectedRows.Count > 0)
            {
                string itemName = dvg_matchlist.SelectedRows[0].Cells["ItemName"].Value.ToString();
                if (_db.DeleteExpenseMatching(itemName))
                {
                    dvg_matchlist.Rows.RemoveAt(dvg_matchlist.SelectedRows[0].Index);
                    // MessageBox.Show("Eşleşme başarıyla silindi!");
                }
                else
                {
                    MessageBox.Show("Eşleşme silinirken hata oluştu!");
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir eşleşme seçin!");
            }
        }
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var txtSearch = sender as TextBox;
            if (txtSearch != null)
            {
                string searchText = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    // Arama metni boşsa tüm verileri geri yükle
                    dgv_expensecatlist.DataSource = _expenseCategories.Copy();
                }
                else
                {
                    // Filtreleme yap (null kontrollü)
                    var filteredRows = _expenseCategories.AsEnumerable()
                        .Where(row =>
                            (row.Field<string>("Label") ?? string.Empty).ToLower().Contains(searchText) ||
                            (row.Field<string>("Info") ?? string.Empty).ToLower().Contains(searchText));

                    // Eğer sonuç yoksa boş ama aynı şemaya sahip tablo ata (CopyToDataTable exception'ını önlemek için)
                    DataTable resultTable = filteredRows.Any() ? filteredRows.CopyToDataTable() : _expenseCategories.Clone();
                    dgv_expensecatlist.DataSource = resultTable;
                }

                if (dgv_expensecatlist.Columns.Contains("CategoryID"))
                    dgv_expensecatlist.Columns["CategoryID"].Visible = false; // Filtreleme sonrası ID’yi gizle
            }
        }
    }
}
