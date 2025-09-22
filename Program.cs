using QuestPDF.Infrastructure;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace HesapTakip
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ESK� AYARLARI YEN� KONUMA TA�I
            AppConfigHelper.MigrateOldSettings();

            if (File.Exists("license.lic"))
            {
                string licenseKey = File.ReadAllText("license.lic");
                if (ActivationForm.LicenseValidator.ValidateLicense(licenseKey, out DateTime expiryDate))
                {
                    if (DateTime.Now <= expiryDate)
                    {
                        // �ZEL CONFIG'TEN OKU
                        string connectionString = AppConfigHelper.DatabasePath;

                        // E�er yoksa, eski settings'ten oku ve yeni config'e ta��
                        if (string.IsNullOrEmpty(connectionString))
                        {
                            connectionString = Properties.Settings.Default.DatabasePath;
                            if (!string.IsNullOrEmpty(connectionString))
                            {
                                AppConfigHelper.DatabasePath = connectionString;
                            }
                        }

                        // Ba�lant� ayar� kontrol�
                        if (string.IsNullOrEmpty(connectionString) || !AppConfigHelper.HasValidConnectionString())
                        {
                            using (var settingsForm = new ConnectionSettingsForm())
                            {
                                if (settingsForm.ShowDialog() == DialogResult.OK)
                                {
                                    AppConfigHelper.SaveConnectionString(
                                        settingsForm.Server,
                                        settingsForm.Database,
                                        settingsForm.User,
                                        settingsForm.Password,
                                        settingsForm.Port
                                    );

                                    // Eski settings'i de g�ncelle
                                    Properties.Settings.Default.DatabasePath = AppConfigHelper.DatabasePath;
                                    Properties.Settings.Default.Save();
                                }
                                else
                                {
                                    MessageBox.Show("Ba�lant� ayarlar� girilmedi. Uygulama kapat�l�yor.");
                                    return;
                                }
                            }
                        }

                        Application.Run(new MainForm());
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Lisans s�reniz dolmu�. L�tfen yenileyin.");
                    }
                }
            }

            Application.Run(new ActivationForm());
        }
    }
}