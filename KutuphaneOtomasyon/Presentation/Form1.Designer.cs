namespace KutuphaneOtomasyon;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        pnlUst = new Panel();
        cmbAramaFiltre = new ComboBox();
        txtAra = new TextBox();
        btnAra = new Button();
        btnYenile = new Button();
        lblDurum = new Label();
        lblBaslik = new Label();
        splitContainer = new SplitContainer();
        pnlForm = new Panel();
        txtOzet = new TextBox();
        lblOzet = new Label();
        txtKitapBedeli = new TextBox();
        lblKitapBedeli = new Label();
        txtYazarlar = new TextBox();
        txtYayinEvi = new TextBox();
        clbTurler = new CheckedListBox();
        lblTurler = new Label();
        clbYazarlar = new CheckedListBox();
        lblYazarlar = new Label();
        cmbYayinEvi = new ComboBox();
        lblYayinEvi = new Label();
        txtDil = new TextBox();
        lblDil = new Label();
        txtIlkYayinYili = new TextBox();
        lblIlkYayinYili = new Label();
        txtOrijinalAd = new TextBox();
        lblOrijinalAd = new Label();
        txtKitapAdi = new TextBox();
        lblKitapAdi = new Label();
        pnlButtons = new FlowLayoutPanel();
        btnEkle = new Button();
        btnGuncelle = new Button();
        btnSil = new Button();
        btnYeni = new Button();
        dgvKitaplar = new DataGridView();
        pnlUst.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        pnlForm.SuspendLayout();
        pnlButtons.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvKitaplar).BeginInit();
        SuspendLayout();
        // 
        // pnlUst
        // 
        pnlUst.BackColor = Color.FromArgb(245, 247, 250);
        pnlUst.Controls.Add(cmbAramaFiltre);
        pnlUst.Controls.Add(txtAra);
        pnlUst.Controls.Add(btnAra);
        pnlUst.Controls.Add(btnYenile);
        pnlUst.Controls.Add(lblDurum);
        pnlUst.Controls.Add(lblBaslik);
        pnlUst.Dock = DockStyle.Top;
        pnlUst.Location = new Point(0, 0);
        pnlUst.Name = "pnlUst";
        pnlUst.Padding = new Padding(16, 12, 16, 12);
        pnlUst.Size = new Size(1184, 82);
        pnlUst.TabIndex = 0;
        // 
        // 
        // cmbAramaFiltre
        // 
        cmbAramaFiltre.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        cmbAramaFiltre.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbAramaFiltre.Font = new Font("Segoe UI", 9F);
        cmbAramaFiltre.Location = new Point(565, 28);
        cmbAramaFiltre.Name = "cmbAramaFiltre";
        cmbAramaFiltre.Size = new Size(126, 23);
        cmbAramaFiltre.TabIndex = 5;
        // 
        // txtAra
        // 
        txtAra.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtAra.Font = new Font("Segoe UI", 10F);
        txtAra.Location = new Point(697, 28);
        txtAra.Name = "txtAra";
        txtAra.PlaceholderText = "Kitap, dil veya yayinevi ara";
        txtAra.Size = new Size(241, 25);
        txtAra.TabIndex = 2;
        txtAra.KeyDown += txtAra_KeyDown;
        // 
        // btnAra
        // 
        btnAra.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAra.BackColor = Color.FromArgb(15, 118, 110);
        btnAra.FlatAppearance.BorderSize = 0;
        btnAra.FlatStyle = FlatStyle.Flat;
        btnAra.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnAra.ForeColor = Color.White;
        btnAra.Location = new Point(946, 27);
        btnAra.Name = "btnAra";
        btnAra.Size = new Size(86, 28);
        btnAra.TabIndex = 3;
        btnAra.Text = "Ara";
        btnAra.UseVisualStyleBackColor = false;
        btnAra.Click += btnAra_Click;
        // 
        // btnYenile
        // 
        btnYenile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnYenile.BackColor = Color.FromArgb(37, 99, 235);
        btnYenile.FlatAppearance.BorderSize = 0;
        btnYenile.FlatStyle = FlatStyle.Flat;
        btnYenile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnYenile.ForeColor = Color.White;
        btnYenile.Location = new Point(1040, 27);
        btnYenile.Name = "btnYenile";
        btnYenile.Size = new Size(128, 28);
        btnYenile.TabIndex = 4;
        btnYenile.Text = "Listeyi Yenile";
        btnYenile.UseVisualStyleBackColor = false;
        btnYenile.Click += btnYenile_Click;
        // 
        // lblDurum
        // 
        lblDurum.AutoSize = true;
        lblDurum.Font = new Font("Segoe UI", 9.5F);
        lblDurum.ForeColor = Color.FromArgb(71, 85, 105);
        lblDurum.Location = new Point(19, 48);
        lblDurum.Name = "lblDurum";
        lblDurum.Size = new Size(167, 17);
        lblDurum.TabIndex = 1;
        lblDurum.Text = "Veritabani baglantisi hazir";
        // 
        // lblBaslik
        // 
        lblBaslik.AutoSize = true;
        lblBaslik.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblBaslik.ForeColor = Color.FromArgb(30, 41, 59);
        lblBaslik.Location = new Point(16, 14);
        lblBaslik.Name = "lblBaslik";
        lblBaslik.Size = new Size(170, 30);
        lblBaslik.TabIndex = 0;
        lblBaslik.Text = "Kitap Yonetimi";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 82);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(pnlForm);
        splitContainer.Panel1MinSize = 340;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(dgvKitaplar);
        splitContainer.Panel2MinSize = 500;
        splitContainer.Size = new Size(1184, 599);
        splitContainer.SplitterDistance = 372;
        splitContainer.TabIndex = 1;
        // 
        // pnlForm
        // 
        pnlForm.AutoScroll = true;
        pnlForm.BackColor = Color.White;
        pnlForm.Controls.Add(txtOzet);
        pnlForm.Controls.Add(lblOzet);
        pnlForm.Controls.Add(txtKitapBedeli);
        pnlForm.Controls.Add(lblKitapBedeli);
        pnlForm.Controls.Add(txtYazarlar);
        pnlForm.Controls.Add(txtYayinEvi);
        pnlForm.Controls.Add(clbTurler);
        pnlForm.Controls.Add(lblTurler);
        pnlForm.Controls.Add(clbYazarlar);
        pnlForm.Controls.Add(lblYazarlar);
        pnlForm.Controls.Add(cmbYayinEvi);
        pnlForm.Controls.Add(lblYayinEvi);
        pnlForm.Controls.Add(txtDil);
        pnlForm.Controls.Add(lblDil);
        pnlForm.Controls.Add(txtIlkYayinYili);
        pnlForm.Controls.Add(lblIlkYayinYili);
        pnlForm.Controls.Add(txtOrijinalAd);
        pnlForm.Controls.Add(lblOrijinalAd);
        pnlForm.Controls.Add(txtKitapAdi);
        pnlForm.Controls.Add(lblKitapAdi);
        pnlForm.Controls.Add(pnlButtons);
        pnlForm.Dock = DockStyle.Fill;
        pnlForm.Location = new Point(0, 0);
        pnlForm.Name = "pnlForm";
        pnlForm.Padding = new Padding(18);
        pnlForm.Size = new Size(372, 599);
        pnlForm.TabIndex = 0;
        // 
        // txtOzet
        // 
        txtOzet.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtOzet.Font = new Font("Segoe UI", 9.5F);
        txtOzet.Location = new Point(21, 497);
        txtOzet.Multiline = true;
        txtOzet.Name = "txtOzet";
        txtOzet.ScrollBars = ScrollBars.Vertical;
        txtOzet.Size = new Size(330, 86);
        txtOzet.TabIndex = 15;
        // 
        // lblOzet
        // 
        lblOzet.AutoSize = true;
        lblOzet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblOzet.ForeColor = Color.FromArgb(51, 65, 85);
        lblOzet.Location = new Point(21, 478);
        lblOzet.Name = "lblOzet";
        lblOzet.Size = new Size(32, 15);
        lblOzet.TabIndex = 14;
        lblOzet.Text = "Ozet";
        // 
        // txtKitapBedeli
        // 
        txtKitapBedeli.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtKitapBedeli.Font = new Font("Segoe UI", 9.5F);
        txtKitapBedeli.Location = new Point(21, 450);
        txtKitapBedeli.Name = "txtKitapBedeli";
        txtKitapBedeli.Size = new Size(330, 24);
        txtKitapBedeli.TabIndex = 19;
        txtKitapBedeli.Text = "250";
        // 
        // lblKitapBedeli
        // 
        lblKitapBedeli.AutoSize = true;
        lblKitapBedeli.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblKitapBedeli.ForeColor = Color.FromArgb(51, 65, 85);
        lblKitapBedeli.Location = new Point(21, 431);
        lblKitapBedeli.Name = "lblKitapBedeli";
        lblKitapBedeli.Size = new Size(69, 15);
        lblKitapBedeli.TabIndex = 20;
        lblKitapBedeli.Text = "Kitap bedeli";
        // 
        // clbTurler
        // 
        clbTurler.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        clbTurler.CheckOnClick = true;
        clbTurler.Font = new Font("Segoe UI", 9F);
        clbTurler.FormattingEnabled = true;
        clbTurler.Location = new Point(21, 333);
        clbTurler.Name = "clbTurler";
        clbTurler.Size = new Size(330, 94);
        clbTurler.TabIndex = 13;
        // 
        // lblTurler
        // 
        lblTurler.AutoSize = true;
        lblTurler.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblTurler.ForeColor = Color.FromArgb(51, 65, 85);
        lblTurler.Location = new Point(21, 314);
        lblTurler.Name = "lblTurler";
        lblTurler.Size = new Size(41, 15);
        lblTurler.TabIndex = 12;
        lblTurler.Text = "Turler";
        // 
        // clbYazarlar
        // 
        clbYazarlar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        clbYazarlar.CheckOnClick = true;
        clbYazarlar.Font = new Font("Segoe UI", 9F);
        clbYazarlar.FormattingEnabled = true;
        clbYazarlar.Location = new Point(21, 217);
        clbYazarlar.Name = "clbYazarlar";
        clbYazarlar.Size = new Size(330, 94);
        clbYazarlar.TabIndex = 11;
        clbYazarlar.Visible = false;
        // 
        // lblYazarlar
        // 
        lblYazarlar.AutoSize = true;
        lblYazarlar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblYazarlar.ForeColor = Color.FromArgb(51, 65, 85);
        lblYazarlar.Location = new Point(21, 198);
        lblYazarlar.Name = "lblYazarlar";
        lblYazarlar.Size = new Size(50, 15);
        lblYazarlar.TabIndex = 10;
        lblYazarlar.Text = "Yazarlar";
        // 
        // cmbYayinEvi
        // 
        cmbYayinEvi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        cmbYayinEvi.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbYayinEvi.Font = new Font("Segoe UI", 9.5F);
        cmbYayinEvi.FormattingEnabled = true;
        cmbYayinEvi.Location = new Point(21, 170);
        cmbYayinEvi.Name = "cmbYayinEvi";
        cmbYayinEvi.Size = new Size(330, 25);
        cmbYayinEvi.TabIndex = 9;
        cmbYayinEvi.Visible = false;
        // 
        // txtYazarlar
        // 
        txtYazarlar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtYazarlar.Font = new Font("Segoe UI", 9.5F);
        txtYazarlar.Location = new Point(21, 217);
        txtYazarlar.Name = "txtYazarlar";
        txtYazarlar.PlaceholderText = "Virgul ile ayir: Yazar 1, Yazar 2";
        txtYazarlar.Size = new Size(330, 24);
        txtYazarlar.TabIndex = 17;
        // 
        // txtYayinEvi
        // 
        txtYayinEvi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtYayinEvi.Font = new Font("Segoe UI", 9.5F);
        txtYayinEvi.Location = new Point(21, 170);
        txtYayinEvi.Name = "txtYayinEvi";
        txtYayinEvi.Size = new Size(330, 24);
        txtYayinEvi.TabIndex = 18;
        // 
        // lblYayinEvi
        // 
        lblYayinEvi.AutoSize = true;
        lblYayinEvi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblYayinEvi.ForeColor = Color.FromArgb(51, 65, 85);
        lblYayinEvi.Location = new Point(21, 151);
        lblYayinEvi.Name = "lblYayinEvi";
        lblYayinEvi.Size = new Size(55, 15);
        lblYayinEvi.TabIndex = 8;
        lblYayinEvi.Text = "Yayinevi";
        // 
        // txtDil
        // 
        txtDil.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDil.Font = new Font("Segoe UI", 9.5F);
        txtDil.Location = new Point(192, 123);
        txtDil.Name = "txtDil";
        txtDil.Size = new Size(159, 24);
        txtDil.TabIndex = 7;
        txtDil.Text = "Turkce";
        // 
        // lblDil
        // 
        lblDil.AutoSize = true;
        lblDil.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblDil.ForeColor = Color.FromArgb(51, 65, 85);
        lblDil.Location = new Point(192, 104);
        lblDil.Name = "lblDil";
        lblDil.Size = new Size(22, 15);
        lblDil.TabIndex = 6;
        lblDil.Text = "Dil";
        // 
        // txtIlkYayinYili
        // 
        txtIlkYayinYili.Font = new Font("Segoe UI", 9.5F);
        txtIlkYayinYili.Location = new Point(21, 123);
        txtIlkYayinYili.Name = "txtIlkYayinYili";
        txtIlkYayinYili.Size = new Size(154, 24);
        txtIlkYayinYili.TabIndex = 5;
        // 
        // lblIlkYayinYili
        // 
        lblIlkYayinYili.AutoSize = true;
        lblIlkYayinYili.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblIlkYayinYili.ForeColor = Color.FromArgb(51, 65, 85);
        lblIlkYayinYili.Location = new Point(21, 104);
        lblIlkYayinYili.Name = "lblIlkYayinYili";
        lblIlkYayinYili.Size = new Size(73, 15);
        lblIlkYayinYili.TabIndex = 4;
        lblIlkYayinYili.Text = "Ilk yayin yili";
        // 
        // txtOrijinalAd
        // 
        txtOrijinalAd.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtOrijinalAd.Font = new Font("Segoe UI", 9.5F);
        txtOrijinalAd.Location = new Point(21, 77);
        txtOrijinalAd.Name = "txtOrijinalAd";
        txtOrijinalAd.Size = new Size(330, 24);
        txtOrijinalAd.TabIndex = 3;
        // 
        // lblOrijinalAd
        // 
        lblOrijinalAd.AutoSize = true;
        lblOrijinalAd.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblOrijinalAd.ForeColor = Color.FromArgb(51, 65, 85);
        lblOrijinalAd.Location = new Point(21, 58);
        lblOrijinalAd.Name = "lblOrijinalAd";
        lblOrijinalAd.Size = new Size(63, 15);
        lblOrijinalAd.TabIndex = 2;
        lblOrijinalAd.Text = "Orijinal ad";
        // 
        // txtKitapAdi
        // 
        txtKitapAdi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtKitapAdi.Font = new Font("Segoe UI", 9.5F);
        txtKitapAdi.Location = new Point(21, 31);
        txtKitapAdi.Name = "txtKitapAdi";
        txtKitapAdi.Size = new Size(330, 24);
        txtKitapAdi.TabIndex = 1;
        // 
        // lblKitapAdi
        // 
        lblKitapAdi.AutoSize = true;
        lblKitapAdi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblKitapAdi.ForeColor = Color.FromArgb(51, 65, 85);
        lblKitapAdi.Location = new Point(21, 12);
        lblKitapAdi.Name = "lblKitapAdi";
        lblKitapAdi.Size = new Size(56, 15);
        lblKitapAdi.TabIndex = 0;
        lblKitapAdi.Text = "Kitap adi";
        // 
        // pnlButtons
        // 
        pnlButtons.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlButtons.Controls.Add(btnEkle);
        pnlButtons.Controls.Add(btnGuncelle);
        pnlButtons.Controls.Add(btnSil);
        pnlButtons.Controls.Add(btnYeni);
        pnlButtons.Location = new Point(21, 594);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Size = new Size(330, 76);
        pnlButtons.TabIndex = 16;
        // 
        // btnEkle
        // 
        btnEkle.BackColor = Color.FromArgb(22, 163, 74);
        btnEkle.FlatAppearance.BorderSize = 0;
        btnEkle.FlatStyle = FlatStyle.Flat;
        btnEkle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnEkle.ForeColor = Color.White;
        btnEkle.Location = new Point(3, 3);
        btnEkle.Name = "btnEkle";
        btnEkle.Size = new Size(98, 32);
        btnEkle.TabIndex = 0;
        btnEkle.Text = "Ekle";
        btnEkle.UseVisualStyleBackColor = false;
        btnEkle.Click += btnEkle_Click;
        // 
        // btnGuncelle
        // 
        btnGuncelle.BackColor = Color.FromArgb(37, 99, 235);
        btnGuncelle.FlatAppearance.BorderSize = 0;
        btnGuncelle.FlatStyle = FlatStyle.Flat;
        btnGuncelle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnGuncelle.ForeColor = Color.White;
        btnGuncelle.Location = new Point(107, 3);
        btnGuncelle.Name = "btnGuncelle";
        btnGuncelle.Size = new Size(110, 32);
        btnGuncelle.TabIndex = 1;
        btnGuncelle.Text = "Guncelle";
        btnGuncelle.UseVisualStyleBackColor = false;
        btnGuncelle.Click += btnGuncelle_Click;
        // 
        // btnSil
        // 
        btnSil.BackColor = Color.FromArgb(220, 38, 38);
        btnSil.FlatAppearance.BorderSize = 0;
        btnSil.FlatStyle = FlatStyle.Flat;
        btnSil.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSil.ForeColor = Color.White;
        btnSil.Location = new Point(223, 3);
        btnSil.Name = "btnSil";
        btnSil.Size = new Size(98, 32);
        btnSil.TabIndex = 2;
        btnSil.Text = "Sil";
        btnSil.UseVisualStyleBackColor = false;
        btnSil.Click += btnSil_Click;
        // 
        // btnYeni
        // 
        btnYeni.BackColor = Color.FromArgb(100, 116, 139);
        btnYeni.FlatAppearance.BorderSize = 0;
        btnYeni.FlatStyle = FlatStyle.Flat;
        btnYeni.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnYeni.ForeColor = Color.White;
        btnYeni.Location = new Point(3, 41);
        btnYeni.Name = "btnYeni";
        btnYeni.Size = new Size(318, 32);
        btnYeni.TabIndex = 3;
        btnYeni.Text = "Formu Temizle";
        btnYeni.UseVisualStyleBackColor = false;
        btnYeni.Click += btnYeni_Click;
        // 
        // dgvKitaplar
        // 
        dgvKitaplar.AllowUserToAddRows = false;
        dgvKitaplar.AllowUserToDeleteRows = false;
        dgvKitaplar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvKitaplar.BackgroundColor = Color.White;
        dgvKitaplar.BorderStyle = BorderStyle.None;
        dgvKitaplar.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvKitaplar.Dock = DockStyle.Fill;
        dgvKitaplar.Location = new Point(0, 0);
        dgvKitaplar.MultiSelect = false;
        dgvKitaplar.Name = "dgvKitaplar";
        dgvKitaplar.ReadOnly = true;
        dgvKitaplar.RowHeadersWidth = 51;
        dgvKitaplar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvKitaplar.Size = new Size(808, 599);
        dgvKitaplar.TabIndex = 0;
        dgvKitaplar.SelectionChanged += dgvKitaplar_SelectionChanged;
        // 
        // Form1
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 681);
        Controls.Add(splitContainer);
        Controls.Add(pnlUst);
        MinimumSize = new Size(1050, 720);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Kutuphane Otomasyon";
        Load += Form1_Load;
        pnlUst.ResumeLayout(false);
        pnlUst.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        pnlForm.ResumeLayout(false);
        pnlForm.PerformLayout();
        pnlButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvKitaplar).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlUst;
    private ComboBox cmbAramaFiltre;
    private TextBox txtAra;
    private Button btnAra;
    private Button btnYenile;
    private Label lblDurum;
    private Label lblBaslik;
    private SplitContainer splitContainer;
    private Panel pnlForm;
    private TextBox txtOzet;
    private Label lblOzet;
    private TextBox txtKitapBedeli;
    private Label lblKitapBedeli;
    private TextBox txtYazarlar;
    private TextBox txtYayinEvi;
    private CheckedListBox clbTurler;
    private Label lblTurler;
    private CheckedListBox clbYazarlar;
    private Label lblYazarlar;
    private ComboBox cmbYayinEvi;
    private Label lblYayinEvi;
    private TextBox txtDil;
    private Label lblDil;
    private TextBox txtIlkYayinYili;
    private Label lblIlkYayinYili;
    private TextBox txtOrijinalAd;
    private Label lblOrijinalAd;
    private TextBox txtKitapAdi;
    private Label lblKitapAdi;
    private FlowLayoutPanel pnlButtons;
    private Button btnEkle;
    private Button btnGuncelle;
    private Button btnSil;
    private Button btnYeni;
    private DataGridView dgvKitaplar;
}
