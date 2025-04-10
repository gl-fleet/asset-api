using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using Z.EntityFramework.Plus;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace asset_allocation_api.Controller
{
    [Route("api/AssetInspectionHistory")]
    [ApiController]
    public class AssetInspectionHistories(AppDbContext context, ILogger<AssetInspectionHistory> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetInspectionHistory> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        
        // GET: api/<AssetInspectionHistories>
        [HttpGet]
        public async Task<IEnumerable<AssetInspectionHistory>> GetAssetInspectionHistories(int AssetId)
        {
            // IEnumerable<AssetInspectionHistory> assetinspectionhistories = _context.AssetInspectionHistories.AsEnumerable();
            IEnumerable<AssetInspectionHistory> assetinspectionhistories = _context.AssetInspectionHistories.Where(a => a.AssetId == AssetId).OrderByDescending(e => e.CheckedDateTime).ThenByDescending(e => e.Id);
            // IQueryable<Asset> assetContext = _context.Assets.Where(a => a.DepartmentId == DepartmentId);
            return assetinspectionhistories;
        }

        // GET api/<AssetInspectionHistories>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetInspectionHistory>> GetAssetInspectionHistory(int id)
        {
            if (_context.AssetInspectionHistories == null)
            {
                return NotFound();
            }
            AssetInspectionHistory? assetinspectionhistory = await _context.AssetInspectionHistories.FindAsync(id);

            if (assetinspectionhistory == null)
            {
                return NotFound();
            }

            return assetinspectionhistory;
        }

        // POST api/<AssetInspectionHistories>
        [HttpPost]
        public async Task<ActionResult<AssetInspectionHistory>> PostAssetInspectionHistory(AssetInspectionHistory assetinspectionhistory)
        {
            if (_context.AssetInspectionHistories == null)
            {
                return Problem("Entity set 'AppDbContext.AssetInspectionHistories'  is null.");
            }
            
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            Asset? asset = await _context.Assets.FindAsync(assetinspectionhistory.AssetId);

            if (asset == null)
            {
                return Problem($"No such asset exists with id : \"{assetinspectionhistory.AssetId}\"");
            }


            _context.AssetInspectionHistories.Add(assetinspectionhistory);
            await _context.SaveChangesAsync(audit);

            //asset.LastMaintenanceId = assetinspectionhistory.Id;
            //_context.Entry(asset).State = EntityState.Modified;
            //await _context.SaveChangesAsync();

            return CreatedAtAction("GetAssetInspectionHistory", new { id = assetinspectionhistory.Id }, assetinspectionhistory);
        }

        // PUT api/<AssetInspectionHistories>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetInspectionHistory(int id, AssetInspectionHistory assetinspectionhistory)
        {
            if (id != assetinspectionhistory.Id)
            {
                return BadRequest();
            }
        
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            _context.Entry(assetinspectionhistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetInspectionHistoryExists(id))
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

        // DELETE api/<AssetInspectionHistories>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetInspectionHistory(int id)
        {
            if (_context.AssetInspectionHistories == null)
            {
                return NotFound();
            }
            AssetInspectionHistory? assetinspectionhistory = await _context.AssetInspectionHistories.FindAsync(id);
            if (assetinspectionhistory == null)
            {
                return NotFound();
            }

            _context.AssetInspectionHistories.Remove(assetinspectionhistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssetInspectionHistoryExists(int id)
        {
            return (_context.AssetInspectionHistories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
