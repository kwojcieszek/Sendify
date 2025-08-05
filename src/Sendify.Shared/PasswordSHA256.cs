using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Sendify.Shared;

public class PasswordSha256 : IPasswordService
{
    public static byte[] Salt { get; set; } = [ 0x0 ];

    public string HashPassword(string password)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: Salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }

    public bool ComparePassword(string password, string hashPassword)
    {
        return HashPassword(password) == hashPassword;
    }
}