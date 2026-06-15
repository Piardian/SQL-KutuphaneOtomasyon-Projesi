namespace KutuphaneOtomasyon;

internal sealed class RegisterForm : Form
{
    private readonly TextBox txtAd = new();
    private readonly TextBox txtSoyad = new();
    private readonly TextBox txtTelefon = new();
    private readonly TextBox txtPassword = new();
    private readonly TextBox txtPasswordConfirm = new();
    private readonly Label lblResult = new();

    public RegisterForm()
    {
        InitializeRegisterForm();
    }

    private void InitializeRegisterForm()
    {
        Text = "Kullanici Kayit";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(400, 456);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        Controls.Add(CreateLabel("Ad", 32));
        txtAd.Location = new Point(36, 56);
        txtAd.Size = new Size(326, 25);
        txtAd.Font = new Font("Segoe UI", 10F);
        Controls.Add(txtAd);

        Controls.Add(CreateLabel("Soyad", 92));
        txtSoyad.Location = new Point(36, 116);
        txtSoyad.Size = new Size(326, 25);
        txtSoyad.Font = new Font("Segoe UI", 10F);
        Controls.Add(txtSoyad);

        Controls.Add(CreateLabel("Telefon Numarasi", 152));
        txtTelefon.Location = new Point(36, 176);
        txtTelefon.Size = new Size(326, 25);
        txtTelefon.Font = new Font("Segoe UI", 10F);
        Controls.Add(txtTelefon);

        Controls.Add(CreateLabel("Sifre", 212));
        txtPassword.Location = new Point(36, 236);
        txtPassword.Size = new Size(326, 25);
        txtPassword.Font = new Font("Segoe UI", 10F);
        txtPassword.PasswordChar = '*';
        Controls.Add(txtPassword);

        Controls.Add(CreateLabel("Sifre dogrulama", 272));
        txtPasswordConfirm.Location = new Point(36, 296);
        txtPasswordConfirm.Size = new Size(326, 25);
        txtPasswordConfirm.Font = new Font("Segoe UI", 10F);
        txtPasswordConfirm.PasswordChar = '*';
        Controls.Add(txtPasswordConfirm);

        lblResult.Location = new Point(36, 330);
        lblResult.Size = new Size(326, 54);
        lblResult.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblResult.ForeColor = Color.FromArgb(15, 118, 110);
        Controls.Add(lblResult);

        var btnRegister = new Button
        {
            Text = "Kayit Ol",
            BackColor = Color.FromArgb(22, 163, 74),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(36, 396),
            Size = new Size(326, 36),
            UseVisualStyleBackColor = false
        };
        btnRegister.FlatAppearance.BorderSize = 0;
        btnRegister.Click += (_, _) => Register();
        Controls.Add(btnRegister);
    }

    private static Label CreateLabel(string text, int top)
    {
        return new Label
        {
            Text = text,
            Location = new Point(36, top),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85)
        };
    }

    private void Register()
    {
        if (txtAd.Text.Trim().Length == 0 || txtSoyad.Text.Trim().Length == 0 || txtTelefon.Text.Trim().Length == 0 || txtPassword.Text.Length == 0)
        {
            lblResult.ForeColor = Color.FromArgb(220, 38, 38);
            lblResult.Text = "Ad, soyad, telefon ve sifre zorunludur.";
            return;
        }

        if (txtPassword.Text != txtPasswordConfirm.Text)
        {
            lblResult.ForeColor = Color.FromArgb(220, 38, 38);
            lblResult.Text = "Sifre ve sifre dogrulama ayni degil.";
            return;
        }

        try
        {
            DatabaseHelper.RegisterKullanici(txtAd.Text, txtSoyad.Text, txtTelefon.Text, txtPassword.Text);
            lblResult.ForeColor = Color.FromArgb(15, 118, 110);
            lblResult.Text = "Kaydiniz alinmistir. Onaylanmasi icin bekleyiniz.";
        }
        catch (Exception ex)
        {
            lblResult.ForeColor = Color.FromArgb(220, 38, 38);
            lblResult.Text = ex.Message;
        }
    }
}
