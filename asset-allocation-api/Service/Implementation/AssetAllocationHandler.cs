using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using asset_allocation_api.Config;
using asset_allocation_api.Context;
using asset_allocation_api.Controller;
using asset_allocation_api.Model.CustomModel;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Output_Model;
using asset_allocation_api.Service.Producer;
using asset_allocation_api.Util;
using Status = asset_allocation_api.Model.Output_Model.Status;

namespace asset_allocation_api.Service.Implementation;

public class AssetAllocationHandler(
    AppDbContext context, ILogger<AssetAllocationHandler> logger, 
    IHttpClientFactory clientFactory,
    KafkaDependentProducer<string, string> kafkaProducer)
{
    private readonly AppDbContext _context = context;
    private readonly KafkaDependentProducer<string, string> _kafkaProducer = kafkaProducer;
    private readonly IHttpClientFactory _clientFactory = clientFactory;
    private readonly ILogger _logger = logger;
    
    public async Task<ResponseGeneric> AssignCaplampWithPli(Asset asset, string departmentId, int personnelNo, int personnelId)
    {
        var resp = new ResponseGeneric()
        {
            Message = "",
            Status = new Status()
            {
                Success = true,
                Code = 200
            }
        };
        try
        {
            // Check asset type name is Minlog, PLI integrated caplamp
            if (asset.AssetType!.Name.IndexOf(AssetAllocationConfig.CAPLAMP_WITH_PLI_ASSET_TYPE_NAME, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Caplamp PLI assign detected. Starting PLI with Caplamp assignment...", departmentId, personnelNo, asset.Id);                   

                var caplampWithPliInThisShift = _context.AssetAllocations.Where(aa =>
                        aa.Asset.AssetType.Name.Contains(AssetAllocationConfig.CAPLAMP_WITH_PLI_ASSET_TYPE_NAME) &&
                        aa.AssignedDate >= DateTime.Now.AddDays(-56) /** Default is 56, which equals to 4 roster **/ &&
                        aa.PersonnelNo == personnelNo &&
                        aa.ReturnedDate == null)
                    .Select(a => a.Asset.DepartmentId)
                    .ToList();

                var caplampWithPliCountInThisShift = caplampWithPliInThisShift.Count;

                // Only OTUG Underground Guide attended personnels take many Caplamp with PLI asset
                if (caplampWithPliCountInThisShift >= 1)
                {
                    if(asset.Description == "Spare")
                    {
                        // 1. Will do Minlog WIFI check ( Done in Front-End )
                        // 2. Should not Assign Minlog or PLI
                        _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Multiple Caplamp with PLI detected. Type: {} Skipping the caplamp assignment...", departmentId, personnelNo, asset.Id, "UG Spare");                   
                        resp.Message = $"Skipping the Caplamp Assign process ...";
                        return resp;
                    }

                    // Ignoring this for now, Since next lines are written to cover this process
                    if(asset.Description == "Visitor") {}
                    else if(!string.IsNullOrEmpty(departmentId) && departmentId != caplampWithPliInThisShift[0].ToString()) {
                        _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Multiple Caplamp with PLI detected. Type: {} The user has not qualified the required training.", departmentId, personnelNo, asset.Id, "Visitor");                   
                        resp.Message = $"User already allocated Caplamp with PLI: {asset.Description ?? "Normal" } / Department: #{caplampWithPliInThisShift[0]}";
                        resp.Status.Code = 400;
                        resp.Status.Success = false;
                        return resp;
                    }

                    if (await IsAttendedUndergroundGuide(personnelNo) == false)
                    {
                        _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Multiple Caplamp with PLI detected. Type: {} The user has not qualified the required training.", departmentId, personnelNo, asset.Id, "Visitor");                   
                        resp.Message = $"User already allocated Caplamp with PLI: {asset.Description ?? "Normal" }";
                        resp.Status.Code = 400;
                        resp.Status.Success = false;
                        return resp;
                    } 
                    else
                    {
                        /** He/She is Guide -> Still can get caplamp from other departments!!! **/
                    }
                }

                if (string.IsNullOrEmpty(departmentId))
                {
                    _logger.LogError("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Department id is null", departmentId, personnelNo, asset.Id);                   
                    throw new Exception("Departmentid is null");                    
                }

                ConfigurationsController cfg = new(_context, NullLogger<ConfigurationsController>.Instance, null);
                var cfgList = await cfg.GetAllConfigurations();

                // Check PLI, Minlog integration config value
                var pliCaplampToggleConfig = cfgList.FirstOrDefault(i =>
                    i.ConfigDesc == AssetAllocationConfig.PLI_CAPLAMP_TOGGLE_CONFIG_NAME &&
                    i.DepartmentId == int.Parse(departmentId));

                // If PLI, Caplamp toggle config is not set return internal server error
                if (pliCaplampToggleConfig == null)
                {
                    _logger.LogError("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] {} config is not set", departmentId, personnelNo, asset.Id, AssetAllocationConfig.PLI_CAPLAMP_TOGGLE_CONFIG_NAME);                   
                    resp.Message = $"Please set {AssetAllocationConfig.PLI_CAPLAMP_TOGGLE_CONFIG_NAME} value.";
                    resp.Status.Code = 400;
                    resp.Status.Success = false;
                    return resp;
                }

                // If PLI, Caplamp toggle config is disabled. Only send tag assign request to PLI.
                if (pliCaplampToggleConfig.IsEnabled == 0)
                {
                    _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] {} config disabled. Sending request to only PLI...", departmentId, personnelNo, asset.Id, AssetAllocationConfig.PLI_CAPLAMP_TOGGLE_CONFIG_NAME);                   
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(30000); // 30sec

                    var apiUrl = $"{AssetAllocationConfig.PLI_ASSIGN_URI}/api/v1/CaplampAssignments/assign";
                    var req = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    var client = clientFactory.CreateClient();

                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.AssetAllocationConfig.microServiceToken}");

                    req.Content = asset.Mac2 != null ? 
                        new StringContent(string.Format(AssetAllocationConfig.caplampBodyJson, personnelId, personnelNo, asset.Mac2, asset.Rfid), Encoding.UTF8, "application/json") : 
                        new StringContent(string.Format(AssetAllocationConfig.caplampBodyJson, personnelId, personnelNo, asset.Serial, asset.Rfid), Encoding.UTF8, "application/json");
                    
                    var clientResponse = await client.SendAsync(req, cancellationTokenSource.Token);
                    var responseBody = await clientResponse.Content.ReadAsStringAsync(cancellationTokenSource.Token);

                    var pliResp = JsonSerializer.Deserialize<ResponseGeneric>(responseBody);
                    _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] PLI response: {}", departmentId, personnelNo, asset.Id, pliResp.Message);                                       
                    resp.Message = pliResp.Message;
                    resp.Status.Code = pliResp.Status.Code;
                    resp.Status.Success = pliResp.Status.Success;
                }
                // If PLI, Caplamp toggle config is enabled. Send tag assign request to Caplamp.
                else
                {
                    _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] {} config enabled. Sending request to PLI, MinLog...", departmentId, personnelNo, asset.Id, AssetAllocationConfig.PLI_CAPLAMP_TOGGLE_CONFIG_NAME);                   
                    
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(30000); // 30sec
                    
                    var assignRequests = AssetAllocationConfig.AssignmentSettings
                        .Where(item => item.Assignable && (item.SystemIdentityName == "PLI" || item.SystemIdentityName == "Minlog"))
                        .Select(async item => await SendCaplampAssignmentRequest(asset, personnelId, personnelNo, departmentId, item.BaseURL, item.SystemIdentityName));
                    
                    var results = await Task.WhenAll(assignRequests);

                    if (results.Any(r => r.Status.Success == false))
                    {
                        resp.Status.Code = 400;
                        resp.Status.Success = false;
                        resp.Message = string.Join(" ", results.Select(r => $"[{r.Data}:{r.Message}]").ToList());
                        return resp;
                    }
                    
                    resp.Message = string.Join(" ", results.Select(r => $"[{r.Data}:{r.Message}]").ToList());
                    resp.Status = results.LastOrDefault()?.Status ?? resp.Status;

                    _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Caplamp with PLI assignment completed successfully.", departmentId, personnelNo, asset.Id);                   
                    
                    // var caplampResp = JsonSerializer.Deserialize<ResponseGeneric>(results);
                    var message = "";
                    resp.Message = message;
                    resp.Status.Code = 200;
                    resp.Status.Success = true;
                }
            }   
            return resp;
        }
        catch (Exception e)
        {
            _logger.LogError("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Unexpected error occurred during Caplamp with PLI assignment. Error: {}", departmentId, personnelNo, asset.Id, e.Message);                   
            resp.Message = $"Unhandled error occurred on caplamp assign request. Message: {e.Message}";
            resp.Status.Code = 500;
            resp.Status.Success = false;
            return resp;
        }
    }

    private async Task<ResponseGeneric> SendCaplampAssignmentRequest(Asset asset, int personnelId, int personnelNo, string departmentId, string apiUrl = null, string systemIdentityName = null)
    {
        try
        {
            apiUrl ??= $"{AssetAllocationConfig.PLI_ASSIGN_URI}/api/v1/CaplampAssignments/assign";

            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {Config.AssetAllocationConfig.microServiceToken}");

            var requestBodyJson = asset.Mac2 != null && systemIdentityName == "PLI"
                ? string.Format(AssetAllocationConfig.caplampBodyJson, personnelId, personnelNo, asset.Mac2, asset.Rfid)
                : string.Format(AssetAllocationConfig.caplampBodyJson, personnelId, personnelNo, asset.Serial, asset.Rfid);

            _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Request body: {}", departmentId, personnelNo, asset.Id, requestBodyJson);                   

            HttpContent httpContent = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Sending request to {} started. URL: {}", departmentId, personnelNo, asset.Id, systemIdentityName, apiUrl);
            var requestStart = DateTime.Now;
            var response = await client.PostAsync(apiUrl, httpContent);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Sending request to only {} ended. Duration: {} Response: {}", departmentId, personnelNo, asset.Id, systemIdentityName, DateTime.Now - requestStart, responseBody);                                       
            var result = new ResponseGeneric
            {
                Data = systemIdentityName,
                Message = "Success",
                Status = new Status
                {
                    Code = 200,
                    Success = true
                }
            };

            if (!response.IsSuccessStatusCode)
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    result.Data = systemIdentityName;
                    result.Message = "Caplamp or user not found in system.";
                    result.Status.Code = 404;
                    result.Status.Success = false;
                    return result;
                }
                else
                {
                    result.Data = systemIdentityName;
                    result.Message = "Can not assign caplamp for the user.";
                    result.Status.Code = 500;
                    result.Status.Success = false;
                    return result;
                }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Error occurred while sending caplamp assignment request. Error: {}", departmentId, personnelNo, asset.Id, ex);                                       
            return new ResponseGeneric
            {
                Data = systemIdentityName,
                Message = "Error occurred while sending request.",
                Status = new Status() { Success = false, Code = 500 }
            };
        }
    }

    public async Task<ResponseGeneric> RemoveCaplampWithPli(Asset asset, int personnelNo)
    {
        var resp = new ResponseGeneric()
        {
            Message = "",
            Status = new Status()
            {
                Success = true,
                Code = 200
            }
        };
        try
        {
            asset.AssetType = await _context.AssetTypes.FindAsync(asset.AssetTypeId);
            // PersonnelId not found error handling
            var personnel = await _context.Personnel.Where(p => p.PersonnelNo == personnelNo).FirstOrDefaultAsync();
            if (personnel == null)
            {
                resp.Message = $"Personnel not found in asset allocation system.";
                resp.Status.Code = 500;
                resp.Status.Success = false;
                return resp;
            }
            
            if (asset.AssetType!.Name!.IndexOf(AssetAllocationConfig.CAPLAMP_WITH_PLI_ASSET_TYPE_NAME, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (asset.Description == "Visitor")
                {
                    resp.Message = "Skipped visitor or spare caplamp";
                    return resp;
                }
                logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [ Asset: {}] Caplamp PLI return detected. PLI tag remove started...", asset.DepartmentId, personnelNo, asset.Id);                   
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(30000); // 30sec

                var apiUrl = $"{AssetAllocationConfig.caplampURI}remove";
                logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [ Asset: {}] PLI tag request URL : {}", asset.DepartmentId, personnelNo, asset.Id, apiUrl);                   

                var req = new HttpRequestMessage(HttpMethod.Delete, apiUrl);
                var client = clientFactory.CreateClient();

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AssetAllocationConfig.microServiceToken}");
                if (asset.Mac2 != null)
                {
                    req.Content = new StringContent(string.Format(AssetAllocationConfig.caplampBodyJson, personnel.PersonnelId, personnelNo, asset.Mac2, asset.Rfid), Encoding.UTF8, "application/json");                    
                }
                else
                {
                    req.Content = new StringContent(string.Format(AssetAllocationConfig.caplampBodyJson, personnel.PersonnelId, personnelNo, asset.Serial, asset.Rfid), Encoding.UTF8, "application/json");
                }

                var clientResponse = await client.SendAsync(req, cancellationTokenSource.Token);
                var responseBody = await clientResponse.Content.ReadAsStringAsync(cancellationTokenSource.Token);
                logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [ Asset: {}] PLI remove response: {}", asset.DepartmentId, personnelNo, asset.Id, responseBody);                                   
                var caplampResp = JsonSerializer.Deserialize<ResponseGeneric>(responseBody);

                resp.Message = "Tag remove failed.";
                resp.Status.Code = caplampResp.status.code;
                resp.Status.Success = caplampResp.status.success;
            }
            return resp;
        }
        catch (Exception e)
        {
            logger.LogError("[ReturnAsset] [Department: {}] [Personnel: {}] [ Asset: {}] Unhandled error occurred on caplamp assign request. Message: {}", asset.DepartmentId, personnelNo, asset.Id, e.Message);                   
            resp.Message = $"Unhandled error occurred on caplamp assign request. Message: {e.Message}";
            resp.Status.Code = 500;
            resp.Status.Success = false;
            return resp;
        }
    }

    public async Task<string> SendToSignalR(AssetAllocation assetAllocation)
    {
        var resp = "";
        try
        {
            _logger.LogDebug("Serializing signalR data");
            assetAllocation.Asset = null;
            assetAllocation.Assets = new List<Asset>();
            await _kafkaProducer.ProduceAsync(AssetAllocationConfig.kafkaSocketAllocationTopic,
                new Message<string, string>
                {
                    Key = assetAllocation.PersonnelNo.ToString(),
                    Value = JsonSerializer.Serialize(assetAllocation)
                });
            _logger.LogDebug("Kafka message produced successfully");
            resp = "SignalR data sent successfully";
        }
        catch (JsonException e)
        {
            _logger.LogError("Json exception occurred on SignalR allocation. Message: {}", e.Message);
            resp = $"SignalR data failed. Message: {e.Message}";
        }
        catch (Exception e)
        {
            _logger.LogError("Unhandled error occurred on SignalR allocation. Message: {}", e.Message);
            resp = $"SignalR data failed. Message: {e.Message}";
        }
        return resp;
    }

    public async Task<ResponseGeneric> CheckQualification(Asset asset, string personnelNo)
    {
        var resp = new ResponseGeneric()
        {
            Message = "",
            Status = new Status()
            {
                Success = true,
                Code = 200
            }
        };

        try
        {
            asset.AssetType.TypeTrainings = _context.TypeTrainings.Where(a => a.AssetTypeId == asset.AssetType.Id).ToList();
            // Tuhain asset type deer training tohiruulsan baival
            if (asset.AssetType.TypeTrainings.Any())
            {
                var trainings = "";
                // Tohiruulsan buh training shalgana
                foreach (var training in asset.AssetType.TypeTrainings)
                {
                    var trainingCode = training.TrainingId;
                    Dictionary<string, string> dict = new()
                    {
                        { "saps", personnelNo},
                        { "qualifications", trainingCode.ToString() },
                        { "options", "boolean" }
                    };

                    // var qualresponse = await HttpUtils.CallMicroservice<string>(_logger, AssetAllocationConfig.imsPersonnelQualifications, HttpMethod.Get, AssetAllocationConfig.microServiceToken, queryParams: dict);
                    // QualificationDetails qualificationDetails = JsonSerializer.Deserialize<QualificationDetails>(qualresponse);

                    // if (qualificationDetails.Data.personnel.FirstOrDefault().attendances.FirstOrDefault().valid)
                    // {
                        resp.Status.Success = true;
                        resp.Status.Code = 200;
                        return resp; // Return immediately if any qualification is invalid
                    // }
                }
                // If all qualifications are valid
                resp.Status.Success = false;
                resp.Status.Code = 500;
                resp.Message = "Qualifications are expired or not attended";
            }
            else
            {
                resp.Message = "No qualifications required.";
            }
            return resp;
        }
        catch (Exception e)
        {
            resp.Message = $"Qualification check failed! Error: {e.Message}";
            resp.Status.Success = false;
            resp.Status.Code = 500;
            return resp;
        }
    }

    public async Task<bool> IsAttendedUndergroundGuide(int personnelNo)
    {
        try
        {
            // OTUG Underground Guide
            var trainingCode = "31369817";
            var otugUndergroundGuideQualCode = _context.Configurations.FirstOrDefault(c =>
                c.ConfigDesc == "OTUG Underground Guide qualification code" && 
                c.IsEnabled == 1);

            if (otugUndergroundGuideQualCode != null)
            {
                trainingCode = otugUndergroundGuideQualCode.ConfigValue;
            }
            Dictionary<string, string> dict = new()
            {
                { "saps", personnelNo.ToString()},
                { "qualifications", trainingCode },
                { "options", "boolean" }
            };

            // var qualresponse = await HttpUtils.CallMicroservice<string>(_logger, AssetAllocationConfig.imsPersonnelQualifications, HttpMethod.Get, AssetAllocationConfig.microServiceToken, queryParams: dict);
            var qualresponse = "";
            var qualificationDetails = JsonSerializer.Deserialize<QualificationDetails>(qualresponse);
            return qualificationDetails.Data.personnel.FirstOrDefault().attendances.FirstOrDefault().valid;    
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred when checking OTUG Underground Guide qualifcation check. Error: {}", e.Message);
            return false;
        }
    }
}
