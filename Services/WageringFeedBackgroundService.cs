using KindredTest.Models;
using KindredTest.Repositories;
using static KindredTest.Models.Common;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

namespace KindredTest.Services
{
    public sealed class WageringFeedBackgroundService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IWageringFeedRepository _feedRepository;

        public WageringFeedBackgroundService(
            IConfiguration configuration,
            IWageringFeedRepository feedRepository)
        {
            _configuration = configuration;
            _feedRepository = feedRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var websocket = new ClientWebSocket();
            var uri =
                $"{_configuration["webSocketUrl"]}?candidateId={_configuration["candidateId"]}";

            await websocket.ConnectAsync(new Uri(uri), cancellationToken);

            var buffer = new byte[4097];

            while (!cancellationToken.IsCancellationRequested &&
                   websocket.State == WebSocketState.Open)
            {
                var response = await websocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (response.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, response.Count);
                var message = JsonSerializer.Deserialize<BaseMessage>(json);

                if (message == null)
                    continue;

                if (message.Type == MessageType.BetPlaced)
                {
                    var bet = ParseBetPlaced(message.Payload);
                    _feedRepository.AddBet(bet);
                }

                if (message.Type == MessageType.EndOfFeed)
                    break;
            }
        }

        private static BetPlaced ParseBetPlaced(JsonElement payload)
        {
            return new BetPlaced
            {
                CustomerId = payload.GetProperty("CustomerId").GetInt32(),
                FixtureId = payload.GetProperty("FixtureId").GetInt32(),
                Stake = payload.GetProperty("Stake").GetDecimal(),
                Odds = payload.GetProperty("Odds").GetDecimal()
            };
        }
    }

}
