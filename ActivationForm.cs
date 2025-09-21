using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static HesapTakip.ActivationForm;

namespace HesapTakip
{
    public partial class ActivationForm : Form
    {
        public class HardwareIdGenerator
        {
            public static string GetHardwareId()
            {
                string cpuId = GetWmiData("Win32_Processor", "ProcessorId");
                string diskId = GetWmiData("Win32_DiskDrive", "SerialNumber");
                string mbId = GetWmiData("Win32_BaseBoard", "SerialNumber");

                string rawData = $"{cpuId}-{diskId}-{mbId}";
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                    return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
                }
            }

            private static string GetWmiData(string className, string propertyName)
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
                    var results = searcher.Get().Cast<ManagementObject>().ToList();

                    if (results.Count == 0)
                        return "N/A";

                    return results[0][propertyName]?.ToString().Trim() ?? "N/A";
                }
                catch
                {
                    return "N/A";
                }
            }
        }
        public static class SecureKeyManager
        {
            private const string KEY_FILE_NAME = "secure.key";
            private const string KEY_SALT = "FpUhTKmoiAbhG742HoLueneoFja1nMhJ";

            public static string GenerateSecureKeyFile()
            {
                try
                {
                    string hardwareId = HardwareIdGenerator.GetHardwareId();

                    using (Aes aes = Aes.Create())
                    {
                        aes.GenerateKey();
                        aes.GenerateIV();

                        // Hardware ID ile anahtar ve IV'i birleştir
                        string keyData = $"{hardwareId}|{Convert.ToBase64String(aes.Key)}|{Convert.ToBase64String(aes.IV)}";

                        // Anahtar dosyasını şifrele ve kaydet
                        byte[] encryptedData = ProtectedData.Protect(
                            Encoding.UTF8.GetBytes(keyData),
                            Encoding.UTF8.GetBytes(KEY_SALT),
                            DataProtectionScope.LocalMachine
                        );

                        File.WriteAllBytes(KEY_FILE_NAME, encryptedData);

                        return $"secure.key dosyası oluşturuldu!\nHardware ID: {hardwareId}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Hata: {ex.Message}";
                }
            }

            public static bool LoadFromSecureKeyFile(out byte[] aesKey, out byte[] aesIV, out string hardwareId)
            {
                aesKey = null;
                aesIV = null;
                hardwareId = null;

                try
                {
                    if (!File.Exists(KEY_FILE_NAME))
                    {
                        MessageBox.Show("secure.key dosyası bulunamadı!");
                        return false;
                    }

                    byte[] encryptedData = File.ReadAllBytes(KEY_FILE_NAME);
                    byte[] decryptedData = ProtectedData.Unprotect(
                        encryptedData,
                        Encoding.UTF8.GetBytes(KEY_SALT),
                        DataProtectionScope.LocalMachine
                    );

                    string keyData = Encoding.UTF8.GetString(decryptedData);
                    string[] parts = keyData.Split('|');

                    if (parts.Length != 3)
                    {
                        MessageBox.Show("Geçersiz secure.key formatı!");
                        return false;
                    }

                    hardwareId = parts[0];
                    aesKey = Convert.FromBase64String(parts[1]);
                    aesIV = Convert.FromBase64String(parts[2]);

                    /* MessageBox.Show($"Yüklenen Hardware ID: {hardwareId}\n" +
                                   $"Key: {Convert.ToBase64String(aesKey)}\n" +
                                   $"IV: {Convert.ToBase64String(aesIV)}"); */

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya yükleme hatası: {ex.Message}");
                    return false;
                }
            }

            public static byte[] GetAesKey()
            {
                if (LoadFromSecureKeyFile(out byte[] key, out byte[] iv, out string hardwareId))
                {
                    return key;
                }
                throw new InvalidOperationException("Anahtar yüklenemedi");
            }

            public static byte[] GetAesIV()
            {
                if (LoadFromSecureKeyFile(out byte[] key, out byte[] iv, out string hardwareId))
                {
                    return iv;
                }
                throw new InvalidOperationException("IV yüklenemedi");
            }
        }
        public class LicenseValidator
        {
            public static bool ValidateLicense(string licenseKey, out DateTime expiryDate)
            {
                expiryDate = DateTime.MinValue;

                if (string.IsNullOrEmpty(licenseKey))
                    return false;

                try
                {
                    byte[] key = SecureKeyManager.GetAesKey();
                    byte[] iv = SecureKeyManager.GetAesIV();

                    // IV boyutunu kontrol et ve düzelt
                    if (iv.Length != 16)
                    {
                        Array.Resize(ref iv, 16);
                    }

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Mode = CipherMode.CBC;

                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                        byte[] encryptedData = Convert.FromBase64String(licenseKey);

                        using (MemoryStream ms = new MemoryStream(encryptedData))
                        {
                            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader sr = new StreamReader(cs))
                                {
                                    string decryptedData = sr.ReadToEnd();
                                    string[] parts = decryptedData.Split('|');

                                    if (parts.Length != 2)
                                        return false;

                                    string currentHardwareId = HardwareIdGenerator.GetHardwareId();
                                    expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd", null);

                                    return (parts[0] == currentHardwareId && DateTime.Now <= expiryDate);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public ActivationForm()
        {
            InitializeComponent();
            lblHardwareId.Text = HardwareIdGenerator.GetHardwareId();
        }

        private void btnGenerateSecureKey_Click(object sender, EventArgs e)
        {
            string result = SecureKeyManager.GenerateSecureKeyFile();
            MessageBox.Show(result);

            // Dosya yolunu göster
            if (File.Exists("secure.key"))
            {
                string fullPath = Path.GetFullPath("secure.key");
                MessageBox.Show($"secure.key dosyası oluşturuldu:\n{fullPath}\n\n" +
                               "Bu dosyayı lisans üreticiye kopyalayın.");
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();

            // Önce secure.key dosyasını yükle
            if (!SecureKeyManager.LoadFromSecureKeyFile(out byte[] key, out byte[] iv, out string hardwareId))
            {
                MessageBox.Show("secure.key dosyası yüklenemedi! Önce dosyayı oluşturun.");
                return;
            }

            // Hardware ID kontrolü
            string currentHardwareId = HardwareIdGenerator.GetHardwareId();
            if (hardwareId != currentHardwareId)
            {
                MessageBox.Show($"Hardware ID uyuşmuyor!\n" +
                               $"Dosyadaki: {hardwareId}\n" +
                               $"Mevcut: {currentHardwareId}\n\n" +
                               "Lütfen doğru secure.key dosyasını kullanın.");
                return;
            }

            if (ValidateLicenseWithKey(licenseKey, key, iv, out DateTime expiryDate))
            {
                try
                {
                    File.WriteAllText("license.lic", licenseKey);
                    MessageBox.Show($"Lisans aktif! Son kullanma: {expiryDate:dd/MM/yyyy}");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lisans kaydedilemedi: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Geçersiz lisans anahtarı!");
            }
        }

        private bool ValidateLicenseWithKey(string licenseKey, byte[] aesKey, byte[] aesIV, out DateTime expiryDate)
        {
            expiryDate = DateTime.MinValue;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] encryptedData = Convert.FromBase64String(licenseKey);

                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                string decryptedData = sr.ReadToEnd();
                                string[] parts = decryptedData.Split('|');

                                if (parts.Length != 2) return false;

                                string currentHardwareId = HardwareIdGenerator.GetHardwareId();
                                expiryDate = DateTime.ParseExact(parts[1], "yyyy-MM-dd", null);

                                return (parts[0] == currentHardwareId && DateTime.Now <= expiryDate);
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}


