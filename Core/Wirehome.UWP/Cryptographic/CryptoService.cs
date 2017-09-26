using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace Wirehome.Contracts.Cryptographic
{
    public class CryptoService : ICryptoService
    {
        private static readonly HashAlgorithmProvider HashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        public string GenerateHash(string input)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashBuffer = HashAlgorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToBase64String(hashBuffer);
        }

        public string GenerateSignature(string key, string content)
        {
            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var macAlgorithm = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var macKey = macAlgorithm.CreateKey(keyMaterial);

            var buffer = CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8);

            var signatureBuffer = CryptographicEngine.Sign(macKey, buffer);
            var signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer);

            return signature;
        }
    }
}
