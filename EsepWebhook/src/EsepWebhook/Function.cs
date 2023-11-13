using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        public string FunctionHandler(object input, ILambdaContext context)
        {
            try
            {
                // Log the raw JSON payload
                context.Logger.LogLine($"Raw JSON Payload: {input}");

                // Attempt to deserialize the input
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

                // Log the deserialized JSON object
                context.Logger.LogLine($"Deserialized JSON: {json}");

                // Extract information from the JSON and continue with your logic...
                string payload = $"{{'text':'Issue Created: {json.issue.html_url}'}}";

                var client = new HttpClient();
                var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = client.Send(webRequest);
                using var reader = new StreamReader(response.Content.ReadAsStream());

                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                // Log any exceptions
                context.Logger.LogLine($"Exception: {ex}");
                throw; // Rethrow the exception to propagate it further
            }
        }
    }
}
