using System.Data;

namespace KutuphaneOtomasyon;

public partial class Form1 : Form
{
    private readonly AppUser? currentUser;
    private int? selectedKitapId;

    public Form1()
    {
        InitializeComponent();
    }

    public Form1(AppUser currentUser)
        : this()
    {
        this.currentUser = currentUser;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        ApplyPermissions();
        LoadLookups();
        LoadKitaplar();
    }

    private void btnAra_Click(object sender, EventArgs e)
    {
        LoadKitaplar();
    }

    private void txtAra_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        LoadKitaplar();
    }

    private void btnYenile_Click(object sender, EventArgs e)
    {
        LoadKitaplar();
    }

    private void btnYeni_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void btnEkle_Click(object sender, EventArgs e)
    {
        if (!CanManageBooks())
        {
            return;
        }

        if (!TryGetFormValues(out var formValues))
        {
            return;
        }

        try
        {
            var kitapId = DatabaseHelper.AddKitap(
                formValues.KitapAdi,
                formValues.OrijinalAd,
                formValues.YayinEviId,
                formValues.IlkYayinYili,
                formValues.Dil,
                formValues.Ozet,
                formValues.KitapBedeli,
                formValues.YazarIds,
                formValues.TurIds);

            selectedKitapId = kitapId;
            LoadKitaplar();
            SelectGridRow(kitapId);
            lblDurum.Text = "Kitap eklendi";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void btnGuncelle_Click(object sender, EventArgs e)
    {
        if (!CanManageBooks())
        {
            return;
        }

        if (selectedKitapId is null)
        {
            MessageBox.Show("Guncellemek icin listeden bir kitap secin.", "Kutuphane Otomasyon");
            return;
        }

        if (!TryGetFormValues(out var formValues))
        {
            return;
        }

        try
        {
            DatabaseHelper.UpdateKitap(
                selectedKitapId.Value,
                formValues.KitapAdi,
                formValues.OrijinalAd,
                formValues.YayinEviId,
                formValues.IlkYayinYili,
                formValues.Dil,
                formValues.Ozet,
                formValues.KitapBedeli,
                formValues.YazarIds,
                formValues.TurIds);

            var kitapId = selectedKitapId.Value;
            LoadKitaplar();
            SelectGridRow(kitapId);
            lblDurum.Text = "Kitap guncellendi";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void btnSil_Click(object sender, EventArgs e)
    {
        if (!CanDeleteBooks())
        {
            return;
        }

        if (selectedKitapId is null)
        {
            MessageBox.Show("Silmek icin listeden bir kitap secin.", "Kutuphane Otomasyon");
            return;
        }

        var result = MessageBox.Show(
            "Secili kitabi silmek istiyor musunuz?",
            "Kitap Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        try
        {
            DatabaseHelper.DeleteKitap(selectedKitapId.Value);
            ClearForm();
            LoadKitaplar();
            lblDurum.Text = "Kitap silindi";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void dgvKitaplar_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvKitaplar.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        selectedKitapId = Convert.ToInt32(row["KitapID"]);
        txtKitapAdi.Text = Convert.ToString(row["Kitap Adı"]);
        txtOrijinalAd.Text = Convert.ToString(row["OrijinalAd"]);
        txtIlkYayinYili.Text = Convert.ToString(row["İlk Yayın Yılı"]);
        txtDil.Text = Convert.ToString(row["Dil"]);
        txtOzet.Text = Convert.ToString(row["Özet"]);
        txtYayinEvi.Text = Convert.ToString(row["Yayınevi Adı"]);
        txtYazarlar.Text = Convert.ToString(row["Yazarlar"]);
        txtKitapBedeli.Text = Convert.ToDecimal(row["Kitap Bedeli"]).ToString("0.00");

        cmbYayinEvi.SelectedValue = row["YayinEviID"] == DBNull.Value ? 0 : Convert.ToInt32(row["YayinEviID"]);
        CheckItems(clbYazarlar, DatabaseHelper.GetBookYazarIds(selectedKitapId.Value));
        CheckItems(clbTurler, DatabaseHelper.GetBookTurIds(selectedKitapId.Value));
    }

    private void LoadKitaplar()
    {
        try
        {
            lblDurum.Text = "Kitaplar yukleniyor...";
            dgvKitaplar.DataSource = DatabaseHelper.GetKitaplar(txtAra.Text, Convert.ToString(cmbAramaFiltre.SelectedItem) ?? "Kitap");
            HideTechnicalColumns();
            lblDurum.Text = $"Toplam kitap kaydi: {dgvKitaplar.Rows.Count}";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private void LoadLookups()
    {
        cmbAramaFiltre.Items.Clear();
        cmbAramaFiltre.Items.AddRange(new object[] { "Kitap", "ISBN", "Dil", "Yayinevi", "Tur", "Ozet", "Yazar" });
        cmbAramaFiltre.SelectedIndex = 0;

        var yayinEvleri = DatabaseHelper.GetYayinEvleri();
        yayinEvleri.Insert(0, new LookupItem(0, "Yayinevi seciniz"));

        cmbYayinEvi.DataSource = yayinEvleri;
        cmbYayinEvi.DisplayMember = nameof(LookupItem.Name);
        cmbYayinEvi.ValueMember = nameof(LookupItem.Id);
        cmbYayinEvi.Visible = false;

        clbYazarlar.Items.Clear();
        foreach (var item in DatabaseHelper.GetYazarlar())
        {
            clbYazarlar.Items.Add(item);
        }

        clbTurler.Items.Clear();
        foreach (var item in DatabaseHelper.GetTurler())
        {
            clbTurler.Items.Add(item);
        }
    }

    private bool TryGetFormValues(out BookFormValues values)
    {
        values = default;

        var kitapAdi = txtKitapAdi.Text.Trim();
        var dil = txtDil.Text.Trim();
        if (kitapAdi.Length == 0 || dil.Length == 0)
        {
            MessageBox.Show("Kitap adi ve dil zorunludur.", "Kutuphane Otomasyon");
            return false;
        }

        short? ilkYayinYili = null;
        if (txtIlkYayinYili.Text.Trim().Length > 0)
        {
            if (!short.TryParse(txtIlkYayinYili.Text.Trim(), out var year))
            {
                MessageBox.Show("Ilk yayin yili sayi olmalidir.", "Kutuphane Otomasyon");
                return false;
            }

            ilkYayinYili = year;
        }

        var yayinEviId = GetOrCreateYayinEviId(txtYayinEvi.Text);
        if (!decimal.TryParse(txtKitapBedeli.Text.Trim(), out var kitapBedeli) || kitapBedeli < 0)
        {
            MessageBox.Show("Kitap bedeli sifir veya pozitif sayi olmalidir.", "Kutuphane Otomasyon");
            return false;
        }

        values = new BookFormValues(
            kitapAdi,
            EmptyToNull(txtOrijinalAd.Text),
            yayinEviId,
            ilkYayinYili,
            dil,
            EmptyToNull(txtOzet.Text),
            kitapBedeli,
            GetOrCreateYazarIds(txtYazarlar.Text),
            GetCheckedIds(clbTurler));

        return true;
    }

    private void ClearForm()
    {
        selectedKitapId = null;
        txtKitapAdi.Clear();
        txtOrijinalAd.Clear();
        txtIlkYayinYili.Clear();
        txtDil.Text = "Turkce";
        txtOzet.Clear();
        txtKitapBedeli.Text = "250";
        cmbYayinEvi.SelectedValue = 0;
        txtYayinEvi.Clear();
        txtYazarlar.Clear();
        CheckItems(clbYazarlar, []);
        CheckItems(clbTurler, []);
        dgvKitaplar.ClearSelection();
        txtKitapAdi.Focus();
    }

    private void ApplyPermissions()
    {
        var canManage = CanManageBooks(showMessage: false);
        btnEkle.Enabled = canManage;
        btnGuncelle.Enabled = canManage;
        btnSil.Enabled = CanDeleteBooks(showMessage: false);
        pnlForm.Enabled = canManage;
        txtKitapBedeli.Enabled = currentUser?.Role is UserRole.Admin || currentUser is null;

        if (!canManage)
        {
            lblDurum.Text = "Bu rol kitap yonetimi icin sadece goruntuleme yetkisine sahip";
        }
    }

    private bool CanManageBooks(bool showMessage = true)
    {
        var canManage = currentUser?.CanManageBooks ?? true;
        if (!canManage && showMessage)
        {
            MessageBox.Show("Bu islem icin personel veya admin yetkisi gerekir.", "Yetki Kontrol");
        }

        return canManage;
    }

    private bool CanDeleteBooks(bool showMessage = true)
    {
        var canDelete = currentUser?.Role is UserRole.Admin || currentUser is null;
        if (!canDelete && showMessage)
        {
            MessageBox.Show("Kitap silme islemi sadece admin yetkisindedir.", "Yetki Kontrol");
        }

        return canDelete;
    }

    private void SelectGridRow(int kitapId)
    {
        foreach (DataGridViewRow row in dgvKitaplar.Rows)
        {
            if (row.DataBoundItem is DataRowView dataRow && Convert.ToInt32(dataRow["KitapID"]) == kitapId)
            {
                row.Selected = true;
                dgvKitaplar.CurrentCell = row.Cells[1];
                return;
            }
        }
    }

    private void HideTechnicalColumns()
    {
        var kitapIdColumn = dgvKitaplar.Columns["KitapID"];
        if (kitapIdColumn is not null)
        {
            kitapIdColumn.Visible = false;
        }

        var yayinEviIdColumn = dgvKitaplar.Columns["YayinEviID"];
        if (yayinEviIdColumn is not null)
        {
            yayinEviIdColumn.Visible = false;
        }

        var orijinalAdColumn = dgvKitaplar.Columns["OrijinalAd"];
        if (orijinalAdColumn is not null)
        {
            orijinalAdColumn.Visible = false;
        }
    }

    private static List<int> GetCheckedIds(CheckedListBox listBox)
    {
        return listBox.CheckedItems
            .OfType<LookupItem>()
            .Select(item => item.Id)
            .ToList();
    }

    private static int? GetOrCreateYayinEviId(string name)
    {
        var trimmed = name.Trim();
        return trimmed.Length == 0 ? null : DatabaseHelper.GetOrCreateYayinEvi(trimmed);
    }

    private static List<int> GetOrCreateYazarIds(string names)
    {
        return names
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(name => name.Length > 0)
            .Select(DatabaseHelper.GetOrCreateYazar)
            .Distinct()
            .ToList();
    }

    private static void CheckItems(CheckedListBox listBox, HashSet<int> selectedIds)
    {
        for (var index = 0; index < listBox.Items.Count; index++)
        {
            var item = (LookupItem)listBox.Items[index];
            listBox.SetItemChecked(index, selectedIds.Contains(item.Id));
        }
    }

    private static string? EmptyToNull(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private void ShowError(Exception ex)
    {
        lblDurum.Text = "Islem hatasi";
        MessageBox.Show(
            $"Islem sirasinda hata olustu:{Environment.NewLine}{ex.Message}",
            "Kutuphane Otomasyon",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private readonly record struct BookFormValues(
        string KitapAdi,
        string? OrijinalAd,
        int? YayinEviId,
        short? IlkYayinYili,
        string Dil,
        string? Ozet,
        decimal KitapBedeli,
        IReadOnlyCollection<int> YazarIds,
        IReadOnlyCollection<int> TurIds);
}

