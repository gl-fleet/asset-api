using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using asset_allocation_api.Context;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Output_Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;

namespace asset_allocation_api.Controller;

[Route("api/[controller]")]
[ApiController]
public class MinlogIntegrationController(
    ILogger<MinlogIntegrationController> logger,
    IHttpClientFactory clientFactory,
    AppDbContext context)
    : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly AppDbContext _context = context;

    
    [HttpGet]
    public async Task<ActionResult<Response<MinlogAssignResponse?>>> MinlogWifiCheck(string? rfid)
    {
        Response<MinlogAssignResponse?> response = new();
        try
        {
            _logger.LogInformation("***********************WIFI CHECK***********************************************");
            _logger.LogInformation("[Wifi Check] [RFID: {}] Request received", rfid);

            ConfigurationsController cfg = new(_context, NullLogger<ConfigurationsController>.Instance, null);
            IEnumerable<Configuration> cfgList = await cfg.GetAllConfigurations();

            
            // Check minlog wifi detection config
            var departmentId = Request.Headers["Departmentid"].ToString();
            if (string.IsNullOrEmpty(departmentId))
                return BadRequest("Departmentid is null");
            Configuration? wifiDetectionConfig =
                cfgList.FirstOrDefault(i => i.ConfigDesc == Config.AssetAllocationConfig.WIFI_CHECK_CONFIG_NAME && i.DepartmentId == int.Parse(departmentId));

            // If minlog wifi detection config is null
            if (wifiDetectionConfig == null)
            {
                logger.LogError("[Wifi Check] [RFID: {}] Config not set. ConfigDesc: {}. Duration: {}", rfid, Config.AssetAllocationConfig.WIFI_CHECK_CONFIG_NAME, DateTime.Now - response.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false,
                    $"{Config.AssetAllocationConfig.WIFI_CHECK_CONFIG_NAME} config is not set!");
            }

            switch (wifiDetectionConfig.IsEnabled)
            {
                // If minlog wifi detection config disabled
                case 0:
                    logger.LogInformation("[Wifi Check] [RFID: {}] Config disabled. Duration: {}", rfid, DateTime.Now - response.ExecuteStart);
                    return ResponseUtils.ReturnResponse(_logger, null, response, null, 200, true,
                        $"{Config.AssetAllocationConfig.WIFI_CHECK_CONFIG_NAME} config is disabled. Please bypass minlog wifi detection step!");
                // If minlog wifi detection config disabled
                case 1:
                {
                    logger.LogInformation("[Wifi Check] [RFID: {}] Config enabled. But skip spare caplamps Duration: {}", rfid, DateTime.Now - response.ExecuteStart);
                    var asset = context.Assets.FirstOrDefault(a => a.Rfid == rfid);
                    if (asset == null)
                        return ResponseUtils.ReturnResponse(_logger, null, response, null, 200, true,
                            $"{rfid} asset not found!");
                    if (asset.Description == "Spare")
                    {
                        return ResponseUtils.ReturnResponse(_logger, null, response, null, 200, true,
                            $"{Config.AssetAllocationConfig.WIFI_CHECK_CONFIG_NAME} config is enabled but skip Spare capllamps. Please bypass minlog wifi detection step!");                        
                    }
                    return ResponseUtils.ReturnResponse(_logger, null, response, null, 200, true,
                        $"{rfid} asset not found!");
                }
            }

            logger.LogInformation("[Wifi Check] [RFID: {}] Sending wifi check request to minlog assign.", rfid);
            var rfidValue = rfid ?? "NO_RFID_PROVIDED";
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(30000); // 30sec

            var apiUrl = $"{Config.AssetAllocationConfig.MINLOG_ASSIGN_URI}/api/v1/checkWifiStatus/{rfidValue}";
            var req = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            var client = clientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {Config.AssetAllocationConfig.MINLOG_ASSIGN_TOKEN}");
            var clientResponse = await client.SendAsync(req, cancellationTokenSource.Token);
            var responseBody = await clientResponse.Content.ReadAsStringAsync(cancellationTokenSource.Token);
            logger.LogInformation("[Wifi Check] [RFID: {}] Response: {}", rfid, responseBody.Replace("\"", string.Empty));
            try
            {
                var wifiCheckResponse = JsonSerializer.Deserialize<MinlogAssignInput>(responseBody);
                logger.LogInformation("[Wifi Check] [RFID: {}] Response parsed successfully. DETECTION_TIME: {}, RESPONSE_TIME: {}, Duration: {}",
                wifiCheckResponse.Data.rfid, wifiCheckResponse.Data.detectionTime,
                wifiCheckResponse.Data.responseTime, DateTime.Now - response.ExecuteStart);

                if (wifiCheckResponse.Data.status == "200" || (wifiCheckResponse.Data.message == "WIFI_FOUND" ||
                                                               wifiCheckResponse.Data.message == "MULE_ERROR"))
                {
                    try
                    {
                        var minlogServerTime = DateTime.ParseExact(wifiCheckResponse.Data.responseTime,
                            "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                        var wifiDetectionTime = DateTime.ParseExact(wifiCheckResponse.Data.detectionTime, 
                            "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

                        var difference = (minlogServerTime - wifiDetectionTime).TotalMinutes;
                        var fromNow = DateTime.Now.AddMinutes(-difference);

                        if (difference > int.Parse(Config.AssetAllocationConfig.MINLOG_WIFI_CHECK_DETECTION_MINUTE))
                        {
                            _logger.LogError("[Wifi Check] [RFID: {}] Detected {} minutes ago. Please perform another Wi-Fi check. Duration: {}", rfid, (int)difference, DateTime.Now - response.ExecuteStart);
                            return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false,
                                $"Detected {(int)difference} minutes ago / Please perform another Wi-Fi check!");
                        }
                        else
                        {
                            _logger.LogInformation("[Wifi Check] [RFID: {}] Completed successfully. Minlog system: {}. Duration: {}", rfid, fromNow, DateTime.Now - response.ExecuteStart);
                            return ResponseUtils.ReturnResponse(_logger, null, response, null, 200, false,
                                $"Minlog system: {fromNow}");
                        }
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("[Wifi Check] [RFID: {}] Invalid detection time. Message: {}. Duration: {}", rfid, e.Message, DateTime.Now - response.ExecuteStart);
                        return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false,
                            $"Minlog system: Invalid detection time / {e.Message}");
                    }
                }
                _logger.LogError("[Wifi Check] [RFID: {}] Error occurred wifi check. Message: {}. Duration: {}", rfid, wifiCheckResponse.Data.message, DateTime.Now - response.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false,
                    wifiCheckResponse.Data.message);
            }
            catch (JsonException)
            {
                try
                {
                    var wifiCheckResponse = JsonSerializer.Deserialize<MinlogErrorData>(responseBody);
                    return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false, $"Wifi check error. Message: {wifiCheckResponse.Message}");
                }
                catch (JsonException)
                {
                    logger.LogError("[Wifi Check] [RFID: {}] Error occurred deserializing minlog response.");    
                    return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false, $"Can not check this caplamp.");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError("An error occurred when checking wifi detection from Minlog. Error: {}. Duration: {}", e.Message, DateTime.Now - response.ExecuteStart);
            return ResponseUtils.ReturnResponse(_logger, null, response, null, 500, false,
                $"Unexpected error occurred. Message: {e.Message}");
        }
    }
}