using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NonReturnableAssetTypesController(AppDbContext context, ILogger<AssetAllocationsController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetAllocationsController> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/NonReturnableAssetTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NonReturnableAssetTypeListResult>>> GetNonReturnableAssetTypes(int Page = 1, int PageSize = 100, string? SortField = "ModifiedDate", string? SortOrder = "desc", string? FilterField = "", string? FilterValue = "")
        {
            Response<NonReturnableAssetTypeListResult?> resp = new();

            if (_context.NonReturnableAssetTypes == null)
            {
                return NotFound();
            }

            SortOrder = SortOrder!.ToLower();
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            if (typeof(NonReturnableAssetType).GetProperty(SortField!) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortField invalid. There is no attribute/property named {0}", values: [SortField]);
            }

            if (!string.IsNullOrEmpty(FilterField) && typeof(NonReturnableAssetType).GetProperty(FilterField) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [FilterField]);
            }

            var departmentId = Request.Headers["Departmentid"].ToString();

            // Use the header value as needed
            if (string.IsNullOrEmpty(departmentId))
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "Departmentid is null");
            IQueryable<NonReturnableAssetType> b = _context.NonReturnableAssetTypes.Where(n => n.DepartmentId == int.Parse(departmentId))
                .Include(b => b.NonReturnableAssetFields)
                .Include(b => b.TypeTrainings.Where(c => c.Type == "NonReturnable"));
            if (!string.IsNullOrEmpty(FilterField))
                b = b.Where(a => EF.Functions.Like((string)EF.Property<object>(a, FilterField), $"%{FilterValue}%"));

            if ("asc".Equals(SortOrder))
                b = b.OrderBy(a => EF.Property<Asset>(a, SortField!));
            else
                b = b.OrderByDescending(a => EF.Property<Asset>(a, SortField!));

            NonReturnableAssetTypeListResult myobj = new((b.Count() - 1) / PageSize + 1, await b.Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync());

            return ResponseUtils.ReturnResponse(_logger, null, resp, myobj, 200, true, "Special types returned successfully");
        }

        // GET: api/NonReturnableAssetTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NonReturnableAssetType>> GetNonReturnableAssetType(int id)
        {
            if (_context.NonReturnableAssetTypes == null)
            {
                return NotFound();
            }
            NonReturnableAssetType? nonReturnableAssetType = await _context.NonReturnableAssetTypes
                    .Where(b => b.Id.Equals(id))
                    .Include(b => b.NonReturnableAssetFields)
                    .Include(b => b.TypeTrainings.Where(c => c.Type == "NonReturnable"))
                    .FirstAsync();

            if (nonReturnableAssetType == null)
            {
                return NotFound();
            }

            return nonReturnableAssetType;
        }

        // PUT: api/NonReturnableAssetTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNonReturnableAssetType(int id, NonReturnableAssetType newAssetType)
        {
            Response<NonReturnableAssetTypeListResult?> resp = new();
            if (id != newAssetType.Id)
            {
                return BadRequest();
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            NonReturnableAssetType? dbAssetType = await _context.NonReturnableAssetTypes
                .Where(b => b.Id.Equals(id))
                .Include(b => b.TypeTrainings.Where(c => c.Type == "NonReturnable"))
                .FirstAsync();

            _context.Entry(dbAssetType).CurrentValues.SetValues(newAssetType);

            foreach (TypeTraining dat in dbAssetType.TypeTrainings.ToList())
            {
                if (!newAssetType.TypeTrainings.Select(x => x.Id).Contains(dat.Id)) dbAssetType.TypeTrainings.Remove(dat);
            }

            foreach (TypeTraining tt in newAssetType.TypeTrainings)
            {
                if (tt.Id == 0)
                {
                    dbAssetType.TypeTrainings.Add(tt);                    
                }
                else
                {
                    _context.Entry(dbAssetType.TypeTrainings.Where(x => x.Id == tt.Id).First()).CurrentValues.SetValues(tt);
                }
            }

            /** Update custom fields **/

            Dictionary<int, string> kv = new Dictionary<int, string>();

            await _context.NonReturnableAssetFields.Where(e => e.AssetTypeId == newAssetType.Id).ForEachAsync( e => {

                Console.WriteLine($" # PREV {e.Id} / {e.FieldName} / {e.ValueType} ");
                kv.Add(e.Id, "Deleted");

            });

            foreach (NonReturnableAssetField e in newAssetType.NonReturnableAssetFields)
            {
                // New asset field id always 0
                if(e.Id == 0)
                {
                    e.Id = 0;
                    e.AssetTypeId = newAssetType.Id;
                    _context.NonReturnableAssetFields.Add(e);
                }
                else if (kv.ContainsKey(e.Id))
                {
                    kv.Remove(e.Id);
                    await _context.NonReturnableAssetFields.Where(f => f.AssetTypeId == newAssetType.Id && f.Id == e.Id).UpdateAsync(u => new NonReturnableAssetField {
                        FieldName = e.FieldName,
                        ValueType = e.ValueType,
                    });
                }

            }

            // if(kv.Count > 0 ) await _context.NonReturnableAssetFields.Where(f => f.AssetTypeId == newAssetType.Id && kv.Keys.Contains(f.Id)).ExecuteDeleteAsync();

            var departmentId = Request.Headers["Departmentid"].ToString();

            // Use the header value as needed
            if (string.IsNullOrEmpty(departmentId))
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "Departmentid is null");
            
            dbAssetType.DepartmentId = int.Parse(departmentId);
            dbAssetType.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync(audit);
            return NoContent();
        }

        // POST: api/NonReturnableAssetTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<NonReturnableAssetType>> PostNonReturnableAssetType(NonReturnableAssetType nonReturnableAssetType)
        {
            Response<NonReturnableAssetTypeListResult?> resp = new();
            if (_context.NonReturnableAssetTypes == null)
            {
                return Problem("Entity set 'AppDbContext.NonReturnableAssetTypes'  is null.");
            }
            
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            var departmentId = Request.Headers["Departmentid"].ToString();

            // Use the header value as needed
            if (string.IsNullOrEmpty(departmentId))
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "Departmentid is null");
            
            nonReturnableAssetType.DepartmentId = int.Parse(departmentId);
            _context.NonReturnableAssetTypes.Add(nonReturnableAssetType);
            await _context.SaveChangesAsync(audit);

            // await _context.TypeTrainings.Where(e => e.AssetTypeId == nonReturnableAssetType.Id && e.Type == "NonReturnable").ExecuteDeleteAsync();
            foreach (TypeTraining training in nonReturnableAssetType.TypeTrainings)
            {
                training.Id = 0;
                training.Type = "NonReturnable";
                training.NonReturnableAssetTypeId = nonReturnableAssetType.Id;
                _context.TypeTrainings.Add(training);
            }

            // await _context.NonReturnableAssetFields.Where(e => e.AssetTypeId == nonReturnableAssetType.Id).ExecuteDeleteAsync();
            foreach (NonReturnableAssetField nraf in nonReturnableAssetType.NonReturnableAssetFields)
            {
                nraf.Id = 0;
                nraf.AssetTypeId = nonReturnableAssetType.Id;
                _context.NonReturnableAssetFields.Add(nraf);
            }

            
            await _context.SaveChangesAsync(audit);
            return CreatedAtAction("GetNonReturnableAssetType", new { id = nonReturnableAssetType.Id }, nonReturnableAssetType);
        }

        // DELETE: api/NonReturnableAssetTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNonReturnableAssetType(int id)
        {
            if (_context.NonReturnableAssetTypes == null)
            {
                return NotFound();
            }
            NonReturnableAssetType? nonReturnableAssetType = await _context.NonReturnableAssetTypes.FindAsync(id);
            if (nonReturnableAssetType == null)
            {
                return NotFound();
            }

            _context.NonReturnableAssetTypes.Remove(nonReturnableAssetType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NonReturnableAssetTypeExists(int id)
        {
            return (_context.NonReturnableAssetTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}