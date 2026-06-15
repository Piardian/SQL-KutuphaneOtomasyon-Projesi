using Microsoft.Data.SqlClient;

namespace KutuphaneOtomasyon;

internal static class DatabaseMigrator
{
    public static void Initialize()
    {
        using var connection = new SqlConnection(DatabaseHelper.ConnectionString);
        connection.Open();
        Execute(connection, """
            IF OBJECT_ID(N'dbo.GirisHesaplari', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.GirisHesaplari
                (
                    HesapID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GirisHesaplari PRIMARY KEY,
                    Rol nvarchar(20) NOT NULL,
                    KayitID int NULL,
                    KullaniciAdi nvarchar(300) NOT NULL,
                    Sifre nvarchar(200) NULL,
                    AktifMi bit NOT NULL CONSTRAINT DF_GirisHesaplari_AktifMi DEFAULT(1),
                    OlusturmaTarihi datetime NOT NULL CONSTRAINT DF_GirisHesaplari_OlusturmaTarihi DEFAULT(GETDATE())
                );
            END;
            IF COL_LENGTH('dbo.GirisHesaplari', 'SifreHash') IS NULL
                ALTER TABLE dbo.GirisHesaplari ADD SifreHash varbinary(32) NULL;
            IF COL_LENGTH('dbo.GirisHesaplari', 'SifreSalt') IS NULL
                ALTER TABLE dbo.GirisHesaplari ADD SifreSalt varbinary(16) NULL;
            ALTER TABLE dbo.GirisHesaplari ALTER COLUMN Sifre nvarchar(200) NULL;
            IF COL_LENGTH('dbo.Kullanicilar', 'Durum') IS NULL
                ALTER TABLE dbo.Kullanicilar ADD Durum nvarchar(20) NOT NULL CONSTRAINT DF_Kullanicilar_Durum DEFAULT(N'Onaylandi');
            IF COL_LENGTH('dbo.Kullanicilar', 'KayitTarihi') IS NULL
                ALTER TABLE dbo.Kullanicilar ADD KayitTarihi datetime NOT NULL CONSTRAINT DF_Kullanicilar_KayitTarihi DEFAULT(GETDATE());
            IF COL_LENGTH('dbo.Kullanicilar', 'MevcutPuan') IS NULL
                ALTER TABLE dbo.Kullanicilar ADD MevcutPuan int NOT NULL CONSTRAINT DF_Kullanicilar_MevcutPuan DEFAULT(0);
            EXEC(N'UPDATE dbo.Kullanicilar SET MevcutPuan = Skor WHERE MevcutPuan = 0 AND Skor <> 0;');
            IF COL_LENGTH('dbo.Kiralamalar', 'KazanilanPuan') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD KazanilanPuan int NOT NULL CONSTRAINT DF_Kiralamalar_KazanilanPuan DEFAULT(0);
            IF COL_LENGTH('dbo.Kiralamalar', 'CezaPuani') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD CezaPuani int NOT NULL CONSTRAINT DF_Kiralamalar_CezaPuani DEFAULT(0);
            IF COL_LENGTH('dbo.Kiralamalar', 'CezaTutari') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD CezaTutari decimal(10,2) NOT NULL CONSTRAINT DF_Kiralamalar_CezaTutari DEFAULT(0);
            IF COL_LENGTH('dbo.Kitaplar', 'KitapBedeli') IS NULL
                ALTER TABLE dbo.Kitaplar ADD KitapBedeli decimal(10,2) NOT NULL CONSTRAINT DF_Kitaplar_KitapBedeli DEFAULT(250);
            IF COL_LENGTH('dbo.Kiralamalar', 'OdemeTutari') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD OdemeTutari decimal(10,2) NOT NULL CONSTRAINT DF_Kiralamalar_OdemeTutari DEFAULT(0);
            IF COL_LENGTH('dbo.Kiralamalar', 'OdemeDurumu') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD OdemeDurumu nvarchar(20) NOT NULL CONSTRAINT DF_Kiralamalar_OdemeDurumu DEFAULT(N'Yok');
            IF COL_LENGTH('dbo.Kiralamalar', 'CezaAciklama') IS NULL
                ALTER TABLE dbo.Kiralamalar ADD CezaAciklama nvarchar(400) NULL;
            IF OBJECT_ID(N'dbo.SistemAyarlari', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.SistemAyarlari
                (
                    AyarAdi nvarchar(100) NOT NULL CONSTRAINT PK_SistemAyarlari PRIMARY KEY,
                    AyarDegeri decimal(10,2) NOT NULL
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM dbo.SistemAyarlari WHERE AyarAdi = N'ZamanindaTeslimPuani')
                INSERT INTO dbo.SistemAyarlari (AyarAdi, AyarDegeri) VALUES (N'ZamanindaTeslimPuani', 10);
            IF NOT EXISTS (SELECT 1 FROM dbo.SistemAyarlari WHERE AyarAdi = N'GecikmeCezaPuani')
                INSERT INTO dbo.SistemAyarlari (AyarAdi, AyarDegeri) VALUES (N'GecikmeCezaPuani', 5);
            IF NOT EXISTS (SELECT 1 FROM dbo.SistemAyarlari WHERE AyarAdi = N'GunlukGecikmeUcreti')
                INSERT INTO dbo.SistemAyarlari (AyarAdi, AyarDegeri) VALUES (N'GunlukGecikmeUcreti', 2);
            IF NOT EXISTS (SELECT 1 FROM dbo.SistemAyarlari WHERE AyarAdi = N'KayipKitapCezaPuani')
                INSERT INTO dbo.SistemAyarlari (AyarAdi, AyarDegeri) VALUES (N'KayipKitapCezaPuani', 25);
            IF NOT EXISTS (SELECT 1 FROM dbo.SistemAyarlari WHERE AyarAdi = N'VarsayilanKitapBedeli')
                INSERT INTO dbo.SistemAyarlari (AyarAdi, AyarDegeri) VALUES (N'VarsayilanKitapBedeli', 250);
            IF OBJECT_ID(N'dbo.PuanHareketleri', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.PuanHareketleri
                (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PuanHareketleri PRIMARY KEY,
                    KullaniciId int NOT NULL,
                    KitapId int NULL,
                    IslemTarihi datetime NOT NULL CONSTRAINT DF_PuanHareketleri_IslemTarihi DEFAULT(GETDATE()),
                    IslemTipi nvarchar(100) NOT NULL,
                    KazanilanPuan int NOT NULL CONSTRAINT DF_PuanHareketleri_KazanilanPuan DEFAULT(0),
                    KaybedilenPuan int NOT NULL CONSTRAINT DF_PuanHareketleri_KaybedilenPuan DEFAULT(0),
                    ToplamPuan int NOT NULL,
                    CONSTRAINT FK_PuanHareketleri_Kullanicilar FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(KullaniciID),
                    CONSTRAINT FK_PuanHareketleri_Kitaplar FOREIGN KEY (KitapId) REFERENCES dbo.Kitaplar(KitapID)
                );
            END;
            IF OBJECT_ID(N'dbo.KayipKitaplar', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.KayipKitaplar
                (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_KayipKitaplar PRIMARY KEY,
                    KullaniciId int NOT NULL,
                    KitapId int NOT NULL,
                    KiralamaId int NULL,
                    BildirimTarihi datetime NOT NULL CONSTRAINT DF_KayipKitaplar_BildirimTarihi DEFAULT(GETDATE()),
                    KitapBedeli decimal(10,2) NOT NULL,
                    CezaPuani int NOT NULL,
                    OdemeDurumu nvarchar(30) NOT NULL CONSTRAINT DF_KayipKitaplar_OdemeDurumu DEFAULT(N'Bekliyor'),
                    CONSTRAINT FK_KayipKitaplar_Kullanicilar FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(KullaniciID),
                    CONSTRAINT FK_KayipKitaplar_Kitaplar FOREIGN KEY (KitapId) REFERENCES dbo.Kitaplar(KitapID)
                );
            END;
            IF OBJECT_ID(N'dbo.KayipKitaplar', N'U') IS NOT NULL AND COL_LENGTH('dbo.KayipKitaplar', 'OdemeTarihi') IS NULL
                ALTER TABLE dbo.KayipKitaplar ADD OdemeTarihi datetime NULL;
            INSERT INTO dbo.PuanHareketleri (KullaniciId, KitapId, IslemTarihi, IslemTipi, KazanilanPuan, KaybedilenPuan, ToplamPuan)
            SELECT
                kr.KullaniciID,
                kk.KitapID,
                ISNULL(kr.TeslimTarihi, GETDATE()),
                CASE
                    WHEN kr.Durum = N'Kayıp Kitap' THEN N'Kitap Kaybedildi'
                    WHEN kr.CezaPuani > 0 THEN N'Kitap Geç Teslim Edildi'
                    WHEN kr.KazanilanPuan > 0 THEN N'Kitap Zamanında Teslim Edildi'
                    ELSE N'Puan Hareketi'
                END,
                kr.KazanilanPuan,
                kr.CezaPuani,
                0
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.KitapKopyalari kk ON kk.KopyaID = kr.KopyaID
            WHERE (kr.KazanilanPuan <> 0 OR kr.CezaPuani <> 0)
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.PuanHareketleri ph
                    WHERE ph.KullaniciId = kr.KullaniciID
                        AND ph.KitapId = kk.KitapID
                        AND ph.IslemTarihi = ISNULL(kr.TeslimTarihi, ph.IslemTarihi)
                );
            ;WITH OrderedMovements AS
            (
                SELECT Id,
                       RunningTotal = SUM(KazanilanPuan - KaybedilenPuan) OVER (PARTITION BY KullaniciId ORDER BY IslemTarihi, Id)
                FROM dbo.PuanHareketleri
            )
            UPDATE ph SET ToplamPuan = om.RunningTotal
            FROM dbo.PuanHareketleri ph
            INNER JOIN OrderedMovements om ON om.Id = ph.Id;
            UPDATE k
            SET MevcutPuan = ISNULL(p.NetPuan, 0),
                Skor = ISNULL(p.NetPuan, 0)
            FROM dbo.Kullanicilar k
            OUTER APPLY (
                SELECT SUM(KazanilanPuan - KaybedilenPuan) AS NetPuan
                FROM dbo.PuanHareketleri ph
                WHERE ph.KullaniciId = k.KullaniciID
            ) p;
            UPDATE dbo.Kiralamalar
            SET Durum = CASE
                    WHEN Durum IN (N'Kirada', N'Aktif') AND TeslimTarihi IS NULL AND IadeTarihi >= GETDATE() THEN N'Aktif'
                    WHEN TeslimTarihi IS NULL AND IadeTarihi < GETDATE() AND Durum NOT IN (N'Kayıp', N'İptal Edildi') THEN N'Gecikmiş'
                    WHEN Durum IN (N'Süresi Geçmiş', N'Geç Teslim Edildi') AND TeslimTarihi IS NOT NULL THEN N'Geç Teslim Edildi'
                    WHEN Durum IN (N'Teslim', N'Müsait', N'Teslim Edildi') AND TeslimTarihi IS NOT NULL THEN N'Teslim Edildi'
                    WHEN Durum IN (N'Kayıp Kitap', N'Kayıp') THEN N'Kayıp'
                    ELSE Durum
                END
            WHERE Durum IN (N'Kirada', N'Aktif', N'Süresi Geçmiş', N'Teslim', N'Müsait', N'Teslim Edildi', N'Geç Teslim Edildi', N'Kayıp Kitap', N'Kayıp')
                OR (TeslimTarihi IS NULL AND IadeTarihi < GETDATE());
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_GirisHesaplari_KullaniciAdi' AND object_id = OBJECT_ID(N'dbo.GirisHesaplari'))
                CREATE UNIQUE INDEX UX_GirisHesaplari_KullaniciAdi ON dbo.GirisHesaplari(KullaniciAdi);
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Kullanicilar_Telefon' AND object_id = OBJECT_ID(N'dbo.Kullanicilar'))
                CREATE UNIQUE INDEX UX_Kullanicilar_Telefon ON dbo.Kullanicilar(Telefon) WHERE Telefon <> N'';
            UPDATE h
            SET KullaniciAdi = k.Telefon
            FROM dbo.GirisHesaplari h
            INNER JOIN dbo.Kullanicilar k ON k.KullaniciID = h.KayitID
            WHERE h.Rol = N'Kullanici'
                AND k.Telefon <> N''
                AND h.KullaniciAdi <> k.Telefon
                AND NOT EXISTS (
                    SELECT 1 FROM dbo.GirisHesaplari otherAccount
                    WHERE otherAccount.KullaniciAdi = k.Telefon AND otherAccount.HesapID <> h.HesapID
                );
            """);

        NormalizeRentalState(connection);
        EnsureAccount(connection, UserRole.Admin, null, "admin", "admin123", true);
        BackfillAccounts(connection);
        UpgradePlainPasswords(connection);
        SeedDemoData(connection);
        CleanupDemoIsbn(connection);
        NormalizeRentalState(connection);
        ApplyOverduePenalties(connection);
        BackfillAccounts(connection);
        UpgradePlainPasswords(connection);
    }

