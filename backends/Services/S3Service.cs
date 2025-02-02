using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;

    public S3Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string bucketName, string fileName, string contentType)
    {
        var putRequest = new Amazon.S3.Model.PutObjectRequest
        {
            InputStream = fileStream,
            BucketName = bucketName,
            Key = fileName,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{bucketName}.s3.amazonaws.com/{fileName}";
    }
}
