using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using VideoProcessing.Entities;

namespace VideoProcessing
{
    public static class Starter
    {
        [FunctionName("Starter")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, [OrchestrationClient] DurableOrchestrationClient starter, TraceWriter log)
        {
            // Parsing query parameter
            //string video = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "video", true) == 0).Value;

            // Reads the request body
            //dynamic data = await req.Content.ReadAsAsync<VideoAMS>();
            string json = await req.Content.ReadAsStringAsync();
            var videoModel = JsonConvert.DeserializeObject<VideoAMS>(json);

            // Sets up the content
            string _accessPolicy = videoModel.AccessPolicyName;
            string _assetName = videoModel.AssetName;
            string _storageAccountName = videoModel.StorageAccountName;
            string _videoPath = videoModel.VideoPath;
            string _videoFileName = videoModel.VideoFileName;

            // Is it valid?
            if (string.IsNullOrEmpty(_accessPolicy) || string.IsNullOrEmpty(_assetName) || string.IsNullOrEmpty(_storageAccountName) || string.IsNullOrEmpty(_videoPath) || string.IsNullOrEmpty(_videoFileName))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Invalid input data. Expected: Access Policy, Asset Name, StorageAccount Name, Video Path, Video Name.");
            }

            log.Info($"All set! Starting the orchestration process for {_videoFileName}...");

            // DTO binding
            //VideoAMS videoDto = new VideoAMS();
            //videoDto.AccessPolicy = _accessPolicy;
            //videoDto.AssetName = _assetName;
            //videoDto.StorageAccountName = _storageAccountName;
            //videoDto.VideoFileName = _videoFileName;
            //videoDto.VideoPath = _videoPath;

            // Starting the orchestration process
            var orchestrationId = await starter.StartNewAsync("O_Orchestrator", videoModel);

            // Checking orchestration status
            return starter.CreateCheckStatusResponse(req, orchestrationId);
        }
    }
}