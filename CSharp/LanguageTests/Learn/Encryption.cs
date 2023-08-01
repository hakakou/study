using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LanguageTests
{
    public class EncryptionTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EncryptionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private static readonly byte[] salt =
            Encoding.Unicode.GetBytes("7NAASASA");

        // iterations must be at least 1000, we will use 2000
        private const int iterations = 2000;

        public static string Encrypt(string plainText, string password)
        {
            byte[] plainBytes = Encoding.Unicode.GetBytes(plainText);
            var aes = Aes.Create();
            var pbkd2 = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = pbkd2.GetBytes(32); // set a 256-bit key
            aes.IV = pbkd2.GetBytes(16); // set a 128-bit IV

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cryptoText, string password)
        {
            byte[] plainBytes = Convert.FromBase64String(cryptoText);
            var aes = Aes.Create();
            var pbkd2 = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = pbkd2.GetBytes(32); // set a 256-bit key
            aes.IV = pbkd2.GetBytes(16); // set a 128-bit IV

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }
            return Encoding.Unicode.GetString(ms.ToArray());
        }

        [Fact]
        public void TestFile()
        {
            string encryptedText = Encrypt("Test", "pass");
            string decryptedText = Decrypt(encryptedText, "pass");
            Assert.Equal("Test", decryptedText);
        }

        [Fact]
        public void TestHash()
        {
            var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            string password = "PASS";
            var sha = SHA256.Create();
            var saltedPassword = password + salt;
            var hash = sha.ComputeHash(Encoding.Unicode.GetBytes(saltedPassword));
        }

        public static string GenerateSignature(string data, RSA rsa)
        {
            byte[] dataBytes = Encoding.Unicode.GetBytes(data);
            var sha = SHA256.Create();
            var hashedData = sha.ComputeHash(dataBytes);
            
            return Convert.ToBase64String(rsa.SignHash(hashedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        }


        public static bool ValidateSignature(string data, string signature, RSA rsa)
        {
            byte[] dataBytes = Encoding.Unicode.GetBytes(data);
            var sha = SHA256.Create();
            var hashedData = sha.ComputeHash(dataBytes);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            return rsa.VerifyHash(hashedData, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        [Fact]
        public void TestSign()
        {
            // Creates a new ephemeral RSA key with the specified key size.
            var rsa = RSA.Create(2048);
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);

            var sig = GenerateSignature("test", rsa);

            rsa = RSA.Create();
            rsa.FromXmlString(publicKey);
            Assert.True(ValidateSignature("test", sig, rsa));
        }
    }
}
