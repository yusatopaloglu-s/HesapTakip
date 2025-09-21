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
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            /*            if (!File.Exists("license.lic") || !LicenseValidator.ValidateLicense(File.ReadAllText("license.lic"), out _))
                        {
                            Application.Run(new ActivationForm()); // Aktivasyon formunu aç
                            return;
                        } */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (File.Exists("license.lic"))
            {
                string licenseKey = File.ReadAllText("license.lic");
                if (ActivationForm.LicenseValidator.ValidateLicense(licenseKey, out DateTime expiryDate))
                {
                    if (DateTime.Now <= expiryDate)
                    {
                        // Baðlantý ayarý kontrolü
                        if (string.IsNullOrEmpty(Properties.Settings.Default.DatabasePath))
                        {
                            using (var settingsForm = new ConnectionSettingsForm())
                            {
                                if (settingsForm.ShowDialog() == DialogResult.OK)
                                {
                                    string connStr = $"Server={settingsForm.Server};Database={settingsForm.Database};User={settingsForm.User};Password={settingsForm.Password};port={settingsForm.Port};Charset=utf8mb4;";
                                    Properties.Settings.Default.DatabasePath = connStr;
                                    Properties.Settings.Default.Save();
                                    
                                }
                                else
                                {
                                    MessageBox.Show("Baðlantý Hatasý"); return;
                                }
                                
                            }
                        } // Lisans geçerli, ana forma geç
                        Application.Run(new MainForm());
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Lisans süreniz dolmuþ. Lütfen yenileyin.");
                    }
                }
            }

            Application.Run(new ActivationForm());
        }

    }
}
