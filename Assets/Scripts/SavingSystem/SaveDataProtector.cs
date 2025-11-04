using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Device;

public static class SaveDataProtector
{
    private static readonly string staticKey = "BurnThisCityHashKey";

    private static byte[] GetDynamicKey()
    {
        string deviceID = staticKey + SystemInfo.deviceUniqueIdentifier;
        return Encoding.UTF8.GetBytes(deviceID);
    }

    public static string Protect(string jsonString)
    {
        byte[] keyBytes = GetDynamicKey();
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

        string base64Data = Convert.ToBase64String(jsonBytes);

        string hashString;
        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(base64Data));
            hashString = Convert.ToBase64String(hashBytes);
        }

        return $"{base64Data}.{hashString}";
    }

    public static string ValidateAndLoad(string protectedString)
    {
        byte[] keyBytes = GetDynamicKey();

        string[] parts = protectedString.Split('.');
        if (parts.Length != 2)
        {
            throw new InvalidOperationException("Formato de guardado inválido o corrupto.");
        }

        string base64Data = parts[0];
        string expectedHash = parts[1];

        string actualHash;
        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(base64Data));
            actualHash = Convert.ToBase64String(hashBytes);
        }

        if (actualHash != expectedHash)
        {
            throw new CryptographicException("¡Archivo de guardado corrupto o modificado! Carga abortada.");
        }

        byte[] jsonBytes = Convert.FromBase64String(base64Data);
        return Encoding.UTF8.GetString(jsonBytes);
    }
}