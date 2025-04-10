using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Models;
using asset_allocation_api.Util;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonnelsController(AppDbContext context, ILogger<PersonnelsController> logger) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<PersonnelsController> _logger = logger;

        // GET: api/Personnels
        [HttpGet]
        public async Task<ActionResult<Response<PersonnelListResult?>>> GetPersonnel(int Page = 1, int PageSize = 10, string? SortField = "PersonnelId", string? SortOrder = "desc", string? FilterField = "", string? FilterValue = "")
        {

            Response<PersonnelListResult?> resp = new();
            if (_context.Personnel == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 500, false, "Personnel context is null");
            }

            SortOrder = SortOrder!.ToLower();
            if (!"asc".Equals(SortOrder) && !"desc".Equals(SortOrder))
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortOrder invalid. Please choose from asc or desc");
            }

            if (typeof(Personnel).GetProperty(SortField!) == null)
            {
                return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "SortField invalid. There is no attribute/property named {0}", values: [SortField]);
            }

            IQueryable<Personnel> personnelContext = _context.Personnel;

            /** __Multi_filtering__ **/
            if (!string.IsNullOrEmpty(FilterField) && !string.IsNullOrEmpty(FilterValue))
            {
                string[] Fields = FilterField.Split(',');
                string[] Values = FilterValue.Split(',');

                var Filter = new Dictionary<string, List<string>>();

                if(Fields.Length == Values.Length)
                {
                    for (int i = 0; i < Fields.Length; i++)
                    {
                        if (!Filter.ContainsKey(Fields[i])) Filter[Fields[i]] = new List<string>();
                        Filter[Fields[i]].Add(Values[i]);
                    }
                    foreach(KeyValuePair<string, List<string>> s in Filter)
                    {
                        logger.LogInformation("Personnel data debug: {} -> {} Size:{}", s.Key, s.Value[0], s.Value.Count);

                        if (!string.IsNullOrEmpty(s.Key) && typeof(Personnel).GetProperty(s.Key) == null) 
                            return ResponseUtils.ReturnResponse(_logger, null, resp, null, 400, false, "FilterField invalid. There is no attribute/property named {0}", values: [s.Key]);
                        if(s.Value.Count == 1) personnelContext = personnelContext.Where(a => EF.Functions.Like((string)EF.Property<object>(a, s.Key), $"{s.Value[0]}%"));
                        else personnelContext = personnelContext.Where(a => s.Value.Contains( (string)EF.Property<object>(a, s.Key) ));
                    }
                }
            }

            int count = personnelContext.Count();

            if ("asc".Equals(SortOrder)) personnelContext = personnelContext.OrderBy(a => EF.Property<Personnel>(a, SortField!));
            else personnelContext = personnelContext.OrderByDescending(a => EF.Property<Personnel>(a, SortField!));

            PersonnelListResult myobj = new((count - 1) / PageSize + 1, await personnelContext.Skip(PageSize * (Page - 1)).Take(PageSize).ToListAsync());

            return ResponseUtils.ReturnResponse(_logger, null, resp, myobj, 200, true, "Personnel data returned successfully.");
        }

        // GET: api/Personnels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Personnel>> GetPersonnel(int id)
        {
            if (_context.Personnel == null)
            {
                return NotFound();
            }
            Personnel? personnel = await _context.Personnel.FindAsync(id);

            if (personnel == null)
            {
                return NotFound();
            }

            return personnel;
        }

        // PUT: api/Personnels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPersonnel(int id, Personnel personnel)
        {
            if (id != personnel.PersonnelNo)
            {
                return BadRequest();
            }

            _context.Entry(personnel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonnelExists(id))
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

        // POST: api/Personnels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Personnel>> PostPersonnel(Personnel personnel)
        {
            if (_context.Personnel == null)
            {
                return Problem("Entity set 'AppDbContext.Personnel'  is null.");
            }
            _context.Personnel.Add(personnel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PersonnelExists(personnel.PersonnelNo))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPersonnel", new { id = personnel.PersonnelNo }, personnel);
        }

        // DELETE: api/Personnels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersonnel(int id)
        {
            if (_context.Personnel == null)
            {
                return NotFound();
            }
            Personnel? personnel = await _context.Personnel.FindAsync(id);
            if (personnel == null)
            {
                return NotFound();
            }

            _context.Personnel.Remove(personnel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PersonnelExists(int id)
        {
            return (_context.Personnel?.Any(e => e.PersonnelNo == id)).GetValueOrDefault();
        }
    }
}