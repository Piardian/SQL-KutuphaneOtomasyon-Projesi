namespace KutuphaneOtomasyon;

internal sealed class MainForm : Form
{
    private readonly AppUser currentUser;
    private readonly Panel contentPanel = new();
    private readonly Label lblTitle = new();
    private readonly Label lblStatus = new();

    public MainForm(AppUser currentUser)
    {
        this.currentUser = currentUser;
        InitializeMainForm();
    }

    public bool LogoutRequested { get; private set; }

    private void InitializeMainForm()
    {
        Text = "Kutuphane Otomasyon";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 760);
        BackColor = Color.White;

        var menu = CreateMenu();
        var header = CreateHeader();

        contentPanel.Dock = DockStyle.Fill;
        contentPanel.BackColor = Color.FromArgb(248, 250, 252);

        MainMenuStrip = menu;
        Controls.Add(contentPanel);
        Controls.Add(header);
        Controls.Add(menu);

        ShowRoleHome();
    }

    private MenuStrip CreateMenu()
    {
        var menu = new MenuStrip
        {
            BackColor = Color.FromArgb(15, 23, 42),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };

        AddMenuItem(menu, "Ana Panel", ShowRoleHome);

        if (currentUser.CanManageBooks)
        {
            AddMenuItem(menu, "Kitap Yonetimi", ShowBooks);
            AddMenuItem(menu, "Kullanici Yonetimi", () => ShowUsers(false));
            AddMenuItem(menu, "Kira Yonetimi", ShowRentManagement);
        }

        if (currentUser.Role is UserRole.Admin)
        {
            AddMenuItem(menu, "Yetki Yonetimi", ShowPersonnel);
        }

        if (currentUser.Role is UserRole.Personel)
        {
            AddMenuItem(menu, "Kayit Istekleri", () => ShowUsers(true));
        }

        if (currentUser.Role is UserRole.Kullanici)
        {
            AddMenuItem(menu, "Kitaplar", ShowBookShowcase);
            AddMenuItem(menu, "Odul ve Cezalarim", ShowUserRewards);
        }

        AddMenuItem(menu, "Oturumu Kapat", Logout);
        return menu;
    }

    private static void AddMenuItem(MenuStrip menu, string text, Action action)
    {
        var item = new ToolStripMenuItem(text);
        item.Click += (_, _) => action();
        menu.Items.Add(item);
    }

    private Panel CreateHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 74,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 10, 20, 10)
        };

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(30, 41, 59);
        lblTitle.Location = new Point(20, 9);

        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9.5F);
        lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
        lblStatus.Location = new Point(23, 45);
        lblStatus.Text = $"{currentUser.DisplayName} - {currentUser.Role}";

        header.Controls.Add(lblTitle);
        header.Controls.Add(lblStatus);
        return header;
    }

    private void ShowRoleHome()
    {
        switch (currentUser.Role)
        {
            case UserRole.Admin:
                ShowAdminHome();
                break;
            case UserRole.Personel:
                ShowPersonnelHome();
                break;
            default:
                ShowUserHome();
                break;
        }
    }

    private void ShowUserHome()
    {
        lblTitle.Text = "Kullanıcı Paneli";
        contentPanel.Controls.Clear();

        if (currentUser.RecordId is null)
        {
            contentPanel.Controls.Add(CreateInfoBlock("Kullanıcı kaydı bulunamadı."));
            return;
        }

        contentPanel.Controls.Add(CreateUserDashboard(currentUser.RecordId.Value));
    }

    private void ShowAdminHome()
    {
        lblTitle.Text = "Admin Paneli";
        contentPanel.Controls.Clear();
        contentPanel.Controls.Add(CreateOperationalDashboard());
        contentPanel.Controls.Add(CreateInfoBlock("Admin yetkisi aktif. Kitap, kullanıcı, kiralama, kayıp ödeme ve yetki yönetimi bu rolde açıktır."));
    }

    private void ShowPersonnelHome()
    {
        lblTitle.Text = "Personel Paneli";
        contentPanel.Controls.Clear();
        contentPanel.Controls.Add(CreateOperationalDashboard());
        contentPanel.Controls.Add(CreateInfoBlock("Personel kiralama, teslim alma ve kayıp işaretleme süreçlerini yönetebilir. Yetki/personel yönetimi admin tarafındadır."));
    }

    private void ShowUserRewards()
    {
        lblTitle.Text = "Ödül ve Cezalarım";
        contentPanel.Controls.Clear();

        if (currentUser.RecordId is null)
        {
            contentPanel.Controls.Add(CreateInfoBlock("Kullanıcı kaydı bulunamadı."));
            return;
        }

        contentPanel.Controls.Add(CreateUserDashboard(currentUser.RecordId.Value));
    }

    private Control CreatePenaltyGrid(int kullaniciId)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = DatabaseHelper.GetUserPenalties(kullaniciId),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };
        return grid;
    }

    private void ShowBooks()
    {
        lblTitle.Text = "Kitap Yonetimi";
        contentPanel.Controls.Clear();

        var form = new Form1(currentUser)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowBookShowcase()
    {
        lblTitle.Text = "Kitaplar";
        contentPanel.Controls.Clear();
        var form = new BookShowcaseForm(currentUser)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowRentalRequests()
    {
        lblTitle.Text = "Kiralama Istekleri";
        contentPanel.Controls.Clear();
        var form = new RentalRequestsForm(currentUser)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowRentManagement()
    {
        lblTitle.Text = "Kira Yonetimi";
        contentPanel.Controls.Clear();
        var form = new RentManagementForm(currentUser)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowPersonnel()
    {
        lblTitle.Text = "Personel Yonetimi";
        contentPanel.Controls.Clear();

        var form = new PersonnelManagementForm
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowUsers(bool pendingOnly)
    {
        lblTitle.Text = pendingOnly ? "Kayit Istekleri" : "Kullanici Yonetimi";
        contentPanel.Controls.Clear();
        var form = new UserManagementForm(pendingOnly, currentUser)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        contentPanel.Controls.Add(form);
        form.Show();
    }

    private void ShowPlaceholder(string title, string body)
    {
        lblTitle.Text = title;
        contentPanel.Controls.Clear();
        contentPanel.Controls.Add(CreateInfoBlock(body));
    }

    private void Logout()
    {
        LogoutRequested = true;
        Close();
    }

    private static Control CreateCards(params (string Title, string Value)[] cards)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 190,
            Padding = new Padding(22),
            ColumnCount = cards.Length,
            RowCount = 1
        };

        for (var index = 0; index < cards.Length; index++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cards.Length));
            layout.Controls.Add(CreateDashboardCard(cards[index].Title, cards[index].Value), index, 0);
        }

        return layout;
    }

    private Control CreateUserDashboard(int kullaniciId)
    {
        var profile = DatabaseHelper.GetUserProfile(kullaniciId);
        var summary = DatabaseHelper.GetUserRewardSummary(kullaniciId);

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(24)
        };

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ItemSize = new Size(160, 34),
            Padding = new Point(14, 5)
        };
        tabs.TabPages.Add(CreateUserTab("Aktif Kiralamalar", profile.ActiveRentals));
        tabs.TabPages.Add(CreateUserTab("Geçmiş Kiralamalar", profile.PastRentals));
        tabs.TabPages.Add(CreateUserTab("Puan Hareketleri", profile.PointMovements));
        tabs.TabPages.Add(CreateUserTab("Cezalar", DatabaseHelper.GetUserPenalties(kullaniciId)));

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(18, 6, 18, 0),
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Text = $"{profile.Ad} {profile.Soyad}   Telefon: {profile.Telefon}   Toplam kiralama: {profile.TotalRentCount}   Kayıp kitap: {profile.LostBookCount}   Puanların teslim ve ceza kayıtlarından hesaplanır."
        };

        var cards = CreateCards(
            ("Mevcut Puan", profile.MevcutPuan.ToString()),
            ("Kazanılan Puan", profile.TotalEarnedPoints.ToString()),
            ("Ceza Puanı", profile.TotalPenaltyPoints.ToString()),
            ("Bekleyen Kayıp Ödemesi", $"{summary.UnpaidPenalty:0.00} TL"),
            ("Aktif Kiralama", summary.ActiveRentCount.ToString()));

        panel.Controls.Add(tabs);
        panel.Controls.Add(CreateActiveRentalCards(DatabaseHelper.GetUserActiveRentalCards(kullaniciId)));
        panel.Controls.Add(info);
        panel.Controls.Add(cards);
        return panel;
    }

    private static Control CreateActiveRentalCards(System.Data.DataTable activeRentals)
    {
        var container = new Panel
        {
            Dock = DockStyle.Top,
            Height = activeRentals.Rows.Count == 0 ? 142 : 264,
            Padding = new Padding(22, 10, 22, 14),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        var title = new Label
        {
            Text = "Aktif Kiralamalarım",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42)
        };

        var hint = new Label
        {
            Text = "Teslim işlemi kullanıcı panelinden kapatılmaz. Kitabı personele teslim et; personel/admin Kira Yönetimi > Teslim Al ile kaydı kapatır.",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 9.8F),
            ForeColor = Color.FromArgb(71, 85, 105)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 2, 0, 0),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        if (activeRentals.Rows.Count == 0)
        {
            flow.Controls.Add(new Label
            {
                Text = "Şu anda aktif kiralaman yok. Kitaplar sekmesinden yeni bir kiralama isteği gönderebilirsin.",
                Width = 980,
                Height = 48,
                Padding = new Padding(14, 12, 14, 0),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
        }
        else
        {
            foreach (System.Data.DataRow row in activeRentals.Rows)
            {
                flow.Controls.Add(CreateActiveRentalCard(row));
            }
        }

        container.Controls.Add(flow);
        container.Controls.Add(hint);
        container.Controls.Add(title);
        return container;
    }

    private static Control CreateActiveRentalCard(System.Data.DataRow row)
    {
        var kitapId = GetRowInt(row, "KitapID");
        var kitapAdi = GetRowText(row, "Kitap Adı");
        var yazarlar = GetRowText(row, "Yazarlar");
        var sonTeslimTarihi = GetRowDate(row, "Son Teslim Tarihi");
        var kalanGun = GetRowInt(row, "Kalan Gün");
        var durum = GetRowText(row, "Durum");
        var kazanilacakPuan = GetRowInt(row, "Kazanılacak Puan");
        var gecikmeCezaPuani = GetRowInt(row, "Gecikme Ceza Puanı");
        var isLate = string.Equals(durum, "Gecikmiş", StringComparison.OrdinalIgnoreCase) || kalanGun < 0;
        var statusText = isLate
            ? $"{Math.Abs(kalanGun)} gün gecikti"
            : kalanGun == 0
                ? "Bugün teslim edilmeli"
                : $"{kalanGun} gün kaldı";
        var statusColor = isLate ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 101, 52);
        var pointText = isLate
            ? $"Gecikme cezası: -{Math.Abs(gecikmeCezaPuani)} puan"
            : $"Zamanında teslim: +{kazanilacakPuan} puan";

        var border = new Panel
        {
            Width = 405,
            Height = 154,
            Margin = new Padding(0, 8, 16, 8),
            Padding = new Padding(1),
            BackColor = isLate ? Color.FromArgb(252, 165, 165) : Color.FromArgb(191, 219, 254)
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = Color.White
        };

        var coverHost = new Panel
        {
            Dock = DockStyle.Left,
            Width = 88,
            BackColor = Color.FromArgb(239, 246, 255),
            Padding = new Padding(4)
        };
        var cover = LoadBookCoverImage(kitapId);
        if (cover is not null)
        {
            coverHost.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = cover,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            });
        }
        else
        {
            coverHost.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = GetInitials(kitapAdi),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold)
            });
        }

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 0, 0, 0),
            ColumnCount = 1,
            RowCount = 6
        };
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        details.Controls.Add(CreateRentalCardLabel(kitapAdi, 11F, FontStyle.Bold, Color.FromArgb(15, 23, 42)), 0, 0);
        details.Controls.Add(CreateRentalCardLabel(string.IsNullOrWhiteSpace(yazarlar) ? "Yazar bilgisi yok" : yazarlar, 9F, FontStyle.Regular, Color.FromArgb(71, 85, 105)), 0, 1);
        details.Controls.Add(CreateRentalCardLabel($"Son teslim: {FormatDate(sonTeslimTarihi)}", 9.3F, FontStyle.Bold, Color.FromArgb(30, 41, 59)), 0, 2);
        details.Controls.Add(CreateRentalCardLabel(statusText, 9.3F, FontStyle.Bold, statusColor), 0, 3);
        details.Controls.Add(CreateRentalCardLabel(pointText, 9.3F, FontStyle.Bold, isLate ? Color.FromArgb(185, 28, 28) : Color.FromArgb(21, 128, 61)), 0, 4);

        var helpButton = new Button
        {
            Text = "Teslim nasıl yapılır?",
            Dock = DockStyle.Fill,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        helpButton.FlatAppearance.BorderSize = 0;
        helpButton.Click += (_, _) => MessageBox.Show(
            "Teslim işlemi kullanıcı tarafından kapatılmaz.\n\nKitabı kütüphane personeline ver. Personel veya admin Kira Yönetimi ekranında ilgili kiralama kaydını seçip Teslim Al butonuna basar.\n\nZamanında teslimde ödül puanı eklenir; son teslim tarihi geçerse gecikme ceza puanı uygulanır.",
            "Teslim Bilgisi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        details.Controls.Add(helpButton, 0, 5);

        card.Controls.Add(details);
        card.Controls.Add(coverHost);
        border.Controls.Add(card);
        return border;
    }

    private static Label CreateRentalCardLabel(string text, float size, FontStyle style, Color color)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static Image? LoadBookCoverImage(int kitapId)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "BookCovers", $"{kitapId}.jpg");
        if (!File.Exists(path))
        {
            return null;
        }

        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    private static string GetInitials(string value)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Take(2).Select(word => char.ToUpperInvariant(word[0])));
    }

    private static string GetRowText(System.Data.DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
            ? Convert.ToString(row[columnName]) ?? string.Empty
            : string.Empty;
    }

    private static int GetRowInt(System.Data.DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
            ? Convert.ToInt32(row[columnName])
            : 0;
    }

    private static DateTime? GetRowDate(System.Data.DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
            ? Convert.ToDateTime(row[columnName])
            : null;
    }

    private static string FormatDate(DateTime? date)
    {
        return date.HasValue ? date.Value.ToString("dd.MM.yyyy HH:mm") : "-";
    }

    private static TabPage CreateUserTab(string title, object dataSource)
    {
        var tab = new TabPage(title)
        {
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        tab.Controls.Add(CreateModernGrid(dataSource));
        return tab;
    }

    private static DataGridView CreateModernGrid(object dataSource)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = dataSource,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            GridColor = Color.FromArgb(226, 232, 240),
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(15, 23, 42);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        grid.RowTemplate.Height = 32;
        return grid;
    }

    private static Control CreateOperationalDashboard()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        var cards = DatabaseHelper.GetMainDashboardCards()
            .Select(item => (item.Key, item.Value))
            .ToArray();

        var grids = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22, 0, 22, 18),
            ColumnCount = 2,
            RowCount = 1
        };
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grids.Controls.Add(CreateTitledGrid("Bugün / Yakında Teslim Edilecekler", DatabaseHelper.GetUpcomingDueRows()), 0, 0);
        grids.Controls.Add(CreateTitledGrid("Gecikmiş Kiralamalar", DatabaseHelper.GetDashboardOverdueRows()), 1, 0);

        panel.Controls.Add(grids);
        panel.Controls.Add(CreateCards(cards));
        return panel;
    }

    private static Control CreateTitledGrid(string title, object dataSource)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            BackColor = Color.White
        };

        panel.Controls.Add(new DataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = dataSource,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        });

        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Height = 30
        });

        return panel;
    }

    private static Label CreateInfoBlock(string body)
    {
        return new Label
        {
            Text = body,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(51, 65, 85),
            Padding = new Padding(26, 12, 26, 8),
            Height = 76
        };
    }

    private static Panel CreateDashboardCard(string title, string value)
    {
        var panel = new Panel
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            Padding = new Padding(18)
        };

        panel.Controls.Add(new Label
        {
            Text = value,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Height = 45
        });

        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Height = 28
        });

        return panel;
    }
}

