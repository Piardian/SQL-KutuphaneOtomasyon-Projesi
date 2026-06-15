namespace KutuphaneOtomasyon;

public sealed class AppUser
{
    public AppUser(UserRole role, int? recordId, string displayName)
    {
        Role = role;
        RecordId = recordId;
        DisplayName = displayName;
    }

    public UserRole Role { get; }

    public int? RecordId { get; }

    public string DisplayName { get; }

    public bool CanManageBooks => Role is UserRole.Admin or UserRole.Personel;

    public bool CanRentBooks => Role is UserRole.Admin or UserRole.Personel or UserRole.Kullanici;

    public bool CanManagePermissions => Role is UserRole.Admin;
}
