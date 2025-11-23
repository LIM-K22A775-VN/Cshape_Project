using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace project.Helpers
{
    public static class DataEncryptionExtensions
    {
        #region [Hashing Extension]

        // Hàm băm SHA256
        public static string ToSHA256Hash(this string password, string? saltKey)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Ghép mật khẩu và salt lại
                string combined = string.Concat(password, saltKey);

                // Băm chuỗi
                byte[] encryptedSHA256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));

                // Chuyển byte thành chuỗi Base64
                return Convert.ToBase64String(encryptedSHA256);
            }
        }

        // Hàm băm SHA512
        public static string ToSHA512Hash(this string password, string? saltKey)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                string combined = string.Concat(password, saltKey);
                byte[] encryptedSHA512 = sha512.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(encryptedSHA512);
            }
        }

        // Hàm băm MD5
        public static string ToMd5Hash(this string password, string? saltKey)
        {
            using (MD5 md5 = MD5.Create())
            {
                string combined = string.Concat(password, saltKey);
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));

                StringBuilder sBuilder = new StringBuilder();

                // Chuyển từng byte sang dạng hex
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        #endregion
    }
}
