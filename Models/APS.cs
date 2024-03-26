using Autodesk.SDKManager;

public partial class APS
{
    private readonly SDKManager _sdkManager;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _bucket;

    public APS(string clientId, string clientSecret, string bucket = null)
    {
        _sdkManager = SdkManagerBuilder.Create().Build();
        _clientId = clientId;
        _clientSecret = clientSecret;
        _bucket = string.IsNullOrEmpty(bucket) ? string.Format("{0}-basic-app", _clientId.ToLower()) : bucket;
    }
}
