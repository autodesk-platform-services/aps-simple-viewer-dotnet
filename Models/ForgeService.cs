using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;

public class Token
{
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}

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
        DynamicJsonResponse _response = await api.GetObjectsAsync(_bucket, PageSize);
        var response = _response.ToObject<BucketObjects>();
        results.AddRange(response.Items);
        while (!string.IsNullOrEmpty(response.Next))
        {
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(response.Next).Query);
            _response = await api.GetObjectsAsync(_bucket, PageSize, null, queryParams["startAt"]);
            response = _response.ToObject<BucketObjects>();
            results.AddRange(response.Items);
        }
        return results;
    }

    public async Task<Manifest> GetManifest(string urn)
    {
        var token = await GetInternalToken();
        var api = new DerivativesApi();
        api.Configuration.AccessToken = token.AccessToken;
        DynamicJsonResponse _response = await api.GetManifestAsync(urn);
        var manifest = _response.ToObject<Manifest>();
        return manifest;
    }

    public async Task<ObjectDetails> UploadModel(string objectName, Stream content, long contentLength)
    {
        await EnsureBucketExists(_bucket);
        var token = await GetInternalToken();
        var api = new ObjectsApi();
        api.Configuration.AccessToken = token.AccessToken;
        dynamic _response = await api.UploadObjectAsync(_bucket, objectName, (int)contentLength, content);
        var obj = _response.ToObject<ObjectDetails>();
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
        dynamic _response = await api.TranslateAsync(payload);
        var job = _response.ToObject<Job>();
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
        return new Token
        {
            AccessToken = auth.access_token,
            ExpiresAt = DateTime.UtcNow.AddSeconds(auth.expires_in)
        };
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
    }
}