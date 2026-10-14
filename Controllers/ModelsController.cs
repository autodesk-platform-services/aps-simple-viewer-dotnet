using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    public record BucketObject(string name, string urn);

    private static readonly Regex ForbiddenFilenameChars = new(@"[<>""`\\/\x00-\x1f\x7f]");

    private static bool IsSafeFilename(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length <= 255 && !ForbiddenFilenameChars.IsMatch(name);
    }

    private readonly APS _aps;

    public ModelsController(APS aps)
    {
        _aps = aps;
    }

    [HttpGet()]
    public async Task<IEnumerable<BucketObject>> GetModels()
    {
        var objects = await _aps.GetObjects();
        return from o in objects
               select new BucketObject(o.ObjectKey, APS.Base64Encode(o.ObjectId));
    }

    [HttpGet("{urn}/status")]
    public async Task<TranslationStatus> GetModelStatus(string urn)
    {
        var status = await _aps.GetTranslationStatus(urn);
        return status;
    }

    public class UploadModelForm
    {
        [FromForm(Name = "model-zip-entrypoint")]
        public string Entrypoint { get; set; }

        [FromForm(Name = "model-file")]
        public IFormFile File { get; set; }
    }

    [HttpPost(), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAndTranslateModel([FromForm] UploadModelForm form)
    {
        if (form.File == null)
        {
            return BadRequest("The required field (\"model-file\") is missing.");
        }
        if (!IsSafeFilename(form.File.FileName))
        {
            return BadRequest("The filename contains invalid characters.");
        }
        using var stream = form.File.OpenReadStream();
        var obj = await _aps.UploadModel(form.File.FileName, stream);
        var job = await _aps.TranslateModel(obj.ObjectId, form.Entrypoint);
        return Ok(new BucketObject(obj.ObjectKey, job.Urn));
    }
}
