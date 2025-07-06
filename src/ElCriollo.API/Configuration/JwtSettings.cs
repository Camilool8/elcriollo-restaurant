namespace ElCriollo.API.Configuration;

/// <summary>
/// Configuración para JSON Web Tokens (JWT)
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Clave secreta para firmar los tokens JWT
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Emisor del token (quien lo genera)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audiencia del token (para quien está destinado)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de expiración del token en minutos
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 60;

    /// <summary>
    /// Tiempo de expiración del refresh token en días
    /// </summary>
    public int RefreshTokenExpiryInDays { get; set; } = 7;

    /// <summary>
    /// Validar si la configuración JWT es válida
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SecretKey) && 
               SecretKey.Length >= 32 && 
               !string.IsNullOrEmpty(Issuer) && 
               !string.IsNullOrEmpty(Audience) &&
               ExpiryInMinutes > 0 &&
               RefreshTokenExpiryInDays > 0;
    }
}