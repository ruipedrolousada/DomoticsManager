using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    private static RSACryptoServiceProvider rsaManager;
    private static RSACryptoServiceProvider rsaAgent;

    public static void GenerateRSAKeys(out string publicKey, out string privateKey)
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            publicKey = Convert.ToBase64String(rsa.ExportCspBlob(false)); // Chave Pública
            privateKey = Convert.ToBase64String(rsa.ExportCspBlob(true)); // Chave Privada
        }
    }

    public static void ImportRSAPublicKey(string publicKey, bool isManager)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportCspBlob(Convert.FromBase64String(publicKey));

        if (isManager)
            rsaManager = rsa;
        else
            rsaAgent = rsa;
    }

    public static void ImportRSAPrivateKey(string privateKey, bool isManager)
    {
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportCspBlob(Convert.FromBase64String(privateKey));

        if (isManager)
            rsaManager = rsa;  //Store Private Key for Manager
        else
            rsaAgent = rsa;  // Unused for now
    }


    public static string EncryptWithRSA(string data, bool isManager)
    {
        var rsa = isManager ? rsaManager : rsaAgent;
        var encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(data), false);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string DecryptWithRSA(string encryptedData, bool isManager)
    {
        var rsa = isManager ? rsaManager : rsaAgent;
        var decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedData), false);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public static string GenerateAESKey()
    {
        using (var aes = Aes.Create())
        {
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
    }

    public static string EncryptWithAES(string data, string key)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Convert.FromBase64String(key);
            aes.GenerateIV();
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encryptedBytes);
            }
        }
    }

    public static string DecryptWithAES(string encryptedData, string key)
    {
        var parts = encryptedData.Split(':');
        var iv = Convert.FromBase64String(parts[0]);
        var encryptedBytes = Convert.FromBase64String(parts[1]);

        using (var aes = Aes.Create())
        {
            aes.Key = Convert.FromBase64String(key);
            aes.IV = iv;
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
