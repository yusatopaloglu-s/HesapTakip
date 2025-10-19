namespace HesapTakip
{
    partial class ExpenseCategoryForm
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
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            textBox1 = new TextBox();
            dgv_expensecatlist = new DataGridView();
            btn_add = new Button();
            btn_rmv = new Button();
            dvg_matchlist = new DataGridView();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_expensecatlist).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dvg_matchlist).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(groupBox3);
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(800, 450);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "İşletme Alış Faturaları için Gider Kalemlerinden DBS Kayıt Alt Türü Eşleme";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            groupBox2.AutoSize = true;
            groupBox2.Controls.Add(dgv_expensecatlist);
            groupBox2.Location = new Point(6, 22);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 416);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "DBS Kayıt Alt Türü Listesi";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(dvg_matchlist);
            groupBox3.Controls.Add(btn_rmv);
            groupBox3.Controls.Add(btn_add);
            groupBox3.Controls.Add(textBox1);
            groupBox3.Location = new Point(212, 22);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(387, 416);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Eşleştirmeler";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(146, 28);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 0;
            // 
            // dgv_expensecatlist
            // 
            dgv_expensecatlist.BackgroundColor = SystemColors.Control;
            dgv_expensecatlist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_expensecatlist.Location = new Point(0, 57);
            dgv_expensecatlist.Name = "dgv_expensecatlist";
            dgv_expensecatlist.Size = new Size(183, 170);
            dgv_expensecatlist.TabIndex = 0;
            // 
            // btn_add
            // 
            btn_add.Location = new Point(298, 21);
            btn_add.Name = "btn_add";
            btn_add.Size = new Size(83, 23);
            btn_add.TabIndex = 1;
            btn_add.Text = "Eşleşme Ekle";
            btn_add.UseVisualStyleBackColor = true;
            // 
            // btn_rmv
            // 
            btn_rmv.Location = new Point(298, 50);
            btn_rmv.Name = "btn_rmv";
            btn_rmv.Size = new Size(83, 23);
            btn_rmv.TabIndex = 2;
            btn_rmv.Text = "Eşleşme Sil";
            btn_rmv.UseVisualStyleBackColor = true;
            // 
            // dvg_matchlist
            // 
            dvg_matchlist.BackgroundColor = SystemColors.Control;
            dvg_matchlist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dvg_matchlist.Location = new Point(6, 57);
            dvg_matchlist.Name = "dvg_matchlist";
            dvg_matchlist.Size = new Size(240, 300);
            dvg_matchlist.TabIndex = 3;
            // 
            // ExpenseCategoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(groupBox1);
            Name = "ExpenseCategoryForm";
            Text = "İşletme Kayıt Alt Türü Eşleme";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_expensecatlist).EndInit();
            ((System.ComponentModel.ISupportInitialize)dvg_matchlist).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Button btn_rmv;
        private Button btn_add;
        private TextBox textBox1;
        private DataGridView dgv_expensecatlist;
        private DataGridView dvg_matchlist;
    }
}