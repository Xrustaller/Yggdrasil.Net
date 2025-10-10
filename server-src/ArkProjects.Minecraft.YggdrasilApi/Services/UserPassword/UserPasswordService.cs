using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;

public class UserPasswordService : IUserPasswordService
{
    private const byte LastHasherVer = 1;
    private const int MinPasswordLength = 6;
    private const int MaxPasswordLength = 30;

    public bool CheckPasswordRequirements(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Contains(' '))
            return false;
        if (password.Length is < MinPasswordLength or > MaxPasswordLength)
            return false;
        return true;
    }

    public bool Validate(string? password, string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;
        string[] parts = passwordHash.Split('$');
        if (parts.Length != 3)
            return false;
        if (parts[0] != "1")
            return false;

        return CreatePasswordHashV1(password, parts[1]) == passwordHash;
    }

    public string CreatePasswordHash(string password)
    {
        return CreatePasswordHashV1(password);
    }

    private static string CreatePasswordHashV1(string password, string? salt = null)
    {
        byte[] saltBytes = salt == null
            ? RandomNumberGenerator.GetBytes(128 / 8)
            : Convert.FromBase64String(salt);
        byte[] hashBytes = KeyDerivation.Pbkdf2(
            password!,
            saltBytes,
            KeyDerivationPrf.HMACSHA256,
            100000,
            256 / 8);
        string saltB64 = Convert.ToBase64String(saltBytes);
        string hashB64 = Convert.ToBase64String(hashBytes);

        string result = $"{LastHasherVer}${saltB64}${hashB64}";
        return result;
    }
}