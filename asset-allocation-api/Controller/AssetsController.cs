using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Output_Model;
using asset_allocation_api.Models;
using asset_allocation_api.Service.Implementation;
using asset_allocation_api.Util;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AssetsController> logger,
        ILogger<PowerScaleFileService> logger1) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetsController> _logger = logger;
        private readonly ILogger<PowerScaleFileService> _loggerfileservice = logger1;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


        // GET: api/Assets
        [HttpGet]
        public async Task<ActionResult<Response<AssetListResult?>>> GetAssets(
            int DepartmentId,
            int TypeId = -1,
            int Page = 1,
            int PageSize = 10,
            string? SortField = "Serial",
            string? SortOrder = "asc",
            string? FilterField = "",
            string? FilterValue = "")
        {
            Response<AssetListResult?> resp = new();
            SortOrder = SortOrder!.ToLower();
            if (_context.Assets == null)
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, "Asset context is null");
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false,
                    "SortOrder invalid. Please choose from asc or desc");
            if (typeof(Asset).GetProperty(SortField!) == null)
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false,
                    "SortField invalid. There is no attribute/property named {0}", values: [SortField]);

            IQueryable<Asset> assetContext = _context.Assets.Where(a => a.DepartmentId == DepartmentId);

            if ("asc".Equals(SortOrder))
                assetContext = assetContext.OrderBy(a => EF.Property<Asset>(a, SortField!));
            else
                assetContext = assetContext.OrderByDescending(a => EF.Property<Asset>(a, SortField!));

            IQueryable<AssetView> b = from a in assetContext
                join at in _context.AssetTypes on a.AssetTypeId equals at.Id into grouping
                from p in grouping.DefaultIfEmpty()
                join ach in _context.AssetCheckHistories on a.LastMaintenanceId equals ach.Id into grouping2
                from p2 in grouping2.DefaultIfEmpty()
                join p3 in _context.Personnel on a.ModifiedUserId equals p3.PersonnelNo into p3Grouping
                from p3Sel in p3Grouping.DefaultIfEmpty()
                where FilterField == "Rfid,Rfid" || FilterField == "Id"  ? p2.Status != "NULL" : (p2.Status == "Active" || a.LastMaintenanceId == null)
                select new AssetView
                {
                    Id = a.Id,
                    Rfid = a.Rfid,
                    Serial = a.Serial,
                    Mac2 = a.Mac2,
                    Mac3 = a.Mac3,
                    AssetTypeId = a.AssetTypeId,
                    TypeName = p.Name,
                    Country = a.Country,
                    Description = a.Description,
                    RegisteredDate = a.RegisteredDate,
                    ManufacturedDate = a.ManufacturedDate,
                    StartedUsingDate = a.StartedUsingDate,
                    ExpireDate = a.ExpireDate,
                    LastMaintenanceId = a.LastMaintenanceId,
                    LastUsingDate = a.LastAllocation.AssignedDate,
                    Status = p2.Status,
                    LastAllocationId = a.LastAllocationId,
                    ModifiedUserId = a.ModifiedUserId,
                    ModifiedUserName = p3Sel.FullName,
                    ModifiedDate = a.ModifiedDate,
                    DepartmentId = a.DepartmentId.ToString(),
                    //DepartmentId butsaay
                    //
                    AssetInspectionResult = a.AssetInspectionHistories
                        .GroupBy(s => new { s.CheckType.CheckName, s.CheckTypeId })
                        .Select(x => new AssetInspectionResult
                        {
                            InspectionType = x.Key.CheckName,
                            InspectionTypeId = x.Key.CheckTypeId,
                            LastInspectionDate = x.Max(z => z.CheckedDateTime)
                        })
                };

            // Multi-filtering logic
            if (!string.IsNullOrEmpty(FilterField) && !string.IsNullOrEmpty(FilterValue))
            {
                if (FilterField.Equals("Serial", StringComparison.OrdinalIgnoreCase))
                {
                    // Special case for "Serial"
                    b = b.Where(a => EF.Functions.Like(a.Serial, $"%{FilterValue}%"));
                }
                else if (FilterField.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    // Special case for "Serial"
                    b = b.Where(a => a.Id == int.Parse(FilterValue));
                }
                else if (FilterField.Equals("StartedUsingDate", StringComparison.OrdinalIgnoreCase))
                {
                    if (DateOnly.TryParse(FilterValue, out var parsedDate))
                    {
                        // Filter where StartedUsingDate matches the parsed date
                        b = b.Where(a => a.StartedUsingDate.HasValue && a.StartedUsingDate.Value == parsedDate);
                    }
                    else
                    {
                        // Handle invalid date format
                        return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false,
                            "Invalid date format provided for StartedUsingDate filter.");
                    }
                } 
                else
                {
                    string[] Fields = FilterField.Split(',');
                    string[] Values = FilterValue.Split(',');

                    var Filter = new Dictionary<string, List<string>>();
                    if (Fields.Length == Values.Length)
                    {
                        for (int i = 0; i < Fields.Length; i++)
                        {
                            if (!Filter.ContainsKey(Fields[i]))
                                Filter[Fields[i]] = new List<string>();
                            Filter[Fields[i]].Add(Values[i]);
                        }

                        foreach (KeyValuePair<string, List<string>> s in Filter)
                        {
                            if (!string.IsNullOrEmpty(s.Key) && typeof(AssetView).GetProperty(s.Key) == null)
                                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false,
                                    "FilterField invalid. There is no attribute/property named {0}", values: [s.Key]);

                            if (s.Value.Count == 1)
                                b = b.Where(a => EF.Functions.Like(EF.Property<string>(a, s.Key)!, $"{s.Value[0]}%"));
                            else
                                b = b.Where(a => s.Value.Contains(EF.Property<string>(a, s.Key)!));
                        }
                    }
                }
            }

            if (TypeId > 0)
                b = b.Where(a => a.AssetTypeId == TypeId);
            var count = await b.CountAsync();
            var items = await b.Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync();
            AssetListResult myobj = new((count - 1) / PageSize + 1, count, items);

            return ResponseUtils.ReturnResponse(_logger, null, resp, myobj, 200, true, "Asset list returned successfully");
        }


        // GET: api/Assets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asset>> GetAsset(int id)
        {
            if (_context.Assets == null)
            {
                return NotFound();
            }

            Asset? asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            return asset;
        }

        // PUT: api/Assets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsset(int id, Asset asset)
        {
            Response<Asset?> resp = new();

            // Load the existing asset from DB to track changes
            var oldAsset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);

            if (oldAsset == null)
            {
                return NotFound();
            }

            if (id != asset.Id)
            {
                return BadRequest();
            }

            asset.ModifiedDate = DateTime.Now;

            // Attach the entity to track changes
            oldAsset.AssetTypeId = asset.AssetTypeId;
            oldAsset.Rfid = asset.Rfid;
            oldAsset.Serial = asset.Serial;
            oldAsset.Mac2 = asset.Mac2;
            oldAsset.ManufacturedDate = asset.ManufacturedDate;
            oldAsset.StartedUsingDate = asset.StartedUsingDate;
            oldAsset.Description = asset.Description;

            try
            {
                // Save changes - Audit logs will be automatically captured
                Audit audit = new()
                {
                    CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
                };
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateException ex)
            {
                if (!AssetExists(id))
                {
                    return NotFound();
                }

                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 409, false,
                        "A record with the same unique key already exists.");
                }

                throw;
            }

            return NoContent();
        }
    
        // PUT: api/move-department
        [HttpPut("MoveDepartment")]
        public async Task<IActionResult> MoveDepartment(MoveAssetDTO moveAssetDto)
        {
            Response<Asset?> resp = new();
            if (_context.Assets == null)
            {
                return Problem("Entity set 'AppDbContext.Assets' is null.");
            }
            
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            try
            {
                // Validate Department and AssetType existence
                var departmentExists = await _context.Departments.FirstOrDefaultAsync(d => d.Id == moveAssetDto.ToDepartmentID);
                var newAssetType = await _context.AssetTypes.FirstOrDefaultAsync(at => at.Id == moveAssetDto.ToAssetTypeID);
                var departmentAssetTypeExists = await _context.DepartmentAssetTypes.AnyAsync(dat => dat.DepartmentId == moveAssetDto.ToDepartmentID && dat.AssetTypeId == moveAssetDto.ToAssetTypeID);

                if (!departmentAssetTypeExists)
                {
                    return ResponseUtils.ReturnResponse(
                        _logger,
                        null,
                        resp,
                        null,
                        400,
                        false,
                        $"No connection exists between Department ID {moveAssetDto.ToDepartmentID} and Asset Type ID {moveAssetDto.ToAssetTypeID}."
                    );
                }

                if (departmentExists == null)
                {
                    return ResponseUtils.ReturnResponse(
                        _logger,
                        null,
                        resp,
                        null,
                        400,
                        false,
                        $"Department with ID {moveAssetDto.ToDepartmentID} does not exist."
                    );
                }

                if (newAssetType == null)
                {
                    return ResponseUtils.ReturnResponse(
                        _logger,
                        null,
                        resp,
                        null,
                        400,
                        false,
                        $"Asset Type with ID {moveAssetDto.ToAssetTypeID} does not exist."
                    );
                }

                // Fetch assets by their IDs
                var assets = await _context.Assets
                    .Where(a => moveAssetDto.AssetIDs.Contains(a.Id))
                    .Include(a => a.AssetType) // Include current asset type for comparison
                    .ToListAsync();

                if (assets == null || !assets.Any())
                {
                    return ResponseUtils.ReturnResponse(
                        _logger,
                        null,
                        resp,
                        null,
                        404,
                        false,
                        "No assets found with the provided IDs."
                    );
                }

                // Initialize lists for tracking results
                List<int> successfulAssetIds = new();
                List<int> unsuccessfulAssetIds = new();
                int unsuccessfulCount = 0;

                // Process assets
                foreach (var asset in assets)
                {
                    if (asset.AssetType?.Name != newAssetType.Name)
                    {
                        unsuccessfulCount++;
                        unsuccessfulAssetIds.Add(asset.Id);
                        return ResponseUtils.ReturnResponse(
                            _logger,
                            null,
                            resp,
                            null,
                            400,
                            false,
                            "Wrong asset type!"
                        );
                    }

                    // Update valid assets
                    asset.DepartmentId = moveAssetDto.ToDepartmentID;
                    asset.AssetTypeId = moveAssetDto.ToAssetTypeID;
                    asset.ModifiedDate = DateTime.UtcNow;
                    asset.ModifiedUserId = moveAssetDto.ModifiedUserId;
                    successfulAssetIds.Add(asset.Id);
                }

                // Save changes for successfully validated assets
                if (successfulAssetIds.Any())
                {
                    await _context.SaveChangesAsync(audit);
                }

                // Prepare response message
                var responseMessage =
                    $"Successfully moved {successfulAssetIds.Count} assets to department {departmentExists.Name}.";
                if (unsuccessfulAssetIds.Any())
                {
                    responseMessage +=
                        $" Failed to move {unsuccessfulCount} assets: {string.Join(", ", unsuccessfulAssetIds)}.";
                }

                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 
                    unsuccessfulAssetIds.Count != 0 ? 400 : 200, 
                    unsuccessfulAssetIds.Count == 0, 
                    responseMessage);

                // Return final response
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Handle the unique constraint error
                    return ResponseUtils.ReturnResponse(
                        _logger,
                        null,
                        resp,
                        null,
                        409,
                        false,
                        "A record with the same unique key already exists."
                    );
                }

                throw; // Re-throw the exception if it's not a unique constraint error
            }
            catch (Exception ex)
            {
                // Log and return a generic error response
                _logger.LogError(ex, "An error occurred while moving assets to another department.");
                return ResponseUtils.ReturnResponse(
                    _logger,
                    null,
                    resp,
                    null,
                    500,
                    false,
                    "An unexpected error occurred."
                );
            }
        }

        // POST: api/Assets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Asset>> PostAsset(Asset asset)
        {
            Response<Asset?> resp = new();
            if (_context.Assets == null)
            {
                return Problem("Entity set 'AppDbContext.Assets'  is null.");
            }

            
            try
            {
                Audit audit = new()
                {
                    CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
                };
                _context.Assets.Add(asset);
                await _context.SaveChangesAsync(audit);

                return CreatedAtAction("GetAsset", new { id = asset.Id }, asset);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Handle the unique constraint error
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 409, false,
                        "A record with the same unique key already exists.");
                }

                throw; // Re-throw the exception if it's not a unique constraint error                    
            }
        }

        // DELETE: api/Assets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            if (_context.Assets == null)
            {
                return NotFound();
            }

            Asset? asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("UploadFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 8000000)]
        public async Task<ActionResult<Asset>> UploadFile(string AssetId, IFormFile Image)
        {
            PowerScaleFileService _powerscalefileservice = new PowerScaleFileService(_loggerfileservice);
            Asset? asset = await _context.Assets.FindAsync(Convert.ToInt32(AssetId));
            var result = await _powerscalefileservice.UploadImage(Image);
            AssetAttachment assetAttachment = new AssetAttachment();
            if (result.Key != null)
            {
                assetAttachment.AssetId = asset.Id;
                assetAttachment.FilePath = result.Key;
                assetAttachment.ModifiedDate = DateTime.Now;
            }

            _context.AssetAttachments.Add(assetAttachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAsset", new { id = asset.Id }, asset);
        }


        [HttpGet("GetFile")]
        [RequestFormLimits(MultipartBodyLengthLimit = 8000000)]
        public async Task<ActionResult<byte[]>> GetFile(int AssetId)
        {
            PowerScaleFileService _powerscalefileservice = new PowerScaleFileService(_loggerfileservice);
            AssetAttachment? attachment = _context.AssetAttachments.Where(a => a.AssetId == AssetId).OrderBy(e => e.Id)
                .LastOrDefault();
            if (attachment == null) return null;
            string filename = attachment.FilePath;
            var result = await _powerscalefileservice.GetImage(filename);
            return result;
        }

        private bool AssetExists(int id)
        {
            return (_context.Assets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}