    private static void ApplyOverduePenalties(SqlConnection connection)
    {
        Execute(connection, """
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
            """);
    }

    private static void CleanupDemoIsbn(SqlConnection connection)
    {
        Execute(connection, """
            ;WITH DemoBasim AS
            (
                SELECT BasimID, KitapID
                FROM dbo.KitapBasimlari
                WHERE ISBN LIKE N'DEMO-%'
            ),
            GercekBasim AS
            (
                SELECT KitapID, BasimID = MIN(BasimID)
                FROM dbo.KitapBasimlari
                WHERE ISBN NOT LIKE N'DEMO-%'
                GROUP BY KitapID
            )
            UPDATE kk
            SET BasimID = gb.BasimID
            FROM dbo.KitapKopyalari kk
            INNER JOIN DemoBasim db ON db.BasimID = kk.BasimID
            INNER JOIN GercekBasim gb ON gb.KitapID = db.KitapID;

            DELETE db
            FROM dbo.KitapBasimlari db
            WHERE db.ISBN LIKE N'DEMO-%'
                AND EXISTS (
                    SELECT 1
                    FROM dbo.KitapBasimlari realBasim
                    WHERE realBasim.KitapID = db.KitapID
                        AND realBasim.ISBN NOT LIKE N'DEMO-%'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.KitapKopyalari kk
                    WHERE kk.BasimID = db.BasimID
                );
            """);
    }

