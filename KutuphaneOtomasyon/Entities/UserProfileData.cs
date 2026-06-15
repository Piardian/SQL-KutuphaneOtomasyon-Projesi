using System.Data;

namespace KutuphaneOtomasyon;

internal sealed class UserProfileData
{
    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    public int MevcutPuan { get; set; }

    public DateTime KayitTarihi { get; set; }

    public int ActiveRentCount { get; set; }

    public int TotalRentCount { get; set; }

    public int TotalEarnedPoints { get; set; }

    public int TotalPenaltyPoints { get; set; }

    public int LateReturnCount { get; set; }

    public DataTable ActiveRentals { get; set; } = new();

    public DataTable PastRentals { get; set; } = new();

    public DataTable OverdueRentals { get; set; } = new();

    public DataTable PointMovements { get; set; } = new();

    public DataTable LostBooks { get; set; } = new();

    public decimal TotalLostBookDebt { get; set; }

    public int LostBookCount { get; set; }
}
