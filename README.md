# ImageFaceProcessingDemonstration

A demonstration on how to leverage Azure Compter Vision API and Functions to process faces on pictures. 

The Function will listen for any newly uploaded Blobs to an Azure Storage Account Blob Container. If detected, it will leverage [Azure Computer Vision](https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/) to determine the meta information out of the images. These information will be stored in a table storage in the same storage account. After the analysis, the image will be moved to another blob in order to persist the image and keep the incoming "queue" empty. 
