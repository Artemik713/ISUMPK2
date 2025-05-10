// ISUMPK2.Web/Auth/DummyPasswordHasher.cs
using Microsoft.AspNetCore.Identity;
using ISUMPK2.Domain.Entities;

namespace ISUMPK2.Web.Auth
{
    public class DummyPasswordHasher : IPasswordHasher<User>
    {
        public string HashPassword(User user, string password)
        {
            return "hashed-password"; // Заглушка
        }

        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        {
            return PasswordVerificationResult.Success; // Заглушка
        }
    }
}
