using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.Authentication;
using Autodesk.Authentication.Model;

public record Token(string AccessToken, DateTime ExpiresAt);

public partial class APS
{
    private Token _internalTokenCache;
    private Token _publicTokenCache;

    private async Task<Token> GetToken(List<Scopes> scopes)
    {
        var authenticationClient = new AuthenticationClient(_sdkManager);
        var auth = await authenticationClient.GetTwoLeggedTokenAsync(_clientId, _clientSecret, scopes);
        return new Token(auth.AccessToken, DateTime.UtcNow.AddSeconds((double)auth.ExpiresIn));
    }

    public async Task<Token> GetPublicToken()
    {
        if (_publicTokenCache == null || _publicTokenCache.ExpiresAt < DateTime.UtcNow)
            _publicTokenCache = await GetToken(new List<Scopes> { Scopes.ViewablesRead });
        return _publicTokenCache;
    }

    private async Task<Token> GetInternalToken()
    {
        if (_internalTokenCache == null || _internalTokenCache.ExpiresAt < DateTime.UtcNow)
            _internalTokenCache = await GetToken(new List<Scopes> { Scopes.BucketCreate, Scopes.BucketRead, Scopes.DataRead, Scopes.DataWrite, Scopes.DataCreate });
        return _internalTokenCache;
    }
}
