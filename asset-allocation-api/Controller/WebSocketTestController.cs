using Microsoft.AspNetCore.Mvc;
using asset_allocation_api.Service.Implementation;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WebSocketTestController(SignalRHub chatHub) : ControllerBase
    {
        private readonly SignalRHub chatHub = chatHub;

        [HttpPost]
        public async Task<object> PostMessage(string message)
        {
            return await chatHub.GetAllocationSummary(int.Parse(message));
        }
    }
}