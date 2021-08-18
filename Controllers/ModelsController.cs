using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace forge_simple_viewer_dotnet
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly ILogger<ModelsController> _logger;
        private readonly IForgeService _forgeService;

        public ModelsController(ILogger<ModelsController> logger, IForgeService forgeService)
        {
            _logger = logger;
            _forgeService = forgeService;
        }

        [HttpGet()]
        public async Task<IEnumerable<forge_simple_viewer_dotnet.Object>> GetModels()
        {
            var objects = await _forgeService.GetObjects();
            return objects;
        }
    }
}