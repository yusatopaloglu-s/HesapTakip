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

            if (!File.Exists("license.lic") || !LicenseValidator.ValidateLicense(File.ReadAllText("license.lic"), out _))
            {
                Application.Run(new ActivationForm()); // Aktivasyon formunu aç
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
                        return;
                    }
                }
            }

            // Güncelleme kontrolünü baþlat (isteðe baðlý, MainForm'da da çaðrýlabilir)
            //CheckForUpdate().GetAwaiter().GetResult();

            Application.Run(new MainForm());
        }

        // Statik ve async olarak tanýmlandý, Main'den çaðýrmak için uygun
         }


}