    private static void NormalizeRentalState(SqlConnection connection)
    {
        Execute(connection, """
            UPDATE dbo.KayipKitaplar
            SET OdemeDurumu = N'Bekliyor'
            WHERE OdemeDurumu IN (N'Ödeme Bekleniyor', N'Beklemede');

            UPDATE kr
            SET OdemeDurumu = CASE WHEN kr.OdemeTutari > 0 AND kr.OdemeDurumu = N'Yok' THEN N'Bekliyor' ELSE kr.OdemeDurumu END,
                CezaAciklama = CASE
                    WHEN kr.CezaAciklama IS NOT NULL AND kr.CezaAciklama <> N'' THEN kr.CezaAciklama
                    WHEN kr.Durum = N'Kayıp' THEN N'Kitap kayıp olarak işaretlendi. Kullanıcıdan kitap bedeli tahsil edilecek.'
                    WHEN kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE() THEN N'Teslim tarihi geçti. Teslim alındığında ceza puanı uygulanacak.'
                    WHEN kr.TeslimTarihi IS NULL THEN N'Aktif kirada: henüz puan değişimi yok.'
                    WHEN kr.TeslimTarihi > kr.IadeTarihi THEN N'Geç teslim edildi: ceza puanı uygulandı.'
                    ELSE N'Zamanında teslim edildi: ödül puanı eklendi.'
                END
            FROM dbo.Kiralamalar kr;

            UPDATE kr
            SET CezaTutari = 0,
                OdemeTutari = 0,
                OdemeDurumu = N'Yok'
            FROM dbo.Kiralamalar kr
            WHERE kr.Durum NOT IN (N'Kayıp', N'Kayıp Kitap')
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.KayipKitaplar kayip
                    WHERE kayip.KiralamaId = kr.KiraID
                );

            DELETE c
            FROM dbo.Cezalar c
            INNER JOIN dbo.Kiralamalar kr ON kr.KiraID = c.KiralamaID
            WHERE kr.Durum NOT IN (N'Kayıp', N'Kayıp Kitap')
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.KayipKitaplar kayip
                    WHERE kayip.KiralamaId = kr.KiraID
                );

            UPDATE kr
            SET Durum = N'Kayıp',
                OdemeTutari = kk.KitapBedeli,
                OdemeDurumu = CASE WHEN kk.OdemeDurumu = N'Ödendi' THEN N'Ödendi' ELSE N'Bekliyor' END,
                CezaAciklama = N'Kitap kayıp olarak işaretlendi. Kullanıcıdan kitap bedeli tahsil edilecek.'
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.KayipKitaplar kk ON kk.KiralamaId = kr.KiraID;
            """);
    }

