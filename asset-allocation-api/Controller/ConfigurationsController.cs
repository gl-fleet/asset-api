using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using Z.EntityFramework.Plus;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace asset_allocation_api.Controller
{
    [Route("api/Configuration")]
    [ApiController]
    public class ConfigurationsController(AppDbContext context, ILogger<ConfigurationsController> logger, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {

        private readonly AppDbContext _context = context;
        private readonly ILogger<ConfigurationsController> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // GET: api/<ConfigurationsController>
        [HttpGet]
        public async Task<IEnumerable<Configuration>> GetAllConfigurations(int DepartmentId = 0)
        {
            IEnumerable<Configuration> configurations = DepartmentId == 0 ? _context.Configurations.AsEnumerable() : _context.Configurations.Where(e => e.DepartmentId == DepartmentId).AsEnumerable();
            return configurations;
        }

        // GET api/<ConfigurationsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Configuration>> Get(int id)
        {
            if (_context.Configurations == null)
            {
                return NotFound();
            }
            Configuration? configuration = await _context.Configurations.FindAsync(id);

            if (configuration == null)
            {
                return NotFound();
            }

            return configuration;
        }

        // POST api/<ConfigurationsController>
        [Authorize(Roles = @"Admin")]
        [HttpPost]
        public async Task<ActionResult<List<Configuration>>> PostConfiguration(ConfigurationInput configuration)
        {

            if (_context.Configurations == null)
            {
                return Problem("Entity set 'AppDbContext.Configurations'  is null.");
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };

            List<Configuration> conf = new();
            foreach (int dep in configuration.DepartmentId)
            {
                conf.Add(new()
                {
                    DepartmentId = dep,
                    ConfigDesc = configuration.ConfigDesc,
                    Category = configuration.Category,
                    ConfigValue = configuration.ConfigValue,
                    IsEnabled = configuration.IsEnabled
                });


                _context.Configurations.Add(conf.Last());
            }

            await _context.SaveChangesAsync(audit);

            return conf;

        }

        // PUT api/<ConfigurationsController>/5
        [Authorize(Roles = @"Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConfiguration(int id, Configuration configuration)
        {
            if (id != configuration.ConfigId)
            {
                return BadRequest();
            }
            Audit audit = new()
            {
                CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System"
            };
            
            try
            {
                Configuration oldConfiguration = await _context.Configurations.Where(a => a.ConfigId == configuration.ConfigId).FirstOrDefaultAsync();
                _context.Entry(oldConfiguration).CurrentValues.SetValues(configuration);
                await _context.SaveChangesAsync(audit);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfigurationExists(id))
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

        // DELETE api/<ConfigurationsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Configurations == null)
            {
                return NotFound();
            }
            Configuration? configuration = await _context.Configurations.FindAsync(id);
            if (configuration == null)
            {
                return NotFound();
            }

            _context.Configurations.Remove(configuration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConfigurationExists(int id)
        {
            return (_context.Configurations?.Any(e => e.ConfigId == id)).GetValueOrDefault();
        }
    }
}
