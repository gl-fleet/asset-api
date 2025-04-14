using System.Security.Claims;

namespace ot_api_asset_allocation.Service.Middleware;

public class RoleCheckerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    
    public RoleCheckerMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, ILogger<RoleCheckerMiddleware> logger)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context) 
    {
        _logger.LogDebug("################################");
        _logger.LogDebug("Before Controller");

        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.Request.Headers.TryGetValue("Departmentid", out var departmentIdHeader))
        {
            await _next(context);
            _logger.LogDebug("After Controller - DepartmentId header not found.");
            return;
        }

        _logger.LogDebug("Departmentid Header: {}", departmentIdHeader);

        var roleDepartmentsClaim = user?.FindFirst("departments")?.Value;
        var roleClaim = user?.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

        if (roleClaim == null || roleClaim.Count == 0)
        {
            await _next(context);
            return;
        }

        // Safe parse roleDepartments
        var roleDepartments = new List<int>();
        if (!string.IsNullOrEmpty(roleDepartmentsClaim))
        {
            roleDepartments = roleDepartmentsClaim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : -1)
                .Where(id => id != -1)
                .ToList();
        }
        
        var departmentLevelPairs = roleDepartments
            .Select((dept, index) => new { DepartmentId = dept, Level = roleClaim.ElementAtOrDefault(index) })
            .ToList();

        _logger.LogDebug("Mapped Department-Level Pairs:");
        foreach (var pair in departmentLevelPairs)
        {
            _logger.LogDebug(" - DepartmentId: {}, Level: {}", pair.DepartmentId, pair.Level);
        }

        // Check and update role claim
        if (int.TryParse(departmentIdHeader, out var departmentId))
        {
            var match = departmentLevelPairs.FirstOrDefault(p => p.DepartmentId == departmentId);
            if (match != null && !string.IsNullOrWhiteSpace(match.Level))
            {
                _logger.LogDebug("Matched DepartmentId {} with Level {}", match.DepartmentId, match.Level);

                // Clone identity without role claims and add new role claim
                var identity = (ClaimsIdentity)user.Identity;
                var claims = identity.Claims
                    .Where(c => c.Type != ClaimTypes.Role)
                    .ToList();

                claims.Add(new Claim(ClaimTypes.Role, match.Level));
                var newIdentity = new ClaimsIdentity(claims, identity.AuthenticationType);
                context.User = new ClaimsPrincipal(newIdentity);
            }
            else
            {
                _logger.LogWarning("Request department id token department mismatched. Removing all roles...");
                var identity = (ClaimsIdentity)user.Identity;
                var claims = identity.Claims
                    .Where(c => c.Type != ClaimTypes.Role)
                    .ToList();

                claims.Add(new Claim(ClaimTypes.Role, "Level-0"));

                var newIdentity = new ClaimsIdentity(claims, identity.AuthenticationType);
                context.User = new ClaimsPrincipal(newIdentity);
            }
        }

        await _next(context);
        _logger.LogDebug("After Controller");
    }
}