//using System.Security.Cryptography;
//using System.Text;

//namespace Shikhsa.Models.Payment
//{
//    public class EncrypDecrpt
//    {
//        private readonly string _encPassphrase;
//        private readonly string _encSalt;
//        private readonly string _decPassphrase;
//        private readonly string _decSalt;

//        private readonly byte[] _iv =
//        {
//            0, 1, 2, 3,
//            4, 5, 6, 7,
//            8, 9, 10, 11,
//            12, 13, 14, 15
//        };

//        private const int Iterations = 65536;

//        public EncrypDecrpt(IConfiguration configuration)
//        {
//            _encPassphrase =
//                configuration["atomtechEncrptkey"]
//                ?? throw new InvalidOperationException(
//                    "atomtechEncrptkey is missing from configuration.");

//            _encSalt =
//                configuration["atomtechEncrptkey"]
//                ?? throw new InvalidOperationException(
//                    "atomtechEncrptkey is missing from configuration.");

//            _decPassphrase =
//                configuration["atomtechDecrptkey"]
//                ?? throw new InvalidOperationException(
//                    "atomtechDecrptkey is missing from configuration.");

//            _decSalt =
//                configuration["atomtechDecrptkey"]
//                ?? throw new InvalidOperationException(
//                    "atomtechDecrptkey is missing from configuration.");
//        }

//        public string Encrypt(
//            string plainText,
//            string passphrase,
//            string salt,
//            byte[] iv,
//            int iterations)
//        {
//            var plainBytes = Encoding.UTF8.GetBytes(plainText);

//            string data = ByteArrayToHexString(
//                Encrypt(
//                    plainBytes,
//                    GetSymmetricAlgorithm(
//                        passphrase,
//                        salt,
//                        iv,
//                        iterations)))
//                .ToUpperInvariant();

//            return data;
//        }

//        public string decrypt(
//            string plainText,
//            string passphrase,
//            string salt,
//            byte[] iv,
//            int iterations)
//        {
//            byte[] str = HexStringToByte(plainText);

//            string data = Encoding.UTF8.GetString(
//                decrypt(
//                    str,
//                    GetSymmetricAlgorithm(
//                        passphrase,
//                        salt,
//                        iv,
//                        iterations)));

//            return data;
//        }

//        public string Encrypt(string plainText)
//        {
//            var plainBytes = Encoding.UTF8.GetBytes(plainText);

//            string data = ByteArrayToHexString(
//                Encrypt(
//                    plainBytes,
//                    GetSymmetricAlgorithm(
//                        _encPassphrase,
//                        _encSalt,
//                        _iv,
//                        Iterations)))
//                .ToUpperInvariant();

//            return data;
//        }

//        public string decrypt(string plainText)
//        {
//            byte[] str = HexStringToByte(plainText);

//            string data = Encoding.UTF8.GetString(
//                decrypt(
//                    str,
//                    GetSymmetricAlgorithm(
//                        _decPassphrase,
//                        _decSalt,
//                        _iv,
//                        Iterations)));

//            return data;
//        }

//        public byte[] Encrypt(
//            byte[] plainBytes,
//            SymmetricAlgorithm sa)
//        {
//            using (sa)
//            using (ICryptoTransform encryptor = sa.CreateEncryptor())
//            {
//                return encryptor.TransformFinalBlock(
//                    plainBytes,
//                    0,
//                    plainBytes.Length);
//            }
//        }

//        public byte[] decrypt(
//            byte[] plainBytes,
//            SymmetricAlgorithm sa)
//        {
//            using (sa)
//            using (ICryptoTransform decryptor = sa.CreateDecryptor())
//            {
//                return decryptor.TransformFinalBlock(
//                    plainBytes,
//                    0,
//                    plainBytes.Length);
//            }
//        }

//        public SymmetricAlgorithm GetSymmetricAlgorithm(
//            string passphrase,
//            string salt,
//            byte[] iv,
//            int iterations)
//        {
//            var ivBytes = new byte[16];

//            using var rfcdb =
//                new Rfc2898DeriveBytes(
//                    passphrase,
//                    Encoding.UTF8.GetBytes(salt),
//                    iterations,
//                    HashAlgorithmName.SHA512);

//            // Original code derives 32 bytes for the key.
//            byte[] keyBytes = rfcdb.GetBytes(32);

//            Array.Copy(
//                iv,
//                ivBytes,
//                Math.Min(ivBytes.Length, iv.Length));

//            var rij = new RijndaelManaged
//            {
//                Mode = CipherMode.CBC,
//                Padding = PaddingMode.PKCS7,
//                FeedbackSize = 128,
//                KeySize = 256,
//                BlockSize = 128,
//                Key = keyBytes,
//                IV = ivBytes
//            };

//            return rij;
//        }

