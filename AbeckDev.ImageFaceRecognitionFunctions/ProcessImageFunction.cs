using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Azure.Storage.Blobs;
using AbeckDev.ImageFaceRecognitionFunctions.Services;

namespace AbeckDev.ImageFaceRecognitionFunctions
{
    public static class ProcessImageFunction
    {
        [FunctionName("ProcessImageFunction")]
        public static async Task RunAsync([BlobTrigger("imagestoprocess/{name}", Connection = "")]Stream incomingImageBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {incomingImageBlob.Length} Bytes");

            //In production setup at least the key needs to be secured via a Vault System like Azure Key Vault
            string COMPUTE_VISION_KEY = System.Environment.GetEnvironmentVariable("VISION_SUBSCRIPTION_KEY");
            string COMPUTE_VISION_ENDPOINT = System.Environment.GetEnvironmentVariable("VISION_API_ENDPOINT");

            //Get Tabkle Storage Client
            var tableStorageService = new TableStorageService();
            //Get StorageAccount Client
            BlobServiceClient storageAccountClient = new BlobServiceClient(System.Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var incomingBlobClient = storageAccountClient.GetBlobContainerClient("imagestoprocess");
            var outgoingBlobClient = storageAccountClient.GetBlobContainerClient("processedimages");


            //Authenticate with Vision API
            ComputerVisionClient visionClient = new ComputerVisionClient(new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(COMPUTE_VISION_KEY))
            {
                Endpoint = COMPUTE_VISION_ENDPOINT
            };

            //Define list of attributes to extract
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            try
            {
                //Get computer vision result from API 
                var visionResult = await visionClient.AnalyzeImageInStreamAsync(incomingImageBlob, features);

                //Check if persons and faces could be detected
                if (CustomVisionHelperService.IsPersonDetectedOnPicture(visionResult))
                {
                    //Persons are detected in the picture, proceed with face detection

                    if(visionResult.Faces.Count() >= 1)
                    {
                        //More than one face detected
                        var debug1 = "Found People and Faces";
                    }
                    else
                    {
                        //No Face detected
                        var debug2 = "Found people but no faces";
                    }

                }

                //No people could be detected (if not hit before ;) )
                var debug3 = "No person or Faces detected!";

                //Write Results to Table storage
                var metaInformation = tableStorageService.WriteImageMetada(visionResult, name);


                //Write Picture to the new location
                var download = await incomingBlobClient.GetBlobClient(name).DownloadContentAsync();
                await outgoingBlobClient.UploadBlobAsync(metaInformation.FileName,download.Value.Content);


                //Clean up in the incoming images blob container
                await incomingBlobClient.DeleteBlobIfExistsAsync(name);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            

        }
    }
}
