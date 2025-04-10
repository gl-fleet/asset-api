using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Util;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NonReturnableAssetPersonnelSettingsController(AppDbContext context, ILogger<NonReturnableAssetPersonnelSettingsController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/NonReturnableAssetPersonnelSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NonReturnableAssetPersonnelSetting>>> GetNonReturnableAssetPersonnelSettings()
        {
            if (_context.NonReturnableAssetPersonnelSettings == null)
            {
                return NotFound();
            }
            
            var departmentId = Request.Headers["Departmentid"].ToString();

            // Use the header value as needed
            if (string.IsNullOrEmpty(departmentId))
                return BadRequest("Departmentid is null");
            return await _context.NonReturnableAssetPersonnelSettings.Where(s => s.Field.AssetType.DepartmentId == int.Parse(departmentId)).ToListAsync();
        }

        // GET: api/NonReturnableAssetPersonnelSettings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NonReturnableAssetPersonnelSetting>> GetNonReturnableAssetPersonnelSetting(int id)
        {
            if (_context.NonReturnableAssetPersonnelSettings == null)
            {
                return NotFound();
            }
            NonReturnableAssetPersonnelSetting? nonReturnableAssetPersonnelSetting = await _context.NonReturnableAssetPersonnelSettings.FindAsync(id);

            if (nonReturnableAssetPersonnelSetting == null)
            {
                return NotFound();
            }

            return nonReturnableAssetPersonnelSetting;
        }

        // PUT: api/NonReturnableAssetPersonnelSettings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNonReturnableAssetPersonnelSetting(int id, NonReturnableAssetPersonnelSetting nonReturnableAssetPersonnelSetting)
        {
            if (_context.NonReturnableAssetPersonnelSettings == null)
            {
                return Problem("Entity set 'AppDbContext.NonReturnableAssetPersonnelSettings'  is null.");
            }

            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            
            NonReturnableAssetPersonnelSetting? a = await _context.NonReturnableAssetPersonnelSettings.Where(e => e.PersonnelId == nonReturnableAssetPersonnelSetting.PersonnelId && e.FieldId == nonReturnableAssetPersonnelSetting.FieldId).FirstOrDefaultAsync();
            if (a == null)
            {
                _context.NonReturnableAssetPersonnelSettings.Add(nonReturnableAssetPersonnelSetting);
                await _context.SaveChangesAsync(audit);

                return CreatedAtAction("GetNonReturnableAssetPersonnelSettings", new { id = nonReturnableAssetPersonnelSetting.Id }, nonReturnableAssetPersonnelSetting);
            }
            else
            {
                a.Value = nonReturnableAssetPersonnelSetting.Value;
                try
                {
                    await _context.SaveChangesAsync(audit);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NonReturnableAssetPersonnelSettingExists(nonReturnableAssetPersonnelSetting.Id))
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

        // POST: api/NonReturnableAssetPersonnelSettings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<NonReturnableAssetPersonnelSetting>> PostNonReturnableAssetPersonnelSetting(NonReturnableAssetPersonnelSetting nonReturnableAssetPersonnelSetting)
        {
            if (_context.NonReturnableAssetPersonnelSettings == null)
            {
                return Problem("Entity set 'AppDbContext.NonReturnableAssetPersonnelSettings'  is null.");
            }

            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            NonReturnableAssetPersonnelSetting? a = await _context.NonReturnableAssetPersonnelSettings.Where(e => e.PersonnelId == nonReturnableAssetPersonnelSetting.PersonnelId && e.FieldId == nonReturnableAssetPersonnelSetting.FieldId).FirstOrDefaultAsync();
            if (a == null)
            {
                _context.NonReturnableAssetPersonnelSettings.Add(nonReturnableAssetPersonnelSetting);
                await _context.SaveChangesAsync(audit);

                return CreatedAtAction("GetNonReturnableAssetPersonnelSettings", new { id = nonReturnableAssetPersonnelSetting.Id }, nonReturnableAssetPersonnelSetting);
            }
            else
            {
                if (a.Value != nonReturnableAssetPersonnelSetting.Value)
                {
                    a.Value = nonReturnableAssetPersonnelSetting.Value;                    
                }
                
                try
                {
                    await _context.SaveChangesAsync(audit);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NonReturnableAssetPersonnelSettingExists(nonReturnableAssetPersonnelSetting.Id))
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

        // DELETE: api/NonReturnableAssetPersonnelSettings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNonReturnableAssetPersonnelSetting(int id)
        {
            if (_context.NonReturnableAssetPersonnelSettings == null)
            {
                return NotFound();
            }
            NonReturnableAssetPersonnelSetting? nonReturnableAssetPersonnelSetting = await _context.NonReturnableAssetPersonnelSettings.FindAsync(id);
            if (nonReturnableAssetPersonnelSetting == null)
            {
                return NotFound();
            }

            _context.NonReturnableAssetPersonnelSettings.Remove(nonReturnableAssetPersonnelSetting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NonReturnableAssetPersonnelSettingExists(int id)
        {
            return (_context.NonReturnableAssetPersonnelSettings?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}