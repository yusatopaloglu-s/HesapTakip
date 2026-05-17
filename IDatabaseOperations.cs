using System.Data;

namespace HesapTakip
{
    public interface IDatabaseOperations
    {
        // Bağlantı ve initialization
        bool TestConnection();
        void InitializeDatabase();
        IDbConnection GetConnection();

        // Müşteri işlemleri
        DataTable GetCustomers();
        bool AddCustomer(string name, bool edefter, string taxid = null, string activitycode = null);
        bool UpdateCustomer(int customerId, string newName, bool edefter, string taxid = null, string activitycode = null, bool deleted = false);
        bool DeleteCustomer(int customerId);

        // Silinmiş müşteriler
        DataTable GetDeletedCustomers();

        // Hareket işlemleri
        DataTable GetTransactions(int customerId);
        bool AddTransaction(int customerId, DateTime date, string description, decimal amount, string type, int? period = null);
        bool UpdateTransaction(int transactionId, DateTime date, string description, decimal amount, string type, int? period = null);
        bool DeleteTransaction(int transactionId);

        // Öneri işlemleri
        List<string> GetSuggestions();
        bool AddSuggestion(string description);
        bool RemoveSuggestion(string description);

        // Toplam bakiye
        decimal CalculateTotalBalance(int customerId);

        // Tablo ve kolon kontrolü
        void EnsureTableAndColumns(string tableName, Dictionary<string, string> columns);
        // E-DEFTER İŞLEMLERİ 
        DataTable GetEDefterTransactions(int customerId);
        bool AddEDefterTransaction(int customerId, DateTime date, decimal kontor, string type);
        bool DeleteEDefterTransaction(int transactionId);
        decimal CalculateEDefterTotal(int customerId);
        bool BulkUpdateEDefterTransactions(List<EDefterTransaction> transactions);

        //DBS Kategorileri
        DataTable GetCategories();
        // Expense matching (optional) - default implementations provided
        bool AddExpenseMatching(string itemName, string subRecordType) { return false; }
        bool DeleteExpenseMatching(string itemName) { return false; }
        DataTable GetExpenseMatchings() { return new DataTable(); }
        // Periods (fiscal years) management (optional)
        DataTable GetPeriods() { return new DataTable(); }
        bool AddPeriod(int periodYear, string displayName = null) { return false; }

        // Şablon yönetimi
        DataTable GetTemplates(); // Tüm şablonları döndürür
        bool AddTemplate(string name, string definitionJson, bool isBuiltIn = false);
        bool UpdateTemplate(int templateId, string name, string definitionJson);
        bool DeleteTemplate(int templateId);
        DataTable GetTemplateColumns(int templateId); // (opsiyonel, eğer kolonlar ayrı tutulacaksa)

        // Filtre yönetimi (FaturaFilters)
        DataTable GetFiltersForTemplate(int templateId);  
        bool AddFilter(int templateId, string itemName, decimal taxRate, string outputColumnPrefix, int displayOrder = 0);
        bool UpdateFilter(int filterId, string itemName, decimal taxRate, string outputColumnPrefix, int displayOrder);
        bool DeleteFilter(int filterId);
        bool ToggleFilterActive(int filterId, bool isActive);
        bool ReorderFilters(int templateId, Dictionary<int, int> filterIdToOrder); 
    }

    // E-Defter işlemleri için yardımcı sınıf
    public class EDefterTransaction
    {
        public int CustomerID { get; set; }
        public DateTime Date { get; set; }
        public decimal Kontor { get; set; }
        public string Type { get; set; } // "ekle" veya "cikar"
    }
}

