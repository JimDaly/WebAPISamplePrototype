using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class XrmAttributeExpression
    {
        public XrmAttributeExpression() : this(null, XrmAggregateType.None, null)
        {
        }

        public XrmAttributeExpression(string attributeName)
        {
            AttributeName = attributeName;
        }

        public XrmAttributeExpression(string attributeName, XrmAggregateType aggregateType)
        {
            AttributeName = attributeName;
            AggregateType = aggregateType;
        }

        public XrmAttributeExpression(string attributeName, XrmAggregateType aggregateType, string alias)
        {
            AttributeName = attributeName;
            AggregateType = aggregateType;
            Alias = alias;
        }

        public XrmAttributeExpression(string attributeName, XrmAggregateType aggregateType, string alias, XrmDateTimeGrouping dateTimeGrouping)
        {
            AttributeName = attributeName;
            AggregateType = aggregateType;
            Alias = alias;
            DateTimeGrouping = dateTimeGrouping;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public XrmAggregateType AggregateType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; } = null;

        public bool HasGroupBy { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public XrmDateTimeGrouping DateTimeGrouping { get; set; }
    }
}