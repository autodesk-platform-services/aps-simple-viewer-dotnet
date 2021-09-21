using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public record AccessToken(string access_token, long expires_in);

    private readonly ForgeService _forgeService;

    public AuthController(ForgeService forgeService)
    {
        _forgeService = forgeService;
    }

    [HttpGet("token")]
    public async Task<AccessToken> GetAccessToken()
    {
        var token = await _forgeService.GetPublicToken();
        return new AccessToken(
            token.AccessToken,
            (long)Math.Round((token.ExpiresAt - DateTime.UtcNow).TotalSeconds)
        );
    }
}