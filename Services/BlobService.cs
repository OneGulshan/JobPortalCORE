using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;

namespace JobPortalCORE.Services // Tera namespace
{
    public class BlobService
    {
        // Yahan apni original connection string daalna (Storage account ki)
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=jobportalblob;AccountKey=adis1DjZMR6BEJ9VnZ3CJnPQfG2d9ao66NCUiQOI4HZKgUo/cBFfckSU//eoHxEadLircvydNuPy+AStm5bPZw==;EndpointSuffix=core.windows.net";

        public string GetResumeSecureUrl(string blobFileName)
        {
            // 'compressed-images' container se connect kar rahe hain
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("compressed-images");
            BlobClient blobClient = containerClient.GetBlobClient(blobFileName);

            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5) // 5 minute wala secure link
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
            return null;
        }
    }
}