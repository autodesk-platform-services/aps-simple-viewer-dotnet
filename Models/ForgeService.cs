using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace forge_simple_viewer_dotnet
{
    public class Bucket
    {
        public string Name { get; set; }
    }

    public class Object
    {
        public string Name { get; set; }
        public string URN { get; set; }
    }

    public class Token
    {
        public string AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public interface IForgeService
    {
        Task<IEnumerable<Object>> GetObjects();
        Task<Token> GetAccessToken();
    }

    public class ForgeService : IForgeService
    {
        private readonly TwoLeggedApi _twoLeggedApi = new TwoLeggedApi();
        private readonly BucketsApi _bucketsApi = new BucketsApi();
        private readonly ObjectsApi _objectsApi = new ObjectsApi();
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

        public async Task<IEnumerable<Object>> GetObjects()
        {
            await EnsureBucketExists(_bucket);
            var objects = new List<Object>();
            var token = await GetInternalToken();
            _objectsApi.Configuration.AccessToken = token.AccessToken;
            dynamic response = await _objectsApi.GetObjectsAsync(_bucket, 100);
            foreach (KeyValuePair<string, dynamic> obj in new DynamicDictionaryItems(response.items))
            {
                objects.Add(new Object
                {
                    Name = obj.Value.objectKey,
                    URN = Base64Encode(obj.Value.objectId)
                });
            }
            return objects;
        }

        public async Task<Token> GetAccessToken()
        {
            return await GetPublicToken();
        }

        private async Task EnsureBucketExists(string bucketKey)
        {
            var token = await GetInternalToken();
            _bucketsApi.Configuration.AccessToken = token.AccessToken;
            try
            {
                dynamic details = await _bucketsApi.GetBucketDetailsAsync(bucketKey);
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == 404)
                {
                    await _bucketsApi.CreateBucketAsync(new PostBucketsPayload
                    {
                        BucketKey = bucketKey,
                        PolicyKey = PostBucketsPayload.PolicyKeyEnum.Temporary
                    });
                }
                else
                {
                    throw e;
                }
            }
        }

        private async Task<Token> GetPublicToken()
        {
            if (_publicTokenCache == null || _publicTokenCache.ExpiresAt < DateTime.UtcNow)
            {
                _publicTokenCache = await GetToken(new Scope[] { Scope.ViewablesRead });
            }
            return _publicTokenCache;
        }

        private async Task<Token> GetInternalToken()
        {
            if (_internalTokenCache == null || _internalTokenCache.ExpiresAt < DateTime.UtcNow)
            {
                _internalTokenCache = await GetToken(new Scope[] { Scope.BucketCreate, Scope.BucketRead, Scope.DataRead, Scope.DataWrite, Scope.DataCreate });
            }
            return _internalTokenCache;
        }

        private async Task<Token> GetToken(Scope[] scopes)
        {
            dynamic auth = await _twoLeggedApi.AuthenticateAsync(
                _clientId,
                _clientSecret,
                "client_credentials",
                scopes
            );
            return new Token
            {
                AccessToken = auth.access_token,
                ExpiresAt = DateTime.UtcNow.AddSeconds(auth.expires_in)
            };
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
        }
    }
}