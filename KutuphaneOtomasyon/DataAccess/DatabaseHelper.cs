using System.Data;
using Microsoft.Data.SqlClient;

namespace KutuphaneOtomasyon;

internal static class DatabaseHelper
{
    internal const string ConnectionString =
        "Server=localhost;Database=KutuphaneDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static DataTable GetKitaplar(string searchText, string filter = "Kitap")
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                k.KitapID,
                k.KitapAdi AS [Kitap Adı],
                ISBN = ISNULL((
                    SELECT STRING_AGG(CAST(kb.ISBN AS nvarchar(max)), N', ')
                    FROM dbo.KitapBasimlari kb
                    WHERE kb.KitapID = k.KitapID
                        AND kb.ISBN IS NOT NULL
                        AND kb.ISBN <> N''
                        AND kb.ISBN NOT LIKE N'DEMO-%'
                ), N''),
                k.OrijinalAd,
                k.YayinEviID,
                ye.YayinEviAdi AS [Yayınevi Adı],
                k.IlkYayinYili AS [İlk Yayın Yılı],
                k.Dil,
                k.Ozet AS [Özet],
                k.KitapBedeli AS [Kitap Bedeli],
                [Toplam Adet] = COUNT(kk.KopyaID),
                [Müsait Adet] = SUM(CASE WHEN kk.Durum = N'Musait' THEN 1 ELSE 0 END),
                [Kirada Adet] = SUM(CASE WHEN kk.Durum IN (N'Kirada') THEN 1 ELSE 0 END),
                [Kitap Durumu] = CASE
                    WHEN COUNT(kk.KopyaID) = 0 THEN N'Pasif'
                    WHEN SUM(CASE WHEN kk.Durum = N'Musait' THEN 1 ELSE 0 END) = COUNT(kk.KopyaID) THEN N'Müsait'
                    WHEN SUM(CASE WHEN kk.Durum = N'Musait' THEN 1 ELSE 0 END) > 0 THEN N'Kısmen Müsait'
                    WHEN SUM(CASE WHEN kk.Durum IN (N'Kirada') THEN 1 ELSE 0 END) = COUNT(kk.KopyaID) THEN N'Tamamı Kirada'
                    ELSE N'Pasif'
                END,
                Yazarlar = ISNULL((
                    SELECT STRING_AGG(y.YazarAdi, ', ')
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID
                ), ''),
                Turler = ISNULL((
                    SELECT STRING_AGG(t.TurAdi, ', ')
                    FROM dbo.KitapTurler kt
                    INNER JOIN dbo.Turler t ON t.TurID = kt.TurID
                    WHERE kt.KitapID = k.KitapID
                ), '')
            FROM dbo.Kitaplar k
            LEFT JOIN dbo.YayinEvleri ye ON ye.YayinEviID = k.YayinEviID
            LEFT JOIN dbo.KitapKopyalari kk ON kk.KitapID = k.KitapID
            WHERE @SearchText = ''
                OR (@Filter = N'Kitap' AND k.KitapAdi LIKE @SearchText + '%')
                OR (@Filter = N'Dil' AND k.Dil LIKE @SearchText + '%')
                OR (@Filter = N'Yayinevi' AND ISNULL(ye.YayinEviAdi, '') LIKE @SearchText + '%')
                OR (@Filter = N'ISBN' AND EXISTS (
                    SELECT 1
                    FROM dbo.KitapBasimlari kb
                    WHERE kb.KitapID = k.KitapID
                        AND kb.ISBN NOT LIKE N'DEMO-%'
                        AND kb.ISBN LIKE @SearchText + N'%'
                ))
                OR (@Filter = N'Ozet' AND ISNULL(k.Ozet, '') LIKE @SearchText + '%')
                OR (@Filter = N'Yazar' AND EXISTS (
                    SELECT 1
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID AND y.YazarAdi LIKE @SearchText + '%'
                ))
                OR (@Filter = N'Tur' AND EXISTS (
                    SELECT 1
                    FROM dbo.KitapTurler kt
                    INNER JOIN dbo.Turler t ON t.TurID = kt.TurID
                    WHERE kt.KitapID = k.KitapID AND t.TurAdi LIKE @SearchText + '%'
                ))
            GROUP BY k.KitapID, k.KitapAdi, k.OrijinalAd, k.YayinEviID, ye.YayinEviAdi, k.IlkYayinYili, k.Dil, k.Ozet, k.KitapBedeli
            ORDER BY k.KitapAdi;
            """,
            connection);
        command.Parameters.AddWithValue("@SearchText", searchText.Trim());
        command.Parameters.AddWithValue("@Filter", filter);
        using var adapter = new SqlDataAdapter(command);

        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static AppUser? Login(UserRole role, string userName, string password)
    {
        userName = userName.Trim();
        password = password.Trim();

        return LoginWithAccount(role, userName, password);
    }

    public static int RegisterKullanici(string ad, string soyad, string telefon, string password)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var command = new SqlCommand(
                """
                INSERT INTO dbo.Kullanicilar (Ad, Soyad, Telefon, Eposta, Adres, Sehir, UyelikTarihi, Skor, Durum, KayitTarihi)
                OUTPUT INSERTED.KullaniciID
                VALUES (@Ad, @Soyad, @Telefon, @Eposta, N'', N'', CONVERT(date, GETDATE()), 0, N'Beklemede', GETDATE());
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("@Ad", ad.Trim());
            command.Parameters.AddWithValue("@Soyad", soyad.Trim());
            command.Parameters.AddWithValue("@Telefon", telefon.Trim());
            command.Parameters.AddWithValue("@Eposta", $"{telefon.Trim()}@kayit.local");
            var kullaniciId = Convert.ToInt32(command.ExecuteScalar());
            AddAccount(connection, transaction, UserRole.Kullanici, kullaniciId, telefon.Trim(), password, false);
            transaction.Commit();
            return kullaniciId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static DataTable GetPersoneller()
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                p.PersonelID,
                p.Ad,
                p.Soyad,
                p.Telefon,
                p.Gorev,
                p.IseBaslamaTarihi,
                p.KutuphaneID,
                k.KutuphaneAdi
            FROM dbo.Personel p
            INNER JOIN dbo.Kutuphaneler k ON k.KutuphaneID = p.KutuphaneID
            ORDER BY p.PersonelID DESC;
            """,
            connection);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetKullanicilar(bool onlyPending = false, string searchText = "")
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                k.KullaniciID,
                k.Ad,
                k.Soyad,
                k.Telefon,
                k.MevcutPuan AS [Puan],
                [Aktif Kiralama Sayısı] = (
                    SELECT COUNT(*)
                    FROM dbo.Kiralamalar kr
                    WHERE kr.KullaniciID = k.KullaniciID AND kr.TeslimTarihi IS NULL AND kr.Durum <> N'Kayıp'
                ),
                [Toplam Kiralama Sayısı] = (
                    SELECT COUNT(*)
                    FROM dbo.Kiralamalar kr
                    WHERE kr.KullaniciID = k.KullaniciID
                ),
                [Geç Teslim Sayısı] = (
                    SELECT COUNT(*)
                    FROM dbo.Kiralamalar kr
                    WHERE kr.KullaniciID = k.KullaniciID
                        AND (kr.Durum = N'Geç Teslim Edildi' OR (kr.TeslimTarihi IS NOT NULL AND kr.TeslimTarihi > kr.IadeTarihi))
                ),
                [Kayıp Kitap Sayısı] = (
                    SELECT COUNT(*)
                    FROM dbo.KayipKitaplar kk
                    WHERE kk.KullaniciId = k.KullaniciID
                ),
                Sifre = N'********',
                [Kayıp Kitap Borcu] = CONCAT(
                    (SELECT COUNT(*) FROM dbo.KayipKitaplar kk WHERE kk.KullaniciId = k.KullaniciID AND kk.OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal')),
                    N' Kitap / ',
                    ISNULL((SELECT SUM(kk.KitapBedeli) FROM dbo.KayipKitaplar kk WHERE kk.KullaniciId = k.KullaniciID AND kk.OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal')), 0),
                    N' TL'
                ),
                k.KayitTarihi
            FROM dbo.Kullanicilar k
            WHERE (@OnlyPending = 0 OR k.Durum = N'Beklemede')
                AND (
                    @SearchText = N''
                    OR k.Ad LIKE @SearchText + N'%'
                    OR k.Soyad LIKE @SearchText + N'%'
                    OR k.Telefon LIKE @SearchText + N'%'
                    OR (k.Ad + N' ' + k.Soyad) LIKE @SearchText + N'%'
                )
            ORDER BY
                k.MevcutPuan DESC,
                (SELECT COUNT(*) FROM dbo.Kiralamalar kr WHERE kr.KullaniciID = k.KullaniciID) DESC,
                k.KayitTarihi DESC,
                k.KullaniciID DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@OnlyPending", onlyPending);
        command.Parameters.AddWithValue("@SearchText", searchText);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void ApproveKullanici(int kullaniciId)
    {
        UpdateKullaniciStatus(kullaniciId, "Onaylandi", true);
    }

    public static void RejectKullanici(int kullaniciId)
    {
        UpdateKullaniciStatus(kullaniciId, "Reddedildi", false);
    }

    public static void DeleteKullanici(int kullaniciId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            ExecuteNonQuery(connection, transaction, """
                DELETE c
                FROM dbo.Cezalar c
                INNER JOIN dbo.Kiralamalar k ON k.KiraID = c.KiralamaID
                WHERE k.KullaniciID = @KullaniciID;
                """, "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.PuanHareketleri WHERE KullaniciId = @KullaniciID;", "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KayipKitaplar WHERE KullaniciId = @KullaniciID;", "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.Kiralamalar WHERE KullaniciID = @KullaniciID;", "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.Rezervasyonlar WHERE KullaniciID = @KullaniciID;", "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.GirisHesaplari WHERE Rol = N'Kullanici' AND KayitID = @KullaniciID;", "@KullaniciID", kullaniciId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.Kullanicilar WHERE KullaniciID = @KullaniciID;", "@KullaniciID", kullaniciId);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static List<LookupItem> GetKutuphaneler()
    {
        return GetLookupItems("SELECT KutuphaneID, KutuphaneAdi FROM dbo.Kutuphaneler ORDER BY KutuphaneAdi;");
    }

    public static int AddPersonel(string ad, string soyad, string telefon, string password)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var command = new SqlCommand(
                """
                INSERT INTO dbo.Personel (Ad, Soyad, Telefon, Gorev, IseBaslamaTarihi, KutuphaneID)
                OUTPUT INSERTED.PersonelID
                VALUES (@Ad, @Soyad, @Telefon, @Gorev, CONVERT(date, GETDATE()), @KutuphaneID);
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("@Ad", ad.Trim());
            command.Parameters.AddWithValue("@Soyad", soyad.Trim());
            command.Parameters.AddWithValue("@Telefon", telefon.Trim());
            command.Parameters.AddWithValue("@Gorev", "Personel");
            command.Parameters.AddWithValue("@KutuphaneID", GetMainKutuphaneId(connection, transaction));
            var personelId = Convert.ToInt32(command.ExecuteScalar());
            AddAccount(connection, transaction, UserRole.Personel, personelId, telefon.Trim(), password, true);
            transaction.Commit();
            return personelId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static int GetDefaultPersonelId()
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand("SELECT TOP 1 PersonelID FROM dbo.Personel ORDER BY PersonelID;", connection);
        connection.Open();
        var result = command.ExecuteScalar();
        if (result is null || result == DBNull.Value)
        {
            throw new InvalidOperationException("Kiralama işlemi için en az bir personel kaydı bulunmalıdır.");
        }

