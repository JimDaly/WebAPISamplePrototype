using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class QueryExpression
    {
        public static readonly QueryExpression Empty = new QueryExpression();

        public QueryExpression()
            : this(null)
        {
        }

        public QueryExpression(string entityName)
        {
            EntityName = entityName;
            Criteria = new FilterExpression();
            PageInfo = new PagingInfo();
            ColumnSet = new ColumnSet();
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Distinct { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool NoLock { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PagingInfo PageInfo { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string QueryHints { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<LinkEntity> LinkEntities
        {
            get
            {
                if (_linkEntities == null)
                {
                    _linkEntities = new DataCollection<LinkEntity>();
                }
                return _linkEntities;
            }
            private set
            {
                _linkEntities = value;
            }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FilterExpression Criteria { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<OrderExpression> Orders
        {
            get
            {
                if (_orders == null)
                {
                    _orders = new DataCollection<OrderExpression>();
                }
                return _orders;
            }
            private set
            {
                _orders = value;
            }
        }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntitySetName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ColumnSet ColumnSet { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? TopCount { get; set; }

        public void AddOrder(string attributeName, OrderType orderType)
        {
            Orders.Add(new OrderExpression(attributeName, orderType));
        }

        public LinkEntity AddLink(string linkToEntityName, string linkFromAttributeName, string linkToAttributeName)
        {
            return AddLink(linkToEntityName, linkFromAttributeName, linkToAttributeName, JoinOperator.Inner);
        }

        public LinkEntity AddLink(string linkToEntityName, string linkFromAttributeName, string linkToAttributeName, JoinOperator joinOperator)
        {
            LinkEntity link = new LinkEntity(
                EntityName,
                linkToEntityName,
                linkFromAttributeName,
                linkToAttributeName,
                joinOperator);

            LinkEntities.Add(link);

            return link;
        }

        private DataCollection<LinkEntity> _linkEntities;
        private DataCollection<OrderExpression> _orders;
    }
}