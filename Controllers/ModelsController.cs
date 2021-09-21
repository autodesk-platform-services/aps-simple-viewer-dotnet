using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    public record BucketObject(string name, string urn);

    private readonly ForgeService _forgeService;

    public ModelsController(ForgeService forgeService)
    {
        _forgeService = forgeService;
    }

    [HttpGet()]
    public async Task<IEnumerable<BucketObject>> GetModels()
    {
        var objects = await _forgeService.GetObjects();
        return from o in objects
               select new BucketObject(o.ObjectKey, ForgeService.Base64Encode(o.ObjectId));
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