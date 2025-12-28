namespace Encryption;

public class JwtToken
{
    public string EncryptedToken { get; set; }

#if DEBUG
    public Guid TokenId { get; set; }
    public DateTime ExpireTime { get; set; }
    public IDictionary<string, string> UserClaims { get; set; }
#endif
}