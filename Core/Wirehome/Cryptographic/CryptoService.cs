using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Wirehome.Contracts.Cryptographic;

namespace Wirehome.Cryptographic
{
    public class CryptoService: ICryptoService
    {
        public string GenerateHash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Encoding.UTF8.GetString(result);
            }
        }

        public string GenerateSignature(string key, string content)
        {
            var body = Encoding.UTF8.GetBytes(content);
            var secret = Encoding.UTF8.GetBytes(key);

            using (var sha = HMACSHA1.Create())
            {
                var key1 = sha.ComputeHash(body);
                var key2 = key1.Concat(secret).ToArray();
                var key3 = sha.ComputeHash(key2);
                return Convert.ToBase64String(key3);
            }
        }

    }
}
