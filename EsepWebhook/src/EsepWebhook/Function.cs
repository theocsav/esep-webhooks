using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient http = new HttpClient();
        private static readonly string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

        // API Gateway (proxy) → Lambda handler
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (string.IsNullOrWhiteSpace(slackUrl))
            {
                context.Logger.LogError("Missing SLACK_URL environment variable.");
                return new APIGatewayProxyResponse { StatusCode = 500, Body = "Server not configured." };
            }

            try
            {
                // GitHub sends JSON in the request body
                var body = string.IsNullOrEmpty(request?.Body) ? "{}" : request.Body;
                var payload = JObject.Parse(body);

                // Extract from GitHub Issues event
                var action  = payload.SelectToken("action")?.ToString() ?? "unknown";
                var issue   = payload.SelectToken("issue");
                var issueUrl = issue?["html_url"]?.ToString();
                var title    = issue?["title"]?.ToString();
                var repo     = payload.SelectToken("repository.full_name")?.ToString();
                var sender   = payload.SelectToken("sender.login")?.ToString();

                string text = issueUrl != null
                    ? $"GitHub Issue *{action}* in `{repo}` by `{sender}`\n*{title}*\n{issueUrl}"
                    : "Received a webhook (not an Issues payload).";

                var slackBody = JsonConvert.SerializeObject(new { text });
                var resp = await http.PostAsync(slackUrl, new StringContent(slackBody, Encoding.UTF8, "application/json"));
                resp.EnsureSuccessStatusCode();

                return new APIGatewayProxyResponse { StatusCode = 200, Body = "ok" };
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return new APIGatewayProxyResponse { StatusCode = 500, Body = "error" };
            }
        }
    }
}
