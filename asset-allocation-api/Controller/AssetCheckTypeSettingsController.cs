using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using Z.EntityFramework.Plus;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace asset_allocation_api.Controller
{
    [Route("api/AssetCheckTypeSetting")]
    [ApiController]
    public class AssetCheckTypeSettingsController(AppDbContext context, ILogger<AssetCheckTypeSetting> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {

        private readonly AppDbContext _context = context;
        private readonly ILogger<AssetCheckTypeSetting> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        
        // GET: api/<AssetCheckTypeSettingsController>
        [HttpGet]
        public async Task<IEnumerable<AssetCheckTypeSetting>> GetAssetCheckTypeSettings()
        {
            IEnumerable<AssetCheckTypeSetting> assetchecktypesetting =  _context.AssetCheckTypeSettings.AsEnumerable();
            return assetchecktypesetting;
        }

        // GET api/<AssetCheckTypeSettingsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetCheckTypeSetting>> GetAssetCheckTypeSetting(int id)
        {
            if (_context.AssetCheckTypeSettings == null)
            {
                return NotFound();
            }
            AssetCheckTypeSetting assetchecktypesettings = await _context.AssetCheckTypeSettings.FindAsync(id);

            if (assetchecktypesettings == null)
            {
                return NotFound();
            }

            return assetchecktypesettings;
        }

        // POST api/<AssetCheckTypeSettingsController>
        [HttpPost]
        public async Task<ActionResult<AssetCheckTypeSetting>> PostAssetCheckTypeSetting(AssetCheckTypeSetting assetchecktypesetting)
        {
            if (_context.AssetCheckTypeSettings == null)
            {
                return Problem("Entity set 'AppDbContext.AssetCheckTypeSettings'  is null.");
            }

            Audit audit = new()
            {
                CreatedBy =  _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            assetchecktypesetting.CreatedDate = DateTime.Now;
            _context.AssetCheckTypeSettings.Add(assetchecktypesetting);
            await _context.SaveChangesAsync(audit);

            return CreatedAtAction("GetAssetCheckTypeSetting", new { id = assetchecktypesetting.Id }, assetchecktypesetting);
        }

        // PUT api/<AssetCheckTypeSettingsController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<AssetCheckTypeSetting>> PutAssetCheckTypeSetting(int id, AssetCheckTypeSetting assetchecktypesetting)
        {
            if (id != assetchecktypesetting.Id)
            {
                return BadRequest();
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            AssetCheckTypeSetting oldAssetCheckTypeSetting = await _context.AssetCheckTypeSettings.Where(a => a.Id == assetchecktypesetting.Id).FirstOrDefaultAsync();

            _context.Entry(oldAssetCheckTypeSetting).CurrentValues.SetValues(assetchecktypesetting);
            await _context.SaveChangesAsync(audit);

            return NoContent();
        }

        // DELETE api/<AssetCheckTypeSettingsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetCheckTypeSetting(int id)
        {
            if (_context.AssetCheckTypeSettings == null)
            {
                return NotFound();
            }
            AssetCheckTypeSetting? assetchecktypesetting = await _context.AssetCheckTypeSettings.FindAsync(id);
            if (assetchecktypesetting == null)
            {
                return NotFound();
            }

            _context.AssetCheckTypeSettings.Remove(assetchecktypesetting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
