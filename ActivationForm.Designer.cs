namespace HesapTakip
{
    partial class ActivationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivationForm));
            txtLicenseKey = new TextBox();
            lblHardwareId = new TextBox();
            btnActivate = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnGenerateKey = new Button();
            SuspendLayout();
            // 
            // txtLicenseKey
            // 
            txtLicenseKey.Location = new Point(123, 72);
            txtLicenseKey.Name = "txtLicenseKey";
            txtLicenseKey.PlaceholderText = "AKTİVASYON KODUNU GİRİNİZ";
            txtLicenseKey.Size = new Size(168, 23);
            txtLicenseKey.TabIndex = 1;
            // 
            // lblHardwareId
            // 
            lblHardwareId.Location = new Point(123, 43);
            lblHardwareId.Name = "lblHardwareId";
            lblHardwareId.ReadOnly = true;
            lblHardwareId.Size = new Size(168, 23);
            lblHardwareId.TabIndex = 2;
            // 
            // btnActivate
            // 
            btnActivate.AccessibleName = "";
            btnActivate.Location = new Point(203, 101);
            btnActivate.Name = "btnActivate";
            btnActivate.Size = new Size(88, 23);
            btnActivate.TabIndex = 3;
            btnActivate.Text = "Aktive Et";
            btnActivate.UseVisualStyleBackColor = true;
            btnActivate.Click += btnActivate_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 46);
            label1.Name = "label1";
            label1.Size = new Size(105, 15);
            label1.TabIndex = 4;
            label1.Text = "CİHAZ NUMARASI";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 75);
            label2.Name = "label2";
            label2.Size = new Size(110, 15);
            label2.TabIndex = 5;
            label2.Text = "AKTİVASYON KODU";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(120, 15);
            label3.TabIndex = 6;
            label3.Text = "AKTİVASYON FORMU";
            // 
            // btnGenerateKey
            // 
            btnGenerateKey.Location = new Point(122, 101);
            btnGenerateKey.Name = "btnGenerateKey";
            btnGenerateKey.Size = new Size(75, 23);
            btnGenerateKey.TabIndex = 7;
            btnGenerateKey.Text = "Üret";
            btnGenerateKey.UseVisualStyleBackColor = true;
            btnGenerateKey.Click += btnGenerateSecureKey_Click;
            // 
            // ActivationForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(306, 144);
            Controls.Add(btnGenerateKey);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnActivate);
            Controls.Add(lblHardwareId);
            Controls.Add(txtLicenseKey);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ActivationForm";
            Text = "Yuşa' Hesap Takip Aktivasyon";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox txtLicenseKey;
        private TextBox lblHardwareId;
        private Button btnActivate;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnGenerateKey;
    }
}