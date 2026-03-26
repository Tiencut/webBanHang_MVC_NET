using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace SV22T1020761.Shop.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            const int iterationCount = 10000;
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterationCount,
                numBytesRequested: 256 / 8));
            var saltText = Convert.ToBase64String(salt);
            return $"{iterationCount}.{saltText}.{hashed}";
        }

        public static bool VerifyHashedPassword(string hashedPasswordWithSalt, string providedPassword)
        {
            try
            {
                var parts = hashedPasswordWithSalt.Split('.');
                var iter = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var stored = parts[2];
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: providedPassword,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: iter,
                    numBytesRequested: 256 / 8));
                return hashed == stored;
            }
            catch
            {
                return false;
            }
        }
    }
}
