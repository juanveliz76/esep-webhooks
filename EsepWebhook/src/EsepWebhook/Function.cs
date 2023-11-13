using System;
using System.IO;
using System.Net.Http;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public string FunctionHandler(object input, ILambdaContext context)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

                string payload = $"{{\"text\":\"Issue Created: {json.issue.html_url}\"}}";

                var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                {
                    Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
                };

                var response = _httpClient.Send(webRequest);
                using var reader = new StreamReader(response.Content.ReadAsStream());

                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, rethrow it, etc.)
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
