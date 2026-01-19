using KindredTest.Models;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static KindredTest.Models.Common;

namespace KindredTest.Repositories
{
    public interface IWageringFeedRepository
    {
        Task<FeedResult> GetAllBetsAsync(CancellationToken cancellationToken);
    }
    public class WageringFeedRepository : IWageringFeedRepository
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private FeedResult? _cachedResult;
        public WageringFeedRepository(IConfiguration configuration, HttpClient httpClient) 
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// To fetch the data from web socker
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<FeedResult> GetAllBetsAsync(CancellationToken cancellationToken)
        {

            if (_cachedResult != null)
            {
                return _cachedResult;
            }

            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                if(_cachedResult == null)
                {
                    _cachedResult = await GetAllBetsFromWebSocketAsync(cancellationToken);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return _cachedResult;
            
        }

        private async Task<FeedResult> GetAllBetsFromWebSocketAsync(CancellationToken cancellationToken)
        {
            var result = new FeedResult();
            result.Bets = new List<BetPlaced>();
            using var websocket = new ClientWebSocket();
            var uri = $"{_configuration["webSocketUrl"]}?candidateId={_configuration["candidateId"]}";
            await websocket.ConnectAsync(new Uri(uri), cancellationToken);
            var buffer = new byte[4097];



            while (!cancellationToken.IsCancellationRequested && websocket.State == WebSocketState.Open)
            {
                var response = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (response.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, response.Count);
                var message = JsonSerializer.Deserialize<BaseMessage>(json);
                if (message != null)
                {
                    if (message.Type == MessageType.BetPlaced)
                    {
                        var bet = ParseBetPlaced(message.Payload);
                        result.Bets.Add(bet);

                        if (!result.Customers.ContainsKey(bet.CustomerId))
                        {
                            var customer = await GetCustomerAsync(bet.CustomerId);
                            result.Customers.Add(bet.CustomerId, customer);
                        }
                    }
                    if (message.Type == MessageType.EndOfFeed)
                    {
                        break;
                    }
                }

            }

            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed", CancellationToken.None);

            return result;
        }
        private BetPlaced ParseBetPlaced(JsonElement payload)
        {
            return new BetPlaced
            {
                CustomerId = payload.GetProperty("CustomerId").GetInt32(),
                FixtureId = payload.GetProperty("FixtureId").GetInt32(),
                Stake = payload.GetProperty("Stake").GetDecimal(),
                Odds = payload.GetProperty("Odds").GetDecimal()!
            };
        }

        private async Task<Customer> GetCustomerAsync(int customerId)
        {
            var url = $"{_configuration["customerBaseUrl"]}/customer?customerId={customerId}&candidateId={_configuration["candidateId"]}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var customer =  JsonSerializer.Deserialize<Customer>(content, 
                new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true 
                });

            if (customer == null)
            {
                throw new InvalidOperationException("Failed to de-serialize customer json");
            }
            return customer;
        }
    }
}

