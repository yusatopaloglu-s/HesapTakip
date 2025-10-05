namespace HesapTakip
{
    partial class EDefterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EDefterForm));
            dgvFirmaList = new DataGridView();
            btnekle = new Button();
            btnsil = new Button();
            btnedit = new Button();
            btnhepsi1 = new Button();
            btncikar = new Button();
            gbKontor = new GroupBox();
            txtkontor = new TextBox();
            lblTotal = new Label();
            dgvKontorList = new DataGridView();
            dtpDate = new DateTimePicker();
            gb_edeftermusteri = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dgvFirmaList).BeginInit();
            gbKontor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKontorList).BeginInit();
            gb_edeftermusteri.SuspendLayout();
            SuspendLayout();
            // 
            // dgvFirmaList
            // 
            dgvFirmaList.AllowUserToAddRows = false;
            dgvFirmaList.AllowUserToDeleteRows = false;
            dgvFirmaList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFirmaList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvFirmaList.BackgroundColor = SystemColors.Control;
            dgvFirmaList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFirmaList.ColumnHeadersVisible = false;
            dgvFirmaList.Location = new Point(0, 15);
            dgvFirmaList.Name = "dgvFirmaList";
            dgvFirmaList.ReadOnly = true;
            dgvFirmaList.RowHeadersVisible = false;
            dgvFirmaList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvFirmaList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFirmaList.Size = new Size(213, 528);
            dgvFirmaList.TabIndex = 5;
            dgvFirmaList.SelectionChanged += dgvCustomers_SelectionChanged;
            // 
            // btnekle
            // 
            btnekle.Location = new Point(460, 85);
            btnekle.Name = "btnekle";
            btnekle.Size = new Size(58, 23);
            btnekle.TabIndex = 2;
            btnekle.Text = "+";
            btnekle.UseVisualStyleBackColor = true;
            btnekle.Click += btnekle_Click;
            // 
            // btnsil
            // 
            btnsil.Location = new Point(524, 114);
            btnsil.Name = "btnsil";
            btnsil.Size = new Size(55, 23);
            btnsil.TabIndex = 3;
            btnsil.Text = "Sil";
            btnsil.UseVisualStyleBackColor = true;
            btnsil.Click += btnsil_Click;
            // 
            // btnedit
            // 
            btnedit.Location = new Point(460, 114);
            btnedit.Name = "btnedit";
            btnedit.Size = new Size(58, 23);
            btnedit.TabIndex = 4;
            btnedit.Text = "Düzelt";
            btnedit.UseVisualStyleBackColor = true;
            btnedit.Visible = false;
            // 
            // btnhepsi1
            // 
            btnhepsi1.Location = new Point(460, 143);
            btnhepsi1.Name = "btnhepsi1";
            btnhepsi1.Size = new Size(119, 23);
            btnhepsi1.TabIndex = 5;
            btnhepsi1.Text = "Hepsinden 1 Düş";
            btnhepsi1.UseVisualStyleBackColor = true;
            btnhepsi1.Click += btnhepsi1_Click;
            // 
            // btncikar
            // 
            btncikar.Location = new Point(524, 85);
            btncikar.Name = "btncikar";
            btncikar.Size = new Size(55, 23);
            btncikar.TabIndex = 6;
            btncikar.Text = "-";
            btncikar.UseVisualStyleBackColor = true;
            btncikar.Click += btncikar_Click;
            // 
            // gbKontor
            // 
            gbKontor.AutoSize = true;
            gbKontor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbKontor.Controls.Add(btncikar);
            gbKontor.Controls.Add(txtkontor);
            gbKontor.Controls.Add(btnhepsi1);
            gbKontor.Controls.Add(lblTotal);
            gbKontor.Controls.Add(btnedit);
            gbKontor.Controls.Add(dgvKontorList);
            gbKontor.Controls.Add(btnsil);
            gbKontor.Controls.Add(dtpDate);
            gbKontor.Controls.Add(btnekle);
            gbKontor.Location = new Point(231, 27);
            gbKontor.Name = "gbKontor";
            gbKontor.Size = new Size(585, 526);
            gbKontor.TabIndex = 4;
            gbKontor.TabStop = false;
            gbKontor.Text = "e-Defter Kontor";
            // 
            // txtkontor
            // 
            txtkontor.Location = new Point(354, 56);
            txtkontor.Name = "txtkontor";
            txtkontor.Size = new Size(100, 23);
            txtkontor.TabIndex = 2;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(354, 492);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(55, 15);
            lblTotal.TabIndex = 3;
            lblTotal.Text = "Kontor: 0";
            // 
            // dgvKontorList
            // 
            dgvKontorList.AllowUserToAddRows = false;
            dgvKontorList.AllowUserToDeleteRows = false;
            dgvKontorList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKontorList.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvKontorList.BackgroundColor = SystemColors.Control;
            dgvKontorList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKontorList.Location = new Point(6, 85);
            dgvKontorList.Name = "dgvKontorList";
            dgvKontorList.Size = new Size(451, 404);
            dgvKontorList.TabIndex = 0;
            // 
            // dtpDate
            // 
            dtpDate.AccessibleRole = AccessibleRole.TitleBar;
            dtpDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpDate.CalendarForeColor = Color.LavenderBlush;
            dtpDate.CalendarMonthBackground = SystemColors.WindowFrame;
            dtpDate.CustomFormat = "dd.MM.yyyy";
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.Location = new Point(354, 22);
            dtpDate.Name = "dtpDate";
            dtpDate.RightToLeftLayout = true;
            dtpDate.Size = new Size(100, 23);
            dtpDate.TabIndex = 1;
            dtpDate.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            // 
            // gb_edeftermusteri
            // 
            gb_edeftermusteri.AutoSize = true;
            gb_edeftermusteri.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gb_edeftermusteri.Controls.Add(dgvFirmaList);
            gb_edeftermusteri.Location = new Point(12, 27);
            gb_edeftermusteri.Name = "gb_edeftermusteri";
            gb_edeftermusteri.Size = new Size(219, 565);
            gb_edeftermusteri.TabIndex = 1;
            gb_edeftermusteri.TabStop = false;
            gb_edeftermusteri.Text = "Müşteri Listesi";
            // 
            // EDefterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(823, 574);
            Controls.Add(gbKontor);
            Controls.Add(gb_edeftermusteri);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "EDefterForm";
            Text = "e-Defter Kontor Takip";
            ((System.ComponentModel.ISupportInitialize)dgvFirmaList).EndInit();
            gbKontor.ResumeLayout(false);
            gbKontor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKontorList).EndInit();
            gb_edeftermusteri.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel customerPanel;
        private DataGridView dgvCustomers;
        private DataGridView dgvFirmaList;
        private Panel panel1;
        private DataGridView dgvKontorList;
        private TextBox txtkontor;
        private Button btnekle;
        private Button btnsil;
        private Label lblTotal;
        private Button btnedit;
        private GroupBox gbKontor;
        public DateTimePicker dtpDate;
        private Button btnhepsi1;
        private Button btncikar;
        private SplitContainer splitContainer1;
        private GroupBox groupBox1;
        private GroupBox gb_edeftermusteri;
    }
}