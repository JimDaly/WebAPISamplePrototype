using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class OrderExpression
    {
        public OrderExpression()
        {
        }

        public OrderExpression(string attributeName, OrderType orderType)
        {
            AttributeName = attributeName;
            OrderType = orderType;
        }

        public OrderExpression(string attributeName, OrderType orderType, string alias)
        {
            AttributeName = attributeName;
            OrderType = orderType;
            Alias = alias;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public OrderType OrderType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }
    }
}