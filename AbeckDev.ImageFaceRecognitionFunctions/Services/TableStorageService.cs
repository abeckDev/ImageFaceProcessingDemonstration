using System;
using AbeckDev.ImageFaceRecognitionFunctions.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Cosmos.Table;

namespace AbeckDev.ImageFaceRecognitionFunctions.Services
{
    public class TableStorageService
    {
        CloudTableClient cloudTableClient;

        CloudTable cloudTable;
        const string TableName = "pictureMetaInformation";

        public TableStorageService()
        {
            cloudTableClient = CloudStorageAccount.Parse(System.Environment.GetEnvironmentVariable("AzureWebJobsStorage")).CreateCloudTableClient();
            cloudTable = CreateTableIfNotExist(TableName);
        }



        CloudTable CreateTableIfNotExist(string TableName)
        {
            CloudTable table = cloudTableClient.GetTableReference(TableName);
            table.CreateIfNotExistsAsync();
            return table;
        }

        public ImageMetainformation WriteImageMetada(ImageAnalysis imageAnalysis, string fileName)
        {
            //Write Image MetaInformation to the Table Storage
            var metaData = new ImageMetainformation()
            {
                FileName = DateTime.Now.ToString("dd-MM-yy")+"_"+fileName,
                PartitionKey = "ImageAnalysis",
            };

            //Read all descriptions
            foreach (var description in imageAnalysis.Description.Captions)
            {
                metaData.Description += " - " + description.Text;
            }

            //Read all tags
            foreach (var tag in imageAnalysis.Description.Tags)
            {
                metaData.Tags += ";" + tag;
            }

            //Read all detected Objects
            foreach (var detectedObject in imageAnalysis.Objects)
            {
                metaData.DetectedObjects += " - " + detectedObject.ObjectProperty;
            }

            metaData.ArePeopleInthePicture = CustomVisionHelperService.IsPersonDetectedOnPicture(imageAnalysis);
            
            //Check for faces
            if (imageAnalysis.Faces.Count > 0)
            {
                //Faces found
                metaData.AreFacesInThePicture = true;
            }
            else
            {
                //No faces detected
                metaData.AreFacesInThePicture = false;
            }
            //Write to Table Storage
            TableResult result = cloudTable.Execute(TableOperation.InsertOrReplace(metaData));



            return metaData;

        }

    }
}
