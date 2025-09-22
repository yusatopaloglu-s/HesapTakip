using System;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace HesapTakip
{
    public static class AppConfigHelper
    {
        // Sabit klasör yolu - her versiyonda aynı kalacak
        private static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HesapTakip");

        public static string ConfigFilePath => Path.Combine(AppDataFolder, "HesapTakip.config");

        static AppConfigHelper()
        {
            // Klasörü oluştur (eğer yoksa)
            if (!Directory.Exists(AppDataFolder))
            {
                Directory.CreateDirectory(AppDataFolder);
            }
        }

        public static string DatabasePath
        {
            get
            {
                try
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(ConfigFilePath);
                        var node = doc.SelectSingleNode("//DatabasePath");
                        return node?.InnerText ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Config okuma hatası: {ex.Message}");
                }

                return string.Empty;
            }
            set
            {
                try
                {
                    XmlDocument doc = new XmlDocument();

                    if (File.Exists(ConfigFilePath))
                    {
                        doc.Load(ConfigFilePath);
                    }
                    else
                    {
                        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                        doc.AppendChild(doc.CreateElement("Configuration"));
                    }

                    var node = doc.SelectSingleNode("//DatabasePath");
                    if (node == null)
                    {
                        node = doc.CreateElement("DatabasePath");
                        doc.DocumentElement.AppendChild(node);
                    }

                    node.InnerText = value;
                    doc.Save(ConfigFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Config kaydetme hatası: {ex.Message}");
                }
            }
        }

        public static void SaveConnectionString(string server, string database, string user, string password, string port)
        {
            string connectionString = $"Server={server};Database={database};User={user};Password={password};Port={port};Charset=utf8mb4;";
            DatabasePath = connectionString;
        }

        public static bool HasValidConnectionString()
        {
            return !string.IsNullOrEmpty(DatabasePath) && DatabasePath.Contains("Server=");
        }

        // Eski ayarları yeni konuma taşıma (migration)
        public static void MigrateOldSettings()
        {
            try
            {
                // Eski ClickOnce settings yolunu bul
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string[] oldPaths = Directory.GetDirectories(localAppData, "HesapTakip_Url_*", SearchOption.TopDirectoryOnly);

                foreach (string oldPath in oldPaths)
                {
                    string[] versionDirs = Directory.GetDirectories(oldPath);
                    foreach (string versionDir in versionDirs)
                    {
                        string userConfigPath = Path.Combine(versionDir, "user.config");
                        if (File.Exists(userConfigPath))
                        {
                            // Eski user.config'ten ayarları oku ve yeni konuma taşı
                            MigrateFromUserConfig(userConfigPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Migration hatası: {ex.Message}");
            }
        }

        private static void MigrateFromUserConfig(string userConfigPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(userConfigPath);

                // Eski ayarları oku
                var settingNodes = doc.SelectNodes("//setting[@name='DatabasePath']");
                if (settingNodes != null && settingNodes.Count > 0)
                {
                    string oldValue = settingNodes[0].InnerText;
                    if (!string.IsNullOrEmpty(oldValue) && string.IsNullOrEmpty(DatabasePath))
                    {
                        // Yeni konuma kaydet
                        DatabasePath = oldValue;
                        Debug.WriteLine("Eski ayarlar yeni konuma taşındı.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"User config migration hatası: {ex.Message}");
            }
        }
    }
}