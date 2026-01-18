using System.Text.Json;
using System.Text.Json.Serialization;
using static KindredTest.Models.Common;

namespace KindredTest.Models
{

    public class BaseMessage
    {
        public MessageType Type { get; set; }
        public JsonElement Payload { get; set; }
        public DateTime TimeStamp { get; set; }

    }

    public class BetPlaced
    {
        public int CustomerId { get; set; }
        public int FixtureId { get; set; }
        public decimal Stake { get; set; }
        public decimal Odds { get; set; }
    }

    public class Common
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum MessageType
        {
            Fixture,
            BetPlaced,
            EndOfFeed
        }
    }

    public class FeedResult
    {
        public List<BetPlaced> Bets { get; set; }
        public Dictionary<int, Customer> Customers { get; init; } = new();
    }

}