    private static void SeedDemoData(SqlConnection connection)
    {
        Execute(connection, """
            SET NOCOUNT ON;

            DECLARE @KutuphaneID int = (SELECT TOP 1 KutuphaneID FROM dbo.Kutuphaneler ORDER BY KutuphaneID);
            IF @KutuphaneID IS NULL
            BEGIN
                INSERT INTO dbo.Kutuphaneler (KutuphaneAdi, Sehir, Ilce, Telefon, Adres)
                VALUES (N'Merkez Kütüphane', N'İstanbul', N'Merkez', N'02120000000', N'Demo Adres');
                SET @KutuphaneID = SCOPE_IDENTITY();
            END;

            DECLARE @RafID int = (SELECT TOP 1 RafID FROM dbo.Raflar WHERE KutuphaneID = @KutuphaneID ORDER BY RafID);
            IF @RafID IS NULL
            BEGIN
                INSERT INTO dbo.Raflar (KutuphaneID, Salon, Kategori, RafKodu)
                VALUES (@KutuphaneID, N'Ana Salon', N'Demo', N'DEMO-RAF');
                SET @RafID = SCOPE_IDENTITY();
            END;

            DECLARE @YayinEviID int = (SELECT TOP 1 YayinEviID FROM dbo.YayinEvleri WHERE YayinEviAdi = N'Demo Yayınları');
            IF @YayinEviID IS NULL
            BEGIN
                INSERT INTO dbo.YayinEvleri (YayinEviAdi, Sehir, Ulke)
                VALUES (N'Demo Yayınları', N'İstanbul', N'Türkiye');
                SET @YayinEviID = SCOPE_IDENTITY();
            END;

            DECLARE @PersonelID int = (SELECT TOP 1 PersonelID FROM dbo.Personel ORDER BY PersonelID);
            IF @PersonelID IS NULL
            BEGIN
                INSERT INTO dbo.Personel (Ad, Soyad, Telefon, Gorev, IseBaslamaTarihi, KutuphaneID)
                VALUES (N'Demo', N'Personel', N'05990000999', N'Personel', CONVERT(date, GETDATE()), @KutuphaneID);
                SET @PersonelID = SCOPE_IDENTITY();
            END;

            DECLARE @Books table (KitapAdi nvarchar(200), Yazar nvarchar(200), Tur nvarchar(100));
            INSERT INTO @Books (KitapAdi, Yazar, Tur)
            VALUES
                (N'Suç ve Ceza', N'Fyodor Dostoyevski', N'Roman'),
                (N'Sefiller', N'Victor Hugo', N'Roman'),
                (N'Kürk Mantolu Madonna', N'Sabahattin Ali', N'Roman'),
                (N'1984', N'George Orwell', N'Distopya'),
                (N'Hayvan Çiftliği', N'George Orwell', N'Distopya'),
                (N'Simyacı', N'Paulo Coelho', N'Roman'),
                (N'Sapiens', N'Yuval Noah Harari', N'Tarih'),
                (N'İnce Memed', N'Yaşar Kemal', N'Roman'),
                (N'Tutunamayanlar', N'Oğuz Atay', N'Roman'),
                (N'Beyaz Diş', N'Jack London', N'Macera'),
                (N'Küçük Prens', N'Antoine de Saint-Exupéry', N'Çocuk'),
                (N'Şeker Portakalı', N'José Mauro de Vasconcelos', N'Roman');

            DECLARE @KitapAdi nvarchar(200), @Yazar nvarchar(200), @Tur nvarchar(100), @KitapID int, @YazarID int, @TurID int, @BasimID int;
            DECLARE book_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT KitapAdi, Yazar, Tur FROM @Books;
            OPEN book_cursor;
            FETCH NEXT FROM book_cursor INTO @KitapAdi, @Yazar, @Tur;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @KitapID = (SELECT TOP 1 KitapID FROM dbo.Kitaplar WHERE KitapAdi = @KitapAdi);
                IF @KitapID IS NULL
                BEGIN
                    INSERT INTO dbo.Kitaplar (KitapAdi, OrijinalAd, YayinEviID, IlkYayinYili, Dil, Ozet)
                    VALUES (@KitapAdi, NULL, @YayinEviID, 2020, N'Türkçe', N'Demo sunum senaryosu için eklenmiş kitap.');
                    SET @KitapID = SCOPE_IDENTITY();
                END;
                UPDATE dbo.Kitaplar
                SET KitapBedeli = CASE @KitapAdi
                    WHEN N'Suç ve Ceza' THEN 420
                    WHEN N'Sefiller' THEN 450
                    WHEN N'Küçük Prens' THEN 450
                    WHEN N'Sapiens' THEN 520
                    ELSE 300
                END
                WHERE KitapID = @KitapID AND KitapBedeli = 250;

                SET @YazarID = (SELECT TOP 1 YazarID FROM dbo.Yazarlar WHERE YazarAdi = @Yazar);
                IF @YazarID IS NULL
                BEGIN
                    INSERT INTO dbo.Yazarlar (YazarAdi, Ulke, DogumYili) VALUES (@Yazar, NULL, NULL);
                    SET @YazarID = SCOPE_IDENTITY();
                END;
                IF NOT EXISTS (SELECT 1 FROM dbo.KitapYazarlar WHERE KitapID = @KitapID AND YazarID = @YazarID)
                    INSERT INTO dbo.KitapYazarlar (KitapID, YazarID, YazarRol) VALUES (@KitapID, @YazarID, N'Yazar');

                SET @TurID = (SELECT TOP 1 TurID FROM dbo.Turler WHERE TurAdi = @Tur);
                IF @TurID IS NULL
                BEGIN
                    INSERT INTO dbo.Turler (TurAdi) VALUES (@Tur);
                    SET @TurID = SCOPE_IDENTITY();
                END;
                IF NOT EXISTS (SELECT 1 FROM dbo.KitapTurler WHERE KitapID = @KitapID AND TurID = @TurID)
                    INSERT INTO dbo.KitapTurler (KitapID, TurID) VALUES (@KitapID, @TurID);

                SET @BasimID = (
                    SELECT TOP 1 BasimID
                    FROM dbo.KitapBasimlari
                    WHERE KitapID = @KitapID AND ISBN NOT LIKE N'DEMO-%'
                    ORDER BY BasimID
                );
                IF @BasimID IS NULL
                    SET @BasimID = (SELECT TOP 1 BasimID FROM dbo.KitapBasimlari WHERE ISBN = CONCAT(N'DEMO-', @KitapID));
                IF @BasimID IS NULL
                BEGIN
                    INSERT INTO dbo.KitapBasimlari (KitapID, YayinEviID, ISBN, BasimYili, BaskiNo, SayfaSayisi, Dil, KapakTipi)
                    VALUES (@KitapID, @YayinEviID, CONCAT(N'DEMO-', @KitapID), 2024, 1, 320, N'Türkçe', N'Karton Kapak');
                    SET @BasimID = SCOPE_IDENTITY();
                END;

                IF NOT EXISTS (SELECT 1 FROM dbo.KitapKopyalari WHERE Barkod = CONCAT(N'DEMO-', @KitapID, N'-1'))
                    INSERT INTO dbo.KitapKopyalari (KitapID, BasimID, KutuphaneID, RafID, Durum, Barkod)
                    VALUES (@KitapID, @BasimID, @KutuphaneID, @RafID, N'Musait', CONCAT(N'DEMO-', @KitapID, N'-1'));
                IF NOT EXISTS (SELECT 1 FROM dbo.KitapKopyalari WHERE Barkod = CONCAT(N'DEMO-', @KitapID, N'-2'))
                    INSERT INTO dbo.KitapKopyalari (KitapID, BasimID, KutuphaneID, RafID, Durum, Barkod)
                    VALUES (@KitapID, @BasimID, @KutuphaneID, @RafID, N'Musait', CONCAT(N'DEMO-', @KitapID, N'-2'));

                FETCH NEXT FROM book_cursor INTO @KitapAdi, @Yazar, @Tur;
            END;
            CLOSE book_cursor;
            DEALLOCATE book_cursor;

            DECLARE @DemoPhones table (Telefon nvarchar(30), Ad nvarchar(80), Soyad nvarchar(80), Eposta nvarchar(160));
            INSERT INTO @DemoPhones (Telefon, Ad, Soyad, Eposta)
            VALUES
                (N'05990000001', N'Ahmet', N'Yılmaz', N'ahmet.yilmaz@demo.local'),
                (N'05990000002', N'Ayşe', N'Demir', N'ayse.demir@demo.local'),
                (N'05990000003', N'Deniz', N'Çelik', N'deniz.celik@demo.local'),
                (N'05990000004', N'Zeynep', N'Çelik', N'zeynep.celik@demo.local'),
                (N'05990000005', N'Elif', N'Şahin', N'elif.sahin@demo.local'),
                (N'05990000006', N'Kerem', N'Demir', N'kerem.demir@demo.local'),
                (N'05990000007', N'Merve', N'Aydın', N'merve.aydin@demo.local');

            INSERT INTO dbo.Kullanicilar (Ad, Soyad, Telefon, Eposta, Adres, Sehir, UyelikTarihi, Skor, Durum, KayitTarihi, MevcutPuan)
            SELECT d.Ad, d.Soyad, d.Telefon, d.Eposta, N'Demo adres', N'İstanbul', DATEADD(month, -8, CONVERT(date, GETDATE())), 0, N'Onaylandi', DATEADD(month, -8, GETDATE()), 0
            FROM @DemoPhones d
            WHERE NOT EXISTS (SELECT 1 FROM dbo.Kullanicilar k WHERE k.Telefon = d.Telefon);

            UPDATE k
            SET Ad = d.Ad, Soyad = d.Soyad, Eposta = d.Eposta, Durum = N'Onaylandi'
            FROM dbo.Kullanicilar k
            INNER JOIN @DemoPhones d ON d.Telefon = k.Telefon;

            DELETE c
            FROM dbo.Cezalar c
            INNER JOIN dbo.Kiralamalar kr ON kr.KiraID = c.KiralamaID
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            WHERE ku.Telefon IN (SELECT Telefon FROM @DemoPhones);
            DELETE kk FROM dbo.KayipKitaplar kk INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kk.KullaniciId WHERE ku.Telefon IN (SELECT Telefon FROM @DemoPhones);
            DELETE ph FROM dbo.PuanHareketleri ph INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = ph.KullaniciId WHERE ku.Telefon IN (SELECT Telefon FROM @DemoPhones);
            DELETE kr FROM dbo.Kiralamalar kr INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID WHERE ku.Telefon IN (SELECT Telefon FROM @DemoPhones);
            UPDATE dbo.KitapKopyalari SET Durum = N'Musait' WHERE Barkod LIKE N'DEMO-%';

            DECLARE @Rentals table
            (
                Telefon nvarchar(30),
                KitapAdi nvarchar(200),
                AlisOffset int,
                IadeOffset int,
                TeslimOffset int NULL,
                Durum nvarchar(30),
                KazanilanPuan int,
                CezaPuani int,
                CezaTutari decimal(10,2),
                KayipBedeli decimal(10,2) NULL
            );
            INSERT INTO @Rentals VALUES
                (N'05990000001', N'Suç ve Ceza', -160, -145, -146, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'Sefiller', -140, -125, -126, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'Kürk Mantolu Madonna', -120, -105, -106, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'1984', -100, -85, -86, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'Hayvan Çiftliği', -80, -65, -66, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'Simyacı', -60, -45, -46, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'Sapiens', -40, -25, -26, N'Teslim', 10, 0, 0, NULL),
                (N'05990000001', N'İnce Memed', -22, -7, -8, N'Teslim', 10, 0, 0, NULL),
                (N'05990000002', N'Suç ve Ceza', -130, -115, -116, N'Teslim', 10, 0, 0, NULL),
                (N'05990000002', N'Sefiller', -110, -95, -96, N'Teslim', 10, 0, 0, NULL),
                (N'05990000002', N'1984', -90, -75, -74, N'Süresi Geçmiş', 10, 10, 0, NULL),
                (N'05990000002', N'Simyacı', -70, -55, -54, N'Süresi Geçmiş', 10, 10, 0, NULL),
                (N'05990000002', N'Sapiens', -50, -35, -36, N'Teslim', 10, 0, 0, NULL),
                (N'05990000002', N'Beyaz Diş', -30, -15, -16, N'Teslim', 10, 0, 0, NULL),
                (N'05990000003', N'Tutunamayanlar', -145, -130, -131, N'Teslim', 10, 0, 0, NULL),
                (N'05990000003', N'Beyaz Diş', -125, -110, -111, N'Teslim', 10, 0, 0, NULL),
                (N'05990000003', N'Sapiens', -105, -90, -91, N'Teslim', 10, 0, 0, NULL),
                (N'05990000003', N'Küçük Prens', -90, -75, -74, N'Kayıp Kitap', 0, 30, 450, 450),
                (N'05990000004', N'Kürk Mantolu Madonna', -110, -95, -96, N'Teslim', 10, 0, 0, NULL),
                (N'05990000004', N'1984', -90, -75, -76, N'Teslim', 10, 0, 0, NULL),
                (N'05990000004', N'Hayvan Çiftliği', -70, -55, -56, N'Teslim', 10, 0, 0, NULL),
                (N'05990000004', N'Simyacı', -50, -35, -36, N'Teslim', 10, 0, 0, NULL),
                (N'05990000004', N'Sapiens', -30, -15, -16, N'Teslim', 10, 0, 0, NULL),
                (N'05990000004', N'Suç ve Ceza', -5, 10, NULL, N'Kirada', 0, 0, 0, NULL),
                (N'05990000004', N'Sefiller', -3, 12, NULL, N'Kirada', 0, 0, 0, NULL),
                (N'05990000005', N'Suç ve Ceza', -125, -110, -111, N'Teslim', 10, 0, 0, NULL),
                (N'05990000005', N'Sefiller', -105, -90, -91, N'Teslim', 10, 0, 0, NULL),
                (N'05990000005', N'1984', -85, -70, -71, N'Teslim', 10, 0, 0, NULL),
                (N'05990000005', N'Simyacı', -65, -50, -51, N'Teslim', 10, 0, 0, NULL),
                (N'05990000005', N'Sapiens', -45, -30, -31, N'Teslim', 10, 0, 0, NULL),
                (N'05990000005', N'Şeker Portakalı', -30, -12, -10, N'Süresi Geçmiş', 0, 5, 0, NULL),
                (N'05990000006', N'İnce Memed', -30, -12, NULL, N'Süresi Geçmiş', 0, 0, 0, NULL);

            DECLARE rental_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT Telefon, KitapAdi, AlisOffset, IadeOffset, TeslimOffset, Durum, KazanilanPuan, CezaPuani, CezaTutari, KayipBedeli FROM @Rentals;
            DECLARE @Telefon nvarchar(30), @AlisOffset int, @IadeOffset int, @TeslimOffset int, @Durum nvarchar(30), @KazanilanPuan int, @CezaPuani int, @CezaTutari decimal(10,2), @KayipBedeli decimal(10,2), @KullaniciID int, @KopyaID int, @KiraID int, @Running int;
            OPEN rental_cursor;
            FETCH NEXT FROM rental_cursor INTO @Telefon, @KitapAdi, @AlisOffset, @IadeOffset, @TeslimOffset, @Durum, @KazanilanPuan, @CezaPuani, @CezaTutari, @KayipBedeli;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @KullaniciID = (SELECT KullaniciID FROM dbo.Kullanicilar WHERE Telefon = @Telefon);
                SET @KitapID = (SELECT TOP 1 KitapID FROM dbo.Kitaplar WHERE KitapAdi = @KitapAdi);
                SET @KopyaID = (
                    SELECT TOP 1 KopyaID
                    FROM dbo.KitapKopyalari
                    WHERE KitapID = @KitapID AND Barkod LIKE N'DEMO-%' AND Durum = N'Musait'
                    ORDER BY KopyaID
                );

                INSERT INTO dbo.Kiralamalar (KullaniciID, KopyaID, AlisTarihi, IadeTarihi, TeslimTarihi, Durum, PersonelID, KazanilanPuan, CezaPuani, CezaTutari)
                VALUES (@KullaniciID, @KopyaID, DATEADD(day, @AlisOffset, GETDATE()), DATEADD(day, @IadeOffset, GETDATE()), CASE WHEN @TeslimOffset IS NULL THEN NULL ELSE DATEADD(day, @TeslimOffset, GETDATE()) END, @Durum, @PersonelID, @KazanilanPuan, @CezaPuani, @CezaTutari);
                SET @KiraID = SCOPE_IDENTITY();

                UPDATE dbo.KitapKopyalari
                SET Durum = CASE WHEN @Durum = N'Kirada' OR @Durum = N'Süresi Geçmiş' THEN N'Kirada' WHEN @Durum = N'Kayıp Kitap' THEN N'Kayıp' ELSE N'Musait' END
                WHERE KopyaID = @KopyaID;

                IF @KayipBedeli IS NOT NULL
                    INSERT INTO dbo.KayipKitaplar (KullaniciId, KitapId, KiralamaId, BildirimTarihi, KitapBedeli, CezaPuani, OdemeDurumu)
                    VALUES (@KullaniciID, @KitapID, @KiraID, DATEADD(day, @TeslimOffset, GETDATE()), @KayipBedeli, @CezaPuani, N'Bekliyor');

                IF @KazanilanPuan <> 0 OR @CezaPuani <> 0
                BEGIN
                    SET @Running = ISNULL((SELECT SUM(KazanilanPuan - KaybedilenPuan) FROM dbo.PuanHareketleri WHERE KullaniciId = @KullaniciID), 0) + @KazanilanPuan - @CezaPuani;
                    INSERT INTO dbo.PuanHareketleri (KullaniciId, KitapId, IslemTarihi, IslemTipi, KazanilanPuan, KaybedilenPuan, ToplamPuan)
                    VALUES (@KullaniciID, @KitapID, ISNULL(DATEADD(day, @TeslimOffset, GETDATE()), GETDATE()),
                        CASE WHEN @Durum = N'Kayıp Kitap' THEN N'Kitap Kaybedildi' WHEN @CezaPuani > 0 THEN N'Kitap Geç Teslim Edildi' ELSE N'Kitap Zamanında Teslim Edildi' END,
                        @KazanilanPuan, @CezaPuani, @Running);
                END;

                FETCH NEXT FROM rental_cursor INTO @Telefon, @KitapAdi, @AlisOffset, @IadeOffset, @TeslimOffset, @Durum, @KazanilanPuan, @CezaPuani, @CezaTutari, @KayipBedeli;
            END;
            CLOSE rental_cursor;
            DEALLOCATE rental_cursor;

            UPDATE k
            SET MevcutPuan = ISNULL(p.NetPuan, 0),
                Skor = ISNULL(p.NetPuan, 0)
            FROM dbo.Kullanicilar k
            INNER JOIN @DemoPhones d ON d.Telefon = k.Telefon
            OUTER APPLY (
                SELECT SUM(KazanilanPuan - KaybedilenPuan) AS NetPuan
                FROM dbo.PuanHareketleri ph
                WHERE ph.KullaniciId = k.KullaniciID
            ) p;

            ;WITH OrderedMovements AS
            (
                SELECT
                    Id,
                    RunningTotal = SUM(KazanilanPuan - KaybedilenPuan) OVER (PARTITION BY KullaniciId ORDER BY IslemTarihi, Id)
                FROM dbo.PuanHareketleri
            )
            UPDATE ph
            SET ToplamPuan = om.RunningTotal
            FROM dbo.PuanHareketleri ph
            INNER JOIN OrderedMovements om ON om.Id = ph.Id;

            UPDATE kr
            SET Durum = CASE
                    WHEN kr.Durum IN (N'Kayıp Kitap', N'Kayıp') THEN N'Kayıp'
                    WHEN kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE() THEN N'Gecikmiş'
                    WHEN kr.TeslimTarihi IS NULL THEN N'Aktif'
                    WHEN kr.TeslimTarihi > kr.IadeTarihi THEN N'Geç Teslim Edildi'
                    ELSE N'Teslim Edildi'
                END,
                OdemeTutari = CASE
                    WHEN kayip.Id IS NOT NULL THEN kayip.KitapBedeli
                    ELSE 0
                END,
                OdemeDurumu = CASE
                    WHEN kayip.Id IS NOT NULL THEN CASE WHEN kayip.OdemeDurumu = N'Ödendi' THEN N'Ödendi' ELSE N'Bekliyor' END
                    ELSE N'Yok'
                END,
                CezaAciklama = CASE
                    WHEN kayip.Id IS NOT NULL THEN N'Kitap kayıp olarak işaretlendi. Kullanıcıdan kitap bedeli tahsil edilecek.'
                    WHEN kr.TeslimTarihi IS NULL AND kr.IadeTarihi < GETDATE() THEN N'Teslim tarihi geçti. Teslim alındığında ceza puanı uygulanacak.'
                    WHEN kr.TeslimTarihi > kr.IadeTarihi THEN N'Geç teslim edildi: ceza puanı uygulandı.'
                    WHEN kr.TeslimTarihi IS NULL THEN N'Aktif kirada: henüz puan değişimi yok.'
                    ELSE N'Zamanında teslim edildi: ödül puanı eklendi.'
                END
            FROM dbo.Kiralamalar kr
            INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciID = kr.KullaniciID
            LEFT JOIN dbo.KayipKitaplar kayip ON kayip.KiralamaId = kr.KiraID
            WHERE ku.Telefon IN (SELECT Telefon FROM @DemoPhones);
            """);
    }

