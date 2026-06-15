using System.Data;

namespace KutuphaneOtomasyon;

internal sealed class UserManagementForm : Form
{
    private readonly bool pendingOnly;
    private readonly AppUser currentUser;
    private readonly DataGridView grid = new();
    private readonly Label lblStatus = new();
    private readonly TextBox txtSearch = new();

    public UserManagementForm(bool pendingOnly, AppUser currentUser)
    {
        this.pendingOnly = pendingOnly;
        this.currentUser = currentUser;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 132,
            Padding = new Padding(18),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        topPanel.Controls.Add(new Label
        {
            Text = pendingOnly ? "Kayıt İstekleri" : "Kullanıcı Yönetimi - Üyelik ve Performans Özeti",
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(18, 10),
            AutoSize = true
        });

        topPanel.Controls.Add(new Label
        {
            Text = "Puan, aktif/toplam kiralama, geç teslim ve kayıp kitap sayıları kullanıcı odaklı gösterilir. Detay için satıra çift tıkla veya Profil Detayı butonunu kullan.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(20, 36),
            Size = new Size(1100, 22),
            AutoEllipsis = true
        });

        topPanel.Controls.Add(new Label
        {
            Text = "Ara",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(20, 63),
            AutoSize = true
        });

        txtSearch.Location = new Point(58, 59);
        txtSearch.Size = new Size(240, 27);
        txtSearch.PlaceholderText = "Ad, soyad veya telefon";
        txtSearch.TextChanged += (_, _) => LoadUsers();
        topPanel.Controls.Add(txtSearch);

        var left = 314;
        if (pendingOnly)
        {
            var approveButton = CreateButton("Onayla", Color.FromArgb(22, 163, 74), left);
            approveButton.Click += (_, _) => UpdateSelectedUser(DatabaseHelper.ApproveKullanici, "Kullanıcı onaylandı.");
            topPanel.Controls.Add(approveButton);
            left += 112;

            var rejectButton = CreateButton("Reddet", Color.FromArgb(220, 38, 38), left);
            rejectButton.Click += (_, _) => UpdateSelectedUser(DatabaseHelper.RejectKullanici, "Kullanıcı reddedildi.");
            topPanel.Controls.Add(rejectButton);
            left += 112;
        }

        var detailButton = CreateButton("Profil", Color.FromArgb(37, 99, 235), left);
        detailButton.Click += (_, _) => ShowSelectedProfile();
        topPanel.Controls.Add(detailButton);
        left += 112;

        if (currentUser.Role is UserRole.Admin)
        {
            var deleteButton = CreateButton("Sil", Color.FromArgb(100, 116, 139), left);
            deleteButton.Click += (_, _) => DeleteSelectedUser();
            topPanel.Controls.Add(deleteButton);
        }

        lblStatus.Location = new Point(18, 102);
        lblStatus.Size = new Size(700, 22);
        lblStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        topPanel.Controls.Add(lblStatus);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.CellDoubleClick += (_, _) => ShowSelectedProfile();

        Controls.Add(grid);
        Controls.Add(topPanel);
        Load += (_, _) => LoadUsers();
    }

    private static Button CreateButton(string text, Color color, int left)
    {
        var button = new Button
        {
            Text = text,
            BackColor = color,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(left, 56),
            Size = new Size(100, 32),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void LoadUsers()
    {
        grid.DataSource = DatabaseHelper.GetKullanicilar(pendingOnly, txtSearch.Text.Trim());
        var idColumn = grid.Columns["KullaniciID"];
        if (idColumn is not null)
        {
            idColumn.Visible = false;
        }

        var passwordColumn = grid.Columns["Sifre"];
        if (passwordColumn is not null)
        {
            passwordColumn.Visible = false;
        }

        var statusColumn = grid.Columns["Durum"];
        if (statusColumn is not null)
        {
            statusColumn.Visible = false;
        }

        lblStatus.Text = pendingOnly ? $"Bekleyen kayıt: {grid.Rows.Count}" : $"Toplam kullanıcı: {grid.Rows.Count}";
    }

    private void UpdateSelectedUser(Action<int> action, string message)
    {
        var id = GetSelectedUserId();
        if (id is null)
        {
            return;
        }

        action(id.Value);
        LoadUsers();
        lblStatus.Text = message;
    }

    private void DeleteSelectedUser()
    {
        var id = GetSelectedUserId();
        if (id is null)
        {
            return;
        }

        var result = MessageBox.Show("Seçili kullanıcı silinsin mi?", "Kullanıcı Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result != DialogResult.Yes)
        {
            return;
        }

        DatabaseHelper.DeleteKullanici(id.Value);
        LoadUsers();
        lblStatus.Text = "Kullanıcı silindi.";
    }

    private void ShowSelectedProfile()
    {
        var id = GetSelectedUserId();
        if (id is null)
        {
            return;
        }

        using var profileForm = new UserProfileForm(id.Value);
        profileForm.ShowDialog(this);
    }

    private int? GetSelectedUserId()
    {
        if (grid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            MessageBox.Show("Önce bir kullanıcı seçin.", "Kullanıcı Yönetimi");
            return null;
        }

        return Convert.ToInt32(row["KullaniciID"]);
    }
}
