using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.ModelDerivative;
using Autodesk.ModelDerivative.Model;
using System;

public record TranslationStatus(string Status, string Progress, IEnumerable<string>? Messages);

public partial class APS
{
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes).TrimEnd('=');
    }

    public async Task<Job> TranslateModel(string objectId, string rootFilename)
    {

        ModelDerivativeClient modelDerivativeClient =new ModelDerivativeClient(_SDKManager);
        var token = await GetInternalToken();

        List<JobPayloadFormat> outputFormats = new()
        {
            // initialising an Svf2 output class will automatically set the type to Svf2.
             new JobSvf2OutputFormat()
            {
                Views =  new List<View>()
                        {
                        View._2d,
                        View._3d
                        },
            },
            // initialising a Thumbnail output class will automatically set the type to Thumbnail.
            new JobThumbnailOutputFormat()
            {
                    Advanced = new JobThumbnailOutputFormatAdvanced(){
                        Width = Width.NUMBER_100,
                        Height = Height.NUMBER_100
                    }
            }
        };
        JobPayload Job = new()
        {
            Input = new JobPayloadInput()
            {
                Urn = objectId,
                CompressedUrn = false,
                RootFilename = rootFilename,
            },
            Output = new JobPayloadOutput()
            {
                Formats = outputFormats,
                Destination = Region.US // This will call the respective endpoint - Either US or EMEA. Defaults to US.
            },
        };

        Job jobResponse = null!;
        try
        {
            jobResponse = await modelDerivativeClient.StartJobAsync(jobPayload: Job, accessToken: token.AccessToken);
        }
        catch
        (ModelDerivativeApiException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return jobResponse!;
    }
    public async Task<TranslationStatus> GetTranslationStatus(string urn)
    {
        var token = await GetInternalToken();
        ModelDerivativeClient modelDerivativeClient = new ModelDerivativeClient(_SDKManager);
        var messages = new List<string>();
        string progress = null!;
        string status = null!;

        try
        {
            Manifest manifestResponse = await modelDerivativeClient.GetManifestAsync(urn, accessToken: token.AccessToken);
            progress = manifestResponse.Progress;
            status = manifestResponse.Status;
        }
        catch (ModelDerivativeApiException ex)
        {
            messages.Add((string)ex.Message);
        }
        return new TranslationStatus((string)status!, progress!, messages);
    }
}