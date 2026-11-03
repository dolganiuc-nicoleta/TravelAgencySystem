using System;
using System.Security.Cryptography;
using System.Text;

namespace TravelAgency.Core.Services
{
    /// <summary>
    /// PBKDF2-SHA256 password hashing with a per-password random salt.
    /// Format: <c>pbkdf2-sha256$&lt;iterations&gt;$&lt;base64 salt&gt;$&lt;base64 hash&gt;</c>.
    /// Hashes produced by the previous unsalted SHA-256 scheme are still accepted by
    /// <see cref="Verify"/> and reported by <see cref="NeedsRehash"/> so they can be upgraded on login.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "pbkdf2-sha256";
        private const int Iterations = 600_000; // OWASP 2023 recommendation for PBKDF2-HMAC-SHA256
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            var parts = passwordHash.Split('$');
            if (parts.Length == 4 && parts[0] == Prefix)
            {
                if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
                    return false;

                byte[] salt, expected;
                try
                {
                    salt = Convert.FromBase64String(parts[2]);
                    expected = Convert.FromBase64String(parts[3]);
                }
                catch (FormatException)
                {
                    return false;
                }

                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }

            return VerifyLegacySha256(password, passwordHash);
        }

        /// <summary>True when the stored hash uses an outdated scheme or iteration count.</summary>
        public static bool NeedsRehash(string passwordHash)
        {
            var parts = (passwordHash ?? "").Split('$');
            return parts.Length != 4
                || parts[0] != Prefix
                || !int.TryParse(parts[1], out var iterations)
                || iterations < Iterations;
        }

        private static bool VerifyLegacySha256(string password, string passwordHash)
        {
            byte[] expected;
            try
            {
                expected = Convert.FromBase64String(passwordHash);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
