using System;
using System.Collections.Generic;
using System.Text;

namespace HA4IoT.Contracts.Cryptographic
{
    public interface ICryptoService
    {
        string GenerateHash(string input);
        string GenerateSignature(string key, string content);
    }
}
