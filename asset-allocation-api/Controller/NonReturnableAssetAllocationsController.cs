using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NonReturnableAssetAllocationsController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        // GET: api/NonReturnableAssetAllocations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NonReturnableAssetAllocation>>> GetNonReturnableAssetAllocations()
        {
            if (_context.NonReturnableAssetAllocations == null)
            {
                return NotFound();
            }
            return await _context.NonReturnableAssetAllocations.ToListAsync();
        }

        // GET: api/NonReturnableAssetAllocations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NonReturnableAssetAllocation>> GetNonReturnableAssetAllocation(int id)
        {
            if (_context.NonReturnableAssetAllocations == null)
            {
                return NotFound();
            }
            NonReturnableAssetAllocation? nonReturnableAssetAllocation = await _context.NonReturnableAssetAllocations.FindAsync(id);

            if (nonReturnableAssetAllocation == null)
            {
                return NotFound();
            }

            return nonReturnableAssetAllocation;
        }

        // PUT: api/NonReturnableAssetAllocations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNonReturnableAssetAllocation(int id, NonReturnableAssetAllocation nonReturnableAssetAllocation)
        {
            if (id != nonReturnableAssetAllocation.Id)
            {
                return BadRequest();
            }

            _context.Entry(nonReturnableAssetAllocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NonReturnableAssetAllocationExists(id))
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

        // POST: api/NonReturnableAssetAllocations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<NonReturnableAssetAllocation>> PostNonReturnableAssetAllocation(NonReturnableAssetAllocation nonReturnableAssetAllocation)
        {
            if (_context.NonReturnableAssetAllocations == null)
            {
                return Problem("Entity set 'AppDbContext.NonReturnableAssetAllocations'  is null.");
            }

            if (nonReturnableAssetAllocation.AssignedDate == null)
                nonReturnableAssetAllocation.AssignedDate = DateTime.Now;

            _context.NonReturnableAssetAllocations.Add(nonReturnableAssetAllocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNonReturnableAssetAllocation", new { id = nonReturnableAssetAllocation.Id }, nonReturnableAssetAllocation);
        }

        // DELETE: api/NonReturnableAssetAllocations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNonReturnableAssetAllocation(int id)
        {
            if (_context.NonReturnableAssetAllocations == null)
            {
                return NotFound();
            }
            NonReturnableAssetAllocation? nonReturnableAssetAllocation = await _context.NonReturnableAssetAllocations.FindAsync(id);
            if (nonReturnableAssetAllocation == null)
            {
                return NotFound();
            }

            _context.NonReturnableAssetAllocations.Remove(nonReturnableAssetAllocation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NonReturnableAssetAllocationExists(int id)
        {
            return (_context.NonReturnableAssetAllocations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}