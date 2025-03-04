using iText.Kernel.Crypto;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirmaDigitalCR
{
    public class RSASignature : IExternalSignature
    {
        private readonly RSA _privateKey;

        public RSASignature(RSA privateKey)
        {
            _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        }

        public string GetDigestAlgorithmName()
        {
            return DigestAlgorithms.SHA256;
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA"; // Algoritmo de cifrado
        }

        public string GetHashAlgorithm()
        {
            return DigestAlgorithms.SHA256; // Algoritmo de firma
        }

        public string GetSignatureAlgorithmName()
        {
            return "RSA"; // Algoritmo de firma
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return null; // No se necesitan parámetros adicionales
        }

        public byte[] Sign(byte[] message)
        {
            return _privateKey.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
