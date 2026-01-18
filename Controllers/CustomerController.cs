using KindredTest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KindredTest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IWageringFeedService _wageringFeedService;
        public CustomerController(IWageringFeedService  wageringFeedService) 
        {
            _wageringFeedService = wageringFeedService;
        }

        [HttpGet("{customer_id}/stats")]
        public async Task<IActionResult> GetStats(int customer_id, CancellationToken cancellationToken)
        {
            var customerStatsDto = await _wageringFeedService.ProcessAllBetsAsync(customer_id, cancellationToken);
            return Ok(customerStatsDto);
        }
    }
}
