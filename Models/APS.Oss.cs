using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.Oss;
using Autodesk.Oss.Model;
using System.Threading;

public partial class APS
{
    private async Task EnsureBucketExists(string bucketKey)
    {
        var token = await GetInternalToken();
        OssClient ossClient = new OssClient(_SDKManager);
        CreateBucketsPayload policyKey = new() { PolicyKey = "Persistent", BucketKey = _bucket };
        try
        {
            await ossClient.GetBucketDetailsAsync(bucketKey, accessToken: token.AccessToken);
        }
        catch (OssApiException e)
        {
            if (e.HttpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                await ossClient.CreateBucketAsync("US", policyKey, accessToken: token.AccessToken);
            else
            {
                throw e;
            }
        }
    }
    public async Task<ObjectDetails> UploadModel(string objectName, string sourceToUpload)
    {
        await EnsureBucketExists(_bucket);
        var token = await GetInternalToken();
        OssClient OssApi = new OssClient(_SDKManager);
        ObjectDetails response = await OssApi.Upload(_bucket, objectName, sourceToUpload, accessToken: token.AccessToken, CancellationToken.None);
        return response;
    }
    public async Task<IEnumerable<ObjectDetails>> GetObjects()
    {
        const int PageSize = 64;
        await EnsureBucketExists(_bucket);
        var token = await GetInternalToken();
        OssClient ossClient = new OssClient(_SDKManager);
        var results = new List<ObjectDetails>();
        var response = await ossClient.GetObjectsAsync(_bucket, PageSize, accessToken: token.AccessToken);
        results.AddRange(response.Items);
        while (!string.IsNullOrEmpty(response.Next))
        {
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(response.Next).Query);
            response = await ossClient.GetObjectsAsync(_bucket, PageSize, null, queryParams["startAt"]);
            results.AddRange(response.Items);
        }
        return results;
    }
}