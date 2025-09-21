using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Security.Cryptography;


namespace HesapTakip
{
    public partial class ActivationForm : Form
    {
        public ActivationForm()
        {
            InitializeComponent();
            lblHardwareId.Text = HardwareIdGenerator.GetHardwareId(); // Donanım ID'sini göster
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();
            if (LicenseValidator.ValidateLicense(licenseKey, out DateTime expiryDate))
            {
                File.WriteAllText("license.lic", licenseKey); // Lisansı kaydet
                MessageBox.Show($"Lisans aktif! Son kullanma: {expiryDate:dd/MM/yyyy}");
                this.Close();
            }
            else
                if (DateTime.Now > expiryDate)
            {
                MessageBox.Show("Lisans süresi dolmuş!");
                
            }
            MessageBox.Show("Geçersiz lisans anahtarı!");
        }
    }

    

    public class HardwareIdGenerator
    {
        public static string GetHardwareId()
        {
            // CPU, Disk ve Anakart bilgilerini topla
            string cpuId = GetWmiData("Win32_Processor", "ProcessorId");
            string diskId = GetWmiData("Win32_DiskDrive", "SerialNumber");
            string mbId = GetWmiData("Win32_BaseBoard", "SerialNumber");

            // Bilgileri birleştir ve hash'e dönüştür
            string rawData = $"{cpuId}-{diskId}-{mbId}";
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16); // 16 karakterlik ID
            }
        }

        private static string GetWmiData(string className, string propertyName)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
                var results = searcher.Get().Cast<ManagementObject>().ToList();

                // Hiç sonuç yoksa "N/A" döndür
                if (results.Count == 0)
                    return "N/A";

                // İlk sonucun özelliğini döndür
                return results[0][propertyName]?.ToString().Trim() ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }
    }

    public static class LicenseGenerator
    {
        // AES Anahtarı (Üretimde güvenli bir yerde saklayın)
        internal static readonly byte[] AES_KEY = Encoding.UTF8.GetBytes("FpUhTKmoiAbhG742HoLueneoFja1nMhJ");
        internal static readonly byte[] AES_IV = new byte[16]; // 128-bit IV
       // public static byte[] GetAesKEY() => _aesKEY;
       // public static byte[] GetAesIV() => _aesIV;

        public static string Generate(string hardwareId, DateTime expiryDate)
        {
            string rawData = $"{hardwareId}|{expiryDate:yyyy-MM-dd}";
            using (Aes aes = Aes.Create())
            {
                aes.Key = AES_KEY;
                aes.IV = AES_IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(rawData);
                        cs.Write(data, 0, data.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }


    public class LicenseValidator
    {
        public static bool ValidateLicense(string licenseKey, out DateTime expiryDate)
        {

            expiryDate = DateTime.MinValue;
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = LicenseGenerator.AES_KEY;
                    aes.IV = LicenseGenerator.AES_IV;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(licenseKey)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                string[] parts = sr.ReadToEnd().Split('|');
                                if (parts.Length != 2) return false;

                                string currentHardwareId = HardwareIdGenerator.GetHardwareId();
                                expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd", null);

                                return (parts[0] == currentHardwareId && DateTime.Now <= expiryDate);
                            }

                        }
                    }
                }
                
                

            }
            catch { return false; }
        }
    }

    
}
