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
    public class NonReturnableAssetPersonnelPermitsController(AppDbContext context, ILogger<NonReturnableAssetPersonnelPermitsController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/NonReturnableAssetPersonnelPermits
        [HttpGet]
        public async Task<ActionResult> GetNonReturnableAssetPersonnelPermits(int Page = 1, int PageSize = 100, string? SortField = "Id", string? SortOrder = "asc", string? FilterField = "", string? FilterValue = "")
        {
            Response<List<NonReturnableAssetPersonnelPermitView>?> resp = new();

            if (_context.NonReturnableAssetAllocations == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 404, false, "AssetAllocations context is null");
            }
            SortOrder = SortOrder!.ToLower();
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            if (typeof(AssetAllocation).GetProperty(SortField!) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortField invalid. There is no attribute/property named {0}", values: [SortField]);
            }

            /* if (!string.IsNullOrEmpty(FilterField) && typeof(AssetAllocation).GetProperty(FilterField) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [FilterField]);
            } */

            IQueryable<NonReturnableAssetPersonnelPermit> NRAPersonnelPermit = _context.NonReturnableAssetPersonnelPermits;

            if (!string.IsNullOrEmpty(FilterField))
                NRAPersonnelPermit = NRAPersonnelPermit.Where(a => EF.Functions.Like((string)EF.Property<object>(a, FilterField), $"%{FilterValue}%"));

            if ("asc".Equals(SortOrder))
                NRAPersonnelPermit = NRAPersonnelPermit.OrderBy(a => EF.Property<Asset>(a, SortField!));
            else
                NRAPersonnelPermit = NRAPersonnelPermit.OrderByDescending(a => EF.Property<Asset>(a, SortField!));
            
            var departmentId = Request.Headers["Departmentid"].ToString();
            if (string.IsNullOrEmpty(departmentId))
                return BadRequest("Departmentid is null");

            List<NonReturnableAssetPersonnelPermitView> returnList = await (from a in NRAPersonnelPermit
                                                                            join b in _context.NonReturnableAssetTypes.Where(n => n.DepartmentId == int.Parse(departmentId)) on a.AssetTypeId equals b.Id into bGrouping
                                                                            from bSel in bGrouping
                                                                            from e in _context.NonReturnableAssetAllocations
                                                                                .Where(eWhere => eWhere.PermitId == a.Id)
                                                                                .OrderByDescending(eOrder => eOrder.AssignedDate)
                                                                                .Take(1).DefaultIfEmpty()
                                                                                //from aSel in bGrouping.Where(aw => aw.DepartmentId == DepartmentId)
                                                                            join c in _context.NonReturnableAssetFields on bSel.Id equals c.AssetTypeId into cGrouping
                                                                            from cSel in cGrouping.DefaultIfEmpty()
                                                                            join d in _context.NonReturnableAssetPersonnelSettings on 
                                                                                new { x1 = a.PersonnelId, x2 = (int?)cSel.Id } equals new { x1 = d.PersonnelId, x2 = d.FieldId } into dGrouping
                                                                            from dSel in dGrouping.DefaultIfEmpty()
                                                                            let allocatedCount = _context.NonReturnableAssetAllocations.Where(aa => 
                                                                                aa.PermitId == a.Id &&
                                                                                aa.AssignedDate >= DateTime.Now.AddDays(-bSel.CooldownDays.GetValueOrDefault()) &&
                                                                                aa.AssignedDate <= DateTime.Now 
                                                                                ).ToList()
                                                                            select new NonReturnableAssetPersonnelPermitView
                                                                            {
                                                                                Id = a.Id,
                                                                                PersonnelNo = a.PersonnelNo,
                                                                                AssetTypeId = a.AssetTypeId,
                                                                                Enabled = a.Enabled,
                                                                                ModifiedUserId = a.ModifiedUserId,
                                                                                ModifiedDate = a.ModifiedDate,
                                                                                TypeName = bSel.Name,
                                                                                TypeIconName = bSel.IconName,
                                                                                TypeLimit = bSel.Limit,
                                                                                TypeCooldownDays = bSel.CooldownDays,
                                                                                AllocationLastDate = e.AssignedDate,
                                                                                // DateTime.Now > AssignedDate + CooldDownDays -> return 1
                                                                                AllocationColor = DateTime.Now.CompareTo(e.AssignedDate.GetValueOrDefault().AddDays(bSel.CooldownDays.GetValueOrDefault())) > 0 ? "Green" : "Red",
                                                                                SettingsId = cSel.Id,
                                                                                SettingsFieldName = cSel.FieldName,
                                                                                SettingsValueType = cSel.ValueType,
                                                                                SettingsValue = dSel.Value,
                                                                                AllocatedCount = allocatedCount.Count()
                                                                            }).Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync();

            return ResponseUtils.ReturnResponse(_logger, null, resp, returnList, 200, true, "Personnel special asset permits returned successfully");
        }

        // GET: api/NonReturnableAssetPersonnelPermits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NonReturnableAssetPersonnelPermit>> GetNonReturnableAssetPersonnelPermit(int id)
        {
            if (_context.NonReturnableAssetPersonnelPermits == null)
            {
                return NotFound();
            }
            NonReturnableAssetPersonnelPermit? nonReturnableAssetPersonnelPermit = await _context.NonReturnableAssetPersonnelPermits.FindAsync(id);

            if (nonReturnableAssetPersonnelPermit == null)
            {
                return NotFound();
            }

            return nonReturnableAssetPersonnelPermit;
        }

        // POST: api/NonReturnableAssetPersonnelPermits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<NonReturnableAssetPersonnelPermit>> PostNonReturnableAssetPersonnelPermit(NonReturnableAssetPersonnelPermit nonReturnableAssetPersonnelPermit)
        {
            if (_context.NonReturnableAssetPersonnelPermits == null)
            {
                return Problem("Entity set 'AppDbContext.NonReturnableAssetPersonnelPermits'  is null.");
            }
            
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            
            NonReturnableAssetPersonnelPermit? a = await _context.NonReturnableAssetPersonnelPermits.Where(e => e.PersonnelId == nonReturnableAssetPersonnelPermit.PersonnelId && e.AssetTypeId == nonReturnableAssetPersonnelPermit.AssetTypeId).FirstOrDefaultAsync();
            if (a == null)
            {
                _context.NonReturnableAssetPersonnelPermits.Add(nonReturnableAssetPersonnelPermit);
                await _context.SaveChangesAsync(audit);

                return CreatedAtAction("GetNonReturnableAssetPersonnelPermit", new { id = nonReturnableAssetPersonnelPermit.Id }, nonReturnableAssetPersonnelPermit);
            }
            else
            {
                a.Enabled = nonReturnableAssetPersonnelPermit.Enabled;
                
                try
                {
                    await _context.SaveChangesAsync(audit);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NonReturnableAssetPersonnelPermitExists(nonReturnableAssetPersonnelPermit.Id))
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
        }

        private bool NonReturnableAssetPersonnelPermitExists(int id)
        {
            return (_context.NonReturnableAssetPersonnelPermits?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}