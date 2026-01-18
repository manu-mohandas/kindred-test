using KindredTest.DTOs;
using KindredTest.Repositories;
using System.Collections.Concurrent;

namespace KindredTest.Services
{
    public interface IWageringFeedService
    {
        Task<CustomerStatsDto> ProcessAllBetsAsync(int customerId, CancellationToken cancellationToken);
    }
    public class WageringFeedService : IWageringFeedService
    {
        private readonly IWageringFeedRepository _feedrepository;
        private readonly ConcurrentDictionary<int, decimal> _totals = new();

        public WageringFeedService(IWageringFeedRepository feedrepository)
        {
            _feedrepository = feedrepository;
        }

        /// <summary>
        /// To process and calculate the pay out for the bets placed by a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CustomerStatsDto> ProcessAllBetsAsync(int customerId, CancellationToken cancellationToken)
        {
            _totals.Clear();
            var feedResult = await _feedrepository.GetAllBetsAsync(cancellationToken);

            foreach (var betsItem in feedResult.Bets)
            {
                var standToWin = CalculateStandToWin(betsItem.Stake, betsItem.Odds);
                _totals.AddOrUpdate(betsItem.CustomerId, standToWin, (_, existing) => existing + standToWin);
            }


            return new CustomerStatsDto
            {
                CustomerId = feedResult.Customers[customerId].Id,
                Name = feedResult.Customers[customerId].CustomerName,
                TotalStandToWin = _totals.TryGetValue(customerId, out var total) ? total : 0
            };
        }

        private decimal CalculateStandToWin(decimal stake, decimal odds)
        {
            var payout = stake * odds;
            return payout - stake;
        }

    }
}
