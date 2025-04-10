using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentPersonnelsController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // GET: api/DepartmentPersonnels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentPersonnel>>> GetDepartmentPersonnel()
        {
            if (_context.DepartmentPersonnel == null)
            {
                return NotFound();
            }
            return await _context.DepartmentPersonnel.ToListAsync();
        }

        // GET: api/DepartmentPersonnels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentPersonnel>> GetDepartmentPersonnel(int id)
        {
            if (_context.DepartmentPersonnel == null)
            {
                return NotFound();
            }
            DepartmentPersonnel? departmentPersonnel = await _context.DepartmentPersonnel.FindAsync(id);

            if (departmentPersonnel == null)
            {
                return NotFound();
            }

            return departmentPersonnel;
        }

        // PUT: api/DepartmentPersonnels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartmentPersonnel(int id, DepartmentPersonnel departmentPersonnel)
        {
            if (id != departmentPersonnel.Id)
            {
                return BadRequest();
            }

            _context.Entry(departmentPersonnel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentPersonnelExists(id))
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

        // POST: api/DepartmentPersonnels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DepartmentPersonnel>> PostDepartmentPersonnel(DepartmentPersonnel departmentPersonnel)
        {
            if (_context.DepartmentPersonnel == null)
            {
                return Problem("Entity set 'AppDbContext.DepartmentPersonnel'  is null.");
            }
            _context.DepartmentPersonnel.Add(departmentPersonnel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartmentPersonnel", new { id = departmentPersonnel.Id }, departmentPersonnel);
        }

        // DELETE: api/DepartmentPersonnels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartmentPersonnel(int id)
        {
            if (_context.DepartmentPersonnel == null)
            {
                return NotFound();
            }
            DepartmentPersonnel? departmentPersonnel = await _context.DepartmentPersonnel.FindAsync(id);
            if (departmentPersonnel == null)
            {
                return NotFound();
            }

            _context.DepartmentPersonnel.Remove(departmentPersonnel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmentPersonnelExists(int id)
        {
            return (_context.DepartmentPersonnel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}