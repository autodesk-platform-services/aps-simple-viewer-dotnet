using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ForgeService _forgeService;

    public AuthController(ForgeService forgeService)
    {
        _forgeService = forgeService;
    }

    [HttpGet("token")]
    public async Task<dynamic> GetAccessToken()
    {
        var token = await _forgeService.GetPublicToken();
        return new
        {
            access_token = token.AccessToken,
            expires_in = Math.Round((token.ExpiresAt - DateTime.UtcNow).TotalSeconds)
        };
    }
}
