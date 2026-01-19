using KindredTest.Models;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static KindredTest.Models.Common;

namespace KindredTest.Repositories
{
    public interface IWageringFeedRepository
    {
        void AddBet(BetPlaced bet);
        decimal GetTotalStandToWin(int customerId);
    }
    public class WageringFeedRepository : IWageringFeedRepository
    {
        private readonly ConcurrentDictionary<int, decimal> _totals = new();
        public void AddBet(BetPlaced bet)
        {
            var standToWin = (bet.Stake * bet.Odds) - bet.Stake;

            _totals.AddOrUpdate(
                bet.CustomerId,
                standToWin,
                (_, existing) => existing + standToWin);
        }

        public decimal GetTotalStandToWin(int customerId)
        {
            return _totals.TryGetValue(customerId, out var total)
                ? total
                : 0;
        }

    }
}

