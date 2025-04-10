using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Models;
using asset_allocation_api.Service.Implementation;
using asset_allocation_api.Util;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetCheckHistoriesController(AppDbContext context, ILogger<AssetCheckHistoriesController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetCheckHistoriesController> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private readonly PowerScaleFileService _powerscalefileservice;

        // GET: api/AssetCheckHistories
        [HttpGet]
        public async Task<ActionResult<Response<AssetCheckHistoryListResult?>>> GetAssetCheckHistories(int DepartmentId, int Page = 1, int PageSize = 10, string? SortField = "CheckedDate", string? SortOrder = "desc", string? FilterField = "", string? FilterValue = "")
        {
            Response<AssetCheckHistoryListResult?> resp = new();

            if (_context.AssetCheckHistories == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "AssetCheckHistories context is null");
            }

            SortOrder = SortOrder!.ToLower();
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            if (typeof(AssetCheckHistoryView).GetProperty(SortField!) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortField invalid. There is no attribute/property named {0}", SortField!);
            }

            IQueryable<AssetCheckHistory> assetCheckHistoryContext = _context.AssetCheckHistories.Where(a => a.Asset != null && a.Asset.DepartmentId == DepartmentId);
            
            IQueryable<AssetCheckHistoryView> b = from ach in assetCheckHistoryContext
                                                   join a in _context.Assets.Where(aw => aw.DepartmentId == DepartmentId) on ach.AssetId equals a.Id into grouping1
                                                   from p1 in grouping1
                                                   join at in _context.AssetTypes on p1.AssetTypeId equals at.Id into grouping4
                                                   from p4 in grouping4.DefaultIfEmpty()
                                                   join act in _context.AssetCheckTypes on ach.AssetCheckTypeId equals act.Id into grouping2
                                                   from p2 in grouping2.DefaultIfEmpty()
                                                   join p in _context.Personnel on ach.CheckedUserId equals p.PersonnelNo into grouping3
                                                   from p3 in grouping3.DefaultIfEmpty()
                                                   // where ach.Status != "Active"
                                                   select new AssetCheckHistoryView
                                                   {
                                                       Id = ach.Id,
                                                       AssetId = ach.AssetId,
                                                       AssetRfid = p1.Rfid,
                                                       AssetSerial = p1.Serial,
                                                       AssetTypeId = p1.AssetTypeId,
                                                       AssetTypeName = p4.Name,
                                                       AssetCheckTypeId = ach.AssetCheckTypeId,
                                                       AssetCheckTypeName = p2.CheckName,
                                                       Status = ach.Status,
                                                       Description = ach.Description,
                                                       CheckedUserId = ach.CheckedUserId,
                                                       CheckedUserFirstName = p3.FirstName,
                                                       CheckedUserLastName = p3.LastName,
                                                       CheckedDate = ach.CheckedDate
                                                   }; // .ToListAsync();

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
                        if (!string.IsNullOrEmpty(s.Key) && typeof(AssetCheckHistoryView).GetProperty(s.Key) == null) 
                            return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [s.Key]);
                        if(s.Key == "CheckedDate") assetCheckHistoryContext = assetCheckHistoryContext.Where(a => EF.Functions.Like((string)EF.Property<object>(a, s.Key), $"%{s.Value[0]}%"));
                        else if (s.Value.Count == 1) b = b.Where(a => EF.Functions.Like(EF.Property<string>(a, s.Key).ToString(), $"{s.Value[0]}%"));
                        else b = b.Where(a => s.Value.Contains(EF.Property<string>(a, s.Key).ToString()));
                    }
                }
            }

            b = "asc".Equals(SortOrder) ? 
                b.OrderBy(a => EF.Property<AssetCheckHistory>(a, SortField!)) : 
                b.OrderByDescending(a => EF.Property<AssetCheckHistory>(a, SortField!));

            var count = await b.CountAsync();
            var items = await b.Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync();

            AssetCheckHistoryListResult myobj = new((count - 1) / PageSize + 1, items);

            return ResponseUtils.ReturnResponse(_logger, null, resp, myobj, 200, true, "Asset check histories returned successfully.");
        }

        // GET: api/AssetCheckHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetCheckHistory>> GetAssetCheckHistory(int id)
        {
            if (_context.AssetCheckHistories == null)
            {
                return NotFound();
            }
            AssetCheckHistory? assetCheckHistory = await _context.AssetCheckHistories.FindAsync(id);

            if (assetCheckHistory == null)
            {
                return NotFound();
            }

            return assetCheckHistory;
        }

        // PUT: api/AssetCheckHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetCheckHistory(int id, AssetCheckHistory assetCheckHistory)
        {
            if (id != assetCheckHistory.Id)
            {
                return BadRequest();
            }
            
            
            Asset? asset = await _context.Assets.FindAsync(assetCheckHistory.AssetId);

            if (asset == null)
            {
                return Problem($"No such asset exists with id : \"{assetCheckHistory.AssetId}\"");
            }


            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            var newAssetCheck = new AssetCheckHistory()
            {
                AssetId = asset.Id,
                Status = assetCheckHistory.Status,
                Description = assetCheckHistory.Description,
                CheckedUserId = int.Parse(Request.Headers["personnelno"].ToString()),
                CheckedDate = DateTime.Now,
            };            
            _context.AssetCheckHistories.Add(newAssetCheck);
            await _context.SaveChangesAsync(audit);
            
            asset.LastMaintenanceId = newAssetCheck.Id;
            await _context.SaveChangesAsync(audit);

            try
            {
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetCheckHistoryExists(id))
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

        // POST: api/AssetCheckHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssetCheckHistory>> PostAssetCheckHistory(AssetCheckHistory assetCheckHistory)
        {
            if (_context.AssetCheckHistories == null)
            {
                return Problem("Entity set 'AppDbContext.AssetCheckHistories'  is null.");
            }

            Asset? asset = await _context.Assets.FindAsync(assetCheckHistory.AssetId);

            if (asset == null)
            {
                return Problem($"No such asset exists with id : \"{assetCheckHistory.AssetId}\"");
            }

    
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            assetCheckHistory.CheckedDate = DateTime.Now;
            _context.AssetCheckHistories.Add(assetCheckHistory);
            await _context.SaveChangesAsync(audit);

            asset.LastMaintenanceId = assetCheckHistory.Id;
            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync(audit);

            return CreatedAtAction("GetAssetCheckHistory", new { id = assetCheckHistory.Id }, assetCheckHistory);
        }

        // DELETE: api/AssetCheckHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetCheckHistory(int id)
        {
            if (_context.AssetCheckHistories == null)
            {
                return NotFound();
            }
            AssetCheckHistory? assetCheckHistory = await _context.AssetCheckHistories.FindAsync(id);
            if (assetCheckHistory == null)
            {
                return NotFound();
            }

            _context.AssetCheckHistories.Remove(assetCheckHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssetCheckHistoryExists(int id)
        {
            return (_context.AssetCheckHistories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}