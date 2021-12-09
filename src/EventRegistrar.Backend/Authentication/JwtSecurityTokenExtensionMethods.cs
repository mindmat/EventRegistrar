using System.IdentityModel.Tokens.Jwt;

namespace EventRegistrar.Backend.Authentication;

public static class JwtSecurityTokenExtensionMethods
{
    public static string GetClaim(this JwtSecurityToken token, string claimKey)
    {
        return token.Claims.FirstOrDefault(clm => clm.Type == claimKey)?.Value;
    }
}