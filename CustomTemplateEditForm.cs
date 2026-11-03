using HesapTakip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;

namespace HesapTakip
{
    public partial class CustomTemplateEditForm : Form
    {
        private string templateDirectory;
        private List<string> templateNames = new List<string>();
        private Dictionary<string, string> templateContents = new Dictionary<string, string>();
        private int selectedTemplateIndex = -1;

        public CustomTemplateEditForm()
        {
            InitializeComponent();
            templateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", "custom");
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(templateDirectory))
            {
                Directory.CreateDirectory(templateDirectory);
                MessageBox.Show("Öncelikle bir şablon oluşturmalısınız.", "Bilgilendirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            // Get all JSON files in the custom folder
            templateNames.Clear();
            templateContents.Clear();

            var files = Directory.GetFiles(templateDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var content = File.ReadAllText(file);
                    templateNames.Add(fileName);
                    templateContents.Add(fileName, content);
                }
                catch { }
            }

            // Sort alphabetically
            templateNames.Sort();

            // Update UI
            cmbTemplates.DataSource = templateNames;
            if (templateNames.Count > 0)
            {
                cmbTemplates.SelectedIndex = 0;
                LoadTemplateContent(templateNames[0]);
            }
            else
            {
                txtContent.Text = "{\"Name\": \"Yeni Şablon\", \"Columns\": []}";
                btnDelete.Enabled = false;
            }
        }

        private void cmbTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTemplates.SelectedIndex >= 0 && cmbTemplates.SelectedIndex < templateNames.Count)
            {
                LoadTemplateContent(cmbTemplates.SelectedItem.ToString());
                btnDelete.Enabled = true;
            }
        }

        private void LoadTemplateContent(string templateName)
        {
            if (templateContents.TryGetValue(templateName, out var content))
            {
                txtContent.Text = content;
            }
            else
            {
                txtContent.Text = "{}";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (selectedTemplateIndex < 0 || selectedTemplateIndex >= templateNames.Count)
                return;

            var templateName = templateNames[selectedTemplateIndex];
            var newContent = txtContent.Text.Trim();

            try
            {
                // Validate JSON
                JsonDocument.Parse(newContent);

                // Save to file
                var filePath = Path.Combine(templateDirectory, $"{templateName}.json");
                File.WriteAllText(filePath, newContent);

                // Refresh the list
                LoadTemplates();

                MessageBox.Show($"Şablon '{templateName}' başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (JsonException)
            {
                MessageBox.Show("Geçersiz JSON formatı! Lütfen geçerli bir JSON girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedTemplateIndex < 0 || selectedTemplateIndex >= templateNames.Count)
                return;

            var templateName = templateNames[selectedTemplateIndex];

            var result = MessageBox($"'{templateName}' şablonunu silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    var filePath = Path.Combine(templateDirectory, $"{templateName}.json");
                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    LoadTemplates();
                    MessageBox.Show($"Şablon '{templateName}' silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Helper method for message box
        private void ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, buttons, icon);
        }
    }
}