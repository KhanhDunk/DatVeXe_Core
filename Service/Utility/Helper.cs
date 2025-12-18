using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Service.Utility
{
    public class Helper
    {
        /// <summary>
        /// Chuyển chuỗi tiếng Việt có dấu thành không dấu và nối bằng dấu '-'
        /// </summary>
        public static string ConvertToSlug(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // 1. Chuẩn hóa chữ thường
            text = text.ToLowerInvariant();

            // 2. Chuyển unicode có dấu sang không dấu
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            string noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

            // 3. Thay khoảng trắng và ký tự không phải chữ/ số bằng dấu '-'
            string slug = Regex.Replace(noAccents, @"[^a-z0-9]+", "-").Trim('-');

            return slug;
        }



        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            string otp = "";
            for (int i = 0; i < length; i++)
                otp += random.Next(0, 10).ToString();
            return otp;
        }

        public static string HashOtp(string otp)
        {
            // Hash OTP để lưu DB
            return BCrypt.Net.BCrypt.HashPassword(otp);
        }

        public static bool VerifyOtp(string otp, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(otp, hash);
        }

    }
}
