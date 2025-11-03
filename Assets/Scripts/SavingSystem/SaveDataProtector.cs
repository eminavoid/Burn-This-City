using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Herramienta estática para proteger y validar los datos de guardado.
/// Usa HMAC-SHA256 para la integridad y Base64 para la obfuscación.
/// </summary>
public static class SaveDataProtector
{
    // ¡¡¡CAMBIA ESTA CLAVE POR ALGO ÚNICO PARA TU JUEGO!!!
    // (Puede ser una frase larga, un GUID, etc.)
    private static readonly string secretKey = "mi_juego_es_genial_y_nadie_lo_hackea_123!";

    public static string Protect(string jsonString)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
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

    /// <summary>
    /// Valida y desprotege el string guardado.
    /// Devuelve el JSON original si es válido.
    /// Lanza una excepción si el hash no coincide (tampered).
    /// </summary>
    public static string ValidateAndLoad(string protectedString)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

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