namespace Argus.Infrastructure.Storage
{
    public interface ITenantLogoStorage
    {
        Task<string> SaveLogoAsync(Guid tenantId, Stream logoStream, string contentType);
        Task<(Stream Stream, string ContentType)> GetLogoAsync(Guid tenantId);
    }

    public class AzureBlobTenantLogoStorage : ITenantLogoStorage
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobTenantLogoStorage(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = "tenant-logos";
        }

        public async Task<string> SaveLogoAsync(Guid tenantId, Stream logoStream, string contentType)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync();

            var blobName = $"{tenantId}/logo{Path.GetExtension(contentType)}";
            var blob = container.GetBlobClient(blobName);

            await blob.UploadAsync(logoStream, new BlobHttpHeaders { ContentType = contentType });
            return blob.Uri.ToString();
        }

        public async Task<(Stream Stream, string ContentType)> GetLogoAsync(Guid tenantId)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobs = container.GetBlobsAsync(prefix: tenantId.ToString());

            await foreach (var blobItem in blobs)
            {
                var blob = container.GetBlobClient(blobItem.Name);
                var properties = await blob.GetPropertiesAsync();
                var stream = await blob.OpenReadAsync();
                
                return (stream, properties.Value.ContentType);
            }

            throw new FileNotFoundException("Logo not found");
        }
    }
}