//        protected static byte[] HexStringToByte(string hexString)
//        {
//            if (string.IsNullOrWhiteSpace(hexString))
//                return Array.Empty<byte>();

//            if (hexString.Length % 2 != 0)
//                throw new ArgumentException(
//                    "Invalid hexadecimal string.",
//                    nameof(hexString));

//            int bytesCount = hexString.Length / 2;
//            byte[] bytes = new byte[bytesCount];

//            for (int x = 0; x < bytesCount; x++)
//            {
//                bytes[x] = Convert.ToByte(
//                    hexString.Substring(x * 2, 2),
//                    16);
//            }

//            return bytes;
//        }

//        public static string ByteArrayToHexString(byte[] ba)
//        {
//            StringBuilder hex = new StringBuilder(ba.Length * 2);

//            foreach (byte b in ba)
//                hex.AppendFormat("{0:x2}", b);

//            return hex.ToString();
//        }
//    }
//}
using System.Security.Cryptography;
using System.Text;

namespace Shikhsa.Models.Payment
{
    public sealed class EncrypDecrpt
    {
        private const int Iterations = 65536;

        private static readonly byte[] DefaultIv =
        {
            0, 1, 2, 3,
            4, 5, 6, 7,
            8, 9, 10, 11,
            12, 13, 14, 15
        };

        public static string Encrypt(
            string plainText,
            string passphrase,
            string salt)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var algorithm =
                CreateAlgorithm(
                    passphrase,
                    salt,
                    DefaultIv,
                    Iterations);

            byte[] plainBytes =
                Encoding.UTF8.GetBytes(plainText);

            using ICryptoTransform encryptor =
                algorithm.CreateEncryptor();

            byte[] encrypted =
                encryptor.TransformFinalBlock(
                    plainBytes,
                    0,
                    plainBytes.Length);

            return Convert.ToHexString(encrypted);
        }

        public static string Decrypt(
            string cipherText,
            string passphrase,
            string salt)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return string.Empty;

            byte[] cipherBytes =
                HexStringToBytes(cipherText);

            using var algorithm =
                CreateAlgorithm(
                    passphrase,
                    salt,
                    DefaultIv,
                    Iterations);

            using ICryptoTransform decryptor =
                algorithm.CreateDecryptor();

            byte[] decrypted =
                decryptor.TransformFinalBlock(
                    cipherBytes,
                    0,
                    cipherBytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }

        public static string Encrypt(
            string plainText,
            string passphrase,
            string salt,
            byte[] iv,
            int iterations)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var algorithm =
                CreateAlgorithm(
                    passphrase,
                    salt,
                    iv,
                    iterations);

            byte[] plainBytes =
                Encoding.UTF8.GetBytes(plainText);

            using ICryptoTransform encryptor =
                algorithm.CreateEncryptor();

            byte[] encrypted =
                encryptor.TransformFinalBlock(
                    plainBytes,
                    0,
                    plainBytes.Length);

            return Convert.ToHexString(encrypted);
        }

        public static string Decrypt(
            string cipherText,
            string passphrase,
            string salt,
            byte[] iv,
            int iterations)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return string.Empty;

            byte[] cipherBytes =
                HexStringToBytes(cipherText);

            using var algorithm =
                CreateAlgorithm(
                    passphrase,
                    salt,
                    iv,
                    iterations);

            using ICryptoTransform decryptor =
                algorithm.CreateDecryptor();

            byte[] decrypted =
                decryptor.TransformFinalBlock(
                    cipherBytes,
                    0,
                    cipherBytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }

        private static SymmetricAlgorithm CreateAlgorithm(
            string passphrase,
            string salt,
            byte[] iv,
            int iterations)
        {
            if (string.IsNullOrWhiteSpace(passphrase))
                throw new ArgumentException(
                    "Passphrase is required.",
                    nameof(passphrase));

            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException(
                    "Salt is required.",
                    nameof(salt));

            if (iv == null || iv.Length != 16)
                throw new ArgumentException(
                    "IV must contain exactly 16 bytes.",
                    nameof(iv));

            using var rfc =
                new Rfc2898DeriveBytes(
                    passphrase,
                    Encoding.UTF8.GetBytes(salt),
                    iterations,
                    HashAlgorithmName.SHA512);

            byte[] key =
                rfc.GetBytes(32);

            var rij =
                new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    FeedbackSize = 128,
                    KeySize = 256,
                    BlockSize = 128,
                    Key = key,
                    IV = iv.ToArray()
                };

            return rij;
        }

        private static byte[] HexStringToBytes(
            string hex)
        {
            hex = hex.Trim();

            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException(
                    "Invalid hexadecimal string.");
            }

            byte[] result =
                new byte[hex.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] =
                    Convert.ToByte(
                        hex.Substring(i * 2, 2),
                        16);
            }

            return result;
        }

        public static string ByteArrayToHexString(
            byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }
    }
}