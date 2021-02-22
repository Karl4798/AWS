using AWS.Infrastructure.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWS
{
    class Program
    {
        static string apiEndpoint = "https://localhost:44379/";
        static string apiMethod = "AwsUpload";
        static string policyRef = "1";

        static async Task Main(string[] args)
        {
            Console.Write("Drop a PDF file on the console and press enter to upload to s3 bucket: ");
            string filePath = Console.ReadLine();

            await UploadImage(filePath);
        }

        public static async Task UploadImage(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open);

                await using var newMemoryStream = new MemoryStream();
                file.CopyTo(newMemoryStream);

                PolicyRequest policy = new PolicyRequest()
                {
                    PolicyPDF = Convert.ToBase64String(newMemoryStream.ToArray())
                };

                HttpRequestMessage clientRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiEndpoint + apiMethod + "/Upload?" + "policyRef=" + policyRef),
                    Content = new StringContent(JsonSerializer.Serialize(policy), Encoding.UTF8, "application/json")
                };
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.SendAsync(clientRequest).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Uploaded PDF File Successfully");
                }
                else
                {
                    Console.WriteLine("Could Not Upload PDF File");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling AWS.API: " + ex.Message);
            }
            Console.ReadKey();
        }
    }
}