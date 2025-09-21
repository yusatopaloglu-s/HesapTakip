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
                            Application.Run(new ActivationForm()); // Aktivasyon formunu a�
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
                        // Ba�lant� ayar� kontrol�
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
                                    MessageBox.Show("Ba�lant� Hatas�"); return;
                                }
                                
                            }
                        } // Lisans ge�erli, ana forma ge�
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
