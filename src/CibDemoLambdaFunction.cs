using System.Net;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace CibDemo
{
    public class CibDemoLambdaFunction
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public CibDemoLambdaFunction()
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"));//Amazon.RegionEndpoint.EUWest3;
            _dynamoDbClient = new AmazonDynamoDBClient(clientConfig);
        }

        public async Task<APIGatewayProxyResponse> GetAllFunctionHandler(dynamic input, ILambdaContext context)
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = "CibDemoTable",
                };

                ScanResponse response = await _dynamoDbClient.ScanAsync(request);

                var message = response.Items.Select(item => new { Id = item["Id"].S, Message = item["Message"].S });

                return new APIGatewayProxyResponse()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = System.Text.Json.JsonSerializer.Serialize(message),
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error scanning DynamoDB table: {ex.Message}");
                throw;
            }
        }
    }
}