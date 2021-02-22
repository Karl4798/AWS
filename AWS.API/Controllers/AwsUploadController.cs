using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Amazon.Runtime;
using Amazon.S3;
using AWS.Infrastructure.Models;
using Amazon.S3.Transfer;
using System.IO;
using Amazon.S3.Model;

namespace AWS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AwsUploadController : Controller
    {

        static string accessKey = "AKIAITWN43J3V3SLXMMQ";
        static string secretKey = "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";

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload(PolicyRequest policy, string policyRef)
        {
            try
            {
                string guid = Guid.NewGuid().ToString();

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.EUWest2
                };
                using var client = new AmazonS3Client(credentials, config);
                await using var memoryStream = new MemoryStream(Convert.FromBase64String(policy.PolicyPDF));

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    Key = guid,
                    BucketName = "karl",
                    CannedACL = S3CannedACL.PublicRead,
                    ContentType = "application/pdf"
                };

                // https://docs.aws.amazon.com/sdkfornet1/latest/apidocs/html/M_Amazon_S3_AmazonS3Client_GetPreSignedURL.htm //

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = "karl",
                    Key = guid,
                    Expires = DateTime.Now.AddMinutes(5)
                };

                string path = client.GetPreSignedURL(request);

                // --------------------------------------------------------------------------------------------------------- //

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return Ok();
        }
    }
}