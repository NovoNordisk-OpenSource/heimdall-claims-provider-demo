namespace Heimdall.ClaimsProviderDemo;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomClaimsProviderTrigger(ILogger<CustomClaimsProviderTrigger> logger)
{
    private readonly ILogger<CustomClaimsProviderTrigger> _logger = logger;

    [Function("CustomClaimsProviderTrigger")]
    public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        // Fetch request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Deserialize the request body
        dynamic? data = JsonSerializer.Deserialize<Data>(requestBody);

        // Read the correlation ID from the Azure AD  request    
        string? correlationId = data?.data.authenticationContext.correlationId;

        // Claims to return to Azure AD
        ResponseContent response = new();

        response.Data.Actions[0].Claims.CorrelationId = correlationId;
        response.Data.Actions[0].Claims.ApiVersion = "1.0.0";
        response.Data.Actions[0].Claims.DateOfBirth = "01/01/2000";
        response.Data.Actions[0].Claims.CustomRoles.Add("Writer");
        response.Data.Actions[0].Claims.CustomRoles.Add("Editor");

        return new OkObjectResult(response);
    }

    public class ResponseContent{
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        public ResponseContent()
        {
            Data = new Data();
        }
    }

    public class Data{
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

        [JsonPropertyName("actions")]
        public List<Action> Actions { get; set; }

        public Data()
        {
            ODataType = "microsoft.graph.onTokenIssuanceStartResponseData";
            Actions = [new Action()];
        }
    }

    public class Action{
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }

        public Action()
        {
            ODataType = "microsoft.graph.tokenIssuanceStart.provideClaimsForToken";
            Claims = new Claims();
        }
    }

    public class Claims{
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? CorrelationId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? DateOfBirth { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ApiVersion { get; set; }

        public List<string> CustomRoles { get; set; }

        public Claims()
        {
            CustomRoles = [];
        }
    }
}