    private static void BackfillAccounts(SqlConnection connection)
    {
        using var command = new SqlCommand("""
            SELECT KullaniciID, Telefon FROM dbo.Kullanicilar
            WHERE Telefon <> N'' AND NOT EXISTS (
                SELECT 1 FROM dbo.GirisHesaplari WHERE Rol = N'Kullanici' AND KayitID = KullaniciID
            );
            """, connection);
        using var reader = command.ExecuteReader();
        var users = new List<(int Id, string Phone)>();
        while (reader.Read())
        {
            users.Add((reader.GetInt32(0), reader.GetString(1).Trim()));
        }

        reader.Close();
        foreach (var user in users)
        {
            EnsureAccount(connection, UserRole.Kullanici, user.Id, user.Phone, user.Id.ToString(), true);
        }

        using var personnelCommand = new SqlCommand("""
            SELECT PersonelID, Telefon FROM dbo.Personel
            WHERE Telefon <> N'' AND NOT EXISTS (
                SELECT 1 FROM dbo.GirisHesaplari WHERE Rol = N'Personel' AND KayitID = PersonelID
            );
            """, connection);
        using var personnelReader = personnelCommand.ExecuteReader();
        var personnel = new List<(int Id, string Phone)>();
        while (personnelReader.Read())
        {
            personnel.Add((personnelReader.GetInt32(0), personnelReader.GetString(1).Trim()));
        }

        personnelReader.Close();
        foreach (var person in personnel)
        {
            EnsureAccount(connection, UserRole.Personel, person.Id, person.Phone, person.Id.ToString(), true);
        }
    }

