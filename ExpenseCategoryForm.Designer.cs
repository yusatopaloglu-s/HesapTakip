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
            groupBox3 = new GroupBox();
            label2 = new Label();
            dvg_matchlist = new DataGridView();
            btn_rmv = new Button();
            btn_add = new Button();
            textBox1 = new TextBox();
            groupBox2 = new GroupBox();
            txtSearch = new TextBox();
            label1 = new Label();
            dgv_expensecatlist = new DataGridView();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dvg_matchlist).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_expensecatlist).BeginInit();
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
            groupBox1.Size = new Size(752, 460);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "İşletme Alış Faturaları için Gider Kalemlerinden DBS Kayıt Alt Türü Eşleme";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(dvg_matchlist);
            groupBox3.Controls.Add(btn_rmv);
            groupBox3.Controls.Add(btn_add);
            groupBox3.Controls.Add(textBox1);
            groupBox3.Location = new Point(337, 22);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(400, 448);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Eşleştirmeler";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 19);
            label2.Name = "label2";
            label2.Size = new Size(104, 15);
            label2.TabIndex = 4;
            label2.Text = "Faturadaki Kalem :";
            // 
            // dvg_matchlist
            // 
            dvg_matchlist.AllowUserToAddRows = false;
            dvg_matchlist.AllowUserToDeleteRows = false;
            dvg_matchlist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dvg_matchlist.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dvg_matchlist.BackgroundColor = SystemColors.Control;
            dvg_matchlist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dvg_matchlist.Location = new Point(6, 48);
            dvg_matchlist.Name = "dvg_matchlist";
            dvg_matchlist.Size = new Size(388, 378);
            dvg_matchlist.TabIndex = 3;
            // 
            // btn_rmv
            // 
            btn_rmv.Location = new Point(310, 16);
            btn_rmv.Name = "btn_rmv";
            btn_rmv.Size = new Size(83, 23);
            btn_rmv.TabIndex = 2;
            btn_rmv.Text = "Eşleşme Sil";
            btn_rmv.UseVisualStyleBackColor = true;
            btn_rmv.Click += BtnRmv_Click;
            // 
            // btn_add
            // 
            btn_add.Location = new Point(221, 15);
            btn_add.Name = "btn_add";
            btn_add.Size = new Size(83, 23);
            btn_add.TabIndex = 1;
            btn_add.Text = "Eşleşme Ekle";
            btn_add.UseVisualStyleBackColor = true;
            btn_add.Click += BtnAdd_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(115, 16);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            groupBox2.Controls.Add(txtSearch);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(dgv_expensecatlist);
            groupBox2.Location = new Point(6, 22);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(320, 432);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "DBS Kayıt Alt Türü Listesi";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(43, 19);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(154, 23);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 22);
            label1.Name = "label1";
            label1.Size = new Size(31, 15);
            label1.TabIndex = 1;
            label1.Text = "Ara :";
            // 
            // dgv_expensecatlist
            // 
            dgv_expensecatlist.AllowUserToAddRows = false;
            dgv_expensecatlist.AllowUserToDeleteRows = false;
            dgv_expensecatlist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv_expensecatlist.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgv_expensecatlist.BackgroundColor = SystemColors.Control;
            dgv_expensecatlist.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv_expensecatlist.Location = new Point(3, 48);
            dgv_expensecatlist.Name = "dgv_expensecatlist";
            dgv_expensecatlist.Size = new Size(311, 377);
            dgv_expensecatlist.TabIndex = 0;
            // 
            // ExpenseCategoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(752, 460);
            Controls.Add(groupBox1);
            Name = "ExpenseCategoryForm";
            Text = "İşletme Kayıt Alt Türü Eşleme";
            groupBox1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dvg_matchlist).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv_expensecatlist).EndInit();
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
        private TextBox txtSearch;
        private Label label1;
        private Label label2;
    }
}