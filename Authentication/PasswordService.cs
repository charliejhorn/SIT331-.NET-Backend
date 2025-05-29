using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Identity;

namespace robot_controller_api.Authentication;

public static class PasswordService
{
    public static string HashPassword(string rawPassword)
    {
        return Argon2.Hash(rawPassword);
    }

    public static PasswordVerificationResult VerifyPassword(string rawPassword, string hashedPassword)
    {
        return Argon2.Verify(hashedPassword, rawPassword) ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}