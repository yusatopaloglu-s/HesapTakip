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
            customerPanel = new Panel();
            dgvFirmaList = new DataGridView();
            label4 = new Label();
            dgvCustomers = new DataGridView();
            panel1 = new Panel();
            txtkontor = new TextBox();
            gbKontor = new GroupBox();
            lblTotal = new Label();
            dgvKontorList = new DataGridView();
            dtpDate = new DateTimePicker();
            btnekle = new Button();
            btnsil = new Button();
            btnedit = new Button();
            btnhepsi1 = new Button();
            btncikar = new Button();
            customerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFirmaList).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).BeginInit();
            panel1.SuspendLayout();
            gbKontor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKontorList).BeginInit();
            SuspendLayout();
            // 
            // customerPanel
            // 
            customerPanel.AutoSize = true;
            customerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            customerPanel.Controls.Add(dgvFirmaList);
            customerPanel.Controls.Add(label4);
            customerPanel.Controls.Add(dgvCustomers);
            customerPanel.Location = new Point(0, 12);
            customerPanel.Name = "customerPanel";
            customerPanel.Size = new Size(261, 494);
            customerPanel.TabIndex = 0;
            // 
            // dgvFirmaList
            // 
            dgvFirmaList.AllowUserToAddRows = false;
            dgvFirmaList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFirmaList.BackgroundColor = SystemColors.Control;
            dgvFirmaList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFirmaList.ColumnHeadersVisible = false;
            dgvFirmaList.Location = new Point(3, 19);
            dgvFirmaList.Name = "dgvFirmaList";
            dgvFirmaList.ReadOnly = true;
            dgvFirmaList.RowHeadersVisible = false;
            dgvFirmaList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvFirmaList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFirmaList.Size = new Size(255, 472);
            dgvFirmaList.TabIndex = 5;
            dgvFirmaList.SelectionChanged += dgvCustomers_SelectionChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(80, 0);
            label4.Name = "label4";
            label4.Size = new Size(82, 15);
            label4.TabIndex = 4;
            label4.Text = "Müşteri Listesi";
            // 
            // dgvCustomers
            // 
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomers.BackgroundColor = SystemColors.Control;
            dgvCustomers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCustomers.ColumnHeadersVisible = false;
            dgvCustomers.Location = new Point(0, 19);
            dgvCustomers.Name = "dgvCustomers";
            dgvCustomers.ReadOnly = true;
            dgvCustomers.RowHeadersVisible = false;
            dgvCustomers.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.Size = new Size(255, 472);
            dgvCustomers.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtkontor);
            panel1.Controls.Add(gbKontor);
            panel1.Location = new Point(267, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(457, 552);
            panel1.TabIndex = 1;
            // 
            // txtkontor
            // 
            txtkontor.Location = new Point(357, 61);
            txtkontor.Name = "txtkontor";
            txtkontor.Size = new Size(100, 23);
            txtkontor.TabIndex = 2;
            // 
            // gbKontor
            // 
            gbKontor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbKontor.Controls.Add(lblTotal);
            gbKontor.Controls.Add(dgvKontorList);
            gbKontor.Controls.Add(dtpDate);
            gbKontor.Dock = DockStyle.Fill;
            gbKontor.Location = new Point(0, 0);
            gbKontor.Name = "gbKontor";
            gbKontor.Size = new Size(457, 552);
            gbKontor.TabIndex = 4;
            gbKontor.TabStop = false;
            gbKontor.Text = "Kontor";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(354, 494);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(55, 15);
            lblTotal.TabIndex = 3;
            lblTotal.Text = "Kontor: 0";
            // 
            // dgvKontorList
            // 
            dgvKontorList.BackgroundColor = SystemColors.Control;
            dgvKontorList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKontorList.Location = new Point(0, 87);
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
            dtpDate.Location = new Point(230, 32);
            dtpDate.Name = "dtpDate";
            dtpDate.RightToLeftLayout = true;
            dtpDate.Size = new Size(224, 23);
            dtpDate.TabIndex = 1;
            dtpDate.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            // 
            // btnekle
            // 
            btnekle.Location = new Point(730, 102);
            btnekle.Name = "btnekle";
            btnekle.Size = new Size(55, 23);
            btnekle.TabIndex = 2;
            btnekle.Text = "+";
            btnekle.UseVisualStyleBackColor = true;
            btnekle.Click += btnekle_Click;
            // 
            // btnsil
            // 
            btnsil.Location = new Point(791, 131);
            btnsil.Name = "btnsil";
            btnsil.Size = new Size(55, 23);
            btnsil.TabIndex = 3;
            btnsil.Text = "Sil";
            btnsil.UseVisualStyleBackColor = true;
            btnsil.Click += btnsil_Click;
            // 
            // btnedit
            // 
            btnedit.Location = new Point(727, 131);
            btnedit.Name = "btnedit";
            btnedit.Size = new Size(58, 23);
            btnedit.TabIndex = 4;
            btnedit.Text = "Düzelt";
            btnedit.UseVisualStyleBackColor = true;
            btnedit.Visible = false;
            // 
            // btnhepsi1
            // 
            btnhepsi1.Location = new Point(727, 160);
            btnhepsi1.Name = "btnhepsi1";
            btnhepsi1.Size = new Size(119, 23);
            btnhepsi1.TabIndex = 5;
            btnhepsi1.Text = "Hepsinden 1 Düş";
            btnhepsi1.UseVisualStyleBackColor = true;
            btnhepsi1.Click += btnhepsi1_Click;
            // 
            // btncikar
            // 
            btncikar.Location = new Point(791, 102);
            btncikar.Name = "btncikar";
            btncikar.Size = new Size(55, 23);
            btncikar.TabIndex = 6;
            btncikar.Text = "-";
            btncikar.UseVisualStyleBackColor = true;
            btncikar.Click += btncikar_Click;
            // 
            // EDefterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(878, 566);
            Controls.Add(btncikar);
            Controls.Add(btnhepsi1);
            Controls.Add(btnedit);
            Controls.Add(btnsil);
            Controls.Add(btnekle);
            Controls.Add(panel1);
            Controls.Add(customerPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "EDefterForm";
            Text = "e-Defter Kontor Takip";
            customerPanel.ResumeLayout(false);
            customerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFirmaList).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            gbKontor.ResumeLayout(false);
            gbKontor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKontorList).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel customerPanel;
        private Label label4;
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
    }
}