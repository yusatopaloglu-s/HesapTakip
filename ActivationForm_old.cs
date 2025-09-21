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
using System.IO;
using System.Security;

namespace HesapTakip
{
    public partial class ActivationForm : Form
    {
        public ActivationForm()
        {
            InitializeComponent();
            lblHardwareId.Text = HardwareIdGenerator.GetHardwareId();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            string licenseKey = txtLicenseKey.Text.Trim();
            if (LicenseValidator.ValidateLicense(licenseKey, out DateTime expiryDate))
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
            else if (expiryDate != DateTime.MinValue && DateTime.Now > expiryDate)
            {
                MessageBox.Show("Lisans süresi dolmuş!");
            }
            else
            {
                MessageBox.Show("Geçersiz lisans anahtarı!");
            }
        }
    }

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
        private const string KEY_SALT = "YourAppSpecificSalt123"; // Uygulamanıza özel bir salt değeri

        public static byte[] GetAesKey()
        {
            try
            {
                // Anahtarı şifrelenmiş dosyadan oku
                if (File.Exists(KEY_FILE_NAME))
                {
                    byte[] encryptedKey = File.ReadAllBytes(KEY_FILE_NAME);
                    return ProtectedData.Unprotect(encryptedKey,
                        Encoding.UTF8.GetBytes(KEY_SALT),
                        DataProtectionScope.LocalMachine);
                }

                // Dosya yoksa yeni anahtar oluştur ve kaydet
                using (Aes aes = Aes.Create())
                {
                    aes.GenerateKey();
                    byte[] protectedKey = ProtectedData.Protect(aes.Key,
                        Encoding.UTF8.GetBytes(KEY_SALT),
                        DataProtectionScope.LocalMachine);

                    File.WriteAllBytes(KEY_FILE_NAME, protectedKey);
                    return aes.Key;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güvenli anahtar yüklenemedi: {ex.Message}");
                throw new InvalidOperationException("Güvenli anahtar yüklenemedi", ex);
            }
        }

        public static byte[] GetAesIV()
        {
            // Sabit IV (Initialization Vector) - Üretimde daha güvenli bir yöntem kullanılmalı
            return Encoding.UTF8.GetBytes("FixedIV1234567890"); // 16 byte
        }
    }

    public class LicenseValidator
    {
        private static readonly Lazy<byte[]> _aesKey = new Lazy<byte[]>(() => SecureKeyManager.GetAesKey());
        private static readonly Lazy<byte[]> _aesIV = new Lazy<byte[]>(() => SecureKeyManager.GetAesIV());

        public static bool ValidateLicense(string licenseKey, out DateTime expiryDate)
        {
            expiryDate = DateTime.MinValue;

            if (string.IsNullOrEmpty(licenseKey))
                return false;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _aesKey.Value;
                    aes.IV = _aesIV.Value;
                    aes.Padding = PaddingMode.PKCS7;

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

                                if (!DateTime.TryParseExact(parts[1], "yyyy-MM-dd", null,
                                    System.Globalization.DateTimeStyles.None, out expiryDate))
                                    return false;

                                return (parts[0] == currentHardwareId && DateTime.Now <= expiryDate);
                            }
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                // Şifreleme hatası - geçersiz lisans
                return false;
            }
            catch (FormatException)
            {
                // Base64 format hatası
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lisans doğrulama hatası: {ex.Message}");
                return false;
            }
        }
    }
}