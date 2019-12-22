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

        public string AttributeName { get; set; }

        public XrmAggregateType AggregateType { get; set; }

        public string Alias { get; set; } = null;

        public bool HasGroupBy { get; set; } = false;

        public XrmDateTimeGrouping DateTimeGrouping { get; set; }
    }
}