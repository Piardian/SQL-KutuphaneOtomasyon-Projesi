using System.Security.Cryptography;

namespace KutuphaneOtomasyon;

internal static class PasswordHelper
{
    public static (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        return (ComputeHash(password, salt), salt);
    }

    public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
    {
        var hash = ComputeHash(password, salt);
        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }

    public static byte[] ComputeHash(string password, byte[] salt)
    {
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var input = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, input, salt.Length, passwordBytes.Length);
        return SHA256.HashData(input);
    }
}
