using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Output_Model;

namespace asset_allocation_api.Controller;

[Route("api/[controller]")]
[ApiController]
public class ImsIntegrationController(ILogger<AssetAllocationsController> logger, IHttpClientFactory clientFactory) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly IHttpClientFactory _clientFactory = clientFactory;

    [HttpGet]
    public async Task<ActionResult<ImsResponse>> MinlogWifiCheck(int? sap)
    {
        try
        {
            var response = new ImsResponse()
            {
                Data = new ImsResponseData()
                {
                    UgAccessAllocation = new UgAccessAllocation(),
                    Face = "",
                    Email = "",
                    JobDesc = ""
                },
                Message = "",
                Code = 200
            };
            var sapValue = sap ?? 0;
            _logger.LogInformation("Get personnel info from IMS request received. SAP: {}", sapValue);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000); // 30sec

            // UG Access Allocation
            var ugAccessAllocationApiUrl = $"{Config.AssetAllocationConfig.UG_ACCESS_ALLOCATION_URI}/api/v1/UgAccessAllocation/CheckData?sap={sapValue}&hotstampNo=0&deviceId=0A400&face=1";
            var req = new HttpRequestMessage(HttpMethod.Get, ugAccessAllocationApiUrl);
            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.AssetAllocationConfig.microServiceToken}");
            var clientResponse = await client.SendAsync(req, cancellationTokenSource.Token);
            var responseBody = await clientResponse.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            logger.LogInformation("Deserializing response...");
            var ugAccessAllocationResponse = JsonSerializer.Deserialize<UgAccessAllocationInput>(responseBody);
            logger.LogInformation("Response parsed successfully. SAP: {}, CARD_NUM: {}", sapValue, ugAccessAllocationResponse.Data.CardNum);

            if (ugAccessAllocationResponse.Data.CardNum != null)
            {
                // Tyco Aggregator
                var tycoAggregatorApiUrl = $"{Config.AssetAllocationConfig.TYCO_AGGREGATOR_URI}/api/v1/personnel/search/{ugAccessAllocationResponse.Data.CardNum}/1/cardNum";
                var tycoAggregatorRequest = new HttpRequestMessage(HttpMethod.Get, tycoAggregatorApiUrl);
                var tycoClient = clientFactory.CreateClient();
                tycoClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.AssetAllocationConfig.microServiceToken}");
                var tycoAggregatorClientResponse = await tycoClient.SendAsync(tycoAggregatorRequest, cancellationTokenSource.Token);
                var tycoAggregatorResponseBody = await tycoAggregatorClientResponse.Content.ReadAsStringAsync(cancellationTokenSource.Token);

                logger.LogInformation("Deserializing tyco aggregator response...");
                var tycoAggregatorResponse = JsonSerializer.Deserialize<TycoAggregatorInput>(tycoAggregatorResponseBody);
                logger.LogInformation("Tyco aggregator response parsed successfully. SAP: {}", sapValue);
            

                response.Data.UgAccessAllocation = ugAccessAllocationResponse.Data;
                response.Data.Face = tycoAggregatorResponse.Tyco.Face;
                response.Data.Email = tycoAggregatorResponse.IMS.Email;
                response.Data.JobDesc = tycoAggregatorResponse.IMS.JobDesc;   
            }
            else
            {
                response.Data.UgAccessAllocation = ugAccessAllocationResponse.Data;
            }
            
            return response;
        }
        catch (Exception e)
        {
            logger.LogError("An error occurred when checking personnel data from IMS. Error: {}", e.Message);
            
            return new ImsResponse()
            {
                Data = null,
                Message = $"Unexpected error occurred. Message: {e.Message}",
                Code = 500
            };
        }
    }
}