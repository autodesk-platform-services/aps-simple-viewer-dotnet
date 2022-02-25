using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;

public record Token(string AccessToken, DateTime ExpiresAt);

public record TranslationStatus(string Status, string Progress, IEnumerable<string>? Messages);

public class ForgeService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _bucket;
    private Token _internalTokenCache;
    private Token _publicTokenCache;

    public ForgeService(string clientId, string clientSecret, string bucket = null)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _bucket = string.IsNullOrEmpty(bucket) ? string.Format("{0}-basic-app", _clientId.ToLower()) : bucket;
    }

    public async Task<IEnumerable<ObjectDetails>> GetObjects()
    {
        const int PageSize = 64;
        await EnsureBucketExists(_bucket);
        var token = await GetInternalToken();
        var api = new ObjectsApi();
        api.Configuration.AccessToken = token.AccessToken;
        var results = new List<ObjectDetails>();
        var response = (await api.GetObjectsAsync(_bucket, PageSize)).ToObject<BucketObjects>();
        results.AddRange(response.Items);
        while (!string.IsNullOrEmpty(response.Next))
        {
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(response.Next).Query);
            response = (await api.GetObjectsAsync(_bucket, PageSize, null, queryParams["startAt"])).ToObject<BucketObjects>();
            results.AddRange(response.Items);
        }
        return results;
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

    public async Task<ObjectDetails> UploadModel(string objectName, Stream content, long contentLength)
    {
        await EnsureBucketExists(_bucket);
        var token = await GetInternalToken();
        var api = new ObjectsApi();
        api.Configuration.AccessToken = token.AccessToken;
        var obj = (await api.UploadObjectAsync(_bucket, objectName, (int)contentLength, content)).ToObject<ObjectDetails>();
        return obj;
    }

    public async Task<Job> TranslateModel(string objectId, string rootFilename)
    {
        var token = await GetInternalToken();
        var api = new DerivativesApi();
        api.Configuration.AccessToken = token.AccessToken;
        var formats = new List<JobPayloadItem> {
            new JobPayloadItem (JobPayloadItem.TypeEnum.Svf, new List<JobPayloadItem.ViewsEnum> { JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._2d })
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

    private async Task EnsureBucketExists(string bucketKey)
    {
        var token = await GetInternalToken();
        var api = new BucketsApi();
        api.Configuration.AccessToken = token.AccessToken;
        try
        {
            await api.GetBucketDetailsAsync(bucketKey);
        }
        catch (ApiException e)
        {
            if (e.ErrorCode == 404)
            {
                await api.CreateBucketAsync(new PostBucketsPayload(bucketKey, null, PostBucketsPayload.PolicyKeyEnum.Temporary));
            }
            else
            {
                throw e;
            }
        }
    }

    public async Task<Token> GetPublicToken()
    {
        if (_publicTokenCache == null || _publicTokenCache.ExpiresAt < DateTime.UtcNow)
            _publicTokenCache = await GetToken(new Scope[] { Scope.ViewablesRead });
        return _publicTokenCache;
    }

    private async Task<Token> GetInternalToken()
    {
        if (_internalTokenCache == null || _internalTokenCache.ExpiresAt < DateTime.UtcNow)
            _internalTokenCache = await GetToken(new Scope[] { Scope.BucketCreate, Scope.BucketRead, Scope.DataRead, Scope.DataWrite, Scope.DataCreate });
        return _internalTokenCache;
    }

    private async Task<Token> GetToken(Scope[] scopes)
    {
        dynamic auth = await new TwoLeggedApi().AuthenticateAsync(_clientId, _clientSecret, "client_credentials", scopes);
        return new Token(auth.access_token, DateTime.UtcNow.AddSeconds(auth.expires_in));
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
    }
}