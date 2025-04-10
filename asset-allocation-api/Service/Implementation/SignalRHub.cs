using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Models;

namespace asset_allocation_api.Service.Implementation
{
    public class SignalRHub(IHubContext<SignalRHub> context, ILogger<SignalRHub> logger, IServiceScopeFactory scopeFactory) : Hub
    {
        protected IHubContext<SignalRHub> _context = context;
        private readonly ILogger<SignalRHub> _logger = logger;
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task SendMessage(string user, string message)
        {
            await _context.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendBroadcast(string message, CancellationToken cancellationToken)
        {
            await _context.Clients.All.SendAsync("ReceiveBroadcast", message, cancellationToken);
        }

        public async Task SendAllocation(AssetAllocation assetAlloc)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            DashboardService _dash = scope.ServiceProvider.GetRequiredService<DashboardService>();
            Dashboard temp = await _dash.ConvertAllocationToDashboard(assetAlloc, assetAlloc.ReturnedDate == null ? 1 : -1);
            
            try
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var asset = _dbContext.Assets.FirstOrDefault(a => a.Id == assetAlloc.AssetId);
                await _context.Clients.All.SendAsync($"AssetAllocated-{asset?.DepartmentId}", temp);
                if (assetAlloc.ReturnedDate == null)
                {
                    _logger.LogDebug("SignalR asset allocation message sent. Department ID: {}, Asset ID: {}", asset?.DepartmentId, assetAlloc.AssetId);                    
                }
                else
                {
                    _logger.LogDebug("SignalR asset return message sent. Department ID: {}, Asset ID: {}", asset?.DepartmentId, assetAlloc.AssetId);
                }

            }
            catch(Exception e)
            {
                _logger.LogError("An error occurred when send signalR message. Data: {} Message:{}", assetAlloc, e.Message );
            }
        }

        public async Task<Response<DashboardListResult?>> GetAllocationSummary(int departmentId)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                DashboardService _dash = scope.ServiceProvider.GetRequiredService<DashboardService>();
                return await _dash.GetAllocationSummary(_logger, departmentId);
            }
            catch (JsonException e)
            {   
                _logger.LogError("JSON Exception occurred on Dashboard service.");
                throw new JsonException();
            }
            catch (Exception e)
            {
                _logger.LogError("Unexpected error occurred on Dashboard service.");
                throw e;
            }
        }
    }
}