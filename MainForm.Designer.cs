namespace HesapTakip
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splitContainer1 = new SplitContainer();
            customerPanel = new Panel();
            btn_showdeletedcustomers = new Button();
            label4 = new Label();
            btnEditCustomer = new Button();
            btnDeleteCustomer = new Button();
            btnAddCustomer = new Button();
            dgvCustomers = new DataGridView();
            gbTransactions = new GroupBox();
            button2 = new Button();
            btn_showdeletedtransactions = new Button();
            btnSaveToDb = new Button();
            btnImportExcel = new Button();
            btnResetSettings = new Button();
            btnEditTransaction = new Button();
            btnRemoveDescipt = new Button();
            btnAddDescipt = new Button();
            btnExportPdf = new Button();
            btnDeleteTransaction = new Button();
            btnExportExcel = new Button();
            tableLayoutPanel2 = new TableLayoutPanel();
            typeLabel = new Label();
            rbIncome = new RadioButton();
            rbExpense = new RadioButton();
            totalPanel = new Panel();
            lblTotal = new Label();
            btnAddTransaction = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            label3 = new Label();
            label2 = new Label();
            txtDescription = new TextBox();
            dtpDate = new DateTimePicker();
            txtAmount = new TextBox();
            label1 = new Label();
            dgvTransactions = new DataGridView();
            lstSuggestions = new ListBox();
            link_yusa = new LinkLabel();
            statusStrip1 = new StatusStrip();
            progressBar1 = new ToolStripProgressBar();
            statusLabel = new ToolStripStatusLabel();
            toolStripStatusLabelVersion = new ToolStripStatusLabel();
            StatustLabel_info = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            veriTabanıYeriniSıfırlaToolStripMenuItem = new ToolStripMenuItem();
            güncellemeKontrolEtToolStripMenuItem = new ToolStripMenuItem();
            veriTabanınıYedekleToolStripMenuItem = new ToolStripMenuItem();
            veriTabanıYeriSıfırlaToolStripMenuItem = new ToolStripMenuItem();
            modülToolStripMenuItem = new ToolStripMenuItem();
            eDefterKontorTakipToolStripMenuItem = new ToolStripMenuItem();
            eFaturaXMLExcelToolStripMenuItem = new ToolStripMenuItem();
            excelParçalaToolStripMenuItem = new ToolStripMenuItem();
            toolTip1 = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            customerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).BeginInit();
            gbTransactions.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            totalPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTransactions).BeginInit();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(customerPanel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(gbTransactions);
            splitContainer1.Size = new Size(866, 569);
            splitContainer1.SplitterDistance = 261;
            splitContainer1.TabIndex = 0;
            // 
            // customerPanel
            // 
            customerPanel.AutoSize = true;
            customerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            customerPanel.Controls.Add(btn_showdeletedcustomers);
            customerPanel.Controls.Add(label4);
            customerPanel.Controls.Add(btnEditCustomer);
            customerPanel.Controls.Add(btnDeleteCustomer);
            customerPanel.Controls.Add(btnAddCustomer);
            customerPanel.Controls.Add(dgvCustomers);
            customerPanel.Dock = DockStyle.Fill;
            customerPanel.Location = new Point(0, 0);
            customerPanel.Name = "customerPanel";
            customerPanel.Size = new Size(261, 569);
            customerPanel.TabIndex = 0;
            // 
            // btn_showdeletedcustomers
            // 
            btn_showdeletedcustomers.Location = new Point(134, 518);
            btn_showdeletedcustomers.Name = "btn_showdeletedcustomers";
            btn_showdeletedcustomers.Size = new Size(116, 23);
            btn_showdeletedcustomers.TabIndex = 5;
            btn_showdeletedcustomers.Text = "Silinenleri Göster";
            btn_showdeletedcustomers.UseVisualStyleBackColor = true;
            btn_showdeletedcustomers.Click += btn_showdeletedcustomers_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(90, 1);
            label4.Name = "label4";
            label4.Size = new Size(82, 15);
            label4.TabIndex = 4;
            label4.Text = "Müşteri Listesi";
            // 
            // btnEditCustomer
            // 
            btnEditCustomer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEditCustomer.Location = new Point(134, 489);
            btnEditCustomer.Name = "btnEditCustomer";
            btnEditCustomer.Size = new Size(116, 23);
            btnEditCustomer.TabIndex = 3;
            btnEditCustomer.Text = "Müşteri Düzenle";
            btnEditCustomer.UseVisualStyleBackColor = true;
            btnEditCustomer.Click += btnEditCustomer_Click;
            // 
            // btnDeleteCustomer
            // 
            btnDeleteCustomer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteCustomer.Location = new Point(12, 518);
            btnDeleteCustomer.Name = "btnDeleteCustomer";
            btnDeleteCustomer.Size = new Size(113, 23);
            btnDeleteCustomer.TabIndex = 2;
            btnDeleteCustomer.Text = "Müşteri Sil";
            btnDeleteCustomer.UseVisualStyleBackColor = true;
            btnDeleteCustomer.Click += btnDeleteCustomer_Click;
            // 
            // btnAddCustomer
            // 
            btnAddCustomer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddCustomer.Location = new Point(12, 489);
            btnAddCustomer.Name = "btnAddCustomer";
            btnAddCustomer.Size = new Size(116, 23);
            btnAddCustomer.TabIndex = 1;
            btnAddCustomer.Text = "Müşteri Ekle";
            btnAddCustomer.UseVisualStyleBackColor = true;
            btnAddCustomer.Click += btnAddCustomer_Click;
            // 
            // dgvCustomers
            // 
            dgvCustomers.AllowUserToAddRows = false;
            dgvCustomers.AllowUserToDeleteRows = false;
            dgvCustomers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCustomers.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvCustomers.BackgroundColor = SystemColors.Control;
            dgvCustomers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCustomers.ColumnHeadersVisible = false;
            dgvCustomers.Location = new Point(0, 19);
            dgvCustomers.Name = "dgvCustomers";
            dgvCustomers.ReadOnly = true;
            dgvCustomers.RowHeadersVisible = false;
            dgvCustomers.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgvCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCustomers.Size = new Size(261, 464);
            dgvCustomers.TabIndex = 0;
            dgvCustomers.SelectionChanged += dgvCustomers_SelectionChanged;
            // 
            // gbTransactions
            // 
            gbTransactions.AutoSize = true;
            gbTransactions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            gbTransactions.Controls.Add(button2);
            gbTransactions.Controls.Add(btn_showdeletedtransactions);
            gbTransactions.Controls.Add(btnSaveToDb);
            gbTransactions.Controls.Add(btnImportExcel);
            gbTransactions.Controls.Add(btnResetSettings);
            gbTransactions.Controls.Add(btnEditTransaction);
            gbTransactions.Controls.Add(btnRemoveDescipt);
            gbTransactions.Controls.Add(btnAddDescipt);
            gbTransactions.Controls.Add(btnExportPdf);
            gbTransactions.Controls.Add(btnDeleteTransaction);
            gbTransactions.Controls.Add(btnExportExcel);
            gbTransactions.Controls.Add(tableLayoutPanel2);
            gbTransactions.Controls.Add(totalPanel);
            gbTransactions.Controls.Add(btnAddTransaction);
            gbTransactions.Controls.Add(tableLayoutPanel1);
            gbTransactions.Controls.Add(dgvTransactions);
            gbTransactions.Controls.Add(lstSuggestions);
            gbTransactions.Dock = DockStyle.Fill;
            gbTransactions.Location = new Point(0, 0);
            gbTransactions.Name = "gbTransactions";
            gbTransactions.Size = new Size(601, 569);
            gbTransactions.TabIndex = 0;
            gbTransactions.TabStop = false;
            gbTransactions.Text = "Hesap Hareketleri";
            // 
            // button2
            // 
            button2.Location = new Point(466, 247);
            button2.Name = "button2";
            button2.Size = new Size(120, 23);
            button2.TabIndex = 21;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            button2.Visible = false;
            // 
            // btn_showdeletedtransactions
            // 
            btn_showdeletedtransactions.Location = new Point(466, 218);
            btn_showdeletedtransactions.Name = "btn_showdeletedtransactions";
            btn_showdeletedtransactions.Size = new Size(120, 23);
            btn_showdeletedtransactions.TabIndex = 20;
            btn_showdeletedtransactions.Text = "Silinenleri Göster";
            btn_showdeletedtransactions.UseVisualStyleBackColor = true;
            btn_showdeletedtransactions.Visible = false;
            btn_showdeletedtransactions.Click += btn_showdeletedtransactions_Click;
            // 
            // btnSaveToDb
            // 
            btnSaveToDb.Location = new Point(463, 489);
            btnSaveToDb.Name = "btnSaveToDb";
            btnSaveToDb.Size = new Size(120, 23);
            btnSaveToDb.TabIndex = 16;
            btnSaveToDb.Text = "Excel'den Kaydet";
            btnSaveToDb.UseVisualStyleBackColor = true;
            btnSaveToDb.Click += btnSaveToDb_Click;
            // 
            // btnImportExcel
            // 
            btnImportExcel.Location = new Point(463, 460);
            btnImportExcel.Name = "btnImportExcel";
            btnImportExcel.Size = new Size(120, 23);
            btnImportExcel.TabIndex = 14;
            btnImportExcel.Text = "Excel'den Yükle";
            btnImportExcel.UseVisualStyleBackColor = true;
            btnImportExcel.Click += btnImport_Click;
            // 
            // btnResetSettings
            // 
            btnResetSettings.Location = new Point(475, 489);
            btnResetSettings.Name = "btnResetSettings";
            btnResetSettings.Size = new Size(85, 23);
            btnResetSettings.TabIndex = 13;
            btnResetSettings.Text = "DB Yeri Sıfırla";
            btnResetSettings.UseVisualStyleBackColor = true;
            btnResetSettings.Visible = false;
            btnResetSettings.Click += btnResetSettings_Click;
            // 
            // btnEditTransaction
            // 
            btnEditTransaction.Location = new Point(466, 189);
            btnEditTransaction.Name = "btnEditTransaction";
            btnEditTransaction.Size = new Size(120, 23);
            btnEditTransaction.TabIndex = 12;
            btnEditTransaction.Text = "Hareket Düzenle";
            btnEditTransaction.UseVisualStyleBackColor = true;
            btnEditTransaction.Click += btnEditTransaction_Click;
            // 
            // btnRemoveDescipt
            // 
            btnRemoveDescipt.Location = new Point(530, 48);
            btnRemoveDescipt.Name = "btnRemoveDescipt";
            btnRemoveDescipt.Size = new Size(53, 23);
            btnRemoveDescipt.TabIndex = 10;
            btnRemoveDescipt.Text = "Çıkar";
            btnRemoveDescipt.UseVisualStyleBackColor = true;
            btnRemoveDescipt.Click += btnRemoveDescript_Click;
            // 
            // btnAddDescipt
            // 
            btnAddDescipt.Location = new Point(473, 48);
            btnAddDescipt.Name = "btnAddDescipt";
            btnAddDescipt.Size = new Size(53, 23);
            btnAddDescipt.TabIndex = 9;
            btnAddDescipt.Text = "Ekle";
            btnAddDescipt.UseVisualStyleBackColor = true;
            btnAddDescipt.Click += btnAddDescript_Click;
            // 
            // btnExportPdf
            // 
            btnExportPdf.Location = new Point(466, 276);
            btnExportPdf.Name = "btnExportPdf";
            btnExportPdf.Size = new Size(120, 23);
            btnExportPdf.TabIndex = 8;
            btnExportPdf.Text = "PDF Aktar";
            btnExportPdf.UseVisualStyleBackColor = true;
            btnExportPdf.Click += btnExportPdf_Click;
            // 
            // btnDeleteTransaction
            // 
            btnDeleteTransaction.Location = new Point(466, 160);
            btnDeleteTransaction.Name = "btnDeleteTransaction";
            btnDeleteTransaction.Size = new Size(120, 23);
            btnDeleteTransaction.TabIndex = 6;
            btnDeleteTransaction.Text = "Hareket Sil";
            toolTip1.SetToolTip(btnDeleteTransaction, "Kaydı siler fakat veritabanında korur\r\n");
            btnDeleteTransaction.UseVisualStyleBackColor = true;
            btnDeleteTransaction.Click += btnDeleteTransaction_Click;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(466, 305);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(120, 23);
            btnExportExcel.TabIndex = 5;
            btnExportExcel.Text = "Excel'e Aktar";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.BackColor = SystemColors.Control;
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.Controls.Add(typeLabel, 0, 0);
            tableLayoutPanel2.Controls.Add(rbIncome, 1, 0);
            tableLayoutPanel2.Controls.Add(rbExpense, 2, 0);
            tableLayoutPanel2.Location = new Point(3, 101);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(454, 29);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // typeLabel
            // 
            typeLabel.AutoSize = true;
            typeLabel.Dock = DockStyle.Fill;
            typeLabel.Location = new Point(3, 0);
            typeLabel.Name = "typeLabel";
            typeLabel.Size = new Size(221, 29);
            typeLabel.TabIndex = 5;
            typeLabel.Text = "Tür";
            typeLabel.TextAlign = ContentAlignment.MiddleCenter;
            typeLabel.Visible = false;
            // 
            // rbIncome
            // 
            rbIncome.AutoSize = true;
            rbIncome.BackColor = SystemColors.Control;
            rbIncome.BackgroundImageLayout = ImageLayout.None;
            rbIncome.Checked = true;
            rbIncome.Dock = DockStyle.Fill;
            rbIncome.FlatAppearance.BorderColor = SystemColors.Control;
            rbIncome.FlatAppearance.CheckedBackColor = SystemColors.Control;
            rbIncome.FlatAppearance.MouseDownBackColor = SystemColors.Control;
            rbIncome.FlatAppearance.MouseOverBackColor = SystemColors.Control;
            rbIncome.Location = new Point(230, 3);
            rbIncome.Name = "rbIncome";
            rbIncome.Size = new Size(107, 23);
            rbIncome.TabIndex = 6;
            rbIncome.TabStop = true;
            rbIncome.Text = "Tahsilat / Gelir";
            rbIncome.TextAlign = ContentAlignment.MiddleCenter;
            rbIncome.UseVisualStyleBackColor = false;
            rbIncome.Visible = false;
            rbIncome.CheckedChanged += rbIncome_CheckedChanged;
            // 
            // rbExpense
            // 
            rbExpense.AutoSize = true;
            rbExpense.BackColor = SystemColors.Control;
            rbExpense.Dock = DockStyle.Fill;
            rbExpense.FlatAppearance.CheckedBackColor = SystemColors.ActiveCaption;
            rbExpense.Location = new Point(343, 3);
            rbExpense.Name = "rbExpense";
            rbExpense.Size = new Size(108, 23);
            rbExpense.TabIndex = 7;
            rbExpense.Text = "Fatura / Gider";
            rbExpense.TextAlign = ContentAlignment.MiddleCenter;
            rbExpense.UseVisualStyleBackColor = false;
            rbExpense.Visible = false;
            rbExpense.CheckedChanged += rbExpense_CheckedChanged;
            // 
            // totalPanel
            // 
            totalPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            totalPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            totalPanel.Controls.Add(lblTotal);
            totalPanel.Location = new Point(258, 509);
            totalPanel.Name = "totalPanel";
            totalPanel.Size = new Size(168, 25);
            totalPanel.TabIndex = 3;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(3, 2);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(119, 15);
            lblTotal.TabIndex = 0;
            lblTotal.Text = "Toplam Bakiye: 0.00 ₺";
            // 
            // btnAddTransaction
            // 
            btnAddTransaction.Location = new Point(466, 131);
            btnAddTransaction.Name = "btnAddTransaction";
            btnAddTransaction.Size = new Size(120, 23);
            btnAddTransaction.TabIndex = 4;
            btnAddTransaction.Text = "Hareket Ekle";
            toolTip1.SetToolTip(btnAddTransaction, "Klavyeden \"+\" Borç/Gelir\r\nveya \"-\" Alacak / Gider\r\nKaydı yapar\r\n");
            btnAddTransaction.UseVisualStyleBackColor = true;
            btnAddTransaction.Click += btnAddTransaction_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.BackgroundImageLayout = ImageLayout.None;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(txtDescription, 1, 1);
            tableLayoutPanel1.Controls.Add(dtpDate, 1, 0);
            tableLayoutPanel1.Controls.Add(txtAmount, 1, 2);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.Location = new Point(3, 19);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(454, 79);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(3, 52);
            label3.Name = "label3";
            label3.Size = new Size(221, 27);
            label3.TabIndex = 3;
            label3.Text = "Tutar";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 26);
            label2.Name = "label2";
            label2.Size = new Size(221, 26);
            label2.TabIndex = 2;
            label2.Text = "Açıklama";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtDescription
            // 
            txtDescription.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtDescription.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtDescription.Dock = DockStyle.Fill;
            txtDescription.Location = new Point(230, 29);
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "FaturaNo veya Tahsilat Türü Ekle-Çıkar";
            txtDescription.Size = new Size(221, 23);
            txtDescription.TabIndex = 2;
            toolTip1.SetToolTip(txtDescription, "Otomatik Doldurma\r\nEkle ile listeye kayıt yapabilir\r\nÇıkar ile listeden çıkartabilirsiniz.");
            // 
            // dtpDate
            // 
            dtpDate.AccessibleRole = AccessibleRole.TitleBar;
            dtpDate.CalendarForeColor = Color.LavenderBlush;
            dtpDate.CalendarMonthBackground = SystemColors.WindowFrame;
            dtpDate.CustomFormat = "dd.MM.yyyy";
            dtpDate.Dock = DockStyle.Fill;
            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.Location = new Point(230, 3);
            dtpDate.Name = "dtpDate";
            dtpDate.RightToLeftLayout = true;
            dtpDate.Size = new Size(221, 23);
            dtpDate.TabIndex = 1;
            dtpDate.Value = new DateTime(2025, 1, 1, 0, 0, 0, 0);
            // 
            // txtAmount
            // 
            txtAmount.Dock = DockStyle.Fill;
            txtAmount.Location = new Point(230, 55);
            txtAmount.Name = "txtAmount";
            txtAmount.Size = new Size(221, 23);
            txtAmount.TabIndex = 3;
            toolTip1.SetToolTip(txtAmount, "Klavyeden \"+\" Borç/Gelir\r\nveya \"-\" Alacak / Gider\r\nKaydı yapar");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(221, 26);
            label1.TabIndex = 1;
            label1.Text = "Tarih";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dgvTransactions
            // 
            dgvTransactions.AllowUserToAddRows = false;
            dgvTransactions.AllowUserToDeleteRows = false;
            dgvTransactions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvTransactions.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvTransactions.BackgroundColor = SystemColors.Control;
            dgvTransactions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTransactions.Location = new Point(6, 131);
            dgvTransactions.Name = "dgvTransactions";
            dgvTransactions.Size = new Size(454, 378);
            dgvTransactions.TabIndex = 0;
            // 
            // lstSuggestions
            // 
            lstSuggestions.FormattingEnabled = true;
            lstSuggestions.Location = new Point(466, 397);
            lstSuggestions.Name = "lstSuggestions";
            lstSuggestions.Size = new Size(120, 94);
            lstSuggestions.TabIndex = 11;
            lstSuggestions.Visible = false;
            // 
            // link_yusa
            // 
            link_yusa.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            link_yusa.AutoSize = true;
            link_yusa.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            link_yusa.Location = new Point(720, 565);
            link_yusa.Name = "link_yusa";
            link_yusa.Size = new Size(145, 15);
            link_yusa.TabIndex = 7;
            link_yusa.TabStop = true;
            link_yusa.Text = "SMMM YUŞA TOPALOĞLU";
            link_yusa.LinkClicked += link_yusa_LinkClicked;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { progressBar1, statusLabel, toolStripStatusLabelVersion, StatustLabel_info });
            statusStrip1.Location = new Point(0, 571);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(866, 22);
            statusStrip1.TabIndex = 19;
            statusStrip1.Text = "statusStrip1";
            // 
            // progressBar1
            // 
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(100, 16);
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(0, 17);
            // 
            // toolStripStatusLabelVersion
            // 
            toolStripStatusLabelVersion.Name = "toolStripStatusLabelVersion";
            toolStripStatusLabelVersion.Size = new Size(0, 17);
            // 
            // StatustLabel_info
            // 
            StatustLabel_info.Name = "StatustLabel_info";
            StatustLabel_info.Size = new Size(0, 17);
            // 
            // menuStrip1
            // 
            menuStrip1.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            menuStrip1.Items.AddRange(new ToolStripItem[] { veriTabanıYeriniSıfırlaToolStripMenuItem, modülToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(866, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // veriTabanıYeriniSıfırlaToolStripMenuItem
            // 
            veriTabanıYeriniSıfırlaToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { güncellemeKontrolEtToolStripMenuItem, veriTabanınıYedekleToolStripMenuItem, veriTabanıYeriSıfırlaToolStripMenuItem });
            veriTabanıYeriniSıfırlaToolStripMenuItem.Name = "veriTabanıYeriniSıfırlaToolStripMenuItem";
            veriTabanıYeriniSıfırlaToolStripMenuItem.Size = new Size(50, 20);
            veriTabanıYeriniSıfırlaToolStripMenuItem.Text = "Menü";
            // 
            // güncellemeKontrolEtToolStripMenuItem
            // 
            güncellemeKontrolEtToolStripMenuItem.Name = "güncellemeKontrolEtToolStripMenuItem";
            güncellemeKontrolEtToolStripMenuItem.Size = new Size(192, 22);
            güncellemeKontrolEtToolStripMenuItem.Text = "Güncelleme Kontrol Et";
            güncellemeKontrolEtToolStripMenuItem.Click += CheckUpdateButton_Click;
            // 
            // veriTabanınıYedekleToolStripMenuItem
            // 
            veriTabanınıYedekleToolStripMenuItem.Name = "veriTabanınıYedekleToolStripMenuItem";
            veriTabanınıYedekleToolStripMenuItem.Size = new Size(192, 22);
            veriTabanınıYedekleToolStripMenuItem.Text = "Veri Tabanını Yedekle";
            veriTabanınıYedekleToolStripMenuItem.Click += veritabaniniYedekleToolStripMenuItem_Click;
            // 
            // veriTabanıYeriSıfırlaToolStripMenuItem
            // 
            veriTabanıYeriSıfırlaToolStripMenuItem.Name = "veriTabanıYeriSıfırlaToolStripMenuItem";
            veriTabanıYeriSıfırlaToolStripMenuItem.Size = new Size(192, 22);
            veriTabanıYeriSıfırlaToolStripMenuItem.Text = "Veri Tabanı Yeri Sıfırla";
            veriTabanıYeriSıfırlaToolStripMenuItem.Click += btnResetSettings_Click;
            // 
            // modülToolStripMenuItem
            // 
            modülToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { eDefterKontorTakipToolStripMenuItem, eFaturaXMLExcelToolStripMenuItem, excelParçalaToolStripMenuItem });
            modülToolStripMenuItem.Name = "modülToolStripMenuItem";
            modülToolStripMenuItem.Size = new Size(54, 20);
            modülToolStripMenuItem.Text = "Modül";
            // 
            // eDefterKontorTakipToolStripMenuItem
            // 
            eDefterKontorTakipToolStripMenuItem.Name = "eDefterKontorTakipToolStripMenuItem";
            eDefterKontorTakipToolStripMenuItem.Size = new Size(183, 22);
            eDefterKontorTakipToolStripMenuItem.Text = "eDefter Kontor Takip";
            eDefterKontorTakipToolStripMenuItem.Click += eDefterToolStripMenuItem_Click;
            // 
            // eFaturaXMLExcelToolStripMenuItem
            // 
            eFaturaXMLExcelToolStripMenuItem.Name = "eFaturaXMLExcelToolStripMenuItem";
            eFaturaXMLExcelToolStripMenuItem.Size = new Size(183, 22);
            eFaturaXMLExcelToolStripMenuItem.Text = "e-Fatura XML - Excel";
            eFaturaXMLExcelToolStripMenuItem.Click += eFaturaXMLExcelToolStripMenuItem_Click;
            // 
            // excelParçalaToolStripMenuItem
            // 
            excelParçalaToolStripMenuItem.Name = "excelParçalaToolStripMenuItem";
            excelParçalaToolStripMenuItem.Size = new Size(183, 22);
            excelParçalaToolStripMenuItem.Text = "Excel Parçala";
            excelParçalaToolStripMenuItem.Click += excelParçalaToolStripMenuItem_Click;
            // 
            // toolTip1
            // 
            toolTip1.AutomaticDelay = 100;
            toolTip1.AutoPopDelay = 1000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 100;
            toolTip1.ShowAlways = true;
            toolTip1.Popup += toolTip1_Popup;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(866, 593);
            Controls.Add(link_yusa);
            Controls.Add(statusStrip1);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Yuşa' Hesap Takip";
            Load += MainForm_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            customerPanel.ResumeLayout(false);
            customerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).EndInit();
            gbTransactions.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            totalPanel.ResumeLayout(false);
            totalPanel.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTransactions).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private Panel customerPanel;
        private DataGridView dgvCustomers;
        private Button btnDeleteCustomer;
        private Button btnAddCustomer;
        private GroupBox gbTransactions;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView dgvTransactions;
        private Label label3;
        private Label label1;
        private Label label2;
        private TextBox txtDescription;
        private Button btnAddTransaction;
        private Panel totalPanel;
        private Label lblTotal;
        private TextBox txtAmount;
        private TableLayoutPanel tableLayoutPanel2;
        private Label typeLabel;
        private RadioButton rbIncome;
        private RadioButton rbExpense;
        private Button btnExportExcel;
        private Button btnDeleteTransaction;
        public DateTimePicker dtpDate;
        private LinkLabel link_yusa;
        private Button btnExportPdf;
        private Button btnRemoveDescipt;
        private Button btnAddDescipt;
        private ListBox lstSuggestions;
        private Button btnEditCustomer;
        private Button btnEditTransaction;
        private Button btnResetSettings;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem veriTabanıYeriniSıfırlaToolStripMenuItem;
        private ToolStripMenuItem veriTabanıYeriSıfırlaToolStripMenuItem;
        private Label label4;
        private Button btnImportExcel;
        private Button btnSaveToDb;
        private ToolStripMenuItem modülToolStripMenuItem;
        private ToolStripMenuItem eDefterKontorTakipToolStripMenuItem;
        private ToolStripMenuItem güncellemeKontrolEtToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar progressBar1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel toolStripStatusLabelVersion;
        private ToolStripMenuItem eFaturaXMLExcelToolStripMenuItem;
        private Button button2;
        private Button btn_showdeletedtransactions;
        private ToolStripMenuItem veriTabanınıYedekleToolStripMenuItem;
        private ToolTip toolTip1;
        private ToolStripMenuItem excelParçalaToolStripMenuItem;
        private Button btn_showdeletedcustomers;
        private ToolStripStatusLabel StatustLabel_info;
    }
}