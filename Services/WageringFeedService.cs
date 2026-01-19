using KindredTest.DTOs;
using KindredTest.Repositories;
using System.Collections.Concurrent;

namespace KindredTest.Services
{
    public interface IWageringFeedService
    {
        Task<CustomerStatsDto> GetCustomerStatsAsync(int customerId, CancellationToken cancellationToken);
    }
    public class WageringFeedService : IWageringFeedService
    {
        private readonly IWageringFeedRepository _feedrepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ConcurrentDictionary<int, decimal> _totals = new();

        public WageringFeedService(IWageringFeedRepository feedrepository, ICustomerRepository customerRepository)
        {
            _feedrepository = feedrepository;
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// To process and calculate the pay out for the bets placed by a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CustomerStatsDto> GetCustomerStatsAsync(int customerId, CancellationToken cancellationToken)
        {
            
            var customer = await _customerRepository.GetCustomerAsync(customerId);
            var totalStandToWin = _feedrepository.GetTotalStandToWin(customerId);

            return new CustomerStatsDto
            {
                CustomerId = customer.Id,
                Name = customer.CustomerName,
                TotalStandToWin = totalStandToWin
            };
        }      

    }
}
