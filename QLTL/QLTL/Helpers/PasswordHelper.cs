using System;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace QLTL.Helpers
{
    public enum PasswordCheckResult
    {
        MatchBcrypt,         // Khớp bcrypt
        MatchLegacyPlain,    // Khớp dạng legacy/plain (ví dụ lưu thường)
        Mismatch,            // Không khớp
        InvalidHashFormat,   // Hash lưu trong DB sai format (không phải bcrypt)
        Error                // Lỗi không xác định
    }

    public static class PasswordHelper
    {
        // Hash password với bcrypt
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // Regex nhận diện bcrypt: $2a$ / $2b$ / $2y$ + cost 2 chữ số + 53 ký tự
        private static readonly Regex BcryptRegex =
            new Regex(@"^\$2[aby]\$\d{2}\$[./A-Za-z0-9]{53}$", RegexOptions.Compiled);

        public static bool IsBcryptHash(string stored) =>
            !string.IsNullOrWhiteSpace(stored) && BcryptRegex.IsMatch(stored);

        /// <summary>
        /// Verify an toàn, không để ném exception ra ngoài.
        /// Hỗ trợ nhận diện legacy (plain): nếu stored không phải bcrypt, so sánh ==.
        /// </summary>
        public static PasswordCheckResult TryVerifyPassword(string password, string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return PasswordCheckResult.InvalidHashFormat;

            var isBcrypt = IsBcryptHash(stored);

            try
            {
                if (isBcrypt)
                {
                    var ok = BCrypt.Net.BCrypt.Verify(password, stored);
                    return ok ? PasswordCheckResult.MatchBcrypt : PasswordCheckResult.Mismatch;
                }

                // Legacy/plain (ví dụ giai đoạn cũ lưu thẳng)
                return password == stored
                    ? PasswordCheckResult.MatchLegacyPlain
                    : PasswordCheckResult.InvalidHashFormat;
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Hash trong DB không phải bcrypt hợp lệ
                return PasswordCheckResult.InvalidHashFormat;
            }
            catch
            {
                return PasswordCheckResult.Error;
            }
        }
    }
}
