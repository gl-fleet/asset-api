using Microsoft.AspNetCore.Mvc;
using asset_allocation_api.Config;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using asset_allocation_api.Context;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QualificationsController(
        ILogger<QualificationsController> logger,
        AppDbContext context
    ) : ControllerBase
    {
        private readonly ILogger<QualificationsController> _logger = logger;
        private readonly AppDbContext _context = context;

        [HttpGet]
        public async Task<object> GetPersonnelQualifications(int PersonnelNo)
        {
            Response<JsonNode?> resp = new();

            var departmentId = Request.Headers["Departmentid"].ToString();

            // Use the header value as needed
            if (string.IsNullOrEmpty(departmentId))
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "Departmentid is null");

            // Fetch all type trainings for returnable asset type and specific department
            var returnableAssetTypeTrainings = context.TypeTrainings
                .Include(t => t.AssetType)
                .Join(context.DepartmentAssetTypes, t => t.AssetType.Id, at => at.AssetTypeId, (t, at) => new
                {
                    TypeTraining = t,
                    DepartmentAssetType = at
                })
                .Where(result => result.DepartmentAssetType.DepartmentId == int.Parse(departmentId))
                .ToList();

            // Fetch all type trainings for nonreturnable asset type and specific department
            var nonReturnableAssetTypeTrainings = context.TypeTrainings.Where(t =>
                t.NonReturnableAssetType.DepartmentId == int.Parse(departmentId)).ToList();
            
            // If no trainings are found for either returnable or non-returnable asset types, return an empty list
            if (!returnableAssetTypeTrainings.Any() && !nonReturnableAssetTypeTrainings.Any())
                return ResponseUtils.ReturnResponse(_logger, null, resp, new JsonArray(), 200, true, "No training config found");
            
            // Prepare fetch qualififcation codes
            var qualifications = string.Join("|",
                returnableAssetTypeTrainings.Select(t => t.TypeTraining.TrainingId.ToString())
                    .Concat(nonReturnableAssetTypeTrainings.Select(t => t.TrainingId.ToString()))
                    .Distinct()
                );

            Dictionary<string, string> dict = new()
            {
                { "saps", PersonnelNo.ToString() },
                { "qualifications", qualifications },
                { "options", "full" }
            };

            // JsonObject? json = JsonNode.Parse((await HttpUtils.CallMicroservice<string>(_logger,
            //     AssetAllocationConfig.imsPersonnelQualifications, HttpMethod.Get,
            //     AssetAllocationConfig.microServiceToken, queryParams: dict))!)!["Data"]!.AsObject();
            
            JsonObject? json = JsonNode.Parse("").AsObject();

            return ResponseUtils.ReturnResponse(_logger, null, resp, json, 200, true, "Personnel qualification checked successfully.");
        }


        [HttpGet]
        public async Task<object> GetPersonQualificationStatus(int PersonnelNo, int QualificationCode)
        {
            Response<JsonNode?> resp = new();

            Dictionary<string, string> dict = new()
            {
                { "saps", PersonnelNo.ToString() },
                { "qualifications", @"" + QualificationCode + "" },
                { "options", "full" }
            };

            // JsonObject? json = JsonNode.Parse((await HttpUtils.CallMicroservice<string>(_logger,
            //     AssetAllocationConfig.imsPersonnelQualifications, HttpMethod.Get,
            //     AssetAllocationConfig.microServiceToken, queryParams: dict))!)!["Data"]!.AsObject();
            
            JsonObject? json = JsonNode.Parse("").AsObject();

            return ResponseUtils.ReturnResponse(_logger, null, resp, json, 200, true, "Personnel qualification returned successfully");
        }


        [HttpGet]
        public async Task<object> GetAllQualifications()
        {
            Response<JsonNode?> resp = new();

            // JsonObject? json = JsonNode.Parse((await HttpUtils.CallMicroservice<string>(_logger,
            //     AssetAllocationConfig.imsAllQualification, HttpMethod.Get,
            //     AssetAllocationConfig.microServiceToken))!)!["Data"]!.AsObject();
            
            JsonObject? json = JsonNode.Parse("").AsObject();

            return ResponseUtils.ReturnResponse(_logger, null, resp, json, 200, true, "Qualification list returned successfully");
        }
    }
}