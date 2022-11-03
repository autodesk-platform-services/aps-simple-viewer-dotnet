using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;

public record TranslationStatus(string Status, string Progress, IEnumerable<string>? Messages);

public partial class APS
{
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
    }

    public async Task<Job> TranslateModel(string objectId, string rootFilename)
    {
        var token = await GetInternalToken();
        var api = new DerivativesApi();
        api.Configuration.AccessToken = token.AccessToken;
        var formats = new List<JobPayloadItem> {
            new JobPayloadItem (JobPayloadItem.TypeEnum.Svf, new List<JobPayloadItem.ViewsEnum> { JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d })
        };
        var payload = new JobPayload(
            new JobPayloadInput(Base64Encode(objectId)),
            new JobPayloadOutput(formats)
        );
        if (!string.IsNullOrEmpty(rootFilename))
        {
            payload.Input.RootFilename = rootFilename;
            payload.Input.CompressedUrn = true;
        }
        var job = (await api.TranslateAsync(payload)).ToObject<Job>();
        return job;
    }

    public async Task<TranslationStatus> GetTranslationStatus(string urn)
    {
        var token = await GetInternalToken();
        var api = new DerivativesApi();
        api.Configuration.AccessToken = token.AccessToken;
        var json = (await api.GetManifestAsync(urn)).ToJson();
        var messages = new List<string>();
        foreach (var message in json.SelectTokens("$.derivatives[*].messages[?(@.type == 'error')].message"))
        {
            if (message.Type == Newtonsoft.Json.Linq.JTokenType.String)
                messages.Add((string)message);
        }
        foreach (var message in json.SelectTokens("$.derivatives[*].children[*].messages[?(@.type == 'error')].message"))
        {
            if (message.Type == Newtonsoft.Json.Linq.JTokenType.String)
                messages.Add((string)message);
        }
        return new TranslationStatus((string)json["status"], (string)json["progress"], messages);
    }
}
