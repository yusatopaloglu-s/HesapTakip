namespace HesapTakip
{
    partial class EFaturaFiltre
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
            comboBox1 = new ComboBox();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            groupBox1 = new GroupBox();
            label4 = new Label();
            combo_kdv_oranlar = new ComboBox();
            label3 = new Label();
            label1 = new Label();
            label2 = new Label();
            btn_yeni_kalem = new Button();
            btn_kaydet = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(147, 12);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(147, 42);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(121, 23);
            textBox1.TabIndex = 1;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(110, 22);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(184, 23);
            textBox2.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btn_yeni_kalem);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(combo_kdv_oranlar);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Location = new Point(47, 70);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(617, 302);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(300, 25);
            label4.Name = "label4";
            label4.Size = new Size(61, 15);
            label4.TabIndex = 5;
            label4.Text = "KDV Oranı";
            label4.Click += label4_Click;
            // 
            // combo_kdv_oranlar
            // 
            combo_kdv_oranlar.AutoCompleteMode = AutoCompleteMode.Suggest;
            combo_kdv_oranlar.AutoCompleteSource = AutoCompleteSource.ListItems;
            combo_kdv_oranlar.FormattingEnabled = true;
            combo_kdv_oranlar.Items.AddRange(new object[] { "0", "1", "10", "20" });
            combo_kdv_oranlar.Location = new Point(367, 22);
            combo_kdv_oranlar.Name = "combo_kdv_oranlar";
            combo_kdv_oranlar.Size = new Size(69, 23);
            combo_kdv_oranlar.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 25);
            label3.Name = "label3";
            label3.Size = new Size(98, 15);
            label3.TabIndex = 3;
            label3.Text = "Faturadaki Kalem";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(68, 9);
            label1.Name = "label1";
            label1.Size = new Size(74, 30);
            label1.TabIndex = 4;
            label1.Text = "Kullanılacak \r\nŞablon Tablo";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(60, 49);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 5;
            label2.Text = "Yeni Tablo Adı";
            // 
            // btn_yeni_kalem
            // 
            btn_yeni_kalem.Location = new Point(462, 22);
            btn_yeni_kalem.Name = "btn_yeni_kalem";
            btn_yeni_kalem.Size = new Size(75, 23);
            btn_yeni_kalem.TabIndex = 6;
            btn_yeni_kalem.Text = "Yeni Ekle";
            btn_yeni_kalem.UseVisualStyleBackColor = true;
            // 
            // btn_kaydet
            // 
            btn_kaydet.Location = new Point(509, 378);
            btn_kaydet.Name = "btn_kaydet";
            btn_kaydet.Size = new Size(75, 23);
            btn_kaydet.TabIndex = 6;
            btn_kaydet.Text = "Kaydet";
            btn_kaydet.UseVisualStyleBackColor = true;
            // 
            // EFaturaFiltre
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 435);
            Controls.Add(btn_kaydet);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(groupBox1);
            Controls.Add(textBox1);
            Controls.Add(comboBox1);
            Name = "EFaturaFiltre";
            Text = "E-Fatura Filtre";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBox1;
        private TextBox textBox1;
        private TextBox textBox2;
        private GroupBox groupBox1;
        private Label label4;
        private ComboBox combo_kdv_oranlar;
        private Label label3;
        private Label label1;
        private Label label2;
        private Button btn_yeni_kalem;
        private Button btn_kaydet;
    }
}