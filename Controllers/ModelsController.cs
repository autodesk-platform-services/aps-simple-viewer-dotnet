using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        public async Task<ActionResult<string>> GetModels()
        {
            var objects = await _forgeService.GetObjects();
            return JsonConvert.SerializeObject(objects);
        }

        public class UploadModelForm
        {
            [FromForm(Name = "model-zip-entrypoint")]
            public string Entrypoint { get; set; }

            [FromForm(Name = "model-file")]
            public IFormFile File { get; set; }
        }

        [HttpPost()]
        public async Task UploadAndTranslateModel([FromForm] UploadModelForm form)
        {
            // For some reason we cannot use the incoming stream directly...
            // so let's save the model into a local temp file first
            var tmpPath = Path.GetTempFileName();
            using (var stream = new FileStream(tmpPath, FileMode.OpenOrCreate))
            {
                await form.File.CopyToAsync(stream);
            }
            using (var stream = System.IO.File.OpenRead(tmpPath))
            {
                dynamic obj = await _forgeService.UploadModel(form.File.FileName, stream, form.File.Length);
                await _forgeService.TranslateModel(obj.objectId, form.Entrypoint);
            }
            System.IO.File.Delete(tmpPath);
        }
    }
}