    private static void UpgradePlainPasswords(SqlConnection connection)
    {
        using var command = new SqlCommand("""
            SELECT HesapID, Sifre
            FROM dbo.GirisHesaplari
            WHERE Sifre IS NOT NULL AND SifreHash IS NULL;
            """, connection);
        using var reader = command.ExecuteReader();
        var accounts = new List<(int Id, string Password)>();
        while (reader.Read())
        {
            accounts.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        reader.Close();
        foreach (var account in accounts)
        {
            var (hash, salt) = PasswordHelper.HashPassword(account.Password);
            using var update = new SqlCommand("UPDATE dbo.GirisHesaplari SET SifreHash = @Hash, SifreSalt = @Salt, Sifre = NULL WHERE HesapID = @HesapID;", connection);
            update.Parameters.AddWithValue("@Hash", hash);
            update.Parameters.AddWithValue("@Salt", salt);
            update.Parameters.AddWithValue("@HesapID", account.Id);
            update.ExecuteNonQuery();
        }
    }

    private static void EnsureAccount(SqlConnection connection, UserRole role, int? recordId, string userName, string password, bool active)
    {
        using var exists = new SqlCommand("SELECT COUNT(*) FROM dbo.GirisHesaplari WHERE Rol = @Rol AND ((KayitID IS NULL AND @KayitID IS NULL) OR KayitID = @KayitID);", connection);
        exists.Parameters.AddWithValue("@Rol", role.ToString());
        exists.Parameters.AddWithValue("@KayitID", (object?)recordId ?? DBNull.Value);
        if (Convert.ToInt32(exists.ExecuteScalar()) > 0)
        {
            return;
        }

        var (hash, salt) = PasswordHelper.HashPassword(password);
        using var insert = new SqlCommand("INSERT INTO dbo.GirisHesaplari (Rol, KayitID, KullaniciAdi, SifreHash, SifreSalt, AktifMi) VALUES (@Rol, @KayitID, @KullaniciAdi, @Hash, @Salt, @AktifMi);", connection);
        insert.Parameters.AddWithValue("@Rol", role.ToString());
        insert.Parameters.AddWithValue("@KayitID", (object?)recordId ?? DBNull.Value);
        insert.Parameters.AddWithValue("@KullaniciAdi", userName);
        insert.Parameters.AddWithValue("@Hash", hash);
        insert.Parameters.AddWithValue("@Salt", salt);
        insert.Parameters.AddWithValue("@AktifMi", active);
        insert.ExecuteNonQuery();
    }

    private static void Execute(SqlConnection connection, string sql)
    {
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}


