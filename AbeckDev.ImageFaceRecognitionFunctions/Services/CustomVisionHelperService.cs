using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace AbeckDev.ImageFaceRecognitionFunctions.Services
{
    public static class CustomVisionHelperService
    {   


        public static bool IsPersonDetectedOnPicture(ImageAnalysis imageAnalysis)
        {

            //Check if persons are detected as object or as tag
            if(imageAnalysis.Objects.Where(o => o.ObjectProperty == "person").Count() > 0 ||
               imageAnalysis.Tags.Where(t => t.Name == "person").Count() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
