using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NonReturnableAssetFieldsController(AppDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/NonReturnableAssetFields
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NonReturnableAssetField>>> GetNonReturnableAssetFields()
        {
          if (_context.NonReturnableAssetFields == null)
          {
              return NotFound();
          }
          var departmentId = Request.Headers["Departmentid"].ToString();
          if (string.IsNullOrEmpty(departmentId))
              return BadRequest("Departmentid is null");
            return await _context.NonReturnableAssetFields.Where(n => n.AssetType.DepartmentId == int.Parse(departmentId)).ToListAsync();
        }

        // GET: api/NonReturnableAssetFields/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NonReturnableAssetField>> GetNonReturnableAssetField(int id)
        {
          if (_context.NonReturnableAssetFields == null)
          {
              return NotFound();
          }
            NonReturnableAssetField? nonReturnableAssetField = await _context.NonReturnableAssetFields.FindAsync(id);

            if (nonReturnableAssetField == null)
            {
                return NotFound();
            }

            return nonReturnableAssetField;
        }

        // PUT: api/NonReturnableAssetFields/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNonReturnableAssetField(int id, NonReturnableAssetField nonReturnableAssetField)
        {
            if (id != nonReturnableAssetField.Id)
            {
                return BadRequest();
            }
            
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };


            _context.Entry(nonReturnableAssetField).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NonReturnableAssetFieldExists(id))
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

        // POST: api/NonReturnableAssetFields
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<NonReturnableAssetField>> PostNonReturnableAssetField(NonReturnableAssetField nonReturnableAssetField)
        {
          if (_context.NonReturnableAssetFields == null)
          {
              return Problem("Entity set 'AppDbContext.NonReturnableAssetFields'  is null.");
          }
           
          Audit audit = new()
          {
              CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
          };

          _context.NonReturnableAssetFields.Add(nonReturnableAssetField);
          await _context.SaveChangesAsync(audit);

          return CreatedAtAction("GetNonReturnableAssetField", new { id = nonReturnableAssetField.Id }, nonReturnableAssetField);
        }

        // DELETE: api/NonReturnableAssetFields/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNonReturnableAssetField(int id)
        {
            if (_context.NonReturnableAssetFields == null)
            {
                return NotFound();
            }
            NonReturnableAssetField? nonReturnableAssetField = await _context.NonReturnableAssetFields.FindAsync(id);
            if (nonReturnableAssetField == null)
            {
                return NotFound();
            }

            _context.NonReturnableAssetFields.Remove(nonReturnableAssetField);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NonReturnableAssetFieldExists(int id)
        {
            return (_context.NonReturnableAssetFields?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