        return Convert.ToInt32(result);
    }

    public static void DeletePersonel(int personelId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var checkCommand = new SqlCommand(
            """
            SELECT
                KiralamaSayisi = (SELECT COUNT(*) FROM dbo.Kiralamalar WHERE PersonelID = @PersonelID),
                RezervasyonSayisi = (SELECT COUNT(*) FROM dbo.Rezervasyonlar WHERE PersonelID = @PersonelID);
            """,
            connection);
        checkCommand.Parameters.AddWithValue("@PersonelID", personelId);
        connection.Open();

        using (var reader = checkCommand.ExecuteReader())
        {
            if (reader.Read() && (reader.GetInt32(0) > 0 || reader.GetInt32(1) > 0))
            {
                throw new InvalidOperationException("Bu personele bagli kiralama veya rezervasyon kaydi oldugu icin personel silinemez.");
            }
        }

        using var accountCommand = new SqlCommand("DELETE FROM dbo.GirisHesaplari WHERE Rol = N'Personel' AND KayitID = @PersonelID;", connection);
        accountCommand.Parameters.AddWithValue("@PersonelID", personelId);
        accountCommand.ExecuteNonQuery();

        using var deleteCommand = new SqlCommand("DELETE FROM dbo.Personel WHERE PersonelID = @PersonelID;", connection);
        deleteCommand.Parameters.AddWithValue("@PersonelID", personelId);
        deleteCommand.ExecuteNonQuery();
    }

    public static DataTable GetAvailableBooksForShowcase(string searchText = "")
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                k.KitapID,
                k.KitapAdi AS [Kitap Adı],
                ISBN = ISNULL((
                    SELECT STRING_AGG(CAST(kb.ISBN AS nvarchar(max)), N', ')
                    FROM dbo.KitapBasimlari kb
                    WHERE kb.KitapID = k.KitapID
                        AND kb.ISBN IS NOT NULL
                        AND kb.ISBN <> N''
                        AND kb.ISBN NOT LIKE N'DEMO-%'
                ), N''),
                k.Dil AS [Dil],
                k.IlkYayinYili AS [İlk Yayın Yılı],
                k.Ozet AS [Özet],
                k.KitapBedeli AS [Kitap Bedeli],
                ISNULL(ye.YayinEviAdi, N'') AS [Yayınevi],
                ToplamKopya = COUNT(kk.KopyaID),
                MusaitKopya = COUNT(CASE WHEN kk.Durum = N'Musait' THEN 1 END),
                KiradaKopya = COUNT(CASE WHEN kk.Durum IN (N'Kirada', N'Rezerve') THEN 1 END),
                [Müsaitlik] = CASE
                    WHEN COUNT(kk.KopyaID) = 0 THEN N'Pasif'
                    WHEN COUNT(CASE WHEN kk.Durum = N'Musait' THEN 1 END) > 0 THEN N'Müsait'
                    ELSE N'Tamamı Kirada'
                END,
                Yazarlar = ISNULL((
                    SELECT STRING_AGG(y.YazarAdi, ', ')
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID
                ), ''),
                Türler = ISNULL((
                    SELECT STRING_AGG(t.TurAdi, ', ')
                    FROM dbo.KitapTurler kt
                    INNER JOIN dbo.Turler t ON t.TurID = kt.TurID
                    WHERE kt.KitapID = k.KitapID
                ), '')
            FROM dbo.Kitaplar k
            LEFT JOIN dbo.YayinEvleri ye ON ye.YayinEviID = k.YayinEviID
            LEFT JOIN dbo.KitapKopyalari kk ON kk.KitapID = k.KitapID
            WHERE @SearchText = N''
                OR k.KitapAdi LIKE @SearchText + N'%'
                OR k.Dil LIKE @SearchText + N'%'
                OR ISNULL(ye.YayinEviAdi, N'') LIKE @SearchText + N'%'
                OR ISNULL(k.Ozet, N'') LIKE @SearchText + N'%'
                OR EXISTS (
                    SELECT 1
                    FROM dbo.KitapBasimlari kb
                    WHERE kb.KitapID = k.KitapID
                        AND kb.ISBN NOT LIKE N'DEMO-%'
                        AND kb.ISBN LIKE @SearchText + N'%'
                )
                OR EXISTS (
                    SELECT 1
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID AND y.YazarAdi LIKE @SearchText + N'%'
                )
                OR EXISTS (
                    SELECT 1
                    FROM dbo.KitapTurler kt
                    INNER JOIN dbo.Turler t ON t.TurID = kt.TurID
                    WHERE kt.KitapID = k.KitapID AND t.TurAdi LIKE @SearchText + N'%'
                )
            GROUP BY k.KitapID, k.KitapAdi, k.Dil, k.IlkYayinYili, k.Ozet, k.KitapBedeli, ye.YayinEviAdi
            ORDER BY COUNT(CASE WHEN kk.Durum = N'Musait' THEN 1 END) DESC, k.KitapAdi;
            """,
            connection);
        command.Parameters.AddWithValue("@SearchText", searchText);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void CreateRentalRequest(int kullaniciId, int kitapId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            DECLARE @KopyaID int = (
                SELECT TOP 1 KopyaID
                FROM dbo.KitapKopyalari
                WHERE KitapID = @KitapID AND Durum = N'Musait'
                ORDER BY KopyaID
            );
            IF @KopyaID IS NULL
                THROW 50001, 'Bu kitap icin musait kopya yok.', 1;
            UPDATE dbo.KitapKopyalari
            SET Durum = N'Rezerve'
            WHERE KopyaID = @KopyaID;
            INSERT INTO dbo.Rezervasyonlar (KullaniciID, KopyaID, RezervasyonTarihi, SonGecerlilikTarihi, Durum, PersonelID)
            VALUES (@KullaniciID, @KopyaID, GETDATE(), DATEADD(day, 3, GETDATE()), N'Kiralama Istegi', NULL);
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        command.Parameters.AddWithValue("@KitapID", kitapId);
        connection.Open();
        command.ExecuteNonQuery();
    }

    public static DataTable GetUserRentalRequests(int kullaniciId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                rz.RezervasyonID,
                k.KitapAdi AS [Kitap Adı],
                rz.RezervasyonTarihi AS [İstek Tarihi],
                rz.SonGecerlilikTarihi AS [Son Geçerlilik],
                rz.Durum AS [Durum]
            FROM dbo.Rezervasyonlar rz
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = rz.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE rz.KullaniciID = @KullaniciID AND rz.Durum = N'Kiralama Istegi'
            ORDER BY rz.RezervasyonTarihi DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void CancelRentalRequest(int kullaniciId, int rezervasyonId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var getCopy = new SqlCommand(
            """
            SELECT KopyaID
            FROM dbo.Rezervasyonlar
            WHERE RezervasyonID = @RezervasyonID
                AND KullaniciID = @KullaniciID
                AND Durum = N'Kiralama Istegi';
            """,
            connection,
            transaction);
        getCopy.Parameters.AddWithValue("@RezervasyonID", rezervasyonId);
        getCopy.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        var copyId = getCopy.ExecuteScalar();

        if (copyId is null)
        {
            transaction.Rollback();
            throw new InvalidOperationException("İptal edilebilecek aktif kiralama isteği bulunamadı.");
        }

        using var updateRequest = new SqlCommand(
            "UPDATE dbo.Rezervasyonlar SET Durum = N'Iptal' WHERE RezervasyonID = @RezervasyonID;",
            connection,
            transaction);
        updateRequest.Parameters.AddWithValue("@RezervasyonID", rezervasyonId);
        updateRequest.ExecuteNonQuery();

        using var updateCopy = new SqlCommand(
            "UPDATE dbo.KitapKopyalari SET Durum = N'Musait' WHERE KopyaID = @KopyaID AND Durum = N'Rezerve';",
            connection,
            transaction);
        updateCopy.Parameters.AddWithValue("@KopyaID", Convert.ToInt32(copyId));
        updateCopy.ExecuteNonQuery();

        transaction.Commit();
    }

    public static DataTable GetRentalRequests()
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                rz.RezervasyonID,
                k.KitapAdi AS [Kitap Adı],
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                ku.Telefon AS [Telefon],
                rz.RezervasyonTarihi AS [İstek Tarihi],
                rz.SonGecerlilikTarihi AS [Son Geçerlilik],
                rz.Durum AS [Durum]
            FROM dbo.Rezervasyonlar rz
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = rz.KullaniciID
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = rz.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE rz.Durum = N'Kiralama Istegi'
            ORDER BY rz.RezervasyonTarihi;
            """,
            connection);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetRentalManagementRows(string filter)
    {
        UpdateOverdueRentals();
        var filterClause = filter switch
        {
            "Aktif Kiralamalar" => "Durum = N'Aktif'",
            "Gecikmişler" => "Durum = N'Gecikmiş'",
            "Teslim Edilenler" => "Durum = N'Teslim Edildi'",
            "Geç Teslim Edilenler" => "Durum = N'Geç Teslim Edildi'",
            "Kayıp Kitaplar" => "Durum = N'Kayıp'",
            "İptal Edilenler" => "Durum = N'İptal Edildi'",
            _ => "1 = 1"
        };
        return FillTable(string.Format(
            """
            WITH RentalRows AS
            (
                SELECT
                    kr.KiraID AS [Kira ID],
                    k.KitapAdi AS [Kitap Adı],
                    ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                    ku.Telefon AS [Telefon Numarası],
                    kr.AlisTarihi AS [Kira Tarihi],
                    kr.IadeTarihi AS [Son Teslim Tarihi],
                    kr.TeslimTarihi AS [Teslim Tarihi],
                    Durum = CASE
                        WHEN kr.Durum = N'İptal Edildi' THEN N'İptal Edildi'
                        WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN N'Kayıp'
                        WHEN kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE() THEN N'Gecikmiş'
                        WHEN kr.TeslimTarihi IS NULL THEN N'Aktif'
                        WHEN kr.Durum = N'Geç Teslim Edildi' OR kr.TeslimTarihi > kr.IadeTarihi THEN N'Geç Teslim Edildi'
                        ELSE N'Teslim Edildi'
                    END,
                    ku.MevcutPuan AS [Kullanıcı Puanı],
                    [Puan Etkisi] = CASE
                        WHEN kr.KazanilanPuan - kr.CezaPuani > 0 THEN CONCAT(N'+', kr.KazanilanPuan - kr.CezaPuani)
                        WHEN kr.KazanilanPuan - kr.CezaPuani < 0 THEN CONVERT(nvarchar(20), kr.KazanilanPuan - kr.CezaPuani)
                        ELSE N'0'
                    END,
                    [Ödeme Tutarı] = CASE
                        WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN COALESCE(kayip.KitapBedeli, NULLIF(kr.OdemeTutari, 0), 0)
                        ELSE 0
                    END,
                    [Ödeme Durumu] = CASE
                        WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN COALESCE(kayip.OdemeDurumu, kr.OdemeDurumu, N'Bekliyor')
                        ELSE N'Yok'
                    END,
                    [Açıklama] = COALESCE(NULLIF(kr.CezaAciklama, N''), CASE
                        WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN N'Kitap kayıp olarak işaretlendi. Kullanıcıdan kitap bedeli tahsil edilecek.'
                        WHEN kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE() THEN N'Teslim tarihi geçti. Teslim alındığında ceza puanı uygulanacak.'
                        WHEN kr.TeslimTarihi IS NULL THEN N'Aktif kirada: henüz puan değişimi yok.'
                        WHEN kr.TeslimTarihi > kr.IadeTarihi THEN N'Geç teslim edildi: ceza puanı uygulandı.'
                        ELSE N'Zamanında teslim edildi: ödül puanı eklendi.'
                    END)
                FROM dbo.Kiralamalar kr
                INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
                INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
                LEFT JOIN dbo.KayipKitaplar kayip ON kayip.KiralamaId = kr.KiraID
            )
            SELECT *
            FROM RentalRows
            WHERE
                {0}
            ORDER BY [Kira Tarihi] DESC;
            """,
            filterClause));
    }

    public static Dictionary<string, int> GetRentalDashboardCounts()
    {
        UpdateOverdueRentals();
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                ToplamKitap = (SELECT COUNT(*) FROM dbo.KitapKopyalari),
                AktifKiradakiKitap = (SELECT COUNT(*) FROM dbo.Kiralamalar WHERE TeslimTarihi IS NULL),
                MusaitKitap = (SELECT COUNT(*) FROM dbo.KitapKopyalari WHERE Durum = N'Musait'),
                SuresiGecmisKiralama = (SELECT COUNT(*) FROM dbo.Kiralamalar WHERE TeslimTarihi IS NULL AND IadeTarihi < GETDATE()),
                KayipKitap = (SELECT COUNT(*) FROM dbo.KayipKitaplar),
                AktifKiraciSayisi = (SELECT COUNT(DISTINCT KullaniciID) FROM dbo.Kiralamalar WHERE TeslimTarihi IS NULL);
            """,
            connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        reader.Read();
        return new Dictionary<string, int>
        {
            ["Toplam Kitap"] = reader.GetInt32(0),
            ["Aktif Kiradaki Kitap"] = reader.GetInt32(1),
            ["Müsait Kitap"] = reader.GetInt32(2),
            ["Süresi Geçmiş Kiralama"] = reader.GetInt32(3),
            ["Kayıp Kitap"] = reader.GetInt32(4),
            ["Aktif Kiracı Sayısı"] = reader.GetInt32(5)
        };
    }

    public static Dictionary<string, string> GetMainDashboardCards()
    {
        UpdateOverdueRentals();
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                ToplamKitap = (SELECT COUNT(*) FROM dbo.KitapKopyalari),
                MusaitKitap = (SELECT COUNT(*) FROM dbo.KitapKopyalari WHERE Durum = N'Musait'),
                KiradaOlanKitap = (SELECT COUNT(*) FROM dbo.KitapKopyalari WHERE Durum = N'Kirada'),
                GecikmisKiralama = (SELECT COUNT(*) FROM dbo.Kiralamalar WHERE TeslimTarihi IS NULL AND IadeTarihi < GETDATE()),
                ToplamKullanici = (SELECT COUNT(*) FROM dbo.Kullanicilar WHERE Durum = N'Onaylandi'),
                AktifKiraci = (SELECT COUNT(DISTINCT KullaniciID) FROM dbo.Kiralamalar WHERE TeslimTarihi IS NULL),
                KayipKitapSayisi = (SELECT COUNT(*) FROM dbo.KayipKitaplar),
                BekleyenOdemeTutari = (
                    SELECT ISNULL(SUM(KitapBedeli), 0)
                    FROM dbo.KayipKitaplar
                    WHERE OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal')
                );
            """,
            connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        reader.Read();
        return new Dictionary<string, string>
        {
            ["Toplam Kitap"] = reader.GetInt32(0).ToString(),
            ["Müsait Kitap"] = reader.GetInt32(1).ToString(),
            ["Kirada Olan Kitap"] = reader.GetInt32(2).ToString(),
            ["Gecikmiş Kiralama"] = reader.GetInt32(3).ToString(),
            ["Toplam Kullanıcı"] = reader.GetInt32(4).ToString(),
            ["Aktif Kiracı"] = reader.GetInt32(5).ToString(),
            ["Kayıp Kitap Sayısı"] = reader.GetInt32(6).ToString(),
            ["Bekleyen Ödeme"] = $"{reader.GetDecimal(7):0.00} TL"
        };
    }

    public static DataTable GetUpcomingDueRows()
    {
        UpdateOverdueRentals();
        return FillTable(
            """
            SELECT TOP 10
                k.KitapAdi AS [Kitap Adı],
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                kr.IadeTarihi AS [Son Teslim Tarihi],
                CASE
                    WHEN kr.IadeTarihi < GETDATE() THEN N'Gecikmiş'
                    ELSE N'Yakında Teslim'
                END AS [Durum]
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.TeslimTarihi IS NULL AND kr.Durum <> N'Kayıp'
            ORDER BY kr.IadeTarihi;
            """);
    }

    public static DataTable GetDashboardOverdueRows()
    {
        UpdateOverdueRentals();
        return FillTable(
            """
            SELECT TOP 10
                k.KitapAdi AS [Kitap Adı],
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                DATEDIFF(day, kr.IadeTarihi, GETDATE()) AS [Kaç Gün Gecikti],
                -CONVERT(int, ISNULL((SELECT AyarDegeri FROM dbo.SistemAyarlari WHERE AyarAdi = N'GecikmeCezaPuani'), 0)) AS [Puan Cezası]
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE()
            ORDER BY kr.IadeTarihi;
            """);
    }

    public static DataTable GetActiveRentalRows()
    {
        UpdateOverdueRentals();
        return FillTable(
            """
            SELECT
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                ku.Telefon AS [Telefon Numarası],
                k.KitapAdi AS [Kitap Adı],
                kr.AlisTarihi AS [Kiralama Tarihi],
                kr.IadeTarihi AS [Son Teslim Tarihi],
                CASE
                    WHEN kr.IadeTarihi < GETDATE() THEN N'Süresi Geçmiş'
                    ELSE N'Kirada'
                END AS [Kitap Durumu],
                CASE
                    WHEN kr.IadeTarihi < GETDATE() THEN CONCAT(DATEDIFF(day, kr.IadeTarihi, GETDATE()), N' gün gecikmiş')
                    ELSE CONCAT(DATEDIFF(day, GETDATE(), kr.IadeTarihi), N' gün kaldı')
                END AS [Kalan Gün],
                kr.KiraID
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.TeslimTarihi IS NULL
            ORDER BY kr.IadeTarihi;
            """);
    }

    public static DataTable GetAvailableBookRows()
    {
        return FillTable(
            """
            SELECT
                k.KitapAdi AS [Kitap Adı],
                Yazar = ISNULL((
                    SELECT STRING_AGG(y.YazarAdi, ', ')
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID
                ), N''),
                Tür = ISNULL((
                    SELECT STRING_AGG(t.TurAdi, ', ')
                    FROM dbo.KitapTurler kt
                    INNER JOIN dbo.Turler t ON t.TurID = kt.TurID
                    WHERE kt.KitapID = k.KitapID
                ), N''),
                [Kitap Durumu] = N'Müsait'
            FROM dbo.KitapKopyalari kk
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kk.Durum = N'Musait'
            ORDER BY k.KitapAdi;
            """);
    }

    public static DataTable GetOverdueRentalRows()
    {
        UpdateOverdueRentals();
        return FillTable(
            """
            SELECT
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                ku.Telefon AS [Telefon Numarası],
                k.KitapAdi AS [Kitap Adı],
                kr.IadeTarihi AS [Son Teslim Tarihi],
                N'Süresi Geçmiş' AS [Kitap Durumu],
                DATEDIFF(day, kr.IadeTarihi, GETDATE()) AS [Gecikme Gün Sayısı],
                ku.MevcutPuan AS [Mevcut Puan],
                -CASE WHEN kr.CezaPuani > 0 THEN kr.CezaPuani ELSE CONVERT(int, ISNULL((SELECT AyarDegeri FROM dbo.SistemAyarlari WHERE AyarAdi = N'GecikmeCezaPuani'), 0)) END AS [Ceza Puanı],
                kr.KiraID
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE()
            ORDER BY kr.IadeTarihi;
            """);
    }

    public static DataTable GetLostBookRows()
    {
        return FillTable(
            """
            SELECT
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                k.KitapAdi AS [Kitap Adı],
                N'Kayıp' AS [Kitap Durumu],
                kayip.KitapBedeli AS [Kitap Bedeli],
                -kayip.CezaPuani AS [Ceza Puanı],
                kayip.OdemeDurumu AS [Ödeme Durumu],
                kayip.BildirimTarihi AS [Kayıp Bildirim Tarihi],
                kayip.KiralamaId AS [KiraID]
            FROM dbo.KayipKitaplar kayip
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kayip.KullaniciId
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kayip.KitapId
            ORDER BY kayip.BildirimTarihi DESC;
            """);
    }

    public static void ReturnRental(int kiraId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var onTimePoints = GetSetting(connection, transaction, "ZamanindaTeslimPuani");
            var latePenaltyPoints = GetSetting(connection, transaction, "GecikmeCezaPuani");

            using var selectCommand = new SqlCommand(
                """
                SELECT kr.KullaniciID, kr.KopyaID, kr.IadeTarihi, kk.KitapID, kr.KazanilanPuan, kr.CezaPuani
                FROM dbo.Kiralamalar kr
                INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                WHERE kr.KiraID = @KiraID AND kr.TeslimTarihi IS NULL;
                """,
                connection,
                transaction);
            selectCommand.Parameters.AddWithValue("@KiraID", kiraId);
            using var reader = selectCommand.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Teslim edilecek aktif kiralama bulunamadi.");
            }

            var kullaniciId = reader.GetInt32(0);
            var kopyaId = reader.GetInt32(1);
            var dueDate = reader.GetDateTime(2);
            var kitapId = reader.GetInt32(3);
            var existingEarnedPoints = reader.GetInt32(4);
            var existingPenaltyPoints = reader.GetInt32(5);
            reader.Close();

            var lateDays = Math.Max(0, (DateTime.Today - dueDate.Date).Days);
            var earnedPoints = lateDays == 0 ? Math.Max(existingEarnedPoints, Convert.ToInt32(onTimePoints)) : existingEarnedPoints;
            var penaltyPoints = lateDays > 0 ? Math.Max(existingPenaltyPoints, Convert.ToInt32(latePenaltyPoints)) : existingPenaltyPoints;
            var newEarnedPoints = Math.Max(0, earnedPoints - existingEarnedPoints);
            var newPenaltyPoints = Math.Max(0, penaltyPoints - existingPenaltyPoints);
            var status = lateDays > 0 ? "Geç Teslim Edildi" : "Teslim Edildi";
            var explanation = lateDays > 0
                ? "Geç teslim edildi: ceza puanı uygulandı."
                : "Zamanında teslim edildi: ödül puanı eklendi.";

            using var updateRental = new SqlCommand(
                """
                UPDATE dbo.Kiralamalar
                SET TeslimTarihi = GETDATE(),
                    Durum = @Durum,
                    KazanilanPuan = @KazanilanPuan,
                    CezaPuani = @CezaPuani,
                    CezaTutari = @CezaTutari,
                    OdemeTutari = @OdemeTutari,
                    OdemeDurumu = @OdemeDurumu,
                    CezaAciklama = @CezaAciklama
                WHERE KiraID = @KiraID;
                """,
                connection,
                transaction);
            updateRental.Parameters.AddWithValue("@Durum", status);
            updateRental.Parameters.AddWithValue("@KazanilanPuan", earnedPoints);
            updateRental.Parameters.AddWithValue("@CezaPuani", penaltyPoints);
            updateRental.Parameters.AddWithValue("@CezaTutari", 0);
            updateRental.Parameters.AddWithValue("@OdemeTutari", 0);
            updateRental.Parameters.AddWithValue("@OdemeDurumu", "Yok");
            updateRental.Parameters.AddWithValue("@CezaAciklama", explanation);
            updateRental.Parameters.AddWithValue("@KiraID", kiraId);
            updateRental.ExecuteNonQuery();

            using var updateCopy = new SqlCommand("UPDATE dbo.KitapKopyalari SET Durum = N'Musait' WHERE KopyaID = @KopyaID;", connection, transaction);
            updateCopy.Parameters.AddWithValue("@KopyaID", kopyaId);
            updateCopy.ExecuteNonQuery();

            using var updateUser = new SqlCommand("UPDATE dbo.Kullanicilar SET MevcutPuan = MevcutPuan + @Delta, Skor = Skor + @Delta WHERE KullaniciID = @KullaniciID;", connection, transaction);
            updateUser.Parameters.AddWithValue("@Delta", newEarnedPoints - newPenaltyPoints);
            updateUser.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            updateUser.ExecuteNonQuery();
            if (newEarnedPoints > 0 || newPenaltyPoints > 0)
            {
                AddPointMovement(connection, transaction, kullaniciId, kitapId, lateDays == 0 ? "Kitap Zamanında Teslim Edildi" : "Kitap Geç Teslim Edildi", newEarnedPoints, newPenaltyPoints);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static UserProfileData GetUserProfile(int kullaniciId)
    {
        UpdateOverdueRentals();
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();

        var profile = new UserProfileData();
        using (var command = new SqlCommand(
            "SELECT Ad, Soyad, Telefon, MevcutPuan, KayitTarihi FROM dbo.Kullanicilar WHERE KullaniciID = @KullaniciID;",
            connection))
        {
            command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Kullanici bulunamadi.");
            }

            profile.Ad = reader.GetString(0).Trim();
            profile.Soyad = reader.GetString(1).Trim();
            profile.Telefon = reader.GetString(2).Trim();
            profile.MevcutPuan = reader.GetInt32(3);
            profile.KayitTarihi = reader.GetDateTime(4);
        }

        profile.ActiveRentals = GetUserRentalTable(connection, kullaniciId, true);
        profile.PastRentals = GetUserRentalTable(connection, kullaniciId, false);
        profile.OverdueRentals = GetUserOverdueTable(connection, kullaniciId);
        profile.PointMovements = GetUserPointMovements(connection, kullaniciId);
        profile.LostBooks = GetUserLostBooks(connection, kullaniciId);
        profile.ActiveRentCount = profile.ActiveRentals.Rows.Count;
        profile.TotalRentCount = profile.PastRentals.Rows.Count;
        profile.TotalEarnedPoints = profile.PointMovements.AsEnumerable().Sum(row => Convert.ToInt32(row["Kazanılan Puan"]));
        profile.TotalPenaltyPoints = profile.PointMovements.AsEnumerable().Sum(row => Convert.ToInt32(row["Kaybedilen Puan"]));
        profile.LateReturnCount = profile.PastRentals.AsEnumerable().Count(row => Convert.ToString(row["Sonuç"]) == "Geç Teslim");
        profile.LostBookCount = profile.LostBooks.Rows.Count;
        profile.TotalLostBookDebt = profile.LostBooks.AsEnumerable()
            .Where(row => !string.Equals(Convert.ToString(row["Ödeme Durumu"]), "Ödendi", StringComparison.OrdinalIgnoreCase))
            .Sum(row => Convert.ToDecimal(row["Kitap Bedeli"]));
        return profile;
    }

    public static DataTable GetUserActiveRentalCards(int kullaniciId)
    {
        UpdateOverdueRentals();
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                kr.KiraID,
                kk.KitapID,
                k.KitapAdi AS [Kitap Adı],
                Yazarlar = ISNULL((
                    SELECT STRING_AGG(y.YazarAdi, N', ')
                    FROM dbo.KitapYazarlar ky
                    INNER JOIN dbo.Yazarlar y ON y.YazarID = ky.YazarID
                    WHERE ky.KitapID = k.KitapID
                ), N''),
                kr.AlisTarihi AS [Kira Tarihi],
                kr.IadeTarihi AS [Son Teslim Tarihi],
                [Kalan Gün] = DATEDIFF(day, CONVERT(date, GETDATE()), CONVERT(date, kr.IadeTarihi)),
                [Durum] = CASE
                    WHEN kr.IadeTarihi < GETDATE() THEN N'Gecikmiş'
                    ELSE N'Aktif'
                END,
                [Kazanılacak Puan] = CASE
                    WHEN kr.IadeTarihi >= GETDATE() THEN CONVERT(int, ISNULL((SELECT AyarDegeri FROM dbo.SistemAyarlari WHERE AyarAdi = N'ZamanindaTeslimPuani'), 0))
                    ELSE 0
                END,
                [Gecikme Ceza Puanı] = CASE
                    WHEN kr.CezaPuani > 0 THEN kr.CezaPuani
                    ELSE CONVERT(int, ISNULL((SELECT AyarDegeri FROM dbo.SistemAyarlari WHERE AyarAdi = N'GecikmeCezaPuani'), 0))
                END,
                [Açıklama] = COALESCE(NULLIF(kr.CezaAciklama, N''), N'Aktif kirada: henüz puan değişimi yok.')
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.KullaniciID = @KullaniciID
                AND kr.TeslimTarihi IS NULL
                AND kr.Durum NOT IN (N'Kayıp', N'İptal Edildi')
            ORDER BY kr.IadeTarihi;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void MarkRentalAsLost(int kiraId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var lostPenaltyPoints = Convert.ToInt32(GetSetting(connection, transaction, "KayipKitapCezaPuani"));
            var defaultBookFee = GetSetting(connection, transaction, "VarsayilanKitapBedeli");
            using var selectCommand = new SqlCommand(
                """
                SELECT kr.KullaniciID, kr.KopyaID, kk.KitapID, ISNULL(NULLIF(k.KitapBedeli, 0), @DefaultBookFee)
                FROM dbo.Kiralamalar kr
                INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
                WHERE kr.KiraID = @KiraID AND kr.TeslimTarihi IS NULL;
                """,
                connection,
                transaction);
            selectCommand.Parameters.AddWithValue("@KiraID", kiraId);
            selectCommand.Parameters.AddWithValue("@DefaultBookFee", defaultBookFee);
            using var reader = selectCommand.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Kayıp işaretlenecek aktif kiralama bulunamadı.");
            }

            var kullaniciId = reader.GetInt32(0);
            var kopyaId = reader.GetInt32(1);
            var kitapId = reader.GetInt32(2);
            var bookFee = reader.GetDecimal(3);
            reader.Close();

            using var insertLost = new SqlCommand(
                """
                INSERT INTO dbo.KayipKitaplar (KullaniciId, KitapId, KiralamaId, BildirimTarihi, KitapBedeli, CezaPuani, OdemeDurumu)
                VALUES (@KullaniciId, @KitapId, @KiralamaId, GETDATE(), @KitapBedeli, @CezaPuani, N'Bekliyor');
                """,
                connection,
                transaction);
            insertLost.Parameters.AddWithValue("@KullaniciId", kullaniciId);
            insertLost.Parameters.AddWithValue("@KitapId", kitapId);
            insertLost.Parameters.AddWithValue("@KiralamaId", kiraId);
            insertLost.Parameters.AddWithValue("@KitapBedeli", bookFee);
            insertLost.Parameters.AddWithValue("@CezaPuani", lostPenaltyPoints);
            insertLost.ExecuteNonQuery();

            using var updateRental = new SqlCommand(
                """
                UPDATE dbo.Kiralamalar
                SET Durum = N'Kayıp',
                    TeslimTarihi = GETDATE(),
                    CezaPuani = @CezaPuani,
                    CezaTutari = 0,
                    OdemeTutari = @OdemeTutari,
                    OdemeDurumu = N'Bekliyor',
                    CezaAciklama = N'Kitap kayıp olarak işaretlendi. Kullanıcıdan kitap bedeli tahsil edilecek.'
                WHERE KiraID = @KiraID;
                """,
                connection,
                transaction);
            updateRental.Parameters.AddWithValue("@CezaPuani", lostPenaltyPoints);
            updateRental.Parameters.AddWithValue("@OdemeTutari", bookFee);
            updateRental.Parameters.AddWithValue("@KiraID", kiraId);
            updateRental.ExecuteNonQuery();

            using var updateCopy = new SqlCommand("UPDATE dbo.KitapKopyalari SET Durum = N'Kayıp' WHERE KopyaID = @KopyaID;", connection, transaction);
            updateCopy.Parameters.AddWithValue("@KopyaID", kopyaId);
            updateCopy.ExecuteNonQuery();

            using var updateUser = new SqlCommand("UPDATE dbo.Kullanicilar SET MevcutPuan = MevcutPuan - @CezaPuani, Skor = Skor - @CezaPuani WHERE KullaniciID = @KullaniciID;", connection, transaction);
            updateUser.Parameters.AddWithValue("@CezaPuani", lostPenaltyPoints);
            updateUser.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            updateUser.ExecuteNonQuery();

            AddPointMovement(connection, transaction, kullaniciId, kitapId, "Kitap Kaybedildi", 0, lostPenaltyPoints);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static DataTable GetPendingLostBookPayments()
    {
        return FillTable(
            """
            SELECT
                kayip.Id AS [Ödeme ID],
                ku.Ad + N' ' + ku.Soyad AS [Kullanıcı],
                ku.Telefon AS [Telefon],
                k.KitapAdi AS [Kitap Adı],
                kayip.KitapBedeli AS [Ödeme Tutarı],
                kayip.CezaPuani AS [Ceza Puanı],
                kayip.OdemeDurumu AS [Ödeme Durumu],
                kayip.BildirimTarihi AS [Bildirim Tarihi],
                kayip.OdemeTarihi AS [Ödeme Tarihi],
                kayip.KiralamaId AS [Kira ID]
            FROM dbo.KayipKitaplar kayip
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kayip.KullaniciId
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kayip.KitapId
            WHERE kayip.OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal')
            ORDER BY kayip.BildirimTarihi DESC;
            """);
    }

    public static void ReceiveLostBookPayment(int kayipKitapId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var selectCommand = new SqlCommand(
                """
                SELECT KiralamaId
                FROM dbo.KayipKitaplar
                WHERE Id = @Id AND OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal');
                """,
                connection,
                transaction);
            selectCommand.Parameters.AddWithValue("@Id", kayipKitapId);
            var kiraIdValue = selectCommand.ExecuteScalar();
            if (kiraIdValue is null || kiraIdValue == DBNull.Value)
            {
                throw new InvalidOperationException("Ödeme alınacak bekleyen kayıp kitap kaydı bulunamadı.");
            }

            var kiraId = Convert.ToInt32(kiraIdValue);

            using var updateLost = new SqlCommand(
                """
                UPDATE dbo.KayipKitaplar
                SET OdemeDurumu = N'Ödendi',
                    OdemeTarihi = GETDATE()
                WHERE Id = @Id;
                """,
                connection,
                transaction);
            updateLost.Parameters.AddWithValue("@Id", kayipKitapId);
            updateLost.ExecuteNonQuery();

            using var updateRental = new SqlCommand(
                """
                UPDATE dbo.Kiralamalar
                SET OdemeDurumu = N'Ödendi',
                    CezaAciklama = N'Kayıp kitap ödemesi alındı.'
                WHERE KiraID = @KiraID;
                """,
                connection,
                transaction);
            updateRental.Parameters.AddWithValue("@KiraID", kiraId);
            updateRental.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static void ApproveRentalRequest(int rezervasyonId, int personelId)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var selectCommand = new SqlCommand(
                "SELECT KullaniciID, KopyaID FROM dbo.Rezervasyonlar WHERE RezervasyonID = @RezervasyonID AND Durum = N'Kiralama Istegi';",
                connection,
                transaction);
            selectCommand.Parameters.AddWithValue("@RezervasyonID", rezervasyonId);
            using var reader = selectCommand.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Onaylanacak kiralama istegi bulunamadi.");
            }

            var kullaniciId = reader.GetInt32(0);
            var kopyaId = reader.GetInt32(1);
            reader.Close();

            using var insertCommand = new SqlCommand(
                "INSERT INTO dbo.Kiralamalar (KullaniciID, KopyaID, AlisTarihi, IadeTarihi, TeslimTarihi, Durum, PersonelID, OdemeTutari, OdemeDurumu, CezaAciklama) VALUES (@KullaniciID, @KopyaID, GETDATE(), DATEADD(day, 15, GETDATE()), NULL, N'Aktif', @PersonelID, 0, N'Yok', N'Aktif kirada: henüz puan değişimi yok.');",
                connection,
                transaction);
            insertCommand.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            insertCommand.Parameters.AddWithValue("@KopyaID", kopyaId);
            insertCommand.Parameters.AddWithValue("@PersonelID", personelId);
            insertCommand.ExecuteNonQuery();

            using var updateCopy = new SqlCommand("UPDATE dbo.KitapKopyalari SET Durum = N'Kirada' WHERE KopyaID = @KopyaID;", connection, transaction);
            updateCopy.Parameters.AddWithValue("@KopyaID", kopyaId);
            updateCopy.ExecuteNonQuery();

            using var updateRequest = new SqlCommand("UPDATE dbo.Rezervasyonlar SET Durum = N'Onaylandi', PersonelID = @PersonelID WHERE RezervasyonID = @RezervasyonID;", connection, transaction);
            updateRequest.Parameters.AddWithValue("@PersonelID", personelId);
            updateRequest.Parameters.AddWithValue("@RezervasyonID", rezervasyonId);
            updateRequest.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void UpdateOverdueRentals()
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            DECLARE @PenaltyPoints int = CONVERT(int, ISNULL((
                SELECT AyarDegeri
                FROM dbo.SistemAyarlari
                WHERE AyarAdi = N'GecikmeCezaPuani'
            ), 0));

            DECLARE @Applied table
            (
                KiraID int NOT NULL,
                KullaniciID int NOT NULL,
                KopyaID int NOT NULL,
                CezaPuani int NOT NULL
            );

            UPDATE dbo.Kiralamalar
            SET Durum = CASE
                    WHEN TeslimTarihi IS NULL AND IadeTarihi < GETDATE() THEN N'Gecikmiş'
                    WHEN TeslimTarihi IS NULL THEN N'Aktif'
                    ELSE Durum
                END,
                CezaPuani = CASE
                    WHEN TeslimTarihi IS NULL AND IadeTarihi < GETDATE() AND CezaPuani = 0 THEN @PenaltyPoints
                    ELSE CezaPuani
                END,
                CezaAciklama = CASE
                    WHEN TeslimTarihi IS NULL AND IadeTarihi < GETDATE() THEN N'Teslim tarihi geçti. Gecikme ceza puanı uygulandı.'
                    WHEN TeslimTarihi IS NULL AND (CezaAciklama IS NULL OR CezaAciklama = N'') THEN N'Aktif kirada: henüz puan değişimi yok.'
                    ELSE CezaAciklama
                END
            OUTPUT inserted.KiraID,
                   inserted.KullaniciID,
                   inserted.KopyaID,
                   CASE WHEN deleted.CezaPuani = 0 AND inserted.CezaPuani > 0 THEN inserted.CezaPuani ELSE 0 END
            INTO @Applied
            WHERE TeslimTarihi IS NULL AND Durum NOT IN (N'Kayıp', N'İptal Edildi');

            ;WITH UserPenalty AS
            (
                SELECT KullaniciID, SUM(CezaPuani) AS CezaPuani
                FROM @Applied
                WHERE CezaPuani > 0
                GROUP BY KullaniciID
            )
            UPDATE ku
            SET MevcutPuan = MevcutPuan - up.CezaPuani,
                Skor = Skor - up.CezaPuani
            FROM dbo.Kullanicilar ku
            INNER JOIN UserPenalty up ON up.KullaniciID = ku.KullaniciID;

            INSERT INTO dbo.PuanHareketleri (KullaniciId, KitapId, IslemTarihi, IslemTipi, KazanilanPuan, KaybedilenPuan, ToplamPuan)
            SELECT
                a.KullaniciID,
                kk.KitapID,
                GETDATE(),
                N'Gecikme Cezası Uygulandı',
                0,
                a.CezaPuani,
                ku.MevcutPuan
            FROM @Applied a
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = a.KopyaID
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = a.KullaniciID
            WHERE a.CezaPuani > 0;
            """,
            connection);
        connection.Open();
        command.ExecuteNonQuery();
    }

    private static decimal GetSetting(SqlConnection connection, SqlTransaction transaction, string settingName)
    {
        using var command = new SqlCommand("SELECT AyarDegeri FROM dbo.SistemAyarlari WHERE AyarAdi = @AyarAdi;", connection, transaction);
        command.Parameters.AddWithValue("@AyarAdi", settingName);
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private static DataTable FillTable(string sql)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static DataTable GetUserRentalTable(SqlConnection connection, int kullaniciId, bool active)
    {
        using var command = new SqlCommand(
            active
                ? """
                  SELECT
                      k.KitapAdi AS [Kitap Adı],
                      kr.AlisTarihi AS [Kira Tarihi],
                      kr.IadeTarihi AS [Son Teslim Tarihi],
                      kr.TeslimTarihi AS [Teslim Tarihi],
                      CASE WHEN kr.IadeTarihi < GETDATE() THEN N'Gecikmiş' ELSE N'Aktif' END AS [Durum],
                      CASE
                          WHEN kr.CezaPuani > 0 THEN CONVERT(nvarchar(20), -kr.CezaPuani)
                          ELSE N'0'
                      END AS [Puan Etkisi],
                      COALESCE(NULLIF(kr.CezaAciklama, N''), N'Aktif kirada: henüz puan değişimi yok.') AS [Açıklama]
                  FROM dbo.Kiralamalar kr
                  INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                  INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
                  WHERE kr.KullaniciID = @KullaniciID AND kr.TeslimTarihi IS NULL
                  ORDER BY kr.AlisTarihi DESC;
                  """
                : """
                  SELECT
                      k.KitapAdi AS [Kitap Adı],
                      kr.AlisTarihi AS [Kira Tarihi],
                      kr.IadeTarihi AS [Son Teslim Tarihi],
                      kr.TeslimTarihi AS [Teslim Tarihi],
                      kr.KazanilanPuan AS [Kazanılan Puan],
                      kr.CezaPuani AS [Ceza Puanı],
                      CASE
                          WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN N'Kayıp Kitap'
                          WHEN kr.TeslimTarihi > kr.IadeTarihi OR kr.Durum = N'Geç Teslim Edildi' THEN N'Geç Teslim'
                          ELSE N'Zamanında Teslim'
                      END AS [Sonuç],
                      [Puan Etkisi] = CASE
                          WHEN kr.KazanilanPuan - kr.CezaPuani > 0 THEN CONCAT(N'+', kr.KazanilanPuan - kr.CezaPuani)
                          WHEN kr.KazanilanPuan - kr.CezaPuani < 0 THEN CONVERT(nvarchar(20), kr.KazanilanPuan - kr.CezaPuani)
                          ELSE N'0'
                      END,
                      [Açıklama] = COALESCE(NULLIF(kr.CezaAciklama, N''), CASE
                          WHEN kr.Durum IN (N'Kayıp', N'Kayıp Kitap') THEN N'Kitabı kaybetti: ceza puanı aldı ve ödeme çıkarıldı.'
                          WHEN kr.TeslimTarihi > kr.IadeTarihi OR kr.Durum = N'Geç Teslim Edildi' THEN N'Geç teslim etti: ceza puanı aldı.'
                          ELSE N'Zamanında teslim etti: ödül puanı aldı.'
                      END)
                  FROM dbo.Kiralamalar kr
                  INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                  INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
                  WHERE kr.KullaniciID = @KullaniciID AND kr.TeslimTarihi IS NOT NULL
                  ORDER BY kr.TeslimTarihi DESC;
                  """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static DataTable GetUserOverdueTable(SqlConnection connection, int kullaniciId)
    {
        using var command = new SqlCommand(
            """
            SELECT
                k.KitapAdi AS [Gecikmis Kitap Adi],
                kr.IadeTarihi AS [Son Teslim Tarihi],
                GecikmeGunSayisi = CASE
                    WHEN kr.TeslimTarihi IS NULL THEN DATEDIFF(day, kr.IadeTarihi, GETDATE())
                    ELSE DATEDIFF(day, kr.IadeTarihi, kr.TeslimTarihi)
                END,
                kr.CezaPuani AS [Ceza Puani]
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapID
            WHERE kr.KullaniciID = @KullaniciID
                AND (
                    (kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE())
                    OR (kr.TeslimTarihi IS NOT NULL AND kr.TeslimTarihi > kr.IadeTarihi)
                )
            ORDER BY kr.IadeTarihi DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static DataTable GetUserPointMovements(SqlConnection connection, int kullaniciId)
    {
        using var command = new SqlCommand(
            """
            SELECT
                ph.IslemTarihi AS [İşlem Tarihi],
                ISNULL(k.KitapAdi, N'') AS [Kitap Adı],
                ph.IslemTipi AS [İşlem Türü],
                ph.KazanilanPuan AS [Kazanılan Puan],
                ph.KaybedilenPuan AS [Kaybedilen Puan],
                ph.ToplamPuan AS [İşlem Sonrası Toplam Puan]
            FROM dbo.PuanHareketleri ph
            LEFT JOIN dbo.Kitaplar k ON k.KitapID = ph.KitapId
            WHERE ph.KullaniciId = @KullaniciID
            ORDER BY ph.IslemTarihi DESC, ph.Id DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static DataTable GetUserLostBooks(SqlConnection connection, int kullaniciId)
    {
        using var command = new SqlCommand(
            """
            SELECT
                k.KitapAdi AS [Kitap Adı],
                kk.BildirimTarihi AS [Kayıp Bildirim Tarihi],
                kk.KitapBedeli AS [Kitap Bedeli],
                kk.CezaPuani AS [Uygulanan Ceza Puanı],
                kk.OdemeDurumu AS [Ödeme Durumu]
            FROM dbo.KayipKitaplar kk
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kk.KitapId
            WHERE kk.KullaniciId = @KullaniciID
            ORDER BY kk.BildirimTarihi DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    private static void AddPointMovement(SqlConnection connection, SqlTransaction transaction, int kullaniciId, int kitapId, string islemTipi, int kazanilanPuan, int kaybedilenPuan)
    {
        using var getTotal = new SqlCommand("SELECT MevcutPuan FROM dbo.Kullanicilar WHERE KullaniciID = @KullaniciID;", connection, transaction);
        getTotal.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        var total = Convert.ToInt32(getTotal.ExecuteScalar());

        using var command = new SqlCommand(
            """
            INSERT INTO dbo.PuanHareketleri (KullaniciId, KitapId, IslemTarihi, IslemTipi, KazanilanPuan, KaybedilenPuan, ToplamPuan)
            VALUES (@KullaniciId, @KitapId, GETDATE(), @IslemTipi, @KazanilanPuan, @KaybedilenPuan, @ToplamPuan);
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("@KullaniciId", kullaniciId);
        command.Parameters.AddWithValue("@KitapId", kitapId);
        command.Parameters.AddWithValue("@IslemTipi", islemTipi);
        command.Parameters.AddWithValue("@KazanilanPuan", kazanilanPuan);
        command.Parameters.AddWithValue("@KaybedilenPuan", kaybedilenPuan);
        command.Parameters.AddWithValue("@ToplamPuan", total);
        command.ExecuteNonQuery();
    }

    public static UserRewardSummary GetUserRewardSummary(int kullaniciId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                Score = k.Skor,
                TotalPenalty = (
                    SELECT ISNULL(SUM(KitapBedeli), 0)
                    FROM dbo.KayipKitaplar kayip
                    WHERE kayip.KullaniciId = k.KullaniciID
                ),
                UnpaidPenalty = (
                    SELECT ISNULL(SUM(KitapBedeli), 0)
                    FROM dbo.KayipKitaplar kayip
                    WHERE kayip.KullaniciId = k.KullaniciID
                        AND kayip.OdemeDurumu NOT IN (N'Ödendi', N'Iptal', N'İptal')
                ),
                ActiveRentCount = (
                    SELECT COUNT(*)
                    FROM dbo.Kiralamalar kr
                    WHERE kr.KullaniciID = k.KullaniciID AND kr.TeslimTarihi IS NULL
                ),
                ReservationCount = (
                    SELECT COUNT(*)
                    FROM dbo.Rezervasyonlar rz
                    WHERE rz.KullaniciID = k.KullaniciID AND rz.Durum <> N'Iptal'
                )
            FROM dbo.Kullanicilar k
            WHERE k.KullaniciID = @KullaniciID
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new UserRewardSummary();
        }

        return new UserRewardSummary
        {
            Score = reader.GetInt32(0),
            TotalPenalty = reader.GetDecimal(1),
            UnpaidPenalty = reader.GetDecimal(2),
            ActiveRentCount = reader.GetInt32(3),
            ReservationCount = reader.GetInt32(4)
        };
    }

    public static DataTable GetUserPenalties(int kullaniciId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                kayip.Id AS [Ödeme ID],
                k.KitapAdi AS [Kitap Adı],
                kayip.KitapBedeli AS [Ödeme Tutarı],
                kayip.CezaPuani AS [Ceza Puanı],
                kayip.OdemeDurumu AS [Ödeme Durumu],
                kayip.BildirimTarihi AS [Bildirim Tarihi],
                kayip.KiralamaId AS [Kira ID]
            FROM dbo.KayipKitaplar kayip
            INNER JOIN dbo.Kitaplar k ON k.KitapID = kayip.KitapId
            WHERE kayip.KullaniciId = @KullaniciID
            ORDER BY kayip.BildirimTarihi DESC;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static List<LookupItem> GetYayinEvleri()
    {
        return GetLookupItems("SELECT YayinEviID, YayinEviAdi FROM dbo.YayinEvleri ORDER BY YayinEviAdi;");
    }

    public static int GetOrCreateYayinEvi(string name)
    {
        return GetOrCreateLookup("dbo.YayinEvleri", "YayinEviID", "YayinEviAdi", name);
    }

    public static List<LookupItem> GetYazarlar()
    {
        return GetLookupItems("SELECT YazarID, YazarAdi FROM dbo.Yazarlar ORDER BY YazarAdi;");
    }

    public static int GetOrCreateYazar(string name)
    {
        return GetOrCreateLookup("dbo.Yazarlar", "YazarID", "YazarAdi", name);
    }

    public static List<LookupItem> GetTurler()
    {
        return GetLookupItems("SELECT TurID, TurAdi FROM dbo.Turler ORDER BY TurAdi;");
    }

    public static HashSet<int> GetBookYazarIds(int kitapId)
    {
        return GetIdSet("SELECT YazarID FROM dbo.KitapYazarlar WHERE KitapID = @KitapID;", kitapId);
    }

    public static HashSet<int> GetBookTurIds(int kitapId)
    {
        return GetIdSet("SELECT TurID FROM dbo.KitapTurler WHERE KitapID = @KitapID;", kitapId);
    }

    public static string GetBookDeleteBlockReason(int kitapId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT
                KopyaSayisi = (SELECT COUNT(*) FROM dbo.KitapKopyalari WHERE KitapID = @KitapID),
                KiralamaSayisi = (
                    SELECT COUNT(*)
                    FROM dbo.Kiralamalar kr
                    INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
                    WHERE kk.KitapID = @KitapID
                ),
                RezervasyonSayisi = (
                    SELECT COUNT(*)
                    FROM dbo.Rezervasyonlar rz
                    INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = rz.KopyaID
                    WHERE kk.KitapID = @KitapID
                );
            """,
            connection);
        command.Parameters.AddWithValue("@KitapID", kitapId);
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return string.Empty;
        }

        var copies = reader.GetInt32(0);
        var rents = reader.GetInt32(1);
        var reservations = reader.GetInt32(2);
        if (copies == 0 && rents == 0 && reservations == 0)
        {
            return string.Empty;
        }

        return $"Bu kitaba bagli {copies} kopya, {rents} kiralama ve {reservations} rezervasyon kaydi var. Iliskili hareket kaydi olan kitap fiziksel olarak silinemez.";
    }

    public static int AddKitap(
        string kitapAdi,
        string? orijinalAd,
        int? yayinEviId,
        short? ilkYayinYili,
        string dil,
        string? ozet,
        decimal kitapBedeli,
        IReadOnlyCollection<int> yazarIds,
        IReadOnlyCollection<int> turIds)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            using var command = new SqlCommand(
                """
                INSERT INTO dbo.Kitaplar (KitapAdi, OrijinalAd, YayinEviID, IlkYayinYili, Dil, Ozet, KitapBedeli)
                OUTPUT INSERTED.KitapID
                VALUES (@KitapAdi, @OrijinalAd, @YayinEviID, @IlkYayinYili, @Dil, @Ozet, @KitapBedeli);
                """,
                connection,
                transaction);
            AddBookParameters(command, kitapAdi, orijinalAd, yayinEviId, ilkYayinYili, dil, ozet, kitapBedeli);
            var kitapId = Convert.ToInt32(command.ExecuteScalar());

            SaveBookRelations(connection, transaction, kitapId, yazarIds, turIds);
            transaction.Commit();
            return kitapId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static void UpdateKitap(
        int kitapId,
        string kitapAdi,
        string? orijinalAd,
        int? yayinEviId,
        short? ilkYayinYili,
        string dil,
        string? ozet,
        decimal kitapBedeli,
        IReadOnlyCollection<int> yazarIds,
        IReadOnlyCollection<int> turIds)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            using var command = new SqlCommand(
                """
                UPDATE dbo.Kitaplar
                SET
                    KitapAdi = @KitapAdi,
                    OrijinalAd = @OrijinalAd,
                    YayinEviID = @YayinEviID,
                    IlkYayinYili = @IlkYayinYili,
                    Dil = @Dil,
                    Ozet = @Ozet,
                    KitapBedeli = @KitapBedeli
                WHERE KitapID = @KitapID;
                """,
                connection,
                transaction);
            command.Parameters.AddWithValue("@KitapID", kitapId);
            AddBookParameters(command, kitapAdi, orijinalAd, yayinEviId, ilkYayinYili, dil, ozet, kitapBedeli);
            command.ExecuteNonQuery();

            SaveBookRelations(connection, transaction, kitapId, yazarIds, turIds);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static void DeleteKitap(int kitapId)
    {
        var blockReason = GetBookDeleteBlockReason(kitapId);
        if (blockReason.Length > 0)
        {
            throw new InvalidOperationException(blockReason);
        }

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KitapYazarlar WHERE KitapID = @KitapID;", kitapId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KitapTurler WHERE KitapID = @KitapID;", kitapId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KitapBasimlari WHERE KitapID = @KitapID;", kitapId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.Kitaplar WHERE KitapID = @KitapID;", kitapId);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static AppUser? LoginWithAccount(UserRole role, string userName, string password)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT TOP 1 h.HesapID, h.KayitID, h.SifreHash, h.SifreSalt, h.Sifre,
                DisplayName = CASE
                    WHEN h.Rol = N'Admin' THEN N'Sistem Admin'
                    WHEN h.Rol = N'Personel' THEN p.Ad + N' ' + p.Soyad
                    ELSE k.Ad + N' ' + k.Soyad
                END
            FROM dbo.GirisHesaplari h
            LEFT JOIN dbo.Personel p ON h.Rol = N'Personel' AND p.PersonelID = h.KayitID
            LEFT JOIN dbo.Kullanicilar k ON h.Rol = N'Kullanici' AND k.KullaniciID = h.KayitID
            WHERE h.Rol = @Rol
                AND h.KullaniciAdi = @KullaniciAdi
                AND h.AktifMi = 1
                AND (h.Rol <> N'Kullanici' OR k.Durum = N'Onaylandi');
            """,
            connection);
        command.Parameters.AddWithValue("@Rol", role.ToString());
        command.Parameters.AddWithValue("@KullaniciAdi", userName);
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var accountId = reader.GetInt32(0);
        int? recordId = reader.IsDBNull(1) ? null : reader.GetInt32(1);
        var hash = reader.IsDBNull(2) ? null : (byte[])reader[2];
        var salt = reader.IsDBNull(3) ? null : (byte[])reader[3];
        var plainPassword = reader.IsDBNull(4) ? null : reader.GetString(4);
        var displayName = reader.GetString(5).Trim();
        reader.Close();

        if (hash is not null && salt is not null && PasswordHelper.VerifyPassword(password, hash, salt))
        {
            return new AppUser(role, recordId, displayName);
        }

        if (plainPassword is not null && plainPassword == password)
        {
            UpgradeAccountPassword(connection, accountId, password);
            return new AppUser(role, recordId, displayName);
        }

        return null;
    }

    private static void AddAccount(SqlConnection connection, SqlTransaction transaction, UserRole role, int? recordId, string userName, string password, bool active)
    {
        var (hash, salt) = PasswordHelper.HashPassword(password);
        using var command = new SqlCommand(
            "INSERT INTO dbo.GirisHesaplari (Rol, KayitID, KullaniciAdi, SifreHash, SifreSalt, AktifMi) VALUES (@Rol, @KayitID, @KullaniciAdi, @Hash, @Salt, @AktifMi);",
            connection,
            transaction);
        command.Parameters.AddWithValue("@Rol", role.ToString());
        command.Parameters.AddWithValue("@KayitID", (object?)recordId ?? DBNull.Value);
        command.Parameters.AddWithValue("@KullaniciAdi", userName);
        command.Parameters.AddWithValue("@Hash", hash);
        command.Parameters.AddWithValue("@Salt", salt);
        command.Parameters.AddWithValue("@AktifMi", active);
        command.ExecuteNonQuery();
    }

    private static void UpgradeAccountPassword(SqlConnection connection, int accountId, string password)
    {
        var (hash, salt) = PasswordHelper.HashPassword(password);
        using var command = new SqlCommand("UPDATE dbo.GirisHesaplari SET SifreHash = @Hash, SifreSalt = @Salt, Sifre = NULL WHERE HesapID = @HesapID;", connection);
        command.Parameters.AddWithValue("@Hash", hash);
        command.Parameters.AddWithValue("@Salt", salt);
        command.Parameters.AddWithValue("@HesapID", accountId);
        command.ExecuteNonQuery();
    }

    private static int GetMainKutuphaneId(SqlConnection connection, SqlTransaction transaction)
    {
        using var command = new SqlCommand("SELECT TOP 1 KutuphaneID FROM dbo.Kutuphaneler ORDER BY KutuphaneID;", connection, transaction);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void UpdateKullaniciStatus(int kullaniciId, string status, bool active)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var userCommand = new SqlCommand("UPDATE dbo.Kullanicilar SET Durum = @Durum WHERE KullaniciID = @KullaniciID;", connection, transaction);
            userCommand.Parameters.AddWithValue("@Durum", status);
            userCommand.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            userCommand.ExecuteNonQuery();

            using var accountCommand = new SqlCommand("UPDATE dbo.GirisHesaplari SET AktifMi = @AktifMi WHERE Rol = N'Kullanici' AND KayitID = @KullaniciID;", connection, transaction);
            accountCommand.Parameters.AddWithValue("@AktifMi", active);
            accountCommand.Parameters.AddWithValue("@KullaniciID", kullaniciId);
            accountCommand.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static AppUser? LoginPersonel(string phone, int personelId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT TOP 1 PersonelID, Ad, Soyad
            FROM dbo.Personel
            WHERE PersonelID = @PersonelID AND Telefon = @Telefon;
            """,
            connection);
        command.Parameters.AddWithValue("@PersonelID", personelId);
        command.Parameters.AddWithValue("@Telefon", phone);
        connection.Open();

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new AppUser(UserRole.Personel, reader.GetInt32(0), $"{reader.GetString(1).Trim()} {reader.GetString(2).Trim()}")
            : null;
    }

    private static AppUser? LoginKullanici(string email, int kullaniciId)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            """
            SELECT TOP 1 KullaniciID, Ad, Soyad
            FROM dbo.Kullanicilar
            WHERE KullaniciID = @KullaniciID AND Eposta = @Eposta;
            """,
            connection);
        command.Parameters.AddWithValue("@KullaniciID", kullaniciId);
        command.Parameters.AddWithValue("@Eposta", email);
        connection.Open();

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new AppUser(UserRole.Kullanici, reader.GetInt32(0), $"{reader.GetString(1).Trim()} {reader.GetString(2).Trim()}")
            : null;
    }

    private static List<LookupItem> GetLookupItems(string sql)
    {
        var items = new List<LookupItem>();
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new LookupItem(reader.GetInt32(0), reader.GetString(1)));
        }

        return items;
    }

    private static int GetOrCreateLookup(string tableName, string idColumn, string nameColumn, string name)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(
            $"""
            SELECT {idColumn} FROM {tableName} WHERE {nameColumn} = @Name;
            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO {tableName} ({nameColumn}) VALUES (@Name);
                SELECT CONVERT(int, SCOPE_IDENTITY());
            END
            """,
            connection);
        command.Parameters.AddWithValue("@Name", name.Trim());
        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static HashSet<int> GetIdSet(string sql, int kitapId)
    {
        var ids = new HashSet<int>();
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@KitapID", kitapId);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    private static void AddBookParameters(
        SqlCommand command,
        string kitapAdi,
        string? orijinalAd,
        int? yayinEviId,
        short? ilkYayinYili,
        string dil,
        string? ozet,
        decimal kitapBedeli)
    {
        command.Parameters.AddWithValue("@KitapAdi", kitapAdi);
        command.Parameters.AddWithValue("@OrijinalAd", (object?)orijinalAd ?? DBNull.Value);
        command.Parameters.AddWithValue("@YayinEviID", (object?)yayinEviId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IlkYayinYili", (object?)ilkYayinYili ?? DBNull.Value);
        command.Parameters.AddWithValue("@Dil", dil);
        command.Parameters.AddWithValue("@Ozet", (object?)ozet ?? DBNull.Value);
        command.Parameters.AddWithValue("@KitapBedeli", kitapBedeli);
    }

    private static void SaveBookRelations(
        SqlConnection connection,
        SqlTransaction transaction,
        int kitapId,
        IReadOnlyCollection<int> yazarIds,
        IReadOnlyCollection<int> turIds)
    {
        ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KitapYazarlar WHERE KitapID = @KitapID;", kitapId);
        ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.KitapTurler WHERE KitapID = @KitapID;", kitapId);

        foreach (var yazarId in yazarIds)
        {
            using var command = new SqlCommand(
                "INSERT INTO dbo.KitapYazarlar (KitapID, YazarID, YazarRol) VALUES (@KitapID, @YazarID, N'Yazar');",
                connection,
                transaction);
            command.Parameters.AddWithValue("@KitapID", kitapId);
            command.Parameters.AddWithValue("@YazarID", yazarId);
            command.ExecuteNonQuery();
        }

        foreach (var turId in turIds)
        {
            using var command = new SqlCommand(
                "INSERT INTO dbo.KitapTurler (KitapID, TurID) VALUES (@KitapID, @TurID);",
                connection,
                transaction);
            command.Parameters.AddWithValue("@KitapID", kitapId);
            command.Parameters.AddWithValue("@TurID", turId);
            command.ExecuteNonQuery();
        }
    }

    private static void ExecuteNonQuery(
        SqlConnection connection,
        SqlTransaction transaction,
        string sql,
        int kitapId)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@KitapID", kitapId);
        command.ExecuteNonQuery();
    }

    private static void ExecuteNonQuery(
        SqlConnection connection,
        SqlTransaction transaction,
        string sql,
        string parameterName,
        int value)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue(parameterName, value);
        command.ExecuteNonQuery();
    }
}


