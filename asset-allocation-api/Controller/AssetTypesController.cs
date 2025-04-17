using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTypesController(AppDbContext context, ILogger<AssetTypesController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetTypesController> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/AssetTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetType>>> GetAssetTypes(int DepartmentId, int Page = 1, int PageSize = 10, string? SortField = "Id", string? SortOrder = "asc", string? FilterField = "", string? FilterValue = "")
        {
            Response<List<AssetType>?> resp = new();

            if (_context.AssetTypes == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, "AssetTypes context is null");
            }
            SortOrder = SortOrder!.ToLower();
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            if (typeof(AssetType).GetProperty(SortField!) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortField invalid. There is no attribute/property named {0}", values: [SortField]);
            }

            if (!string.IsNullOrEmpty(FilterField) && typeof(AssetType).GetProperty(FilterField) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [FilterField]);
            }
            /*
            IQueryable<CustomAssetType> result = (from at in _context.AssetTypes
                                             join dat in _context.DepartmentAssetTypes on new { X = at.Id, Y = DepartmentId } equals new { X = dat.AssetTypeId!.Value, Y = dat.DepartmentId }
                                             select new CustomAssetType
                                             {
                                                 Id = at.Id,
                                                 Name = at.Name,
                                                 ReturnHour = at.ReturnHour,
                                                 ModifiedUserId = at.ModifiedUserId,
                                                 ModifiedDate = at.ModifiedDate,
                                                 DepartmentAssetTypes =
                                             });*/
            IQueryable<AssetType> result = _context.AssetTypes
                .Include(b => b.DepartmentAssetTypes)
                .Include(b => b.AssetCheckTypeSettings).ThenInclude(a => a.CheckType)
                .Where(b => b.DepartmentAssetTypes.Any(ub => ub.DepartmentId == DepartmentId))
                .Include(b => b.TypeTrainings.Where(c => c.Type == "Returnable"));

            if (!string.IsNullOrEmpty(FilterField))
                result = result.Where(a => EF.Functions.Like((string)EF.Property<object>(a, FilterField), $"%{FilterValue}%"));

            if ("asc".Equals(SortOrder))
                result = result.OrderBy(a => EF.Property<Asset>(a, SortField!));
            else
                result = result.OrderByDescending(a => EF.Property<AssetType>(a, SortField!));
            return await result.ToListAsync();
        }

        // GET: api/AssetTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetType>> GetAssetType(int id)
        {
            if (_context.AssetTypes == null)
            {
                return NotFound();
            }

            AssetType? assetType = await _context.AssetTypes
            .Where(b => b.Id.Equals(id))
            .Include(b => b.DepartmentAssetTypes)
            .Include(b => b.AssetCheckTypeSettings).ThenInclude(a => a.CheckType)
            .Include(b => b.TypeTrainings.Where(c => c.Type == "Returnable"))
            .FirstAsync();

            if (assetType == null)
            {
                return NotFound();
            }

            return assetType;
        }

        // PUT: api/AssetTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetType(int id, AssetType newAssetType)
        {
            if (id != newAssetType.Id)
            {
                return BadRequest();
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            
            // Find asset type by id
            AssetType? dbAssetType = await _context.AssetTypes
             .Where(b => b.Id.Equals(id))
            .Include(b => b.DepartmentAssetTypes)
            .Include(b => b.AssetCheckTypeSettings)
            .Include(b => b.TypeTrainings.Where(c => c.Type == "Returnable"))
            .FirstAsync();

            _context.Entry(dbAssetType).CurrentValues.SetValues(newAssetType);

            foreach (DepartmentAssetType dat in dbAssetType.DepartmentAssetTypes.ToList())
            {
                if (!newAssetType.DepartmentAssetTypes.Select(x => x.Id).Contains(dat.Id))
                    dbAssetType.DepartmentAssetTypes.Remove(dat);
            }

            foreach (DepartmentAssetType dat in newAssetType.DepartmentAssetTypes)
            {
                if (dat.Id == 0)
                    dbAssetType.DepartmentAssetTypes.Add(dat);
                else
                    _context.Entry(dbAssetType.DepartmentAssetTypes.Where(x => x.Id == dat.Id).First()).CurrentValues.SetValues(dat);
            }

            foreach (TypeTraining dat in dbAssetType.TypeTrainings.ToList())
            {
                if (!newAssetType.TypeTrainings.Select(x => x.Id).Contains(dat.Id))
                    dbAssetType.TypeTrainings.Remove(dat);
            }

            foreach (TypeTraining tt in newAssetType.TypeTrainings)
            {
                if (tt.Id == 0)
                    dbAssetType.TypeTrainings.Add(tt);
                else
                    _context.Entry(dbAssetType.TypeTrainings.Where(x => x.Id == tt.Id).First()).CurrentValues.SetValues(tt);
            }

            foreach (AssetCheckTypeSetting checktypesetting in newAssetType.AssetCheckTypeSettings.ToList())
            {
                if (checktypesetting.Id == 0)
                {
                    checktypesetting.AssetTypeId = newAssetType.Id;
                    checktypesetting.CreatedUserId = newAssetType.ModifiedUserId;

                    // Set Expirelimit value if provided in the request
                    if (checktypesetting.Expirelimit.HasValue)
                    {
                        if (newAssetType.AssetCheckTypeSettings.First().Expirelimit.HasValue)
                        {
                            if (newAssetType.AssetCheckTypeSettings.First().Expirelimit != null && newAssetType.AssetCheckTypeSettings.First().CheckNear != null &&
                                newAssetType.AssetCheckTypeSettings.First().ExpireNear != null)
                            {
                                checktypesetting.Expirelimit = newAssetType.AssetCheckTypeSettings.First().Expirelimit;
                                checktypesetting.CheckNear = newAssetType.AssetCheckTypeSettings.First().CheckNear;
                                checktypesetting.ExpireNear = newAssetType.AssetCheckTypeSettings.First().ExpireNear;                        
                            }                        
                        }
                    }

                    _context.AssetCheckTypeSettings.Add(checktypesetting);
                }
                else
                {
                    // Retrieve the existing setting
                    var existingSetting = dbAssetType.AssetCheckTypeSettings.First(x => x.Id == checktypesetting.Id);
                    _context.Entry(existingSetting).CurrentValues.SetValues(checktypesetting);
                    
                    if (checktypesetting.Expirelimit.HasValue)
                    {
                        if (newAssetType.AssetCheckTypeSettings.First().Expirelimit.HasValue)
                        {
                            if (newAssetType.AssetCheckTypeSettings.First().Expirelimit != null && newAssetType.AssetCheckTypeSettings.First().CheckNear != null &&
                                newAssetType.AssetCheckTypeSettings.First().ExpireNear != null)
                            {
                                checktypesetting.Expirelimit = newAssetType.AssetCheckTypeSettings.First().Expirelimit;
                                checktypesetting.CheckNear = newAssetType.AssetCheckTypeSettings.First().CheckNear;
                                checktypesetting.ExpireNear = newAssetType.AssetCheckTypeSettings.First().ExpireNear;                        
                            }                        
                        }
                    }
                }
            }
            

            // dbAssetType.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync(audit);
            return NoContent();
        }

        // POST: api/AssetTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssetType>> PostAssetType(AssetType assetType)
        {
            if (_context.AssetTypes == null)
            {
                return Problem("Entity set 'AppDbContext.AssetTypes'  is null.");
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            assetType.ModifiedDate = DateTime.Now;
            _context.AssetTypes.Add(assetType);
            await _context.SaveChangesAsync(audit);

            // await _context.TypeTrainings.Where(e => e.AssetTypeId == assetType.Id && e.Type == "Returnable").ExecuteDeleteAsync();
            foreach (TypeTraining training in assetType.TypeTrainings)
            {
                training.Id = 0;
                training.Type = "Returnable";
                training.AssetTypeId = assetType.Id;
                _context.TypeTrainings.Add(training);
            }

            // await _context.DepartmentAssetTypes.Where(e => e.AssetTypeId == assetType.Id).ExecuteDeleteAsync();
            foreach (DepartmentAssetType depAT in assetType.DepartmentAssetTypes)
            {
                depAT.Id = 0;
                depAT.AssetTypeId = assetType.Id;
                depAT.ModifiedUserId = assetType.ModifiedUserId;
                // depAT.ModifiedDate = DateTime.Now;
                _context.DepartmentAssetTypes.Add(depAT);
            }
           
            foreach (AssetCheckTypeSetting checktypesetting in assetType.AssetCheckTypeSettings)
            {
                if (checktypesetting.Id == 0)
                {
                    checktypesetting.Id = 0;
                    checktypesetting.AssetTypeId = assetType.Id;
                    checktypesetting.CreatedUserId = assetType.ModifiedUserId;
                    checktypesetting.CreatedDate = DateTime.Now;
                    _context.AssetCheckTypeSettings.Add(checktypesetting);
                }
                else
                    _context.Entry(assetType.AssetCheckTypeSettings.Where(x => x.Id == checktypesetting.Id).First()).CurrentValues.SetValues(checktypesetting);
            }

            await _context.SaveChangesAsync(audit);

            return CreatedAtAction("GetAssetType", new { id = assetType.Id }, assetType);
        }

        // DELETE: api/AssetTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetType(int id)
        {
            if (_context.AssetTypes == null)
            {
                return NotFound();
            }
            AssetType? assetType = await _context.AssetTypes.FindAsync(id);
            if (assetType == null)
            {
                return NotFound();
            }

            _context.AssetTypes.Remove(assetType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssetTypeExists(int id)
        {
            return (_context.AssetTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}