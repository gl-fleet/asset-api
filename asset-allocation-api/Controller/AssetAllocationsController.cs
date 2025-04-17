using Confluent.Kafka;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using asset_allocation_api.Config;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Output_Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using asset_allocation_api.Service.Implementation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetAllocationsController(
        AppDbContext context, 
        ILogger<AssetAllocationsController> logger, 
        AssetAllocationHandler assetAllocationHandler) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;
        private readonly AssetAllocationHandler _assetAllocationHandler = assetAllocationHandler;

        // GET: api/AssetAllocations
        [HttpGet]
        public async Task<ActionResult<Response<AssetAllocationListResult?>>> GetAssetAllocations(
            int DepartmentId, int Page = 1, int PageSize = 10, 
            string? SortField = "AssignedDate", string? SortOrder = "desc", 
            int MaxDays = 56, /** Default is 56, which equals to 4 roster **/
            int MaxHistory = 1000000,
            string? FilterField = "", string? FilterValue = "")
        {
            Response<AssetAllocationListResult?> resp = new();

            if (_context.AssetAllocations == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "AssetAllocations context is null");
            }

            SortOrder = SortOrder!.ToLower();
            if (SortOrder != "asc" && SortOrder != "desc")
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation($"[GetAssetAllocation] Query build started / Department: {DepartmentId} MaxDays: {MaxDays} MaxHistory: {MaxHistory}");
            var assetAllocationsContext = _context.AssetAllocations.AsQueryable();

            var b = from aac in assetAllocationsContext
                    join aSel in _context.Assets.Where(a => a.DepartmentId == DepartmentId) on aac.AssetId equals aSel.Id
                    join atSel in _context.AssetTypes on aSel.AssetTypeId equals atSel.Id into atGrouping
                    from atSel in atGrouping.DefaultIfEmpty()
                    join pSel in _context.Personnel on aac.PersonnelNo equals pSel.PersonnelNo into pGrouping
                    from pSel in pGrouping.DefaultIfEmpty()
                    join p1Sel in _context.Personnel on aac.AssignedUserId equals p1Sel.PersonnelNo into p1Grouping
                    from p1Sel in p1Grouping.DefaultIfEmpty()
                    join p2Sel in _context.Personnel on aac.ReturnedUserId equals p2Sel.PersonnelNo into p2Grouping
                    from p2Sel in p2Grouping.DefaultIfEmpty()
                    select new AssetAllocationView
                    {
                        Id = aac.Id,
                        AssetId = aac.AssetId,
                        AssetRfid = aSel.Rfid,
                        AssetSerial = aSel.Serial,
                        AssetTypeId = aSel.AssetTypeId,
                        AssetTypeName = atSel.Name,
                        PersonnelNo = aac.PersonnelNo,
                        PersonnelFirstName = pSel.FirstName,
                        PersonnelLastName = pSel.LastName,
                        AssignedUserId = aac.AssignedUserId,
                        AssignedUserFirstName = p1Sel.FirstName,
                        AssignedUserLastName = p1Sel.LastName,
                        AssignedDate = aac.AssignedDate,
                        ReturnedUserId = aac.ReturnedUserId,
                        ReturnedUserFirstName = p2Sel.FirstName,
                        ReturnedUserLastName = p2Sel.LastName,
                        ReturnedDate = aac.ReturnedDate,
                        Description = aSel.Description,
                    };

            /** __Multi_filtering__ **/
            if (!string.IsNullOrEmpty(FilterField) && !string.IsNullOrEmpty(FilterValue))
            {
                string[] Fields = FilterField.Split(',');
                string[] Values = FilterValue.Split(',');
            
                var Filter = new Dictionary<string, List<string>>();
            
                if(Fields.Length == Values.Length)
                {
                    for (int i = 0; i < Fields.Length; i++)
                    {
                        if (!Filter.ContainsKey(Fields[i])) Filter[Fields[i]] = new List<string>();
                        Filter[Fields[i]].Add(Values[i]);
                    }
                    foreach(KeyValuePair<string, List<string>> s in Filter)
                    {

                        if (!string.IsNullOrEmpty(s.Key) && typeof(AssetAllocationView).GetProperty(s.Key) == null) return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [s.Key]);

                        logger.LogInformation($"Filter key {s.Key}, Filter value: {s.Value[0]} ({s.Value.Count})");
                        
                        if (s.Value.Any(val => val.Contains('|')))
                        {
                            if (s.Key == "AssignedDate")
                            {
                                var dateValue = s.Value.FirstOrDefault()?.Split('|');
                                if (dateValue is { Length: 2 })
                                {
                                    DateTime startDate = DateTime.Parse(dateValue[0]);
                                    DateTime endDate = DateTime.Parse(dateValue[1]);
                                    b = b.Where(row => row.AssignedDate >= startDate && row.AssignedDate <= endDate);
                                }
                            }
                            else
                            {
                                b = b.Where(row => s.Value.Contains(EF.Property<string>(row, s.Key)));
                            }
                        }
                        else if (s.Value.Count == 1)
                        {
                            if (s.Value[0] == "[null]") b = b.Where(a => EF.Property<object>(a, s.Key).Equals(null) );
                            else if (s.Value[0] == "[notnull]") b = b.Where(a => !EF.Property<object>(a, s.Key).Equals(null) );
                            else b = b.Where(a => EF.Functions.Like(EF.Property<string>(a, s.Key).ToString(), $"{s.Value[0]}%"));
                        }
                        else b = b.Where(a => s.Value.Contains(EF.Property<string>(a, s.Key).ToString()));
                    }
                }
            }

            b = "asc".Equals(SortOrder) ? 
                b.OrderBy(a => EF.Property<Asset>(a, SortField!)) : 
                b.OrderByDescending(a => EF.Property<Asset>(a, SortField!));
            
            logger.LogDebug("[GetAssetAllocation] Query build ended. Duration {}", DateTime.Now - resp.ExecuteStart);

            var count = 0;
            if (FilterField.Split(',').Contains("PersonnelNo"))
            {
                logger.LogDebug("[GetAssetAllocation] Personnel list request detected.");
                // Last 2 roster 14*2
                b = b.Where(row => row.AssignedDate >= DateTime.Now.AddDays(-(MaxDays)));
                // count = await b.CountAsync();
            }
            else if (FilterField.Split(',').Contains("AssetId"))
            {
                logger.LogDebug("[GetAssetAllocation] AssetId filter request detected.");
                count = await b.CountAsync();
            }
            else
            {
                count = MaxHistory; // max asset allocation history restriction
            }
 
            logger.LogInformation($"[GetAssetAllocation] Count async ended. Duration {DateTime.Now - resp.ExecuteStart} / Count {count}");

            var items = await b.Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync();
            logger.LogInformation("[GetAssetAllocation] Page result ended. Duration {}", DateTime.Now - resp.ExecuteStart);

            AssetAllocationListResult myobj = new((count - 1) / PageSize + 1, items);
            logger.LogInformation("[GetAssetAllocation] Completed after {} ", DateTime.Now - resp.ExecuteStart);

            return ResponseUtils.ReturnResponse(_logger, null, resp, myobj, 200, true, "Asset allocation data returned successfully.");
        }

        // GET: api/AssetAllocations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetAllocation>> GetAssetAllocation(int id)
        {
            if (_context.AssetAllocations == null)
            {
                return NotFound();
            }
            AssetAllocation? assetAllocation = await _context.AssetAllocations.FindAsync(id);

            if (assetAllocation == null)
            {
                return NotFound();
            }

            return assetAllocation;
        }

        // POST: api/AssetAllocations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Route("Assign")]
        [HttpPost]
        public async Task<ActionResult<Response<AssetAllocation?>>> PostAssetAllocation(AssetAllocationAssignInput input)
        {
            Response<AssetAllocation?> resp = new();
            var departmentId = Request.Headers["Departmentid"].ToString();
            logger.LogInformation("***********************ALLOCATE***********************************************");
            logger.LogInformation("[PostAssetAllocation] Request received. Department: {DepartmentId}, Personnel: {PersonnelNo}, Asset: {AssetId}", departmentId, input.PersonnelNo, input.AssetId);

            if (_context.AssetAllocations == null)
            {
                logger.LogError("[PostAssetAllocation] Entity set is null. Duration: {}", DateTime.Now - resp.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, "Entity set is null.");
            }

            // Fetch Asset & Last Allocation in a single query
            var assetData = await _context.Assets
                .Where(a => a.Id == input.AssetId)
                .Include(a => a.AssetCheckHistories)
                .Include(a => a.LastAllocation)
                .Select(a => new 
                {
                    Asset = a,
                    LastAllocation = a.LastAllocation,
                    LastCheckHistory = a.AssetCheckHistories.OrderByDescending(h => h.CheckedDate).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (assetData == null)
            {
                logger.LogError("[PostAssetAllocation] Asset not found. Duration: {}", DateTime.Now - resp.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "Asset not found.");
            }

            assetData.Asset.AssetType = await _context.AssetTypes.FindAsync(assetData.Asset.AssetTypeId);

            // Auto-return logic inside transaction
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                if (assetData.LastAllocation != null && assetData.LastAllocation.ReturnedDate == null)
                {
                    if (DateTimeUtils.IsNowShift(assetData.LastAllocation.AssignedDate!.Value))
                    {
                        logger.LogError("[PostAssetAllocation] Asset is already allocated. Duration: {}", DateTime.Now - resp.ExecuteStart);
                        return ResponseUtils.ReturnResponse(_logger, null, resp, assetData.LastAllocation, 500, false, 
                            "Asset is already allocated on PersonnelNo: {0}", values: [assetData.LastAllocation.PersonnelNo]);
                    }
                    
                    logger.LogInformation("[PostAssetAllocation] Auto return detected. Returning asset...");
                    assetData.LastAllocation.ReturnedDate = DateTime.Now;
                    _context.AssetAllocations.Update(assetData.LastAllocation);
                    await _context.SaveChangesAsync();
                }
                
                await transaction.CommitAsync();
            }

            // Check last status efficiently
            if (assetData.LastCheckHistory != null && assetData.LastCheckHistory.Status != "Active")
            {
                logger.LogError("[PostAssetAllocation] Can not allocate asset. Status {} updated in {}. Duration: {}", 
                    assetData.LastCheckHistory.Status, assetData.LastCheckHistory.CheckedDate, DateTime.Now - resp.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, 
                    $"Can not allocate asset. Current status {assetData.LastCheckHistory.Status} updated in {assetData.LastCheckHistory.CheckedDate}");
            }

            // Qualification check
            var qualificationResponse = await _assetAllocationHandler.CheckQualification(assetData.Asset, input.PersonnelNo.ToString());
            if (!qualificationResponse.Status.Success)
            {
                logger.LogError("[PostAssetAllocation] Person qualification invalid. Message: {}, Duration: {}", 
                    qualificationResponse.Message, DateTime.Now - resp.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, qualificationResponse.Message);
            }

                        
            AssetAllocation? assetAllocation = new()
            {
                AssetId = input.AssetId,
                PersonnelNo = input.PersonnelNo,
                AssignedUserId = input.AssignedUserId,
                AssignedDate = DateTime.Now,
            };
            
            // Caplamp with PLI assign to Minlog, PLI logic
            var caplampAssignResponse = await assetAllocationHandler.AssignCaplampWithPli(assetData.Asset, departmentId, input.PersonnelNo, input.PersonnelId);
            if (caplampAssignResponse.Status.Code != 200)
            {
                logger.LogError("[PostAssetAllocation] [Department: {Department}] [Personnel: {PersonnelNo}] [Asset: {AssetId}] Error in Caplamp with PLI assignment. Message: {Message} Duration: {Duration}",
                    departmentId, input.PersonnelNo, input.AssetId, caplampAssignResponse.Message, DateTime.Now - resp.ExecuteStart);
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, caplampAssignResponse.Message);
            }
    
            _context.AssetAllocations.Add(assetAllocation);
            await _context.SaveChangesAsync();

            assetData.Asset.LastAllocationId = assetAllocation.Id;
            _context.Entry(assetData.Asset).State = EntityState.Modified;

            if (assetData.LastAllocation != null)
            {
                assetData.LastAllocation.AssetId = assetData.Asset.Id;
                _context.Entry(assetData.LastAllocation).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            assetAllocationHandler.SendToSignalR(assetAllocation);
            
            logger.LogInformation("[PostAssetAllocation] [Department: {}] [Personnel: {}] [ Asset: {}] Asset assignment process ended successfully. Duration: {}", departmentId, input.PersonnelNo, input.AssetId, DateTime.Now - resp.ExecuteStart);   
            return ResponseUtils.ReturnResponse(_logger, null, resp, assetAllocation, 201, true, "Asset assign success");

        }

        [Route("Return")]
        [HttpPost]
        public async Task<ActionResult<Response<AssetAllocation?>>> ReturnAssetAllocation(AssetAllocationReturnInput input)
        {
            Response<AssetAllocation?> resp = new();
            logger.LogInformation("***********************RETURN***********************************************");
            logger.LogInformation("[ReturnAsset] Request received...");
            AssetAllocation? assetAllocation;
            Asset? asset;

            if (input.AssetAllocationId != null)
            {
                assetAllocation = await _context.AssetAllocations.FindAsync(input.AssetAllocationId);
                if (assetAllocation == null)
                {
                    logger.LogError("[ReturnAsset] [Department: {}] [Personnel: {}] AssetAllocationId {} not found! Duration: {}", assetAllocation.Asset.DepartmentId, assetAllocation.PersonnelNo, input.AssetAllocationId, DateTime.Now - resp.ExecuteStart);   
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "assetAllocationId {0} not found!", values: [input.AssetAllocationId]);
                }
                asset = await _context.Assets.FindAsync(assetAllocation.AssetId);
                if (asset == null)
                {
                    logger.LogError("[ReturnAsset] [Department: {}] [Personnel: {}] AssetAllocation found, but asset not found by AssetAllocation.AssetId, AssetAllocation.AssetId: {} Duration: {}", assetAllocation.Asset.DepartmentId, assetAllocation.PersonnelNo, input.AssetId, DateTime.Now - resp.ExecuteStart);                   
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "AssetAllocation found, but asset not found by AssetAllocation.AssetId, AssetAllocation.AssetId: {0}", values: [assetAllocation.AssetId]);
                }
            }
            else if (input.AssetId != null)
            {
                asset = await _context.Assets.FindAsync(input.AssetId);
                if (asset == null)
                {
                    logger.LogError("[ReturnAsset] AssetId {} not found! Duration: {}", input.AssetId, DateTime.Now - resp.ExecuteStart);                   
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "assetId {0} not found!", values: [input.AssetId]);
                }
                assetAllocation = await _context.AssetAllocations.FindAsync(asset.LastAllocationId);
                if (assetAllocation == null)
                {
                    logger.LogError("[ReturnAsset] Asset found, but asset allocation not found by Asset.LastAllocationId, LastAllocationId: {}, Duration: {}", asset.LastAllocationId, DateTime.Now - resp.ExecuteStart);                   
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "Asset found, but asset allocation not found by Asset.LastAllocationId, LastAllocationId: {0}", values: [asset.LastAllocationId]);
                }
            }
            else
            {
                logger.LogError("[ReturnAsset] Please provide one of allocationId or assetId. Duration: {}", DateTime.Now - resp.ExecuteStart);                   
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "Please provide one of allocationId or assetId");
            }
            
            // Remove caplamp request to caplamp assign
            var tagRemoveResponse = await assetAllocationHandler.RemoveCaplampWithPli(asset, assetAllocation.PersonnelNo ?? 0);
            if (tagRemoveResponse.Status.Code != 200)
            {
                logger.LogError("[ReturnAsset] An error occurred when returning caplamp to PLI. Duration:{}", DateTime.Now - resp.ExecuteStart);                   
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, tagRemoveResponse.Message);
            }
            
            asset.LastAllocationId = null;
            _context.Entry(asset).State = EntityState.Modified;

            bool fullReturn = false;
            if (assetAllocation.ReturnedDate == null)
            {
                assetAllocation.ReturnedUserId = input.ReturnedUserId;
                assetAllocation.ReturnedDate = DateTime.Now;
                _context.Entry(assetAllocation).State = EntityState.Modified;
                fullReturn = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            if (fullReturn)
            {
                await assetAllocationHandler.SendToSignalR(assetAllocation);
                logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [AssetId: {}] Asset return successful", asset.DepartmentId, assetAllocation.PersonnelNo, assetAllocation.AssetId); 
                logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [AssetId: {}] [AssetAllocationId: {}] Return completed. Duration: {}]", asset.DepartmentId, assetAllocation.PersonnelNo, assetAllocation.AssetId, assetAllocation.Id, DateTime.Now - resp.ExecuteStart);                   
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 200, true, "Asset return successful PersonnelNo:{0} AssetId:{1}", values: [assetAllocation.PersonnelNo, assetAllocation.AssetId]);
            }

            logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [AssetId: {}] [AssetAllocationId: {}] Already returned allocation record. But cleared the Asset.LastAllocationId anyways", asset.DepartmentId, assetAllocation.PersonnelNo, assetAllocation.AssetId, assetAllocation.Id);                   
            logger.LogInformation("[ReturnAsset] [Department: {}] [Personnel: {}] [AssetId: {}] [AssetAllocationId: {}] Return completed. Duration: {}", asset.DepartmentId, assetAllocation.PersonnelNo, assetAllocation.AssetId, assetAllocation.Id, DateTime.Now - resp.ExecuteStart);                   
            return ResponseUtils.ReturnResponse(_logger, null, resp, null, 202, true, "Already returned allocation record. But cleared the Asset.LastAllocationId anyways. AssetAllocationId: {0} AssetId: {1} PersonnelNo: {2}", values: [assetAllocation.Id, asset.Id, assetAllocation.PersonnelNo]);
        }

        /*
        // PUT: api/AssetAllocations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetAllocation(int id, AssetAllocation assetAllocation)
        {
            if (id != assetAllocation.Id)
            {
                return BadRequest();
            }

            _context.Entry(assetAllocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetAllocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool AssetAllocationExists(int id)
        {
            return (_context.AssetAllocations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        */
    }
}