using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Model.custom_model;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentAssetTypesController(AppDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/DepartmentAssetTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentAssetTypeView>>> GetDepartmentAssetTypes(int? departmentId)
        {
            if (_context.DepartmentAssetTypes == null)
            {
                return NotFound();
            }

            IQueryable<DepartmentAssetType> departmentAssetTypeContext;

            if (departmentId != null && departmentId > 0)
                departmentAssetTypeContext = _context.DepartmentAssetTypes.Where(a => a.DepartmentId == departmentId).Include(a => a.AssetType.AssetCheckTypeSettings).ThenInclude(a => a.CheckType);
            else
                departmentAssetTypeContext = _context.DepartmentAssetTypes.Where(a => 1 == 1).Include(a => a.AssetType.AssetCheckTypeSettings).ThenInclude(a => a.CheckType);

            return await (from dat in departmentAssetTypeContext
                          join d in _context.Departments on dat.DepartmentId equals d.Id
                          join at in _context.AssetTypes on dat.AssetTypeId equals at.Id
                          select new DepartmentAssetTypeView
                          {
                              Id = dat.Id,
                              DepartmentId = dat.DepartmentId,
                              DepartmentName = d.Name,
                              AssetTypeId = dat.AssetTypeId,
                              AssetTypeName = at.Name,
                              ModifiedUserId = dat.ModifiedUserId,
                              ModifiedDate = dat.ModifiedDate,
                              AssetCheckTypeSettings = dat.AssetType.AssetCheckTypeSettings
                          }).ToListAsync();
        }

        // GET: api/DepartmentAssetTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentAssetType>> GetDepartmentAssetType(int id)
        {
            if (_context.DepartmentAssetTypes == null)
            {
                return NotFound();
            }
            DepartmentAssetType? departmentAssetType = await _context.DepartmentAssetTypes.FindAsync(id);

            if (departmentAssetType == null)
            {
                return NotFound();
            }

            return departmentAssetType;
        }

        // PUT: api/DepartmentAssetTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartmentAssetType(int id, DepartmentAssetType departmentAssetType)
        {
            if (id != departmentAssetType.Id)
            {
                return BadRequest();
            }

            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            _context.Entry(departmentAssetType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentAssetTypeExists(id))
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

        // POST: api/DepartmentAssetTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DepartmentAssetType>> PostDepartmentAssetType(DepartmentAssetType departmentAssetType)
        {
            if (_context.DepartmentAssetTypes == null)
            {
                return Problem("Entity set 'AppDbContext.DepartmentAssetTypes'  is null.");
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            _context.DepartmentAssetTypes.Add(departmentAssetType);
            await _context.SaveChangesAsync(audit);

            return CreatedAtAction("GetDepartmentAssetType", new { id = departmentAssetType.Id }, departmentAssetType);
        }

        // DELETE: api/DepartmentAssetTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartmentAssetType(int id)
        {
            if (_context.DepartmentAssetTypes == null)
            {
                return NotFound();
            }
            DepartmentAssetType? departmentAssetType = await _context.DepartmentAssetTypes.FindAsync(id);
            if (departmentAssetType == null)
            {
                return NotFound();
            }

            _context.DepartmentAssetTypes.Remove(departmentAssetType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmentAssetTypeExists(int id)
        {
            return (_context.DepartmentAssetTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}