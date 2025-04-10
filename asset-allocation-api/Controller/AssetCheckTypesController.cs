using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetCheckTypesController(AppDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/AssetCheckTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetCheckType>>> GetAssetCheckTypes()
        {
            if (_context.AssetCheckTypes == null)
            {
                return NotFound();
            }
            return await _context.AssetCheckTypes.ToListAsync();
        }

        // GET: api/AssetCheckTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetCheckType>> GetAssetCheckType(int id)
        {
            if (_context.AssetCheckTypes == null)
            {
                return NotFound();
            }
            AssetCheckType? assetCheckType = await _context.AssetCheckTypes.FindAsync(id);

            if (assetCheckType == null)
            {
                return NotFound();
            }

            return assetCheckType;
        }

        // PUT: api/AssetCheckTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetCheckType(int id, AssetCheckType assetCheckType)
        {
            if (id != assetCheckType.Id)
            {
                return BadRequest();
            }

            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            try
            {
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetCheckTypeExists(id))
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

        // POST: api/AssetCheckTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AssetCheckType>> PostAssetCheckType(AssetCheckType assetCheckType)
        {
            if (_context.AssetCheckTypes == null)
            {
                return Problem("Entity set 'AppDbContext.AssetCheckTypes'  is null.");
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            _context.AssetCheckTypes.Add(assetCheckType);
            await _context.SaveChangesAsync(audit);

            return CreatedAtAction("GetAssetCheckType", new { id = assetCheckType.Id }, assetCheckType);
        }

        // DELETE: api/AssetCheckTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetCheckType(int id)
        {
            if (_context.AssetCheckTypes == null)
            {
                return NotFound();
            }
            AssetCheckType? assetCheckType = await _context.AssetCheckTypes.FindAsync(id);
            if (assetCheckType == null)
            {
                return NotFound();
            }

            _context.AssetCheckTypes.Remove(assetCheckType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssetCheckTypeExists(int id)
        {
            return (_context.AssetCheckTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}