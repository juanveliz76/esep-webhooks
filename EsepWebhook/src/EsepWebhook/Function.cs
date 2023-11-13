using System;
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
        /// <summary>
        /// Lambda function handler for processing GitHub webhook events.
        /// </summary>
        /// <param name="input">The input data from the GitHub webhook.</param>
        /// <param name="context">The Lambda execution context.</param>
        /// <returns>A string representing the result of the function.</returns>
        public string FunctionHandler(object input, ILambdaContext context)
        {
            try
            {
                // Log the input data for inspection.
                context.Logger.LogLine($"Input data: {input}");

                // Deserialize the input data.
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
                
                // Extract the issue URL.
                string issueUrl = json.issue.html_url.ToString();
                
                // Log the extracted issue URL.
                context.Logger.LogLine($"Issue URL: {issueUrl}");

                // Construct the payload for the Slack message.
                string payload = $"{{'text':'Issue Created: {issueUrl}'}}";

                // Create an HTTP client and send the Slack message.
                using (var client = new HttpClient())
                {
                    var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };

                    var response = client.Send(webRequest);

                    using (var reader = new StreamReader(response.Content.ReadAsStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                // Log the JSON parsing error.
                context.Logger.LogLine($"Error parsing JSON: {ex.Message}");
                // Handle the error or return an appropriate response.
                return $"Error parsing JSON: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Log other exceptions.
                context.Logger.LogLine($"Error: {ex.Message}");
                // Handle the error or return an appropriate response.
                return $"Error: {ex.Message}";
            }
        }
    }
}
