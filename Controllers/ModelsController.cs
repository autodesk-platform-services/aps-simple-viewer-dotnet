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

    public record TranslationStatus(string status, string progress, IEnumerable<string>? messages);

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

    [HttpGet("{urn}/status")]
    public async Task<TranslationStatus> GetModelStatus(string urn)
    {
        var messages = new List<string>();
        try
        {
            var manifest = await _forgeService.GetManifest(urn);
            foreach (var derivative in manifest.Derivatives)
            {
                foreach (var child in derivative.Children)
                {
                    if (child.Messages != null)
                    {
                        foreach (var message in child.Messages)
                            messages.AddRange(message._Message);
                    }
                }
            }
            return new TranslationStatus(manifest.Status, manifest.Progress, messages);
        }
        catch (Autodesk.Forge.Client.ApiException ex)
        {
            if (ex.ErrorCode == 404)
                return new TranslationStatus("n/a", "", messages);
            else
                throw ex;
        }
    }

    public class UploadModelForm
    {
        [FromForm(Name = "model-zip-entrypoint")]
        public string Entrypoint { get; set; }

        [FromForm(Name = "model-file")]
        public IFormFile? File { get; set; }
    }

    [HttpPost()]
    public async Task<BucketObject> UploadAndTranslateModel([FromForm] UploadModelForm form)
    {
        using (var stream = new MemoryStream())
        {
            await form.File.CopyToAsync(stream);
            stream.Position = 0;
            var obj = await _forgeService.UploadModel(form.File.FileName, stream, form.File.Length);
            var job = await _forgeService.TranslateModel(obj.ObjectId, form.Entrypoint);
            return new BucketObject(obj.ObjectKey, job.Urn);
        }
    }
}